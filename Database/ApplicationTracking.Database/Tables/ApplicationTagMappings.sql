CREATE TABLE [dbo].[ApplicationTagMappings]
(
    [MappingId]     INT IDENTITY(1,1) NOT NULL,
    [ApplicationId] INT               NOT NULL,
    [TagId]         INT               NOT NULL,
    CONSTRAINT [PK_ApplicationTagMappings] PRIMARY KEY CLUSTERED ([MappingId] ASC),
    CONSTRAINT [FK_AppTagMap_Application] FOREIGN KEY ([ApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AppTagMap_Tag] FOREIGN KEY ([TagId])
        REFERENCES [dbo].[ApplicationTags] ([TagId]) ON DELETE CASCADE,
    CONSTRAINT [UQ_AppTagMap_App_Tag] UNIQUE NONCLUSTERED ([ApplicationId], [TagId])
);
GO

CREATE NONCLUSTERED INDEX [IX_AppTagMap_ApplicationId] ON [dbo].[ApplicationTagMappings] ([ApplicationId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_AppTagMap_TagId] ON [dbo].[ApplicationTagMappings] ([TagId] ASC);
GO