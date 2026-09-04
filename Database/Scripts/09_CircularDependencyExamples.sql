/*
    ============================================================================
    Application Dependency Tracker - Circular Dependency Examples

    Adds two intentional dependency cycles to demonstrate that the application
    handles circular dependencies safely:

      - Mutual dependency  : Settlement Service <-> Payment Ledger
      - Triangle dependency: Fraud Detection Service -> Risk Scoring Service
                             -> Case Management Service -> Fraud Detection Service

    Both examples are anchored into the existing 4-level dataset so they appear
    in the "All applications" graph and in per-application graphs.

    Idempotent: safe to run more than once. Does not touch existing data.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* ============================================================================
   Applications (only inserted if absent)
   =========================================================================== */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Settlement Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Settlement Service', N'Payment settlement processing and funding instructions.', N'Finance', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT'), N'Production', N'Critical', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Payment Ledger')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Payment Ledger', N'Canonical ledger of payment and settlement events.', N'Finance', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT'), N'Production', N'Critical', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Fraud Detection Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Fraud Detection Service', N'Real-time transaction fraud scoring.', N'Risk', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Risk Team'), N'Production', N'High', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Risk Scoring Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Risk Scoring Service', N'Credit and transaction risk models.', N'Risk', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Risk Team'), N'Production', N'High', N'Active');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Case Management Service')
    INSERT INTO [dbo].[Applications] ([Name], [Description], [BusinessOwner], [TechnicalOwnershipTeamId], [Environment], [CriticalityLevel], [Status])
    VALUES (N'Case Management Service', N'Fraud and dispute case workflow management.', N'Operations', (SELECT [TeamId] FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Commerce Team'), N'Production', N'Medium', N'Active');

/* ============================================================================
   Dependencies (only inserted if absent)
   Source depends on Target (Source is consumer, Target is provider)
   =========================================================================== */

/* Mutual dependency: Settlement Service <-> Payment Ledger */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Settlement Service') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Payment Ledger') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Post settlement events to ledger', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Settlement Service' AND t.[Name]=N'Payment Ledger';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Payment Ledger') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Settlement Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Settlement status callbacks', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Payment Ledger' AND t.[Name]=N'Settlement Service';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Billing Service') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Settlement Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Submit settlements', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Billing Service' AND t.[Name]=N'Settlement Service';

/* Triangle dependency: Fraud Detection -> Risk Scoring -> Case Management -> Fraud Detection */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Fraud Detection Service') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Risk Scoring Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Request risk score', N'Critical', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Fraud Detection Service' AND t.[Name]=N'Risk Scoring Service';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Risk Scoring Service') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Case Management Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Create risk case', N'Low', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Risk Scoring Service' AND t.[Name]=N'Case Management Service';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Case Management Service') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Fraud Detection Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Report fraud feedback', N'Low', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Case Management Service' AND t.[Name]=N'Fraud Detection Service';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Dependencies] WHERE [SourceApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Order Service') AND [TargetApplicationId] = (SELECT ApplicationId FROM Applications WHERE Name=N'Fraud Detection Service') AND [DependencyType]=N'API')
    INSERT INTO [dbo].[Dependencies] ([SourceApplicationId], [TargetApplicationId], [DependencyType], [Direction], [Description], [CriticalityLevel], [Frequency])
    SELECT s.ApplicationId, t.ApplicationId, N'API', N'Upstream', N'Screen orders for fraud', N'Low', N'Real-time'
    FROM [dbo].[Applications] s, [dbo].[Applications] t WHERE s.[Name]=N'Order Service' AND t.[Name]=N'Fraud Detection Service';

/* ============================================================================
   Audit log entry
   =========================================================================== */
INSERT INTO [dbo].[ActivityLog] ([Action], [EntityType], [EntityId], [EntityName], [Details], [PerformedBy])
VALUES (N'Created', N'System', 0, N'Database Seeded', N'Circular dependency example data seeded', N'SYSTEM');

PRINT N'Circular dependency examples complete.';
GO
