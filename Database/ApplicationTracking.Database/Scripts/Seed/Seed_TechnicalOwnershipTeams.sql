INSERT INTO [dbo].[TechnicalOwnershipTeams] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Platform Team',     N'Core platform and hosting.', 10),
    (N'Commerce Team',     N'Commerce and order systems.', 20),
    (N'Finance IT',        N'Finance, payment and billing systems.', 30),
    (N'Security Team',     N'Security and identity systems.', 40),
    (N'CRM Team',          N'CRM and sales systems.', 50),
    (N'Data Platform Team', N'Data stores and analytics.', 60),
    (N'Mainframe Team',     N'Mainframe systems.', 70),
    (N'Risk Team',          N'Risk and fraud systems.', 80),
    (N'Logistics IT',       N'Logistics and warehouse systems.', 90)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[TechnicalOwnershipTeams] t WHERE t.[Name] = s.[Name]);
GO