CREATE TABLE [dbo].[ApplicationPropertyValues]
(
    [PropertyValueId]      INT IDENTITY(1,1) NOT NULL,
    [ApplicationId]        INT               NOT NULL,
    [PropertyDefinitionId] INT               NOT NULL,
    [Value]                NVARCHAR(2000)    NULL,
    [ModifiedDate]         DATETIME2(0)      NOT NULL CONSTRAINT [DF_APV_ModifiedDate] DEFAULT (GETUTCDATE()),
    [ModifiedBy]           NVARCHAR(128)     NULL,
    CONSTRAINT [PK_ApplicationPropertyValues] PRIMARY KEY CLUSTERED ([PropertyValueId] ASC),
    CONSTRAINT [FK_APV_Application] FOREIGN KEY ([ApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_APV_Definition] FOREIGN KEY ([PropertyDefinitionId])
        REFERENCES [dbo].[ApplicationPropertyDefinitions] ([PropertyDefinitionId]) ON DELETE CASCADE,
    CONSTRAINT [UQ_APV_Application_Definition] UNIQUE NONCLUSTERED ([ApplicationId], [PropertyDefinitionId])
);
GO

CREATE NONCLUSTERED INDEX [IX_APV_Application] ON [dbo].[ApplicationPropertyValues] ([ApplicationId] ASC);
GO
