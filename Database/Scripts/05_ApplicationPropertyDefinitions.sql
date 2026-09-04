/*
    ============================================================================
    Application Dependency Tracker - Application Properties
    Run order: after 04_SeedData_4Levels.sql

    Dynamic key/value properties stored per application, driven by a managed
    definition table. Each definition may carry an optional scan pattern
    (a regular expression) used by the configuration scanner to auto-detect
    candidate dependencies in web.config / app.config files.

    WARNING: This script drops and recreates the ApplicationPropertyValues and
    ApplicationPropertyDefinitions tables.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

IF OBJECT_ID(N'[dbo].[ApplicationPropertyValues]', N'U') IS NOT NULL DROP TABLE [dbo].[ApplicationPropertyValues];
IF OBJECT_ID(N'[dbo].[ApplicationPropertyDefinitions]', N'U') IS NOT NULL DROP TABLE [dbo].[ApplicationPropertyDefinitions];
GO

/* ============================================================================
   ApplicationPropertyDefinitions - the catalog of dynamic property types
   ------------------------------------------------------------------------- */
CREATE TABLE [dbo].[ApplicationPropertyDefinitions]
(
    [PropertyDefinitionId] INT IDENTITY(1,1) NOT NULL,
    [Key]                  NVARCHAR(100)     NOT NULL,
    [Label]                NVARCHAR(200)     NOT NULL,
    [DataType]             NVARCHAR(20)      NOT NULL CONSTRAINT [DF_APD_DataType] DEFAULT (N'Text'), -- Text, Url, Multiline, Number
    [ScanPattern]          NVARCHAR(500)     NULL,   -- regex matched against config keys/values by the scanner
    [Description]          NVARCHAR(500)     NULL,
    [IsActive]             BIT               NOT NULL CONSTRAINT [DF_APD_IsActive] DEFAULT (1),
    [SortOrder]            INT               NOT NULL CONSTRAINT [DF_APD_SortOrder] DEFAULT (0),
    [CreatedDate]          DATETIME2(0)      NOT NULL CONSTRAINT [DF_APD_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate]         DATETIME2(0)      NOT NULL CONSTRAINT [DF_APD_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]            NVARCHAR(128)     NULL,
    [ModifiedBy]           NVARCHAR(128)     NULL,
    CONSTRAINT [PK_ApplicationPropertyDefinitions] PRIMARY KEY CLUSTERED ([PropertyDefinitionId] ASC),
    CONSTRAINT [UQ_ApplicationPropertyDefinitions_Key] UNIQUE NONCLUSTERED ([Key] ASC),
    CONSTRAINT [CK_ApplicationPropertyDefinitions_DataType] CHECK ([DataType] IN (N'Text', N'Url', N'Multiline', N'Number'))
);
GO

/* ============================================================================
   ApplicationPropertyValues - the per-application value for each definition
   ------------------------------------------------------------------------- */
CREATE TABLE [dbo].[ApplicationPropertyValues]
(
    [PropertyValueId]      INT IDENTITY(1,1) NOT NULL,
    [ApplicationId]        INT               NOT NULL,
    [PropertyDefinitionId] INT               NOT NULL,
    [Value]                NVARCHAR(2000)    NULL,
    [ModifiedDate]         DATETIME2(0)      NOT NULL CONSTRAINT [DF_APV_ModifiedDate] DEFAULT (GETUTCDATE()),
    [ModifiedBy]           NVARCHAR(128)     NULL,
    CONSTRAINT [PK_ApplicationPropertyValues] PRIMARY KEY CLUSTERED ([PropertyValueId] ASC),
    CONSTRAINT [FK_APV_Application] FOREIGN KEY ([ApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_APV_Definition] FOREIGN KEY ([PropertyDefinitionId])
        REFERENCES [dbo].[ApplicationPropertyDefinitions] ([PropertyDefinitionId]) ON DELETE CASCADE,
    CONSTRAINT [UQ_APV_Application_Definition] UNIQUE NONCLUSTERED ([ApplicationId], [PropertyDefinitionId])
);
GO

CREATE NONCLUSTERED INDEX [IX_APV_Application] ON [dbo].[ApplicationPropertyValues] ([ApplicationId] ASC);
GO

/* ============================================================================
   Seed: Application Logging App Code definition
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[ApplicationPropertyDefinitions]
    ([Key], [Label], [DataType], [ScanPattern], [Description], [IsActive], [SortOrder])
VALUES
    (N'ApplicationLoggingAppCode', N'Application Logging App Code', N'Text',
     N'ApplicationLoggingAppCode',
     N'App code used to route log events for this application. The scan pattern matches the config key (or value) that holds this code.',
     1, 10);
GO

/* ============================================================================
   Seed: values for the seeded applications
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[ApplicationPropertyValues]
    ([ApplicationId], [PropertyDefinitionId], [Value], [ModifiedBy])
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
                   N'CRM Service', N'Master Data Service', N'Reporting Warehouse', N'Legacy Mainframe');
GO

PRINT N'Application property definitions and values seeded.';
GO
