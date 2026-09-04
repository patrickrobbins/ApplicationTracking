/*
    ============================================================================
    Application Dependency Tracker - Application Versions
    Target: SQL Server 2016+ / SQL Server LocalDB
    Run order: after 10_ApplicationDlls.sql (or after 01/02 on fresh installs)

    Adds the [Version] column to Applications so the same logical application can
    be tracked as multiple deployed versions (one row per version), and replaces
    the single-name unique constraint with a composite (Name, Version) constraint.

    Idempotent: safe to re-run against the live database.
    ============================================================================
*/

USE [DependencyTracker];
GO

SET NOCOUNT ON;

/* 1. Add the [Version] column if it does not exist yet. */
IF NOT EXISTS (
    SELECT 1 FROM [sys].[columns]
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Applications]') AND [name] = N'Version'
)
BEGIN
    ALTER TABLE [dbo].[Applications] ADD [Version] NVARCHAR(50) NULL;
    PRINT N'Added Applications.Version column.';
END
GO

/* 2. Backfill existing rows with a default version so they all remain unique. */
UPDATE [dbo].[Applications]
SET [Version] = N'1.0.0'
WHERE [Version] IS NULL OR LTRIM(RTRIM([Version])) = N'';
GO

/* 3. Drop the single-name unique constraint. */
IF EXISTS (
    SELECT 1 FROM [sys].[indexes]
    WHERE [name] = N'UQ_Applications_Name' AND [object_id] = OBJECT_ID(N'[dbo].[Applications]')
)
BEGIN
    ALTER TABLE [dbo].[Applications] DROP CONSTRAINT [UQ_Applications_Name];
    PRINT N'Dropped UQ_Applications_Name.';
END
GO

/* 4. Create the composite (Name, Version) unique constraint. */
IF NOT EXISTS (
    SELECT 1 FROM [sys].[indexes]
    WHERE [name] = N'UQ_Applications_Name_Version' AND [object_id] = OBJECT_ID(N'[dbo].[Applications]')
)
BEGIN
    ALTER TABLE [dbo].[Applications]
        ADD CONSTRAINT [UQ_Applications_Name_Version] UNIQUE NONCLUSTERED ([Name] ASC, [Version] ASC);
    PRINT N'Created UQ_Applications_Name_Version.';
END
GO

PRINT N'Application version migration complete.';
GO
