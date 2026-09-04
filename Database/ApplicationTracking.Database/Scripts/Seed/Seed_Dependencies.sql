SET NOCOUNT ON;

-- Inserts a dependency edge where Source depends on Target, looked up by name.
-- Each MDS-targeting edge is inserted against BOTH deployed versions of
-- Master Data Service (1.0.0 and 1.2.0) to mirror the logical-app seeding.
IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Customer Portal' AND t.[Name] = N'Order Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Place and track orders', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Customer Portal' AND t.[Name] = N'Order Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Customer Portal' AND t.[Name] = N'Account Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Account self-service', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Customer Portal' AND t.[Name] = N'Account Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Partner Portal' AND t.[Name] = N'Order Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Partner order management', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Partner Portal' AND t.[Name] = N'Order Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Customer Portal' AND t.[Name] = N'Identity Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Customer sign-in', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Customer Portal' AND t.[Name] = N'Identity Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Customer Portal' AND t.[Name] = N'Notification Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Order status notifications', N'Low', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Customer Portal' AND t.[Name] = N'Notification Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Partner Portal' AND t.[Name] = N'Identity Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Partner single sign-on', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Partner Portal' AND t.[Name] = N'Identity Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Partner Portal' AND t.[Name] = N'Notification Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Order alert notifications', N'Low', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Partner Portal' AND t.[Name] = N'Notification Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Order Service' AND t.[Name] = N'Identity Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Service authentication', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Order Service' AND t.[Name] = N'Identity Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Order Service' AND t.[Name] = N'Notification Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Order fulfillment notifications', N'Low', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Order Service' AND t.[Name] = N'Notification Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Billing Service' AND t.[Name] = N'Payment Gateway' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Charge capture', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Billing Service' AND t.[Name] = N'Payment Gateway';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Account Service' AND t.[Name] = N'Identity Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Account authentication', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Account Service' AND t.[Name] = N'Identity Service';
GO

IF NOT EXISTS (
    SELECT 1 FROM [dbo].[Dependencies] d
    INNER JOIN [dbo].[Applications] s ON s.ApplicationId = d.SourceApplicationId
    INNER JOIN [dbo].[Applications] t ON t.ApplicationId = d.TargetApplicationId
    WHERE s.[Name] = N'Account Service' AND t.[Name] = N'CRM Service' AND d.[DependencyType] = N'API'
)
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Customer data synchronization', N'Low', N'Daily'
    FROM [dbo].[Applications] s, [dbo].[Applications] t
    WHERE s.[Name] = N'Account Service' AND t.[Name] = N'CRM Service';
GO

-- Master Data Service edges target BOTH versions (1.0.0 and 1.2.0).
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Product and price reference data', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Order Service' AND t.[Name] = N'Master Data Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'File', N'Upstream', N'Nightly order file feed to mainframe', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Order Service' AND t.[Name] = N'Legacy Mainframe'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'File'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Billing reference data', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Billing Service' AND t.[Name] = N'Master Data Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'File', N'Upstream', N'Nightly settlement file to mainframe', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Billing Service' AND t.[Name] = N'Legacy Mainframe'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'File'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Account master data lookup', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Account Service' AND t.[Name] = N'Master Data Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Customer master sync', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'CRM Service' AND t.[Name] = N'Master Data Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'File', N'Upstream', N'Nightly settlement file to mainframe', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Payment Gateway' AND t.[Name] = N'Legacy Mainframe'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'File'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Contact reference data', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Notification Service' AND t.[Name] = N'Master Data Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'Database', N'Upstream', N'Nightly order extract', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Reporting Warehouse' AND t.[Name] = N'Order Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'Database'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'Database', N'Upstream', N'Nightly billing extract', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Reporting Warehouse' AND t.[Name] = N'Billing Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'Database'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'Database', N'Upstream', N'Nightly account extract', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Reporting Warehouse' AND t.[Name] = N'Account Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'Database'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'Database', N'Upstream', N'Nightly CRM extract', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Reporting Warehouse' AND t.[Name] = N'CRM Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'Database'
  );
GO

-- Circular dependency examples (mutual pair + triangle).
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Post settlement events to ledger', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Settlement Service' AND t.[Name] = N'Payment Ledger'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Settlement status callbacks', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Payment Ledger' AND t.[Name] = N'Settlement Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Submit settlements', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Billing Service' AND t.[Name] = N'Settlement Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Request risk score', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Fraud Detection Service' AND t.[Name] = N'Risk Scoring Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Create risk case', N'Low', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Risk Scoring Service' AND t.[Name] = N'Case Management Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Report fraud feedback', N'Low', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Case Management Service' AND t.[Name] = N'Fraud Detection Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Screen orders for fraud', N'Low', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t
WHERE s.[Name] = N'Order Service' AND t.[Name] = N'Fraud Detection Service'
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[Dependencies] d
      WHERE d.SourceApplicationId = s.ApplicationId AND d.TargetApplicationId = t.ApplicationId AND d.[DependencyType] = N'API'
  );
GO
