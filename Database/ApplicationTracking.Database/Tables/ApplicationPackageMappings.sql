CREATE TABLE [dbo].[ApplicationPackageMappings]
(
    [MappingId]     INT IDENTITY(1,1) NOT NULL,
    [ApplicationId] INT               NOT NULL,
    [PackageId]     INT               NOT NULL,
    [Version]       NVARCHAR(100)     NULL,
    CONSTRAINT [PK_ApplicationPackageMappings] PRIMARY KEY CLUSTERED ([MappingId] ASC),
    CONSTRAINT [FK_AppPackageMap_Application] FOREIGN KEY ([ApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AppPackageMap_Package] FOREIGN KEY ([PackageId])
        REFERENCES [dbo].[Packages] ([PackageId]) ON DELETE CASCADE,
    CONSTRAINT [UQ_AppPackageMap_App_Package] UNIQUE NONCLUSTERED ([ApplicationId] ASC, [PackageId] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_AppPackageMap_ApplicationId] ON [dbo].[ApplicationPackageMappings] ([ApplicationId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_AppPackageMap_PackageId] ON [dbo].[ApplicationPackageMappings] ([PackageId] ASC);
GO