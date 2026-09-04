CREATE TABLE [dbo].[ApplicationTechnologyMappings]
(
    [MappingId]     INT IDENTITY(1,1) NOT NULL,
    [ApplicationId] INT               NOT NULL,
    [TechnologyId]  INT               NOT NULL,
    CONSTRAINT [PK_ApplicationTechnologyMappings] PRIMARY KEY CLUSTERED ([MappingId] ASC),
    CONSTRAINT [FK_AppTechMap_Application] FOREIGN KEY ([ApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AppTechMap_Technology] FOREIGN KEY ([TechnologyId])
        REFERENCES [dbo].[ApplicationTechnologies] ([TechnologyId]) ON DELETE CASCADE,
    CONSTRAINT [UQ_AppTechMap_App_Technology] UNIQUE NONCLUSTERED ([ApplicationId], [TechnologyId])
);
GO

CREATE NONCLUSTERED INDEX [IX_AppTechMap_ApplicationId] ON [dbo].[ApplicationTechnologyMappings] ([ApplicationId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_AppTechMap_TechnologyId] ON [dbo].[ApplicationTechnologyMappings] ([TechnologyId] ASC);
GO
