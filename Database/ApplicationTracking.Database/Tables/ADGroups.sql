CREATE TABLE [dbo].[ADGroups]
(
    [GroupId]     INT IDENTITY(1,1) NOT NULL,
    [GroupName]   NVARCHAR(200)     NOT NULL,
    [Role]        NVARCHAR(50)      NOT NULL,
    [Description] NVARCHAR(500)     NULL,
    [IsActive]    BIT               NOT NULL CONSTRAINT [DF_ADGroups_IsActive] DEFAULT (1),
    [CreatedDate] DATETIME2(0)      NOT NULL CONSTRAINT [DF_ADGroups_CreatedDate] DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ADGroups] PRIMARY KEY CLUSTERED ([GroupId] ASC),
    CONSTRAINT [UQ_ADGroups_Name] UNIQUE NONCLUSTERED ([GroupName] ASC),
    CONSTRAINT [CK_ADGroups_Role] CHECK ([Role] IN (N'Admin', N'Maintenance', N'Viewer'))
);
GO
