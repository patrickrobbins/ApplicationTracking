CREATE TABLE [dbo].[Applications]
(
    [ApplicationId]     INT IDENTITY(1,1) NOT NULL,
    [Name]              NVARCHAR(200)     NOT NULL,
    [Version]           NVARCHAR(50)      NULL,
    [Description]       NVARCHAR(2000)    NULL,
    [BusinessOwner]     NVARCHAR(200)     NULL,
    [BusinessOwnerEmail] NVARCHAR(200)     NULL,
    [BusinessGroup]     NVARCHAR(200)     NULL,
    [BusinessBackup]    NVARCHAR(200)     NULL,
    [BusinessBackupEmail] NVARCHAR(200)     NULL,
    [TechnicalOwnerEmail] NVARCHAR(200)     NULL,
    [CategoryId]        INT               NULL,
    [FamilyId]          INT               NULL,
    [TechnicalOwnershipTeamId] INT        NULL,
    [Environment]       NVARCHAR(50)      NULL,
    [CriticalityLevel]  NVARCHAR(20)      NULL,
    [Status]            NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Applications_Status] DEFAULT (N'Active'),
    [ExternalUrl]       NVARCHAR(500)     NULL,
    [SourcePath]        NVARCHAR(500)     NULL,
    [ApplicationIDE]    NVARCHAR(200)     NULL,
    [FrameworkVersion]  NVARCHAR(100)     NULL,
    [DocumentationLink] NVARCHAR(500)     NULL,
    [SourceControlLocation] NVARCHAR(500) NULL,
    [HoursOfOperation]  NVARCHAR(200)     NULL,
    [MaintenanceWindow] NVARCHAR(200)     NULL,
    [MaintenanceNotificationEmail] NVARCHAR(200) NULL,
    [ApplicationSummary] NVARCHAR(2000)   NULL,
    [TriageSteps]       NVARCHAR(2000)    NULL,
    [IsDeleted]         BIT               NOT NULL CONSTRAINT [DF_Applications_IsDeleted] DEFAULT (0),
    [DefaultDependencyCriticality] NVARCHAR(20) NULL,
    [DefaultDependencyImpact]      NVARCHAR(2000) NULL,
    [CreatedDate]       DATETIME2(0)      NOT NULL CONSTRAINT [DF_Applications_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate]      DATETIME2(0)      NOT NULL CONSTRAINT [DF_Applications_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]         NVARCHAR(128)     NULL,
    [ModifiedBy]        NVARCHAR(128)     NULL,
    CONSTRAINT [PK_Applications] PRIMARY KEY CLUSTERED ([ApplicationId] ASC),
    CONSTRAINT [UQ_Applications_Name_Version] UNIQUE NONCLUSTERED ([Name] ASC, [Version] ASC),
    CONSTRAINT [FK_Applications_Category] FOREIGN KEY ([CategoryId])
        REFERENCES [dbo].[ApplicationCategories] ([CategoryId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Applications_Family] FOREIGN KEY ([FamilyId])
        REFERENCES [dbo].[ApplicationFamilies] ([FamilyId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Applications_OwnershipTeam] FOREIGN KEY ([TechnicalOwnershipTeamId])
        REFERENCES [dbo].[TechnicalOwnershipTeams] ([TeamId]) ON DELETE SET NULL,
    CONSTRAINT [CK_Applications_Environment] CHECK ([Environment] IN (N'Production', N'Staging', N'Development')),
    CONSTRAINT [CK_Applications_Criticality] CHECK ([CriticalityLevel] IN (N'Critical', N'High', N'Medium', N'Low')),
    CONSTRAINT [CK_Applications_Status] CHECK ([Status] IN (N'Active', N'Retired', N'Planned')),
    CONSTRAINT [CK_Applications_DefaultDependencyCriticality]
        CHECK ([DefaultDependencyCriticality] IN (N'Critical', N'High', N'Medium', N'Low'))
);
GO

CREATE NONCLUSTERED INDEX [IX_Applications_Name] ON [dbo].[Applications] ([Name] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Applications_Status] ON [dbo].[Applications] ([Status] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Applications_Environment] ON [dbo].[Applications] ([Environment] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Applications_CategoryId] ON [dbo].[Applications] ([CategoryId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Applications_FamilyId] ON [dbo].[Applications] ([FamilyId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Applications_TechnicalOwnershipTeamId] ON [dbo].[Applications] ([TechnicalOwnershipTeamId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Applications_SourcePath] ON [dbo].[Applications] ([SourcePath] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Applications_IsDeleted] ON [dbo].[Applications] ([IsDeleted] ASC);
GO
