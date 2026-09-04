/*
    ============================================================================
    Application Dependency Tracker - Seed Data
    Creates sample applications, dependencies, AD group defaults, and audit log.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* ============================================================================
   AD Group defaults
   Replace DOMAIN\ with your actual domain name.
   =========================================================================== */
IF NOT EXISTS (SELECT 1 FROM [dbo].[ADGroups] WHERE [GroupName] = N'DOMAIN\DependencyTracker_Admin')
    INSERT INTO [dbo].[ADGroups] ([GroupName], [Role], [Description], [IsActive])
    VALUES (N'DOMAIN\DependencyTracker_Admin', N'Admin', N'Full administrative access', 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[ADGroups] WHERE [GroupName] = N'DOMAIN\DependencyTracker_Maintenance')
    INSERT INTO [dbo].[ADGroups] ([GroupName], [Role], [Description], [IsActive])
    VALUES (N'DOMAIN\DependencyTracker_Maintenance', N'Maintenance', N'Can create and edit applications and dependencies', 1);

/* ============================================================================
   Sample Applications
   =========================================================================== */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Portal Web')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Portal Web', N'Customer-facing web portal providing self-service access to accounts, billing, and support.', N'Marketing', N'Platform Team', N'Production', N'Critical', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Identity Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Identity Service', N'Central authentication and single sign-on service used across all applications.', N'Security', N'Security Team', N'Production', N'Critical', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Customer CRM')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Customer CRM', N'Customer relationship management system tracking all sales and support interactions.', N'Sales', N'CRM Team', N'Production', N'High', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Order Management')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Order Management', N'Order entry, processing and fulfillment workflows.', N'Operations', N'Commerce Team', N'Production', N'Critical', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Billing Engine')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Billing Engine', N'Invoicing, payment processing and financial reconciliation engine.', N'Finance', N'Finance IT', N'Production', N'Critical', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Warehouse API')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Warehouse API', N'Inventory management and warehouse fulfillment REST API.', N'Logistics', N'Logistics IT', N'Production', N'High', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Reporting Warehouse')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Reporting Warehouse', N'BI data warehouse feeding dashboards and analytical reports.', N'BI Team', N'Data Platform Team', N'Production', N'Medium', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Notification Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Notification Service', N'Email, SMS and push notification delivery service.', N'Marketing', N'Platform Team', N'Production', N'High', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Master Data Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Master Data Service', N'Canonical reference data - customers, products, locations.', N'Data Governance', N'Data Platform Team', N'Production', N'High', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Legacy Mainframe')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Legacy Mainframe', N'Legacy order and billing mainframe system being phased out.', N'Operations', N'Mainframe Team', N'Production', N'Medium', N'Retired');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Staging Portal')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwner], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Staging Portal', N'Staging environment for the portal web application.', N'Marketing', N'Platform Team', N'Staging', N'Low', N'Active');

/* ============================================================================
   Sample Dependencies
   Source depends on Target (Source is consumer, Target is provider)
   =========================================================================== */

/* Portal Web */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Portal Web') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Identity Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT p.ApplicationId, i.ApplicationId, N'API', N'Upstream', N'SSO login flow', N'Critical', N'Real-time'
    FROM [dbo].[Applications] p, [dbo].[Applications] i WHERE p.[Name]=N'Portal Web' AND i.[Name]=N'Identity Service';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Portal Web') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Order Management') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT p.ApplicationId, o.ApplicationId, N'API', N'Upstream', N'Order placement', N'Critical', N'Real-time'
    FROM [dbo].[Applications] p, [dbo].[Applications] o WHERE p.[Name]=N'Portal Web' AND o.[Name]=N'Order Management';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Portal Web') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Customer CRM') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT p.ApplicationId, c.ApplicationId, N'API', N'Upstream', N'Customer profile lookup', N'Low', N'Real-time'
    FROM [dbo].[Applications] p, [dbo].[Applications] c WHERE p.[Name]=N'Portal Web' AND c.[Name]=N'Customer CRM';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Portal Web') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Master Data Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT p.ApplicationId, m.ApplicationId, N'API', N'Upstream', N'Product catalog lookup', N'Critical', N'Real-time'
    FROM [dbo].[Applications] p, [dbo].[Applications] m WHERE p.[Name]=N'Portal Web' AND m.[Name]=N'Master Data Service';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Portal Web') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Billing Engine') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT p.ApplicationId, b.ApplicationId, N'API', N'Upstream', N'Bill presentment', N'Critical', N'Real-time'
    FROM [dbo].[Applications] p, [dbo].[Applications] b WHERE p.[Name]=N'Portal Web' AND b.[Name]=N'Billing Engine';

/* Order Management */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Order Management') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Warehouse API') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT o.ApplicationId, w.ApplicationId, N'API', N'Upstream', N'Inventory check', N'Critical', N'Real-time'
    FROM [dbo].[Applications] o, [dbo].[Applications] w WHERE o.[Name]=N'Order Management' AND w.[Name]=N'Warehouse API';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Order Management') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Billing Engine') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT o.ApplicationId, b.ApplicationId, N'API', N'Upstream', N'Order invoicing', N'Critical', N'Real-time'
    FROM [dbo].[Applications] o, [dbo].[Applications] b WHERE o.[Name]=N'Order Management' AND b.[Name]=N'Billing Engine';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Order Management') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Identity Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT o.ApplicationId, i.ApplicationId, N'API', N'Upstream', N'Service auth', N'Low', N'Real-time'
    FROM [dbo].[Applications] o, [dbo].[Applications] i WHERE o.[Name]=N'Order Management' AND i.[Name]=N'Identity Service';

/* Billing Engine */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Billing Engine') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Master Data Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT b.ApplicationId, m.ApplicationId, N'API', N'Upstream', N'Customer reference data', N'Critical', N'Real-time'
    FROM [dbo].[Applications] b, [dbo].[Applications] m WHERE b.[Name]=N'Billing Engine' AND m.[Name]=N'Master Data Service';

/* Reporting Warehouse */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Reporting Warehouse') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Customer CRM') AND [DependencyType]=N'Database')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT r.ApplicationId, c.ApplicationId, N'Database', N'Upstream', N'Nightly CRM extract', N'Low', N'Daily'
    FROM [dbo].[Applications] r, [dbo].[Applications] c WHERE r.[Name]=N'Reporting Warehouse' AND c.[Name]=N'Customer CRM';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Reporting Warehouse') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Order Management') AND [DependencyType]=N'Database')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT r.ApplicationId, o.ApplicationId, N'Database', N'Upstream', N'Nightly order extract', N'Low', N'Daily'
    FROM [dbo].[Applications] r, [dbo].[Applications] o WHERE r.[Name]=N'Reporting Warehouse' AND o.[Name]=N'Order Management';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Reporting Warehouse') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Billing Engine') AND [DependencyType]=N'Database')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT r.ApplicationId, b.ApplicationId, N'Database', N'Upstream', N'Nightly billing extract', N'Low', N'Daily'
    FROM [dbo].[Applications] r, [dbo].[Applications] b WHERE r.[Name]=N'Reporting Warehouse' AND b.[Name]=N'Billing Engine';

/* Notification Service */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Notification Service') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Identity Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT n.ApplicationId, i.ApplicationId, N'API', N'Upstream', N'Recipient validation', N'Low', N'Real-time'
    FROM [dbo].[Applications] n, [dbo].[Applications] i WHERE n.[Name]=N'Notification Service' AND i.[Name]=N'Identity Service';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Customer CRM') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Master Data Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT c.ApplicationId, m.ApplicationId, N'API', N'Upstream', N'Customer master sync', N'Low', N'Daily'
    FROM [dbo].[Applications] c, [dbo].[Applications] m WHERE c.[Name]=N'Customer CRM' AND m.[Name]=N'Master Data Service';

/* Warehouse API */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Warehouse API') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Master Data Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT w.ApplicationId, m.ApplicationId, N'API', N'Upstream', N'Location/product data', N'Critical', N'Real-time'
    FROM [dbo].[Applications] w, [dbo].[Applications] m WHERE w.[Name]=N'Warehouse API' AND m.[Name]=N'Master Data Service';

/* Staging Portal depends on the same services */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Staging Portal') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Identity Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, i.ApplicationId, N'API', N'Upstream', N'SSO login flow (test)', N'Low', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] i WHERE s.[Name]=N'Staging Portal' AND i.[Name]=N'Identity Service';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Staging Portal') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Order Management') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, o.ApplicationId, N'API', N'Upstream', N'Order placement (test)', N'Low', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] o WHERE s.[Name]=N'Staging Portal' AND o.[Name]=N'Order Management';

/* ============================================================================
   Initial audit log entry
   =========================================================================== */
INSERT INTO [dbo].[ActivityLog] ([Action], [EntityType], [EntityId], [EntityName], [Details], [PerformedBy])
VALUES (N'Created', N'System', 0, N'Database Seeded', N'Seed data script executed', N'SYSTEM');

PRINT N'Seed data complete.';
GO
