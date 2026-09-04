/*
    ============================================================================
    Application Dependency Tracker - Application contact/stewardship fields
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: 05_ApplicationPropertyDefinitions.sql -> 06_ApplicationContactFields.sql
    ============================================================================
*/

USE [DependencyTracker];
GO

IF COL_LENGTH(N'dbo.Applications', N'BusinessGroup') IS NULL
    ALTER TABLE [dbo].[Applications] ADD [BusinessGroup] NVARCHAR(200) NULL;
GO

IF COL_LENGTH(N'dbo.Applications', N'BusinessOwnerEmail') IS NULL
    ALTER TABLE [dbo].[Applications] ADD [BusinessOwnerEmail] NVARCHAR(200) NULL;
GO

IF COL_LENGTH(N'dbo.Applications', N'BusinessBackup') IS NULL
    ALTER TABLE [dbo].[Applications] ADD [BusinessBackup] NVARCHAR(200) NULL;
GO

IF COL_LENGTH(N'dbo.Applications', N'BusinessBackupEmail') IS NULL
    ALTER TABLE [dbo].[Applications] ADD [BusinessBackupEmail] NVARCHAR(200) NULL;
GO

IF COL_LENGTH(N'dbo.Applications', N'TechnicalOwnerEmail') IS NULL
    ALTER TABLE [dbo].[Applications] ADD [TechnicalOwnerEmail] NVARCHAR(200) NULL;
GO
