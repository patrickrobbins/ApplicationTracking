CREATE PROCEDURE [dbo].[GetDependencyChain]
    @ApplicationId INT,
    @MaxDepth      INT = 10,
    @Direction     NVARCHAR(20) = N'Both'   -- 'Upstream', 'Downstream', 'Both'
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH DepTree AS
    (
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
        CASE WHEN @Direction = N'Downstream' THEN srcCat.[Name] ELSE tgtCat.[Name] END,
        CASE WHEN @Direction = N'Downstream' THEN src.[Name] ELSE tgt.[Name] END;
END
GO
