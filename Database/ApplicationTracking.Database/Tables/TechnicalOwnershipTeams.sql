CREATE TABLE [dbo].[TechnicalOwnershipTeams]
(
    [TeamId]      INT IDENTITY(1,1) NOT NULL,
    [Name]        NVARCHAR(100)     NOT NULL,
    [Description] NVARCHAR(500)     NULL,
    [IsActive]    BIT               NOT NULL CONSTRAINT [DF_OwnershipTeam_IsActive] DEFAULT (1),
    [SortOrder]   INT               NOT NULL CONSTRAINT [DF_OwnershipTeam_SortOrder] DEFAULT (0),
    [CreatedDate]  DATETIME2(0)     NOT NULL CONSTRAINT [DF_OwnershipTeam_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate] DATETIME2(0)     NOT NULL CONSTRAINT [DF_OwnershipTeam_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]    NVARCHAR(128)    NULL,
    [ModifiedBy]   NVARCHAR(128)    NULL,
    CONSTRAINT [PK_TechnicalOwnershipTeams] PRIMARY KEY CLUSTERED ([TeamId] ASC),
    CONSTRAINT [UQ_TechnicalOwnershipTeams_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);
GO