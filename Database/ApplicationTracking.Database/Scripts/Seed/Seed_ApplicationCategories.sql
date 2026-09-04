INSERT INTO [dbo].[ApplicationCategories] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Internal',         N'Systems and services operated by the organization.', 10),
    (N'External',         N'Third-party / vendor systems outside the organization.', 20),
    (N'Library',          N'Reusable code libraries and shared components.', 30),
    (N'Database',         N'Data stores and warehouses.', 40),
    (N'Active Directory', N'Active Directory / identity infrastructure.', 50)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationCategories] c WHERE c.[Name] = s.[Name]);
GO
