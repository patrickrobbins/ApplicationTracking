IF NOT EXISTS (SELECT 1 FROM [dbo].[ADGroups] WHERE [GroupName] = N'DOMAIN\DependencyTracker_Admin')
    INSERT INTO [dbo].[ADGroups] ([GroupName], [Role], [Description], [IsActive])
    VALUES (N'DOMAIN\DependencyTracker_Admin', N'Admin', N'Full administrative access', 1);
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[ADGroups] WHERE [GroupName] = N'DOMAIN\DependencyTracker_Maintenance')
    INSERT INTO [dbo].[ADGroups] ([GroupName], [Role], [Description], [IsActive])
    VALUES (N'DOMAIN\DependencyTracker_Maintenance', N'Maintenance', N'Can create and edit applications and dependencies', 1);
GO
