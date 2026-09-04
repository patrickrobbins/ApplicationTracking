SET NOCOUNT ON;

-- Seed the technology tag list (idempotent).
INSERT INTO [dbo].[ApplicationTechnologies] ([Name], [Description], [IsActive], [SortOrder])
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
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationTechnologies] t WHERE t.[Name] = src.[Name]);
GO

-- Seed example mappings so the technology filter is verifiable.
INSERT INTO [dbo].[ApplicationTechnologyMappings] ([ApplicationId], [TechnologyId])
SELECT a.ApplicationId, t.TechnologyId
FROM [dbo].[Applications] a
CROSS JOIN [dbo].[ApplicationTechnologies] t
WHERE t.[Name] IN (N'.NET Framework', N'ASP.NET', N'SQL Server')
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[ApplicationTechnologyMappings] m
      WHERE m.ApplicationId = a.ApplicationId AND m.TechnologyId = t.TechnologyId
  );
GO
