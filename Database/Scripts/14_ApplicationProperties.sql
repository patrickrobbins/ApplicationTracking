/*
    ============================================================================
    Application Dependency Tracker - Application Properties & Technologies
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: 13_DependencyCriticality.sql -> 14_ApplicationProperties.sql

    Adds the additional application property columns (IDE, framework version,
    documentation link, hours of operation, maintenance window, maintenance
    notification email, source control location, application summary and
    triage steps) to [Applications].

    Adds a managed ApplicationTechnologies tag list and the many-to-many
    ApplicationTechnologyMappings join table so applications can be tagged
    with technologies that are searchable and filterable.

    Idempotent: safe to re-run against the live database.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* 1. Additional scalar property columns on Applications. */
IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'ApplicationIDE'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [ApplicationIDE] NVARCHAR(200) NULL;
    PRINT N'Added Applications.ApplicationIDE column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'FrameworkVersion'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [FrameworkVersion] NVARCHAR(100) NULL;
    PRINT N'Added Applications.FrameworkVersion column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'DocumentationLink'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [DocumentationLink] NVARCHAR(500) NULL;
    PRINT N'Added Applications.DocumentationLink column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'SourceControlLocation'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [SourceControlLocation] NVARCHAR(500) NULL;
    PRINT N'Added Applications.SourceControlLocation column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'HoursOfOperation'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [HoursOfOperation] NVARCHAR(200) NULL;
    PRINT N'Added Applications.HoursOfOperation column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'MaintenanceWindow'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [MaintenanceWindow] NVARCHAR(200) NULL;
    PRINT N'Added Applications.MaintenanceWindow column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'MaintenanceNotificationEmail'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [MaintenanceNotificationEmail] NVARCHAR(200) NULL;
    PRINT N'Added Applications.MaintenanceNotificationEmail column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'ApplicationSummary'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [ApplicationSummary] NVARCHAR(2000) NULL;
    PRINT N'Added Applications.ApplicationSummary column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'TriageSteps'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [TriageSteps] NVARCHAR(2000) NULL;
    PRINT N'Added Applications.TriageSteps column.';
END
GO

/* 2. ApplicationTechnologies - managed, dynamic list of technology tags. */
IF OBJECT_ID(N'[dbo].[ApplicationTechnologies]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationTechnologies]
    (
        [TechnologyId] INT IDENTITY(1,1) NOT NULL,
        [Name]         NVARCHAR(100)     NOT NULL,
        [Description]  NVARCHAR(500)     NULL,
        [IsActive]     BIT               NOT NULL CONSTRAINT [DF_AppTech_IsActive] DEFAULT (1),
        [SortOrder]    INT               NOT NULL CONSTRAINT [DF_AppTech_SortOrder] DEFAULT (0),
        [CreatedDate]  DATETIME2(0)      NOT NULL CONSTRAINT [DF_AppTech_CreatedDate] DEFAULT (GETUTCDATE()),
        [ModifiedDate] DATETIME2(0)      NOT NULL CONSTRAINT [DF_AppTech_ModifiedDate] DEFAULT (GETUTCDATE()),
        [CreatedBy]    NVARCHAR(128)     NULL,
        [ModifiedBy]   NVARCHAR(128)     NULL,
        CONSTRAINT [PK_ApplicationTechnologies] PRIMARY KEY CLUSTERED ([TechnologyId] ASC),
        CONSTRAINT [UQ_ApplicationTechnologies_Name] UNIQUE NONCLUSTERED ([Name] ASC)
    );
    PRINT N'Created ApplicationTechnologies table.';
END
GO

/* 3. ApplicationTechnologyMappings - join between applications and tags. */
IF OBJECT_ID(N'[dbo].[ApplicationTechnologyMappings]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationTechnologyMappings]
    (
        [MappingId]     INT IDENTITY(1,1) NOT NULL,
        [ApplicationId] INT               NOT NULL,   -- -> Applications
        [TechnologyId]  INT               NOT NULL,   -- -> ApplicationTechnologies
        CONSTRAINT [PK_ApplicationTechnologyMappings] PRIMARY KEY CLUSTERED ([MappingId] ASC),
        CONSTRAINT [FK_AppTechMap_Application] FOREIGN KEY ([ApplicationId])
            REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
        CONSTRAINT [FK_AppTechMap_Technology] FOREIGN KEY ([TechnologyId])
            REFERENCES [dbo].[ApplicationTechnologies] ([TechnologyId]) ON DELETE CASCADE,
        CONSTRAINT [UQ_AppTechMap_App_Technology] UNIQUE NONCLUSTERED ([ApplicationId], [TechnologyId])
    );
    PRINT N'Created ApplicationTechnologyMappings table.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[indexes]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[ApplicationTechnologyMappings]') AND [name] = N'IX_AppTechMap_ApplicationId'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AppTechMap_ApplicationId] ON [dbo].[ApplicationTechnologyMappings] ([ApplicationId] ASC);
    PRINT N'Created IX_AppTechMap_ApplicationId index.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[indexes]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[ApplicationTechnologyMappings]') AND [name] = N'IX_AppTechMap_TechnologyId'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AppTechMap_TechnologyId] ON [dbo].[ApplicationTechnologyMappings] ([TechnologyId] ASC);
    PRINT N'Created IX_AppTechMap_TechnologyId index.';
END
GO

/* 4. Seed the technology tag list (idempotent). */
INSERT INTO [dbo].[ApplicationTechnologies]
    ([Name], [Description], [IsActive], [SortOrder])
SELECT src.[Name], NULL, 1, src.[SortOrder]
FROM (VALUES
    (N'.NET Framework', 10),
    (N'ASP.NET',        20),
    (N'ASP.NET Core',   30),
    (N'Angular',        40),
    (N'React',          50),
    (N'SQL Server',     60),
    (N'Oracle',         70),
    (N'Redis',          80),
    (N'RabbitMQ',       90),
    (N'Windows Service', 100),
    (N'Java',           110),
    (N'Node.js',        120)
) AS src([Name], [SortOrder])
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[ApplicationTechnologies] t WHERE t.[Name] = src.[Name]
);
GO

/* 5. Seed a few example mappings so the technology filter is verifiable
      (idempotent; the unique constraint guards re-runs). */
INSERT INTO [dbo].[ApplicationTechnologyMappings]
    ([ApplicationId], [TechnologyId])
SELECT a.ApplicationId, t.TechnologyId
FROM [dbo].[Applications] a
CROSS JOIN [dbo].[ApplicationTechnologies] t
WHERE t.[Name] IN (N'.NET Framework', N'ASP.NET', N'SQL Server')
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[ApplicationTechnologyMappings] m
      WHERE m.ApplicationId = a.ApplicationId AND m.TechnologyId = t.TechnologyId
  );
GO

PRINT N'Application properties/technologies migration complete.';
GO
