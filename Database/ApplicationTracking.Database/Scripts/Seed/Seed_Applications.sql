SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Customer Portal' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Customer Portal', N'1.0.0', N'Customer-facing web portal for self-service account, order and billing access.', N'Marketing', N'Production', N'Critical', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Platform Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Partner Portal' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Partner Portal', N'1.0.0', N'Extranet portal used by business partners to manage orders and reports.', N'Sales', N'Production', N'High', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'External') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Platform Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Order Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Order Service', N'1.0.0', N'Order entry, validation and orchestration service.', N'Operations', N'Production', N'Critical', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Commerce Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Billing Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Billing Service', N'1.0.0', N'Invoicing, payment and settlement processing.', N'Finance', N'Production', N'Critical', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Account Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Account Service', N'1.0.0', N'Customer account lifecycle management and profile data.', N'Operations', N'Production', N'High', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Commerce Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Identity Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Identity Service', N'1.0.0', N'Central authentication, SSO and identity management.', N'Security', N'Production', N'Critical', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Active Directory') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Security Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Notification Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Notification Service', N'1.0.0', N'Email, SMS and push notification delivery.', N'Marketing', N'Production', N'High', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Platform Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Payment Gateway' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Payment Gateway', N'1.0.0', N'Card and bank payment processing integration.', N'Finance', N'Production', N'Critical', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'External') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'CRM Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'CRM Service', N'1.0.0', N'Customer relationship management data and workflows.', N'Sales', N'Production', N'High', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'External') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'CRM Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Master Data Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Master Data Service', N'1.0.0', N'Canonical customer, product and location reference data.', N'Data Governance', N'Production', N'Critical', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Data Platform Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Reporting Warehouse' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Reporting Warehouse', N'1.0.0', N'BI warehouse feeding dashboards and analytical reports.', N'BI Team', N'Production', N'Medium', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Database') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Data Platform Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Legacy Mainframe' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Legacy Mainframe', N'1.0.0', N'Legacy order and billing mainframe system being phased out.', N'Operations', N'Production', N'Medium', N'Retired', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Mainframe Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Settlement Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Settlement Service', N'1.0.0', N'Payment settlement processing and funding instructions.', N'Finance', N'Production', N'Critical', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Payment Ledger' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Payment Ledger', N'1.0.0', N'Canonical ledger of payment and settlement events.', N'Finance', N'Production', N'Critical', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Finance IT') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Fraud Detection Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Fraud Detection Service', N'1.0.0', N'Real-time transaction fraud scoring.', N'Risk', N'Production', N'High', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Risk Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Risk Scoring Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Risk Scoring Service', N'1.0.0', N'Credit and transaction risk models.', N'Risk', N'Production', N'High', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Risk Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Case Management Service' AND ISNULL([Version], N'') = N'1.0.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Case Management Service', N'1.0.0', N'Fraud and dispute case workflow management.', N'Operations', N'Production', N'Medium', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Commerce Team') t;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Applications] WHERE [Name] = N'Master Data Service' AND ISNULL([Version], N'') = N'1.2.0')
    INSERT INTO [dbo].[Applications] ([Name], [Version], [Description], [BusinessOwner], [Environment], [CriticalityLevel], [Status], [CategoryId], [TechnicalOwnershipTeamId])
    SELECT N'Master Data Service', N'1.2.0', N'Canonical customer, product and location reference data (v1.2 rollout).', N'Data Governance', N'Production', N'Critical', N'Active', c.CategoryId, t.TeamId
    FROM (SELECT CategoryId FROM [dbo].[ApplicationCategories] WHERE [Name] = N'Internal') c
    CROSS JOIN (SELECT TeamId FROM [dbo].[TechnicalOwnershipTeams] WHERE [Name] = N'Data Platform Team') t;
GO