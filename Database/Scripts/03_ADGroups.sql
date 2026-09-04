/*
    ============================================================================
    Application Dependency Tracker - Active Directory Group Setup
    Run this on a domain controller or from a machine with AD PowerShell access
    to create the recommended AD groups for role-based access.
    ============================================================================

    Roles:
      - Viewer:      Everyone authenticated (default). Optionally create a group.
      - Maintenance: Can create/edit applications and dependencies.
      - Admin:       Full access + AD group management.
*/

-- Recommended AD groups to create in Active Directory:
--   DependencyTracker_Admins
--   DependencyTracker_Maintenance

-- After creating the groups, update the ADGroups table:

USE [DependencyTracker];
GO

-- Update the seed group names to your actual domain
UPDATE [dbo].[ADGroups] SET [GroupName] = N'YOURDOMAIN\DependencyTracker_Admins' WHERE [Role] = N'Admin';
UPDATE [dbo].[ADGroups] SET [GroupName] = N'YOURDOMAIN\DependencyTracker_Maintenance' WHERE [Role] = N'Maintenance';
GO

-- Optional: add a dedicated Viewer group (if you do NOT want all users to view)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ADGroups] WHERE [Role] = N'Viewer')
    INSERT INTO [dbo].[ADGroups] ([GroupName], [Role], [Description], [IsActive])
    VALUES (N'YOURDOMAIN\DependencyTracker_Viewers', N'Viewer', N'Read-only access', 1);
GO

PRINT N'AD group configuration updated. Replace YOURDOMAIN with the actual domain.';
GO
