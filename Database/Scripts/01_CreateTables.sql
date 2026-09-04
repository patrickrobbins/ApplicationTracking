/*
    ============================================================================
    Application Dependency Tracker - Database Schema
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: 01_CreateTables.sql -> 02_SeedData.sql
    ============================================================================
*/

IF DB_ID(N'DependencyTracker') IS NULL
BEGIN
    CREATE DATABASE [DependencyTracker];
END
GO

USE [DependencyTracker];
GO

IF OBJECT_ID(N'[dbo].[ActivityLog]', N'U') IS NOT NULL DROP TABLE [dbo].[ActivityLog];
IF OBJECT_ID(N'[dbo].[Dependencies]', N'U') IS NOT NULL DROP TABLE [dbo].[Dependencies];
IF OBJECT_ID(N'[dbo].[ApplicationTechnologyMappings]', N'U') IS NOT NULL DROP TABLE [dbo].[ApplicationTechnologyMappings];
IF OBJECT_ID(N'[dbo].[ApplicationDlls]', N'U') IS NOT NULL DROP TABLE [dbo].[ApplicationDlls];
IF OBJECT_ID(N'[dbo].[Applications]', N'U') IS NOT NULL DROP TABLE [dbo].[Applications];
IF OBJECT_ID(N'[dbo].[ApplicationTechnologies]', N'U') IS NOT NULL DROP TABLE [dbo].[ApplicationTechnologies];
IF OBJECT_ID(N'[dbo].[ApplicationCategories]', N'U') IS NOT NULL DROP TABLE [dbo].[ApplicationCategories];
IF OBJECT_ID(N'[dbo].[ADGroups]', N'U') IS NOT NULL DROP TABLE [dbo].[ADGroups];
GO

/* ============================================================================
   ApplicationCategories - managed, dynamic list of application categories
   (e.g. Internal, External, Library, Database, Active Directory)
   ------------------------------------------------------------------------- */
CREATE TABLE [dbo].[ApplicationCategories]
(
    [CategoryId]   INT IDENTITY(1,1) NOT NULL,
    [Name]         NVARCHAR(100)     NOT NULL,
    [Description]  NVARCHAR(500)     NULL,
    [IsActive]     BIT               NOT NULL CONSTRAINT [DF_AppCat_IsActive] DEFAULT (1),
    [SortOrder]    INT               NOT NULL CONSTRAINT [DF_AppCat_SortOrder] DEFAULT (0),
    [CreatedDate]  DATETIME2(0)      NOT NULL CONSTRAINT [DF_AppCat_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate] DATETIME2(0)      NOT NULL CONSTRAINT [DF_AppCat_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]    NVARCHAR(128)     NULL,
    [ModifiedBy]   NVARCHAR(128)     NULL,
    CONSTRAINT [PK_ApplicationCategories] PRIMARY KEY CLUSTERED ([CategoryId] ASC),
    CONSTRAINT [UQ_ApplicationCategories_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);
GO

/* ============================================================================
   Applications
   ------------------------------------------------------------------------- */
CREATE TABLE [dbo].[Applications]
(
    [ApplicationId]     INT IDENTITY(1,1) NOT NULL,
    [Name]              NVARCHAR(200)     NOT NULL,
    [Version]           NVARCHAR(50)      NULL,       -- deployed version; multiple versions share the same Name
    [Description]       NVARCHAR(2000)    NULL,
    [BusinessOwner]     NVARCHAR(200)     NULL,
    [BusinessOwnerEmail] NVARCHAR(200)     NULL,
    [BusinessGroup]     NVARCHAR(200)     NULL,
    [BusinessBackup]    NVARCHAR(200)     NULL,
    [BusinessBackupEmail] NVARCHAR(200)     NULL,
    [TechnicalOwner]    NVARCHAR(200)     NULL,
    [TechnicalOwnerEmail] NVARCHAR(200)     NULL,
    [CategoryId]        INT               NULL,       -- -> ApplicationCategories (managed list)
    [Environment]       NVARCHAR(50)      NULL,       -- Production, Staging, Development
    [CriticalityLevel]  NVARCHAR(20)      NULL,       -- Critical, High, Medium, Low
    [Status]            NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Applications_Status] DEFAULT (N'Active'), -- Active, Retired, Planned
    [ExternalUrl]       NVARCHAR(500)     NULL,
    [SourcePath]        NVARCHAR(500)     NULL,       -- physical dir / UNC used by the IIS discovery scanner
    [ApplicationIDE]    NVARCHAR(200)     NULL,       -- development IDE used to build the application
    [FrameworkVersion]  NVARCHAR(100)     NULL,       -- runtime framework / .NET version
    [DocumentationLink] NVARCHAR(500)     NULL,       -- URL of the application documentation
    [SourceControlLocation] NVARCHAR(500) NULL,       -- repository URL / VCS path
    [HoursOfOperation]  NVARCHAR(200)     NULL,       -- supported operating hours, e.g. 24x7
    [MaintenanceWindow] NVARCHAR(200)     NULL,       -- scheduled maintenance window
    [MaintenanceNotificationEmail] NVARCHAR(200) NULL, -- email notified of maintenance
    [ApplicationSummary] NVARCHAR(2000)   NULL,       -- short summary of what the application does
    [TriageSteps]       NVARCHAR(2000)    NULL,       -- incident triage / runbook steps
    [IsDeleted]         BIT               NOT NULL CONSTRAINT [DF_Applications_IsDeleted] DEFAULT (0), -- soft-delete flag; 1 = hidden, restorable
    [DefaultDependencyCriticality] NVARCHAR(20) NULL, -- default criticality when this app is the provider (target) of a dependency
    [DefaultDependencyImpact]      NVARCHAR(2000) NULL, -- default impact note applied to dependencies on this app
    [CreatedDate]       DATETIME2(0)      NOT NULL CONSTRAINT [DF_Applications_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate]      DATETIME2(0)      NOT NULL CONSTRAINT [DF_Applications_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]         NVARCHAR(128)     NULL,
    [ModifiedBy]        NVARCHAR(128)     NULL,
    CONSTRAINT [PK_Applications] PRIMARY KEY CLUSTERED ([ApplicationId] ASC),
    CONSTRAINT [UQ_Applications_Name_Version] UNIQUE NONCLUSTERED ([Name] ASC, [Version] ASC),
    CONSTRAINT [FK_Applications_Category] FOREIGN KEY ([CategoryId])
        REFERENCES [dbo].[ApplicationCategories] ([CategoryId]) ON DELETE SET NULL,
    CONSTRAINT [CK_Applications_Environment] CHECK ([Environment] IN (N'Production', N'Staging', N'Development')),
    CONSTRAINT [CK_Applications_Criticality] CHECK ([CriticalityLevel] IN (N'Critical', N'High', N'Medium', N'Low')),
    CONSTRAINT [CK_Applications_Status] CHECK ([Status] IN (N'Active', N'Retired', N'Planned')),
    CONSTRAINT [CK_Applications_DefaultDependencyCriticality]
        CHECK ([DefaultDependencyCriticality] IN (N'Critical', N'High', N'Medium', N'Low'))
);
GO

CREATE NONCLUSTERED INDEX [IX_Applications_Name] ON [dbo].[Applications] ([Name] ASC);
CREATE NONCLUSTERED INDEX [IX_Applications_Status] ON [dbo].[Applications] ([Status] ASC);
CREATE NONCLUSTERED INDEX [IX_Applications_Environment] ON [dbo].[Applications] ([Environment] ASC);
CREATE NONCLUSTERED INDEX [IX_Applications_CategoryId] ON [dbo].[Applications] ([CategoryId] ASC);
CREATE NONCLUSTERED INDEX [IX_Applications_SourcePath] ON [dbo].[Applications] ([SourcePath] ASC);
CREATE NONCLUSTERED INDEX [IX_Applications_IsDeleted] ON [dbo].[Applications] ([IsDeleted] ASC);
GO

/* ============================================================================
   ApplicationTechnologies - managed, dynamic list of technology tags applied
   to applications (e.g. .NET Framework, ASP.NET, SQL Server, Angular).
   ------------------------------------------------------------------------- */
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
GO

/* ============================================================================
   ApplicationTechnologyMappings - many-to-many join between applications and
   technology tags. Deleting an application or a tag removes its mappings.
   ------------------------------------------------------------------------- */
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
GO

CREATE NONCLUSTERED INDEX [IX_AppTechMap_ApplicationId] ON [dbo].[ApplicationTechnologyMappings] ([ApplicationId] ASC);
CREATE NONCLUSTERED INDEX [IX_AppTechMap_TechnologyId] ON [dbo].[ApplicationTechnologyMappings] ([TechnologyId] ASC);
GO

/* ============================================================================
   ApplicationDlls - libraries (DLLs) used by each application, with the
   specific version deployed. Populated by the folder scanner (application
   directory and subdirectories, e.g. bin).
   ------------------------------------------------------------------------- */
CREATE TABLE [dbo].[ApplicationDlls]
(
    [ApplicationDllId] INT IDENTITY(1,1) NOT NULL,
    [ApplicationId]    INT               NOT NULL,   -- -> Applications
    [FileName]         NVARCHAR(260)     NOT NULL,   -- assembly / file name (e.g. Newtonsoft.Json.dll)
    [Version]          NVARCHAR(100)     NULL,       -- deployed file version (e.g. 13.0.3.27908)
    [RelativePath]     NVARCHAR(1000)    NULL,       -- path relative to the application source path
    [ModifiedDate]     DATETIME2(0)      NOT NULL CONSTRAINT [DF_ApplicationDlls_ModifiedDate] DEFAULT (GETUTCDATE()),
    [ModifiedBy]       NVARCHAR(128)     NULL,
    CONSTRAINT [PK_ApplicationDlls] PRIMARY KEY CLUSTERED ([ApplicationDllId] ASC),
    CONSTRAINT [FK_ApplicationDlls_Application] FOREIGN KEY ([ApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ApplicationDlls_AppFile] UNIQUE NONCLUSTERED ([ApplicationId], [FileName])
);
GO

CREATE NONCLUSTERED INDEX [IX_ApplicationDlls_ApplicationId] ON [dbo].[ApplicationDlls] ([ApplicationId] ASC);
GO

/* ============================================================================
   Dependencies
   A row means: SourceApplicationId DEPENDS ON TargetApplicationId
   (Source is the consumer, Target is the provider)
   ------------------------------------------------------------------------- */
CREATE TABLE [dbo].[Dependencies]
(
    [DependencyId]          INT IDENTITY(1,1) NOT NULL,
    [SourceApplicationId]   INT               NOT NULL,   -- the app that depends
    [TargetApplicationId]   INT               NOT NULL,   -- the app that is depended upon
    [DependencyType]        NVARCHAR(50)      NOT NULL,   -- API, Database, File, Message, UI, Infrastructure
    [Direction]             NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Dependencies_Direction] DEFAULT (N'Upstream'),
    [Description]           NVARCHAR(500)     NULL,
    [CriticalityLevel]      NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Dependencies_CriticalityLevel] DEFAULT (N'Low'), -- Critical, High, Medium, Low
    [Impact]                NVARCHAR(2000)    NULL,       -- what portion of the consumer stops working when this dependency is down
    [Frequency]             NVARCHAR(50)      NULL,       -- Real-time, Hourly, Daily, Weekly, Monthly
    [CreatedDate]           DATETIME2(0)      NOT NULL CONSTRAINT [DF_Dependencies_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate]          DATETIME2(0)      NOT NULL CONSTRAINT [DF_Dependencies_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]             NVARCHAR(128)     NULL,
    [ModifiedBy]            NVARCHAR(128)     NULL,
    CONSTRAINT [PK_Dependencies] PRIMARY KEY CLUSTERED ([DependencyId] ASC),
    CONSTRAINT [FK_Dependencies_Source] FOREIGN KEY ([SourceApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]),
    CONSTRAINT [FK_Dependencies_Target] FOREIGN KEY ([TargetApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]),
    CONSTRAINT [UQ_Dependencies_Relationship]
        UNIQUE NONCLUSTERED ([SourceApplicationId], [TargetApplicationId], [DependencyType]),
    CONSTRAINT [CK_Dependencies_NotSelf] CHECK ([SourceApplicationId] <> [TargetApplicationId]),
    CONSTRAINT [CK_Dependencies_Type] CHECK ([DependencyType] IN (N'API', N'Database', N'File', N'Message', N'UI', N'Infrastructure')),
    CONSTRAINT [CK_Dependencies_Direction] CHECK ([Direction] IN (N'Upstream', N'Downstream', N'Bidirectional')),
    CONSTRAINT [CK_Dependencies_Criticality] CHECK ([CriticalityLevel] IN (N'Critical', N'High', N'Medium', N'Low'))
);
GO

CREATE NONCLUSTERED INDEX [IX_Dependencies_Source] ON [dbo].[Dependencies] ([SourceApplicationId] ASC) INCLUDE ([TargetApplicationId]);
CREATE NONCLUSTERED INDEX [IX_Dependencies_Target] ON [dbo].[Dependencies] ([TargetApplicationId] ASC) INCLUDE ([SourceApplicationId]);
CREATE NONCLUSTERED INDEX [IX_Dependencies_Type] ON [dbo].[Dependencies] ([DependencyType] ASC);
GO

/* ============================================================================
   ADGroups - maps Active Directory groups to application roles
   ------------------------------------------------------------------------- */
CREATE TABLE [dbo].[ADGroups]
(
    [GroupId]     INT IDENTITY(1,1) NOT NULL,
    [GroupName]   NVARCHAR(200)     NOT NULL,   -- DOMAIN\GroupName
    [Role]        NVARCHAR(50)      NOT NULL,   -- Admin, Maintenance, Viewer
    [Description] NVARCHAR(500)     NULL,
    [IsActive]    BIT               NOT NULL CONSTRAINT [DF_ADGroups_IsActive] DEFAULT (1),
    [CreatedDate] DATETIME2(0)      NOT NULL CONSTRAINT [DF_ADGroups_CreatedDate] DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ADGroups] PRIMARY KEY CLUSTERED ([GroupId] ASC),
    CONSTRAINT [UQ_ADGroups_Name] UNIQUE NONCLUSTERED ([GroupName] ASC),
    CONSTRAINT [CK_ADGroups_Role] CHECK ([Role] IN (N'Admin', N'Maintenance', N'Viewer'))
);
GO

/* ============================================================================
   ActivityLog - audit trail of all create/update/delete operations
   ------------------------------------------------------------------------- */
CREATE TABLE [dbo].[ActivityLog]
(
    [LogId]         INT IDENTITY(1,1) NOT NULL,
    [Action]        NVARCHAR(50)      NOT NULL,   -- Created, Updated, Deleted, Login, Export
    [EntityType]    NVARCHAR(50)      NOT NULL,   -- Application, Dependency, ADGroup, System
    [EntityId]      INT               NOT NULL,
    [EntityName]    NVARCHAR(200)     NULL,
    [Details]       NVARCHAR(2000)    NULL,
    [PerformedBy]   NVARCHAR(128)     NOT NULL,
    [PerformedDate] DATETIME2(0)      NOT NULL CONSTRAINT [DF_ActivityLog_PerformedDate] DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ActivityLog] PRIMARY KEY CLUSTERED ([LogId] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_ActivityLog_PerformedDate] ON [dbo].[ActivityLog] ([PerformedDate] DESC);
CREATE NONCLUSTERED INDEX [IX_ActivityLog_Entity] ON [dbo].[ActivityLog] ([EntityType] ASC, [EntityId] ASC);
GO

/* ============================================================================
   Stored Procedure: GetDependencyChain
   Returns the transitive dependency chain for an application with depth.

   Traversal uses a visited-path guard to prevent infinite recursion when the
   dependency graph contains cycles, and reports each edge at its minimum depth.
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[GetDependencyChain]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[GetDependencyChain];
GO
CREATE PROCEDURE [dbo].[GetDependencyChain]
    @ApplicationId INT,
    @MaxDepth      INT = 10,
    @Direction     NVARCHAR(20) = N'Both'   -- 'Upstream', 'Downstream', 'Both'
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH DepTree AS
    (
        -- Anchor: direct edges touching the starting application
        SELECT
            d.SourceApplicationId,
            d.TargetApplicationId,
            d.DependencyId,
            d.DependencyType,
            d.CriticalityLevel,
            d.Impact,
            d.Frequency,
            d.Direction,
            1 AS [Depth],
            CAST(d.SourceApplicationId AS NVARCHAR(MAX)) AS [Path]
        FROM [dbo].[Dependencies] d
        WHERE
            (@Direction IN (N'Both', N'Downstream') AND d.TargetApplicationId = @ApplicationId)
            OR
            (@Direction IN (N'Both', N'Upstream') AND d.SourceApplicationId = @ApplicationId)

        UNION ALL

        -- Recursive: traverse upstream (who does the target depend on?)
        SELECT
            d.SourceApplicationId,
            d.TargetApplicationId,
            d.DependencyId,
            d.DependencyType,
            d.CriticalityLevel,
            d.Impact,
            d.Frequency,
            d.Direction,
            dt.[Depth] + 1 AS [Depth],
            CONCAT(dt.[Path], N',', d.TargetApplicationId) AS [Path]
        FROM [dbo].[Dependencies] d
        INNER JOIN DepTree dt
            ON d.SourceApplicationId = dt.TargetApplicationId
            AND @Direction IN (N'Both', N'Upstream')
            AND dt.[Depth] < @MaxDepth
            AND dt.[Path] NOT LIKE N'%,' + CAST(d.TargetApplicationId AS NVARCHAR(20)) + N',%'
            AND dt.[Path] NOT LIKE CAST(d.TargetApplicationId AS NVARCHAR(20)) + N',%'

        UNION ALL

        -- Recursive: traverse downstream (who depends on the source?)
        SELECT
            d.SourceApplicationId,
            d.TargetApplicationId,
            d.DependencyId,
            d.DependencyType,
            d.CriticalityLevel,
            d.Impact,
            d.Frequency,
            d.Direction,
            dt.[Depth] + 1 AS [Depth],
            CONCAT(dt.[Path], N',', d.SourceApplicationId) AS [Path]
        FROM [dbo].[Dependencies] d
        INNER JOIN DepTree dt
            ON d.TargetApplicationId = dt.SourceApplicationId
            AND @Direction IN (N'Both', N'Downstream')
            AND dt.[Depth] < @MaxDepth
            AND dt.[Path] NOT LIKE N'%,' + CAST(d.SourceApplicationId AS NVARCHAR(20)) + N',%'
            AND dt.[Path] NOT LIKE CAST(d.SourceApplicationId AS NVARCHAR(20)) + N',%'
    )
    SELECT
        deps.SourceApplicationId,
        src.[Name]                       AS [SourceName],
        src.[Environment]                AS [SourceEnvironment],
        src.[CriticalityLevel]           AS [SourceCriticality],
        src.[Status]                     AS [SourceStatus],
        srcCat.[Name]                    AS [SourceCategory],
        deps.TargetApplicationId,
        tgt.[Name]                       AS [TargetName],
        tgt.[Environment]                AS [TargetEnvironment],
        tgt.[CriticalityLevel]           AS [TargetCriticality],
        tgt.[Status]                     AS [TargetStatus],
        tgtCat.[Name]                    AS [TargetCategory],
        deps.DependencyId,
        deps.DependencyType,
        deps.CriticalityLevel,
        deps.Impact,
        deps.Frequency,
        deps.Direction,
        MIN(deps.[Depth])               AS [Depth]
    FROM DepTree deps
    INNER JOIN [dbo].[Applications] src ON src.ApplicationId = deps.SourceApplicationId AND src.[IsDeleted] = 0
    INNER JOIN [dbo].[Applications] tgt ON tgt.ApplicationId = deps.TargetApplicationId AND tgt.[IsDeleted] = 0
    LEFT JOIN [dbo].[ApplicationCategories] srcCat ON srcCat.[CategoryId] = src.[CategoryId]
    LEFT JOIN [dbo].[ApplicationCategories] tgtCat ON tgtCat.[CategoryId] = tgt.[CategoryId]
    GROUP BY
        deps.SourceApplicationId, src.[Name], src.[Environment], src.[CriticalityLevel], src.[Status], srcCat.[Name],
        deps.TargetApplicationId, tgt.[Name], tgt.[Environment], tgt.[CriticalityLevel], tgt.[Status], tgtCat.[Name],
        deps.DependencyId, deps.DependencyType, deps.CriticalityLevel, deps.Impact, deps.Frequency, deps.Direction
    ORDER BY [Depth] ASC,
        -- Sort each hop list by the neighbor's application type (category), then name.
        CASE WHEN @Direction = N'Downstream' THEN srcCat.[Name] ELSE tgtCat.[Name] END,
        CASE WHEN @Direction = N'Downstream' THEN src.[Name] ELSE tgt.[Name] END;
END
GO
