/*
    ============================================================================
    Application Dependency Tracker - IIS discovery support
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: 07_ApplicationCategories.sql -> 08_IISDiscovery.sql

    Adds SourcePath to Applications. SourcePath records the physical directory
    (local path or UNC share) that an application was discovered from by the
    IIS / network file scanner. It is the stable key used to match a discovered
    application to an existing tracked application so a re-scan UPDATES rather
    than duplicates.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Applications]') AND name = N'SourcePath'
)
BEGIN
    ALTER TABLE [dbo].[Applications]
        ADD [SourcePath] NVARCHAR(500) NULL;

    CREATE NONCLUSTERED INDEX [IX_Applications_SourcePath]
        ON [dbo].[Applications] ([SourcePath] ASC);
END
GO
