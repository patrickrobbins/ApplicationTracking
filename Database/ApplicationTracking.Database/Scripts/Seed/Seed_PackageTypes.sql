INSERT INTO [dbo].[PackageTypes] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Npm',   N'Node.js / JavaScript packages from the npm registry.', 10),
    (N'NuGet', N'.NET packages from the NuGet gallery.', 20)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[PackageTypes] t WHERE t.[Name] = s.[Name]);
GO