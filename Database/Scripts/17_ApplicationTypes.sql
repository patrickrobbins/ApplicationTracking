/*
    ============================================================================
    Application Dependency Tracker - Application types
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: 16_ApplicationPackages.sql -> 17_ApplicationTypes.sql

    Adds a lightweight, admin-managed categorization for what kind of
    system each application is (e.g. Batch Job, Web Site, Web API, Windows
    Service, Desktop Application, Mobile Application, Cloud Service).

    All statements are idempotent so the script can be re-run safely.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* ============================================================================
   ApplicationTypes - the catalog of application kinds
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[ApplicationTypes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationTypes]
    (
        [ApplicationTypeId] INT IDENTITY(1,1) NOT NULL,
        [Name]              NVARCHAR(100)     NOT NULL,
        [Description]       NVARCHAR(500)     NULL,
        [IsActive]          BIT               NOT NULL CONSTRAINT [DF_AppType_IsActive] DEFAULT (1),
        [SortOrder]         INT               NOT NULL CONSTRAINT [DF_AppType_SortOrder] DEFAULT (0),
        [CreatedDate]       DATETIME2(0)      NOT NULL CONSTRAINT [DF_AppType_CreatedDate] DEFAULT (GETUTCDATE()),
        [ModifiedDate]      DATETIME2(0)      NOT NULL CONSTRAINT [DF_AppType_ModifiedDate] DEFAULT (GETUTCDATE()),
        [CreatedBy]         NVARCHAR(128)     NULL,
        [ModifiedBy]        NVARCHAR(128)     NULL,
        CONSTRAINT [PK_ApplicationTypes] PRIMARY KEY CLUSTERED ([ApplicationTypeId] ASC),
        CONSTRAINT [UQ_ApplicationTypes_Name] UNIQUE NONCLUSTERED ([Name] ASC)
    );
END
GO

/* ============================================================================
   Application type column + FK on Applications
   ------------------------------------------------------------------------- */
IF COL_LENGTH(N'dbo.Applications', N'ApplicationTypeId') IS NULL
    ALTER TABLE [dbo].[Applications] ADD [ApplicationTypeId] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = N'FK_Applications_ApplicationType')
    ALTER TABLE [dbo].[Applications] ADD CONSTRAINT [FK_Applications_ApplicationType]
        FOREIGN KEY ([ApplicationTypeId]) REFERENCES [dbo].[ApplicationTypes] ([ApplicationTypeId]) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_Applications_ApplicationTypeId' AND [object_id] = OBJECT_ID(N'dbo.Applications'))
    CREATE NONCLUSTERED INDEX [IX_Applications_ApplicationTypeId] ON [dbo].[Applications] ([ApplicationTypeId] ASC);
GO

/* ============================================================================
   Seed the default application types (idempotent)
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[ApplicationTypes] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Batch Job',           N'Scheduled batch processing or data transformation.', 10),
    (N'Web Site',            N'Web-based user interface delivered to browsers.', 20),
    (N'Web API',             N'HTTP API or service layer consumed programmatically.', 30),
    (N'Windows Service',     N'Background service running on Windows.', 40),
    (N'Desktop Application', N'Installed application running on a user workstation.', 50),
    (N'Mobile Application',  N'Application running on a mobile device.', 60),
    (N'Cloud Service',       N'Cloud-hosted service or platform.', 70)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationTypes] t WHERE t.[Name] = s.[Name]);
GO

PRINT N'Application types ready.';
GO