/*
    ============================================================================
    Application Dependency Tracker - 4-Level Seed Data

    Replaces the demo applications/dependencies with a clean dataset arranged
    in four dependency levels, with shared (fan-in) dependencies so the graph
    view demonstrates reuse of common services.

    WARNING: This script deletes all existing Applications and Dependencies
    before seeding. It does not touch ADGroups or the audit history.

    Hierarchy:
      L0  Entry points      : Customer Portal, Partner Portal
      L1  Business services : Order Service, Billing Service, Account Service
      L2  Shared services   : Identity Service, Notification Service,
                              Payment Gateway, CRM Service
      L3  Core data         : Master Data Service, Reporting Warehouse,
                              Legacy Mainframe

    Shared dependencies (fan-in):
      - Identity Service     used by Customer Portal, Partner Portal,
                             Order Service, Account Service
      - Notification Service used by Customer Portal, Partner Portal, Order Service
      - Master Data Service  used by Order, Billing, Account, CRM, Notification
      - Legacy Mainframe     used by Order, Billing, Payment Gateway
      - Order Service        used by Customer Portal, Partner Portal,
                             Billing Service, Reporting Warehouse

    Circular dependency examples (intentional, to prove cycle handling):
      - Mutual dependency    : Settlement Service <-> Payment Ledger
      - Triangle dependency  : Fraud Detection Service -> Risk Scoring Service
                               -> Case Management Service -> Fraud Detection Service
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* ============================================================================
   Reset demo data (FK order: Dependencies, ApplicationDlls before Applications)
   =========================================================================== */
DELETE FROM [dbo].[Dependencies];
IF OBJECT_ID(N'[dbo].[ApplicationDlls]', N'U') IS NOT NULL DELETE FROM [dbo].[ApplicationDlls];
DELETE FROM [dbo].[Applications];
GO

/* ============================================================================
   Applications by level
   =========================================================================== */

/* ---- Level 0: Entry points -------------------------------------------- */
INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Customer Portal', N'Customer-facing web portal for self-service account, order and billing access.', N'Marketing', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Platform Team'), N'Production', N'Critical', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Partner Portal', N'Extranet portal used by business partners to manage orders and reports.', N'Sales', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Platform Team'), N'Production', N'High', N'Active');

/* ---- Level 1: Business services --------------------------------------- */
INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Order Service', N'Order entry, validation and orchestration service.', N'Operations', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Commerce Team'), N'Production', N'Critical', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Billing Service', N'Invoicing, payment and settlement processing.', N'Finance', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT'), N'Production', N'Critical', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Account Service', N'Customer account lifecycle management and profile data.', N'Operations', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Commerce Team'), N'Production', N'High', N'Active');

/* ---- Level 2: Shared services ----------------------------------------- */
INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Identity Service', N'Central authentication, SSO and identity management.', N'Security', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Security Team'), N'Production', N'Critical', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Notification Service', N'Email, SMS and push notification delivery.', N'Marketing', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Platform Team'), N'Production', N'High', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Payment Gateway', N'Card and bank payment processing integration.', N'Finance', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT'), N'Production', N'Critical', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'CRM Service', N'Customer relationship management data and workflows.', N'Sales', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'CRM Team'), N'Production', N'High', N'Active');

/* ---- Level 3: Core data / infrastructure ------------------------------ */
INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Master Data Service', N'Canonical customer, product and location reference data.', N'Data Governance', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Data Platform Team'), N'Production', N'Critical', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Reporting Warehouse', N'BI warehouse feeding dashboards and analytical reports.', N'BI Team', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Data Platform Team'), N'Production', N'Medium', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Legacy Mainframe', N'Legacy order and billing mainframe system being phased out.', N'Operations', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Mainframe Team'), N'Production', N'Medium', N'Retired');

/* ---- Circular dependency examples -------------------------------------- */
INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Settlement Service', N'Payment settlement processing and funding instructions.', N'Finance', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT'), N'Production', N'Critical', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Payment Ledger', N'Canonical ledger of payment and settlement events.', N'Finance', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT'), N'Production', N'Critical', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Fraud Detection Service', N'Real-time transaction fraud scoring.', N'Risk', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Risk Team'), N'Production', N'High', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Risk Scoring Service', N'Credit and transaction risk models.', N'Risk', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Risk Team'), N'Production', N'High', N'Active');

INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Case Management Service', N'Fraud and dispute case workflow management.', N'Operations', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Commerce Team'), N'Production', N'Medium', N'Active');

/* ---- Multi-version demo ------------------------------------------------ */
/* Master Data Service is deployed as a second, newer version (1.2.0). The   */
/* consumers above (Order, Billing, Account, CRM, Notification) depend on    */
/* the logical application, so the seed creates edges into BOTH versions.    */
INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
VALUES (N'Master Data Service', N'1.2.0', N'Canonical customer, product and location reference data (v1.2 rollout).', N'Data Governance', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Data Platform Team'), N'Production', N'Critical', N'Active');

/* Backfill the version on any row not seeded with one. */
UPDATE [dbo].[Applications] SET [Version] = N'1.0.0' WHERE [Version] IS NULL OR LTRIM(RTRIM([Version])) = N'';
GO

/* Assign application types (categories) so the registry, graph and the
   upstream/downstream lists demonstrate type-based sorting.                 */
UPDATE a SET a.[CategoryId] = c.[CategoryId]
FROM [dbo].[Applications] a
INNER JOIN (VALUES
    (N'Customer Portal',        N'Internal'),
    (N'Partner Portal',         N'External'),
    (N'Order Service',          N'Internal'),
    (N'Billing Service',        N'Internal'),
    (N'Account Service',        N'Internal'),
    (N'Identity Service',       N'Active Directory'),
    (N'Notification Service',   N'Internal'),
    (N'Payment Gateway',        N'External'),
    (N'CRM Service',            N'External'),
    (N'Master Data Service',    N'Internal'),
    (N'Reporting Warehouse',    N'Database'),
    (N'Legacy Mainframe',       N'Internal'),
    (N'Settlement Service',     N'Internal'),
    (N'Payment Ledger',         N'Internal'),
    (N'Fraud Detection Service', N'Internal'),
    (N'Risk Scoring Service',   N'Internal'),
    (N'Case Management Service', N'Internal')
) v([AppName], [CategoryName])
    ON a.[Name] = v.[AppName]
INNER JOIN [dbo].[ApplicationCategories] c ON c.[Name] = v.[CategoryName];
GO

/* ============================================================================
   Dependencies
   Source depends on Target (Source is consumer, Target is provider).
   =========================================================================== */

/* ---- L0 -> L1 --------------------------------------------------------- */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Place and track orders', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Customer Portal' AND t.[Name]=N'Order Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Account self-service', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Customer Portal' AND t.[Name]=N'Account Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Partner order management', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Partner Portal' AND t.[Name]=N'Order Service';

/* ---- L0 -> L2 (shared services) -------------------------------------- */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Customer sign-in', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Customer Portal' AND t.[Name]=N'Identity Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Order status notifications', N'Low', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Customer Portal' AND t.[Name]=N'Notification Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Partner single sign-on', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Partner Portal' AND t.[Name]=N'Identity Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Order alert notifications', N'Low', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Partner Portal' AND t.[Name]=N'Notification Service';

/* ---- L1 -> L2 (shared services) -------------------------------------- */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Service authentication', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Order Service' AND t.[Name]=N'Identity Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Order fulfillment notifications', N'Low', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Order Service' AND t.[Name]=N'Notification Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Charge capture', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Billing Service' AND t.[Name]=N'Payment Gateway';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Account authentication', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Account Service' AND t.[Name]=N'Identity Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Customer data synchronization', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Account Service' AND t.[Name]=N'CRM Service';

/* ---- L1 -> L3 (core data) -------------------------------------------- */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Product and price reference data', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Order Service' AND t.[Name]=N'Master Data Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'File', N'Upstream', N'Nightly order file feed to mainframe', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Order Service' AND t.[Name]=N'Legacy Mainframe';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Billing reference data', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Billing Service' AND t.[Name]=N'Master Data Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'File', N'Upstream', N'Nightly settlement file to mainframe', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Billing Service' AND t.[Name]=N'Legacy Mainframe';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Account master data lookup', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Account Service' AND t.[Name]=N'Master Data Service';

/* ---- L2 -> L3 (core data) -------------------------------------------- */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Customer master sync', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'CRM Service' AND t.[Name]=N'Master Data Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'File', N'Upstream', N'Nightly settlement file to mainframe', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Payment Gateway' AND t.[Name]=N'Legacy Mainframe';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Contact reference data', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Notification Service' AND t.[Name]=N'Master Data Service';

/* ---- L3 consumer (fan-in into Reporting Warehouse) -------------------- */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'Database', N'Upstream', N'Nightly order extract', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Reporting Warehouse' AND t.[Name]=N'Order Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'Database', N'Upstream', N'Nightly billing extract', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Reporting Warehouse' AND t.[Name]=N'Billing Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'Database', N'Upstream', N'Nightly account extract', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Reporting Warehouse' AND t.[Name]=N'Account Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'Database', N'Upstream', N'Nightly CRM extract', N'Low', N'Daily'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Reporting Warehouse' AND t.[Name]=N'CRM Service';

/* ---- Circular dependency examples -------------------------------------- */
/* Mutual dependency: Settlement Service <-> Payment Ledger                 */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Post settlement events to ledger', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Settlement Service' AND t.[Name]=N'Payment Ledger';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Settlement status callbacks', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Payment Ledger' AND t.[Name]=N'Settlement Service';

/* Anchor the mutual pair into the existing graph                           */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Submit settlements', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Billing Service' AND t.[Name]=N'Settlement Service';

/* Triangle dependency: Fraud Detection -> Risk Scoring -> Case Management -> Fraud Detection */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Request risk score', N'Critical', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Fraud Detection Service' AND t.[Name]=N'Risk Scoring Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Create risk case', N'Low', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Risk Scoring Service' AND t.[Name]=N'Case Management Service';

INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Report fraud feedback', N'Low', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Case Management Service' AND t.[Name]=N'Fraud Detection Service';

/* Anchor the triangle into the existing graph                              */
INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Screen orders for fraud', N'Low', N'Real-time'
FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Order Service' AND t.[Name]=N'Fraud Detection Service';

/* ============================================================================
   Application DLLs - libraries each application uses, with deployed versions.
   One row per (application, file name); the unique constraint guards re-runs.
   =========================================================================== */

/* ---- Level 0: Entry points -------------------------------------------- */
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

/* ---- Level 1: Business services --------------------------------------- */
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

/* ---- Level 2: Shared services ----------------------------------------- */
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

/* ---- Level 3: Core data / infrastructure ------------------------------ */
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

/* ---- Circular dependency examples -------------------------------------- */
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

/* ============================================================================
   Initial audit log entry
   =========================================================================== */
INSERT INTO [dbo].[ActivityLog] ([Action], [EntityType], [EntityId], [EntityName], [Details], [PerformedBy])
VALUES (N'Created', N'System', 0, N'Database Seeded', N'4-level seed data script executed', N'SYSTEM');

PRINT N'4-level seed data complete.';
GO
