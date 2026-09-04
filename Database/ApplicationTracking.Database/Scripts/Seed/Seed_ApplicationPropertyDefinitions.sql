SET NOCOUNT ON;

-- Seed the Application Logging App Code property definition (idempotent).
IF NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationPropertyDefinitions] WHERE [Key] = N'ApplicationLoggingAppCode')
    INSERT INTO [dbo].[ApplicationPropertyDefinitions]
        ([Key], [Label], [DataType], [ScanPattern], [Description], [IsActive], [SortOrder])
    VALUES
        (N'ApplicationLoggingAppCode', N'Application Logging App Code', N'Text',
         N'ApplicationLoggingAppCode',
         N'App code used to route log events for this application. The scan pattern matches the config key (or value) that holds this code.',
         1, 10);
GO

-- Seed property values for the seeded applications (idempotent).
INSERT INTO [dbo].[ApplicationPropertyValues] ([ApplicationId], [PropertyDefinitionId], [Value], [ModifiedBy])
SELECT
    a.ApplicationId,
    d.PropertyDefinitionId,
    CASE a.[Name]
        WHEN N'Customer Portal'    THEN N'CP-PRD'
        WHEN N'Partner Portal'     THEN N'PP-PRD'
        WHEN N'Order Service'      THEN N'ORD-PRD'
        WHEN N'Billing Service'    THEN N'BIL-PRD'
        WHEN N'Account Service'    THEN N'ACC-PRD'
        WHEN N'Identity Service'   THEN N'IDN-PRD'
        WHEN N'Notification Service' THEN N'NOT-PRD'
        WHEN N'Payment Gateway'    THEN N'PAY-PRD'
        WHEN N'CRM Service'        THEN N'CRM-PRD'
        WHEN N'Master Data Service' THEN N'MDS-PRD'
        WHEN N'Reporting Warehouse' THEN N'RWH-PRD'
        WHEN N'Legacy Mainframe'   THEN N'LMF-PRD'
    END,
    N'SYSTEM'
FROM [dbo].[Applications] a
CROSS JOIN [dbo].[ApplicationPropertyDefinitions] d
WHERE d.[Key] = N'ApplicationLoggingAppCode'
  AND a.[Name] IN (N'Customer Portal', N'Partner Portal', N'Order Service', N'Billing Service',
                   N'Account Service', N'Identity Service', N'Notification Service', N'Payment Gateway',
                   N'CRM Service', N'Master Data Service', N'Reporting Warehouse', N'Legacy Mainframe')
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[ApplicationPropertyValues] v
      WHERE v.ApplicationId = a.ApplicationId AND v.PropertyDefinitionId = d.PropertyDefinitionId
  );
GO
