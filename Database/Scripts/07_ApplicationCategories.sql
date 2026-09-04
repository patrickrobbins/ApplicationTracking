/*
    ============================================================================
    Application Dependency Tracker - Application categories
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: 06_ApplicationContactFields.sql -> 07_ApplicationCategories.sql

    A managed, dynamic list of categories that classify each application (for
    example: Internal, External, Library, Database, Active Directory). Categories
    are maintained by an administrator and referenced from Applications through a
    nullable foreign key (deleting a category leaves applications uncategorized).
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* ============================================================================
   ApplicationCategories - the catalog of application categories
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'[dbo].[ApplicationCategories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationCategories]
    (
        [CategoryId]   INT IDENTITY(1,1) NOT NULL,
        [Name]         NVARCHAR(100)     NOT NULL,
        [Description]  NVARCHAR(500)     NULL,
        [IsActive]     BIT               NOT NULL CONSTRAINT [DF_AppCat_IsActive] DEFAULT (1),
        [SortOrder]    INT               NOT NULL CONSTRAINT [DF_AppCat_SortOrder] DEFAULT (0),
        [CreatedDate]  DATETIME2(0)      NOT NULL CONSTRAINT [DF_AppCat_CreatedDate] DEFAULT (GETUTCDATE()),
        [ModifiedDate] DATETIME2(0)      NOT NULL CONSTRAINT [DF_AppCat_ModifiedDate] DEFAULT (GETUTCDATE()),
        [CreatedBy]    NVARCHAR(128)     NULL,
        [ModifiedBy]   NVARCHAR(128)     NULL,
        CONSTRAINT [PK_ApplicationCategories] PRIMARY KEY CLUSTERED ([CategoryId] ASC),
        CONSTRAINT [UQ_ApplicationCategories_Name] UNIQUE NONCLUSTERED ([Name] ASC)
    );
END
GO

/* Category column + FK on Applications */
IF COL_LENGTH(N'dbo.Applications', N'CategoryId') IS NULL
    ALTER TABLE [dbo].[Applications] ADD [CategoryId] INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[foreign_keys] WHERE [name] = N'FK_Applications_Category')
    ALTER TABLE [dbo].[Applications] ADD CONSTRAINT [FK_Applications_Category]
        FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[ApplicationCategories] ([CategoryId]) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [sys].[indexes] WHERE [name] = N'IX_Applications_CategoryId' AND [object_id] = OBJECT_ID(N'dbo.Applications'))
    CREATE NONCLUSTERED INDEX [IX_Applications_CategoryId] ON [dbo].[Applications] ([CategoryId] ASC);
GO

/* ============================================================================
   Seed default categories (idempotent)
   ------------------------------------------------------------------------- */
INSERT INTO [dbo].[ApplicationCategories] ([Name], [Description], [SortOrder])
SELECT s.[Name], s.[Description], s.[SortOrder]
FROM (VALUES
    (N'Internal',         N'Systems and services operated by the organization.', 10),
    (N'External',         N'Third-party / vendor systems outside the organization.', 20),
    (N'Library',          N'Reusable code libraries and shared components.', 30),
    (N'Database',         N'Data stores and warehouses.', 40),
    (N'Active Directory', N'Active Directory / identity infrastructure.', 50)
) AS s([Name], [Description], [SortOrder])
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[ApplicationCategories] c WHERE c.[Name] = s.[Name]);
GO

/* ============================================================================
   Assign categories to the seeded applications
   ------------------------------------------------------------------------- */
UPDATE a
SET a.[CategoryId] = c.[CategoryId]
FROM [dbo].[Applications] a
INNER JOIN [dbo].[ApplicationCategories] c
    ON c.[Name] = CASE a.[Name]
        WHEN N'Customer Portal'     THEN N'Internal'
        WHEN N'Partner Portal'      THEN N'Internal'
        WHEN N'Order Service'       THEN N'Internal'
        WHEN N'Billing Service'     THEN N'Internal'
        WHEN N'Account Service'     THEN N'Internal'
        WHEN N'Identity Service'    THEN N'Internal'
        WHEN N'Notification Service' THEN N'Internal'
        WHEN N'Payment Gateway'     THEN N'External'
        WHEN N'CRM Service'         THEN N'External'
        WHEN N'Master Data Service' THEN N'Internal'
        WHEN N'Reporting Warehouse' THEN N'Database'
        WHEN N'Legacy Mainframe'    THEN N'External'
    END
WHERE a.[Name] IN (N'Customer Portal', N'Partner Portal', N'Order Service', N'Billing Service',
                   N'Account Service', N'Identity Service', N'Notification Service', N'Payment Gateway',
                   N'CRM Service', N'Master Data Service', N'Reporting Warehouse', N'Legacy Mainframe');
GO

PRINT N'Application categories seeded and assigned.';
GO
