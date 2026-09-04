CREATE TABLE [dbo].[ApplicationDlls]
(
    [ApplicationDllId] INT IDENTITY(1,1) NOT NULL,
    [ApplicationId]    INT               NOT NULL,
    [FileName]         NVARCHAR(260)     NOT NULL,
    [Version]          NVARCHAR(100)     NULL,
    [RelativePath]     NVARCHAR(1000)    NULL,
    [ModifiedDate]     DATETIME2(0)      NOT NULL CONSTRAINT [DF_ApplicationDlls_ModifiedDate] DEFAULT (GETUTCDATE()),
    [ModifiedBy]       NVARCHAR(128)     NULL,
    CONSTRAINT [PK_ApplicationDlls] PRIMARY KEY CLUSTERED ([ApplicationDllId] ASC),
    CONSTRAINT [FK_ApplicationDlls_Application] FOREIGN KEY ([ApplicationId])
        REFERENCES [dbo].[Applications] ([ApplicationId]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ApplicationDlls_AppFile] UNIQUE NONCLUSTERED ([ApplicationId], [FileName])
);
GO

CREATE NONCLUSTERED INDEX [IX_ApplicationDlls_ApplicationId] ON [dbo].[ApplicationDlls] ([ApplicationId] ASC);
GO
