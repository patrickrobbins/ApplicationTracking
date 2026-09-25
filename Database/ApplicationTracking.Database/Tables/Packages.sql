CREATE TABLE [dbo].[Packages]
(
    [PackageId]     INT IDENTITY(1,1) NOT NULL,
    [PackageTypeId] INT               NOT NULL,
    [Name]          NVARCHAR(200)     NOT NULL,
    [IsActive]      BIT               NOT NULL CONSTRAINT [DF_Package_IsActive] DEFAULT (1),
    [SortOrder]     INT               NOT NULL CONSTRAINT [DF_Package_SortOrder] DEFAULT (0),
    [CreatedDate]   DATETIME2(0)      NOT NULL CONSTRAINT [DF_Package_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate]  DATETIME2(0)      NOT NULL CONSTRAINT [DF_Package_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]     NVARCHAR(128)     NULL,
    [ModifiedBy]    NVARCHAR(128)     NULL,
    CONSTRAINT [PK_Packages] PRIMARY KEY CLUSTERED ([PackageId] ASC),
    CONSTRAINT [UQ_Packages_TypeName] UNIQUE NONCLUSTERED ([PackageTypeId] ASC, [Name] ASC),
    CONSTRAINT [FK_Packages_PackageType] FOREIGN KEY ([PackageTypeId])
        REFERENCES [dbo].[PackageTypes] ([PackageTypeId]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_Packages_PackageTypeId] ON [dbo].[Packages] ([PackageTypeId] ASC);
GO