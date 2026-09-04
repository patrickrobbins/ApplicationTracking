SET NOCOUNT ON;

-- One row per (application, file name); the unique constraint guards re-runs.
INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'System.Web.Mvc.dll',          N'5.2.7.0',       N'bin\System.Web.Mvc.dll'),
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'EntityFramework.dll',         N'6.4.4.0',       N'bin\EntityFramework.dll'),
    (N'Microsoft.Owin.dll',          N'4.2.2.0',       N'bin\Microsoft.Owin.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Customer Portal'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'System.Web.Mvc.dll',          N'5.2.7.0',       N'bin\System.Web.Mvc.dll'),
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'System.Net.Http.dll',         N'4.2.0.0',       N'bin\System.Net.Http.dll'),
    (N'EntityFramework.dll',         N'6.4.4.0',       N'bin\EntityFramework.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Partner Portal'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'EntityFramework.dll',         N'6.4.4.0',       N'bin\EntityFramework.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll'),
    (N'RabbitMQ.Client.dll',         N'6.4.0.0',       N'bin\RabbitMQ.Client.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Order Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'EntityFramework.dll',         N'6.4.4.0',       N'bin\EntityFramework.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll'),
    (N'Quartz.dll',                  N'3.5.0.0',       N'bin\Quartz.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Billing Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'12.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'EntityFramework.dll',         N'6.4.4.0',       N'bin\EntityFramework.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll'),
    (N'Dapper.dll',                  N'2.0.123.0',     N'bin\Dapper.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Account Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Microsoft.Owin.dll',               N'4.2.2.0',   N'bin\Microsoft.Owin.dll'),
    (N'Microsoft.Owin.Security.OAuth.dll', N'4.2.2.0',   N'bin\Microsoft.Owin.Security.OAuth.dll'),
    (N'System.Web.Mvc.dll',               N'5.2.7.0',   N'bin\System.Web.Mvc.dll'),
    (N'Newtonsoft.Json.dll',              N'13.0.3.27908', N'bin\Newtonsoft.Json.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Identity Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'RabbitMQ.Client.dll',         N'6.4.0.0',       N'bin\RabbitMQ.Client.dll'),
    (N'Serilog.dll',                 N'2.12.0.0',      N'bin\Serilog.dll'),
    (N'System.Net.Http.dll',         N'4.2.0.0',       N'bin\System.Net.Http.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Notification Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll'),
    (N'StackExchange.Redis.dll',     N'2.6.86.0',      N'bin\StackExchange.Redis.dll'),
    (N'Apache.NMS.ActiveMQ.dll',     N'2.0.0.0',       N'bin\Apache.NMS.ActiveMQ.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Payment Gateway'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'12.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'EntityFramework.dll',         N'6.2.0.0',       N'bin\EntityFramework.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'CRM Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'EntityFramework.dll',         N'6.4.4.0',       N'bin\EntityFramework.dll'),
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll'),
    (N'Dapper.dll',                  N'2.0.123.0',     N'bin\Dapper.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Master Data Service'
  AND a.[Version] = N'1.0.0'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'13.0.4.0',       N'bin\Newtonsoft.Json.dll'),
    (N'EntityFramework.dll',         N'6.4.4.0',        N'bin\EntityFramework.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.6.0',        N'bin\System.Data.SqlClient.dll'),
    (N'Dapper.dll',                  N'2.1.35.0',       N'bin\Dapper.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Master Data Service'
  AND a.[Version] = N'1.2.0'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Dapper.dll',                  N'2.0.123.0',     N'bin\Dapper.dll'),
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll'),
    (N'Hangfire.Core.dll',           N'1.8.6.0',       N'bin\Hangfire.Core.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Reporting Warehouse'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'System.Xml.Linq.dll',         N'4.0.0.0',       N'bin\System.Xml.Linq.dll'),
    (N'System.Net.Http.dll',         N'4.2.0.0',       N'bin\System.Net.Http.dll'),
    (N'IBM.Data.DB2.dll',            N'11.5.0.0',      N'bin\IBM.Data.DB2.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Legacy Mainframe'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll'),
    (N'AutoMapper.dll',              N'12.0.1.0',      N'bin\AutoMapper.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Settlement Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'EntityFramework.dll',         N'6.4.4.0',       N'bin\EntityFramework.dll'),
    (N'Dapper.dll',                  N'2.0.123.0',     N'bin\Dapper.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Payment Ledger'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll'),
    (N'StackExchange.Redis.dll',     N'2.6.86.0',      N'bin\StackExchange.Redis.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Fraud Detection Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'Microsoft.ML.dll',            N'2.0.0.0',       N'bin\Microsoft.ML.dll'),
    (N'System.Data.SqlClient.dll',   N'4.8.5.0',       N'bin\System.Data.SqlClient.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Risk Scoring Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO

INSERT INTO [dbo].[ApplicationDlls] ([ApplicationId], [FileName], [Version], [RelativePath], [ModifiedDate], [ModifiedBy])
SELECT a.ApplicationId, v.[FileName], v.[Version], v.[RelativePath], GETUTCDATE(), N'SYSTEM'
FROM [dbo].[Applications] a
CROSS APPLY (VALUES
    (N'System.Web.Mvc.dll',          N'5.2.7.0',       N'bin\System.Web.Mvc.dll'),
    (N'Newtonsoft.Json.dll',         N'13.0.3.27908',  N'bin\Newtonsoft.Json.dll'),
    (N'EntityFramework.dll',         N'6.4.4.0',       N'bin\EntityFramework.dll')
) v([FileName], [Version], [RelativePath])
WHERE a.[Name] = N'Case Management Service'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationDlls] d WHERE d.ApplicationId = a.ApplicationId AND d.FileName = v.[FileName]);
GO
