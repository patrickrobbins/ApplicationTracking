CREATE TABLE [dbo].[ActivityLog]
(
    [LogId]         INT IDENTITY(1,1) NOT NULL,
    [Action]        NVARCHAR(50)      NOT NULL,
    [EntityType]    NVARCHAR(50)      NOT NULL,
    [EntityId]      INT               NOT NULL,
    [EntityName]    NVARCHAR(200)     NULL,
    [Details]       NVARCHAR(2000)    NULL,
    [PerformedBy]   NVARCHAR(128)     NOT NULL,
    [PerformedDate] DATETIME2(0)      NOT NULL CONSTRAINT [DF_ActivityLog_PerformedDate] DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ActivityLog] PRIMARY KEY CLUSTERED ([LogId] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_ActivityLog_PerformedDate] ON [dbo].[ActivityLog] ([PerformedDate] DESC);
GO
CREATE NONCLUSTERED INDEX [IX_ActivityLog_Entity] ON [dbo].[ActivityLog] ([EntityType] ASC, [EntityId] ASC);
GO
