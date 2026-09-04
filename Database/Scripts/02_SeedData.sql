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
   Technical ownership teams
   =========================================================================== */
INSERT INTO [dbo].[TechnicalOwnershipTeams] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Platform Team',      N'Core platform and hosting.', 10),
    (N'Commerce Team',      N'Commerce and order systems.', 20),
    (N'Finance IT',         N'Finance, payment and billing systems.', 30),
    (N'Security Team',      N'Security and identity systems.', 40),
    (N'CRM Team',           N'CRM and sales systems.', 50),
    (N'Data Platform Team', N'Data stores and analytics.', 60),
    (N'Mainframe Team',     N'Mainframe systems.', 70),
    (N'Risk Team',          N'Risk and fraud systems.', 80),
    (N'Logistics IT',       N'Logistics and warehouse systems.', 90)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[TechnicalOwnershipTeams] t WHERE t.[Name] = s.[Name]);
GO

/* ============================================================================
   Application families
   =========================================================================== */
INSERT INTO [dbo].[ApplicationFamilies] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Business Applications', N'Customer-, partner- and staff-facing business systems.', 10),
    (N'Shared Services',       N'Shared platforms and services consumed by multiple systems.', 20),
    (N'Data & Analytics',      N'Data stores, warehouses and analytics platforms.', 30),
    (N'Infrastructure',        N'Foundational infrastructure and identity services.', 40)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationFamilies] f WHERE f.[Name] = s.[Name]);
GO

/* ============================================================================
   Sample Applications
   =========================================================================== */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Portal Web')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Portal Web', N'Customer-facing web portal providing self-service access to accounts, billing, and support.', N'Marketing', t.TeamId, N'Production', N'Critical', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Platform Team') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Identity Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Identity Service', N'Central authentication and single sign-on service used across all applications.', N'Security', t.TeamId, N'Production', N'Critical', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Security Team') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Customer CRM')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Customer CRM', N'Customer relationship management system tracking all sales and support interactions.', N'Sales', t.TeamId, N'Production', N'High', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'CRM Team') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Order Management')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Order Management', N'Order entry, processing and fulfillment workflows.', N'Operations', t.TeamId, N'Production', N'Critical', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Commerce Team') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Billing Engine')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Billing Engine', N'Invoicing, payment processing and financial reconciliation engine.', N'Finance', t.TeamId, N'Production', N'Critical', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Warehouse API')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Warehouse API', N'Inventory management and warehouse fulfillment REST API.', N'Logistics', t.TeamId, N'Production', N'High', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Logistics IT') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Reporting Warehouse')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Reporting Warehouse', N'BI data warehouse feeding dashboards and analytical reports.', N'BI Team', t.TeamId, N'Production', N'Medium', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Data Platform Team') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Notification Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Notification Service', N'Email, SMS and push notification delivery service.', N'Marketing', t.TeamId, N'Production', N'High', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Platform Team') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Master Data Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Master Data Service', N'Canonical reference data - customers, products, locations.', N'Data Governance', t.TeamId, N'Production', N'High', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Data Platform Team') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Legacy Mainframe')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Legacy Mainframe', N'Legacy order and billing mainframe system being phased out.', N'Operations', t.TeamId, N'Production', N'Medium', N'Retired'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Mainframe Team') t;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Staging Portal')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    SELECT N'Staging Portal', N'Staging environment for the portal web application.', N'Marketing', t.TeamId, N'Staging', N'Low', N'Active'
    FROM (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Platform Team') t;

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
