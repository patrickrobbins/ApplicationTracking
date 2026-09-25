/*
    ============================================================================
    Application Dependency Tracker - Application packages
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: 15_ApplicationFamiliesTeamsTags.sql -> 16_ApplicationPackages.sql

    Adds a way to record the packages each application consumes:
      - PackageTypes:               managed catalog of package ecosystems
                                    (e.g. Npm, NuGet).
      - Packages:                   shared pool of normalized package names,
                                    keyed by package type. Created on-the-fly
                                    and reused by (type, name) across apps.
      - ApplicationPackageMappings: many-to-many join between applications and
                                    packages, with the deployed version tracked
                                    per application.

    All statements are idempotent so the script can be re-run safely.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* ============================================================================
   PackageTypes - the catalog of package ecosystems
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[PackageTypes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PackageTypes]
    (
        [PackageTypeId] INT IDENTITY(1,1) NOT NULL,
        [Name]          NVARCHAR(100)     NOT NULL,
        [Description]   NVARCHAR(500)     NULL,
        [IsActive]      BIT               NOT NULL CONSTRAINT [DF_PackageType_IsActive] DEFAULT (1),
        [SortOrder]     INT               NOT NULL CONSTRAINT [DF_PackageType_SortOrder] DEFAULT (0),
        [CreatedDate]   DATETIME2(0)      NOT NULL CONSTRAINT [DF_PackageType_CreatedDate] DEFAULT (GETUTCDATE()),
        [ModifiedDate]  DATETIME2(0)      NOT NULL CONSTRAINT [DF_PackageType_ModifiedDate] DEFAULT (GETUTCDATE()),
        [CreatedBy]     NVARCHAR(128)     NULL,
        [ModifiedBy]    NVARCHAR(128)     NULL,
        CONSTRAINT [PK_PackageTypes] PRIMARY KEY CLUSTERED ([PackageTypeId] ASC),
        CONSTRAINT [UQ_PackageTypes_Name] UNIQUE NONCLUSTERED ([Name] ASC)
    );
END
GO

/* ============================================================================
   Packages - the shared package pool keyed by (type, name)
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[Packages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Packages]
    (
        [PackageId]     INT IDENTITY(1,1) NOT NULL,
        [PackageTypeId] INT               NOT NULL,   -- -> PackageTypes
        [Name]          NVARCHAR(200)     NOT NULL,
        [IsActive]      BIT               NOT NULL CONSTRAINT [DF_Package_IsActive] DEFAULT (1),
        [SortOrder]     INT               NOT NULL CONSTRAINT [DF_Package_SortOrder] DEFAULT (0),
        [CreatedDate]   DATETIME2(0)      NOT NULL CONSTRAINT [DF_Package_CreatedDate] DEFAULT (GETUTCDATE()),
        [ModifiedDate]  DATETIME2(0)      NOT NULL CONSTRAINT [DF_Package_ModifiedDate] DEFAULT (GETUTCDATE()),
        [CreatedBy]     NVARCHAR(128)     NULL,
        [ModifiedBy]    NVARCHAR(128)     NULL,
        CONSTRAINT [PK_Packages] PRIMARY KEY CLUSTERED ([PackageId] ASC),
        CONSTRAINT [UQ_Packages_TypeName] UNIQUE NONCLUSTERED ([PackageTypeId] ASC, [Name] ASC),
        CONSTRAINT [FK_Packages_PackageType] FOREIGN KEY ([PackageTypeId])
            REFERENCES [dbo].[PackageTypes] ([PackageTypeId]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_Packages_PackageTypeId' AND [object_id] = OBJECT_ID(N'dbo.Packages'))
    CREATE NONCLUSTERED INDEX [IX_Packages_PackageTypeId] ON [dbo].[Packages] ([PackageTypeId] ASC);
GO

/* ============================================================================
   ApplicationPackageMappings - many-to-many join between apps and packages
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[ApplicationPackageMappings]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationPackageMappings]
    (
        [MappingId]     INT IDENTITY(1,1) NOT NULL,
        [ApplicationId] INT               NOT NULL,   -- -> Applications
        [PackageId]     INT               NOT NULL,   -- -> Packages
        [Version]       NVARCHAR(100)     NULL,       -- deployed package version
        CONSTRAINT [PK_ApplicationPackageMappings] PRIMARY KEY CLUSTERED ([MappingId] ASC),
        CONSTRAINT [FK_AppPackageMap_Application] FOREIGN KEY ([ApplicationId])
            REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
        CONSTRAINT [FK_AppPackageMap_Package] FOREIGN KEY ([PackageId])
            REFERENCES [dbo].[Packages] ([PackageId]) ON DELETE CASCADE,
        CONSTRAINT [UQ_AppPackageMap_App_Package] UNIQUE NONCLUSTERED ([ApplicationId] ASC, [PackageId] ASC)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_AppPackageMap_ApplicationId' AND [object_id] = OBJECT_ID(N'dbo.ApplicationPackageMappings'))
    CREATE NONCLUSTERED INDEX [IX_AppPackageMap_ApplicationId] ON [dbo].[ApplicationPackageMappings] ([ApplicationId] ASC);
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_AppPackageMap_PackageId' AND [object_id] = OBJECT_ID(N'dbo.ApplicationPackageMappings'))
    CREATE NONCLUSTERED INDEX [IX_AppPackageMap_PackageId] ON [dbo].[ApplicationPackageMappings] ([PackageId] ASC);
GO

/* ============================================================================
   Seed the default package types (idempotent)
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[PackageTypes] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Npm',   N'Node.js / JavaScript packages from the npm registry.', 10),
    (N'NuGet', N'.NET packages from the NuGet gallery.', 20)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[PackageTypes] t WHERE t.[Name] = s.[Name]);
GO

PRINT N'Application packages ready.';
GO