/*
    ============================================================================
    Application Dependency Tracker - Dependency Criticality, Impact & Defaults
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: 12_ApplicationSoftDelete.sql -> 13_DependencyCriticality.sql

    Replaces the boolean [IsCritical] on Dependencies with a criticality LEVEL
    (Critical, High, Medium, Low) and adds an [Impact] note describing what
    portion of the consuming application stops working when the dependency is
    down. Applications gain default criticality/impact values that are applied
    (pre-filled) when a dependency is created against them as the target.

    Existing rows are migrated: IsCritical = 1 -> Critical, 0 -> Low.

    Idempotent: safe to re-run against the live database.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* 1. Add Dependencies.CriticalityLevel if it does not exist yet. */
IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Dependencies]') AND [name] = N'CriticalityLevel'
)
BEGIN
    ALTER TABLE [dbo].[Dependencies]
        ADD [CriticalityLevel] NVARCHAR(20) NOT NULL
            CONSTRAINT [DF_Dependencies_CriticalityLevel] DEFAULT (N'Low');
    PRINT N'Added Dependencies.CriticalityLevel column.';
END
GO

/* 2. Backfill criticality from the legacy IsCritical flag (1 -> Critical, else Low).
      Runs only while the legacy column still exists (first run); re-runs no-op.
      Uses dynamic SQL so the batch compiles even after the column is dropped. */
IF EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Dependencies]') AND [name] = N'IsCritical'
)
BEGIN
    EXEC sp_executesql N'UPDATE [dbo].[Dependencies] SET [CriticalityLevel] = CASE WHEN [IsCritical] = 1 THEN N''Critical'' ELSE N''Low'' END;';
END
GO

/* 3. Add Dependencies.Impact (outage impact note) if it does not exist yet. */
IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Dependencies]') AND [name] = N'Impact'
)
BEGIN
    ALTER TABLE [dbo].[Dependencies]
        ADD [Impact] NVARCHAR(2000) NULL;
    PRINT N'Added Dependencies.Impact column.';
END
GO

/* 4. Drop the legacy IsCritical column (and its default constraint). */
IF EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Dependencies]') AND [name] = N'IsCritical'
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM [sys].[default_constraints]
        WHERE [name] = N'DF_Dependencies_IsCritical' AND [parent_object_id] = OBJECT_ID(N'[dbo].[Dependencies]')
    )
    BEGIN
        ALTER TABLE [dbo].[Dependencies] DROP CONSTRAINT [DF_Dependencies_IsCritical];
    END
    ALTER TABLE [dbo].[Dependencies] DROP COLUMN [IsCritical];
    PRINT N'Dropped Dependencies.IsCritical column.';
END
GO

/* 5. Check constraint for dependency criticality. */
IF NOT EXISTS (
    SELECT 1 FROM [sys].[check_constraints]
    WHERE [name] = N'CK_Dependencies_Criticality' AND [parent_object_id] = OBJECT_ID(N'[dbo].[Dependencies]')
)
BEGIN
    ALTER TABLE [dbo].[Dependencies]
        ADD CONSTRAINT [CK_Dependencies_Criticality]
        CHECK ([CriticalityLevel] IN (N'Critical', N'High', N'Medium', N'Low'));
    PRINT N'Added CK_Dependencies_Criticality.';
END
GO

/* 6. Default criticality / impact declared by an application when it is the
      provider (target) of a dependency. */
IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'DefaultDependencyCriticality'
)
BEGIN
    ALTER TABLE [dbo].[Applications]
        ADD [DefaultDependencyCriticality] NVARCHAR(20) NULL;
    PRINT N'Added Applications.DefaultDependencyCriticality column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'DefaultDependencyImpact'
)
BEGIN
    ALTER TABLE [dbo].[Applications]
        ADD [DefaultDependencyImpact] NVARCHAR(2000) NULL;
    PRINT N'Added Applications.DefaultDependencyImpact column.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM [sys].[check_constraints]
    WHERE [name] = N'CK_Applications_DefaultDependencyCriticality' AND [parent_object_id] = OBJECT_ID(N'[dbo].[Applications]')
)
BEGIN
    ALTER TABLE [dbo].[Applications]
        ADD CONSTRAINT [CK_Applications_DefaultDependencyCriticality]
        CHECK ([DefaultDependencyCriticality] IN (N'Critical', N'High', N'Medium', N'Low'));
    PRINT N'Added CK_Applications_DefaultDependencyCriticality.';
END
GO

/* 7. Re-create GetDependencyChain so it returns the criticality LEVEL and the
      impact note instead of the legacy boolean. */
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

PRINT N'Dependency criticality/impact/defaults migration complete.';
GO
