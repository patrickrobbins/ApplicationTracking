/*
    ============================================================================
    Application Dependency Tracker - Application families, technical ownership
    teams, and shared tags
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: 14_ApplicationProperties.sql -> 15_ApplicationFamiliesTeamsTags.sql

    Adds four concepts:
      - ApplicationFamilies:      admin-managed one-to-many family on each app.
      - TechnicalOwnershipTeams:  admin-managed team ownership. Replaces the
                                  free-text [TechnicalOwner] column on
                                  Applications (the email field is retained).
      - ApplicationTags:          shared, user-generated tags (many-to-many).
      - ApplicationTagMappings:   join between applications and tags.

    All statements are idempotent so the script can be re-run safely.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* ============================================================================
   ApplicationFamilies - the catalog of application families
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[ApplicationFamilies]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationFamilies]
    (
        [FamilyId]    INT IDENTITY(1,1) NOT NULL,
        [Name]        NVARCHAR(100)     NOT NULL,
        [Description] NVARCHAR(500)     NULL,
        [IsActive]    BIT               NOT NULL CONSTRAINT [DF_AppFamily_IsActive] DEFAULT (1),
        [SortOrder]   INT               NOT NULL CONSTRAINT [DF_AppFamily_SortOrder] DEFAULT (0),
        [CreatedDate]  DATETIME2(0)     NOT NULL CONSTRAINT [DF_AppFamily_CreatedDate] DEFAULT (GETUTCDATE()),
        [ModifiedDate] DATETIME2(0)     NOT NULL CONSTRAINT [DF_AppFamily_ModifiedDate] DEFAULT (GETUTCDATE()),
        [CreatedBy]    NVARCHAR(128)    NULL,
        [ModifiedBy]   NVARCHAR(128)    NULL,
        CONSTRAINT [PK_ApplicationFamilies] PRIMARY KEY CLUSTERED ([FamilyId] ASC),
        CONSTRAINT [UQ_ApplicationFamilies_Name] UNIQUE NONCLUSTERED ([Name] ASC)
    );
END
GO

/* ============================================================================
   TechnicalOwnershipTeams - the catalog of technical ownership teams
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[TechnicalOwnershipTeams]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TechnicalOwnershipTeams]
    (
        [TeamId]      INT IDENTITY(1,1) NOT NULL,
        [Name]        NVARCHAR(100)     NOT NULL,
        [Description] NVARCHAR(500)     NULL,
        [IsActive]    BIT               NOT NULL CONSTRAINT [DF_OwnershipTeam_IsActive] DEFAULT (1),
        [SortOrder]   INT               NOT NULL CONSTRAINT [DF_OwnershipTeam_SortOrder] DEFAULT (0),
        [CreatedDate]  DATETIME2(0)     NOT NULL CONSTRAINT [DF_OwnershipTeam_CreatedDate] DEFAULT (GETUTCDATE()),
        [ModifiedDate] DATETIME2(0)     NOT NULL CONSTRAINT [DF_OwnershipTeam_ModifiedDate] DEFAULT (GETUTCDATE()),
        [CreatedBy]    NVARCHAR(128)    NULL,
        [ModifiedBy]   NVARCHAR(128)    NULL,
        CONSTRAINT [PK_TechnicalOwnershipTeams] PRIMARY KEY CLUSTERED ([TeamId] ASC),
        CONSTRAINT [UQ_TechnicalOwnershipTeams_Name] UNIQUE NONCLUSTERED ([Name] ASC)
    );
END
GO

/* ============================================================================
   ApplicationTags - the shared tag pool
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[ApplicationTags]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationTags]
    (
        [TagId]       INT IDENTITY(1,1) NOT NULL,
        [Name]        NVARCHAR(100)     NOT NULL,
        [Description] NVARCHAR(500)     NULL,
        [IsActive]    BIT               NOT NULL CONSTRAINT [DF_AppTag_IsActive] DEFAULT (1),
        [SortOrder]   INT               NOT NULL CONSTRAINT [DF_AppTag_SortOrder] DEFAULT (0),
        [CreatedDate]  DATETIME2(0)     NOT NULL CONSTRAINT [DF_AppTag_CreatedDate] DEFAULT (GETUTCDATE()),
        [ModifiedDate] DATETIME2(0)     NOT NULL CONSTRAINT [DF_AppTag_ModifiedDate] DEFAULT (GETUTCDATE()),
        [CreatedBy]    NVARCHAR(128)    NULL,
        [ModifiedBy]   NVARCHAR(128)    NULL,
        CONSTRAINT [PK_ApplicationTags] PRIMARY KEY CLUSTERED ([TagId] ASC),
        CONSTRAINT [UQ_ApplicationTags_Name] UNIQUE NONCLUSTERED ([Name] ASC)
    );
END
GO

/* ============================================================================
   ApplicationTagMappings - many-to-many join between apps and tags
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[ApplicationTagMappings]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationTagMappings]
    (
        [MappingId]     INT IDENTITY(1,1) NOT NULL,
        [ApplicationId] INT               NOT NULL,   -- -> Applications
        [TagId]         INT               NOT NULL,   -- -> ApplicationTags
        CONSTRAINT [PK_ApplicationTagMappings] PRIMARY KEY CLUSTERED ([MappingId] ASC),
        CONSTRAINT [FK_AppTagMap_Application] FOREIGN KEY ([ApplicationId])
            REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
        CONSTRAINT [FK_AppTagMap_Tag] FOREIGN KEY ([TagId])
            REFERENCES [dbo].[ApplicationTags] ([TagId]) ON DELETE CASCADE,
        CONSTRAINT [UQ_AppTagMap_App_Tag] UNIQUE NONCLUSTERED ([ApplicationId], [TagId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_AppTagMap_ApplicationId' AND [object_id] = OBJECT_ID(N'dbo.ApplicationTagMappings'))
    CREATE NONCLUSTERED INDEX [IX_AppTagMap_ApplicationId] ON [dbo].[ApplicationTagMappings] ([ApplicationId] ASC);
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_AppTagMap_TagId' AND [object_id] = OBJECT_ID(N'dbo.ApplicationTagMappings'))
    CREATE NONCLUSTERED INDEX [IX_AppTagMap_TagId] ON [dbo].[ApplicationTagMappings] ([TagId] ASC);
GO

/* ============================================================================
   Family column + FK on Applications
   ------------------------------------------------------------------------- */
IF COL_LENGTH(N'dbo.Applications', N'FamilyId') IS NULL
    ALTER TABLE [dbo].[Applications] ADD [FamilyId] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = N'FK_Applications_Family')
    ALTER TABLE [dbo].[Applications] ADD CONSTRAINT [FK_Applications_Family]
        FOREIGN KEY ([FamilyId]) REFERENCES [dbo].[ApplicationFamilies] ([FamilyId]) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_Applications_FamilyId' AND [object_id] = OBJECT_ID(N'dbo.Applications'))
    CREATE NONCLUSTERED INDEX [IX_Applications_FamilyId] ON [dbo].[Applications] ([FamilyId] ASC);
GO

/* ============================================================================
   Technical ownership team column + FK on Applications
   ------------------------------------------------------------------------- */
IF COL_LENGTH(N'dbo.Applications', N'TechnicalOwnershipTeamId') IS NULL
    ALTER TABLE [dbo].[Applications] ADD [TechnicalOwnershipTeamId] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = N'FK_Applications_OwnershipTeam')
    ALTER TABLE [dbo].[Applications] ADD CONSTRAINT [FK_Applications_OwnershipTeam]
        FOREIGN KEY ([TechnicalOwnershipTeamId]) REFERENCES [dbo].[TechnicalOwnershipTeams] ([TeamId]) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_Applications_TechnicalOwnershipTeamId' AND [object_id] = OBJECT_ID(N'dbo.Applications'))
    CREATE NONCLUSTERED INDEX [IX_Applications_TechnicalOwnershipTeamId] ON [dbo].[Applications] ([TechnicalOwnershipTeamId] ASC);
GO

/* ============================================================================
   Seed default families (idempotent)
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[ApplicationFamilies] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Business Applications', N'Customer-, partner- and staff-facing business systems.', 10),
    (N'Shared Services',       N'Shared platforms and services consumed by multiple systems.', 20),
    (N'Data & Analytics',      N'Data stores, warehouses and analytics platforms.', 30),
    (N'Infrastructure',        N'Foundational infrastructure and identity services.', 40)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationFamilies] f WHERE f.[Name] = s.[Name]);
GO

/* ============================================================================
Backfill: promote the old free-text [TechnicalOwner] values into the team
    catalog and link matching applications. Guarded on the column still existing
    and executed with dynamic SQL (name resolution is deferred) so the script can
    be safely re-run after the column has already been dropped.
    ------------------------------------------------------------------------- */
IF COL_LENGTH(N'dbo.Applications', N'TechnicalOwner') IS NOT NULL
BEGIN
    EXEC(N'
        INSERT INTO [dbo].[TechnicalOwnershipTeams] ([Name], [SortOrder])
        SELECT DISTINCT a.[TechnicalOwner], 0
        FROM [dbo].[Applications] a
        WHERE a.[TechnicalOwner] IS NOT NULL AND LTRIM(RTRIM(a.[TechnicalOwner])) <> N''''
            AND NOT EXISTS (SELECT 1 FROM [dbo].[TechnicalOwnershipTeams] t WHERE t.[Name] = a.[TechnicalOwner]);
    ');

    EXEC(N'
        UPDATE a
        SET a.[TechnicalOwnershipTeamId] = t.[TeamId]
        FROM [dbo].[Applications] a
        INNER JOIN [dbo].[TechnicalOwnershipTeams] t ON t.[Name] = a.[TechnicalOwner]
        WHERE a.[TechnicalOwner] IS NOT NULL AND LTRIM(RTRIM(a.[TechnicalOwner])) <> N'''';
    ');
END
GO

IF COL_LENGTH(N'dbo.Applications', N'TechnicalOwner') IS NOT NULL
    ALTER TABLE [dbo].[Applications] DROP COLUMN [TechnicalOwner];
GO

/* ============================================================================
   Seed the default technical ownership teams (we no longer reference the
   dropped column, so this runs after the DROP).
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[TechnicalOwnershipTeams] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Platform Team',      N'Core platform and hosting.', 10),
    (N'Commerce Team',      N'Commerce and order systems.', 20),
    (N'Finance IT',         N'Finance, payment and billing systems.', 30),
    (N'Security Team',      N'Security and identity systems.', 40),
    (N'CRM Team',           N'CRM and sales systems.', 50),
    (N'Data Platform Team', N'Data stores and analytics.', 60),
    (N'Mainframe Team',     N'Mainframe systems.', 70),
    (N'Risk Team',          N'Risk and fraud systems.', 80),
    (N'Logistics IT',       N'Logistics and warehouse systems.', 90)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[TechnicalOwnershipTeams] t WHERE t.[Name] = s.[Name]);
GO

PRINT N'Application families, technical ownership teams and tags ready.';
GO