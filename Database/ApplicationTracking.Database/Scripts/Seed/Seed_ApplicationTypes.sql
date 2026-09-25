INSERT INTO [dbo].[ApplicationTypes] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Batch Job',           N'Scheduled batch processing or data transformation.', 10),
    (N'Web Site',            N'Web-based user interface delivered to browsers.', 20),
    (N'Web API',             N'HTTP API or service layer consumed programmatically.', 30),
    (N'Windows Service',     N'Background service running on Windows.', 40),
    (N'Desktop Application', N'Installed application running on a user workstation.', 50),
    (N'Mobile Application',  N'Application running on a mobile device.', 60),
    (N'Cloud Service',       N'Cloud-hosted service or platform.', 70)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationTypes] t WHERE t.[Name] = s.[Name]);
GO