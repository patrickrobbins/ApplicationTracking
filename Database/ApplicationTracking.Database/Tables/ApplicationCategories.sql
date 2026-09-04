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
GO
