INSERT INTO [dbo].[ApplicationFamilies] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Business Applications',  N'Customer-, partner- and staff-facing business systems.', 10),
    (N'Shared Services',        N'Shared platforms and services consumed by multiple systems.', 20),
    (N'Data & Analytics',       N'Data stores, warehouses and analytics platforms.', 30),
    (N'Infrastructure',         N'Foundational infrastructure and identity services.', 40)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationFamilies] f WHERE f.[Name] = s.[Name]);
GO