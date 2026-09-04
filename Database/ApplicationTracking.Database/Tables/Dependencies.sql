CREATE TABLE [dbo].[Dependencies]
(
    [DependencyId]          INT IDENTITY(1,1) NOT NULL,
    [SourceApplicationId]   INT               NOT NULL,
    [TargetApplicationId]   INT               NOT NULL,
    [DependencyType]        NVARCHAR(50)      NOT NULL,
    [Direction]             NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Dependencies_Direction] DEFAULT (N'Upstream'),
    [Description]           NVARCHAR(500)     NULL,
    [CriticalityLevel]      NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Dependencies_CriticalityLevel] DEFAULT (N'Low'),
    [Impact]                NVARCHAR(2000)    NULL,
    [Frequency]             NVARCHAR(50)      NULL,
    [CreatedDate]           DATETIME2(0)      NOT NULL CONSTRAINT [DF_Dependencies_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate]          DATETIME2(0)      NOT NULL CONSTRAINT [DF_Dependencies_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]             NVARCHAR(128)     NULL,
    [ModifiedBy]            NVARCHAR(128)     NULL,
    CONSTRAINT [PK_Dependencies] PRIMARY KEY CLUSTERED ([DependencyId] ASC),
    CONSTRAINT [FK_Dependencies_Source] FOREIGN KEY ([SourceApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]),
    CONSTRAINT [FK_Dependencies_Target] FOREIGN KEY ([TargetApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]),
    CONSTRAINT [UQ_Dependencies_Relationship]
        UNIQUE NONCLUSTERED ([SourceApplicationId], [TargetApplicationId], [DependencyType]),
    CONSTRAINT [CK_Dependencies_NotSelf] CHECK ([SourceApplicationId] <> [TargetApplicationId]),
    CONSTRAINT [CK_Dependencies_Type] CHECK ([DependencyType] IN (N'API', N'Database', N'File', N'Message', N'UI', N'Infrastructure')),
    CONSTRAINT [CK_Dependencies_Direction] CHECK ([Direction] IN (N'Upstream', N'Downstream', N'Bidirectional')),
    CONSTRAINT [CK_Dependencies_Criticality] CHECK ([CriticalityLevel] IN (N'Critical', N'High', N'Medium', N'Low'))
);
GO

CREATE NONCLUSTERED INDEX [IX_Dependencies_Source] ON [dbo].[Dependencies] ([SourceApplicationId] ASC) INCLUDE ([TargetApplicationId]);
GO
CREATE NONCLUSTERED INDEX [IX_Dependencies_Target] ON [dbo].[Dependencies] ([TargetApplicationId] ASC) INCLUDE ([SourceApplicationId]);
GO
CREATE NONCLUSTERED INDEX [IX_Dependencies_Type] ON [dbo].[Dependencies] ([DependencyType] ASC);
GO
