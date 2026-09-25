CREATE TABLE [dbo].[PackageTypes]
(
    [PackageTypeId] INT IDENTITY(1,1) NOT NULL,
    [Name]          NVARCHAR(100)     NOT NULL,
    [Description]   NVARCHAR(500)     NULL,
    [IsActive]      BIT               NOT NULL CONSTRAINT [DF_PackageType_IsActive] DEFAULT (1),
    [SortOrder]     INT               NOT NULL CONSTRAINT [DF_PackageType_SortOrder] DEFAULT (0),
    [CreatedDate]   DATETIME2(0)      NOT NULL CONSTRAINT [DF_PackageType_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate]  DATETIME2(0)      NOT NULL CONSTRAINT [DF_PackageType_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]     NVARCHAR(128)     NULL,
    [ModifiedBy]    NVARCHAR(128)     NULL,
    CONSTRAINT [PK_PackageTypes] PRIMARY KEY CLUSTERED ([PackageTypeId] ASC),
    CONSTRAINT [UQ_PackageTypes_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);
GO