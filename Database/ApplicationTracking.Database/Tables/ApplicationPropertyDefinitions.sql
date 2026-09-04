CREATE TABLE [dbo].[ApplicationPropertyDefinitions]
(
    [PropertyDefinitionId] INT IDENTITY(1,1) NOT NULL,
    [Key]                  NVARCHAR(100)     NOT NULL,
    [Label]                NVARCHAR(200)     NOT NULL,
    [DataType]             NVARCHAR(20)      NOT NULL CONSTRAINT [DF_APD_DataType] DEFAULT (N'Text'),
    [ScanPattern]          NVARCHAR(500)     NULL,
    [Description]          NVARCHAR(500)     NULL,
    [IsActive]             BIT               NOT NULL CONSTRAINT [DF_APD_IsActive] DEFAULT (1),
    [SortOrder]            INT               NOT NULL CONSTRAINT [DF_APD_SortOrder] DEFAULT (0),
    [CreatedDate]          DATETIME2(0)      NOT NULL CONSTRAINT [DF_APD_CreatedDate] DEFAULT (GETUTCDATE()),
    [ModifiedDate]         DATETIME2(0)      NOT NULL CONSTRAINT [DF_APD_ModifiedDate] DEFAULT (GETUTCDATE()),
    [CreatedBy]            NVARCHAR(128)     NULL,
    [ModifiedBy]           NVARCHAR(128)     NULL,
    CONSTRAINT [PK_ApplicationPropertyDefinitions] PRIMARY KEY CLUSTERED ([PropertyDefinitionId] ASC),
    CONSTRAINT [UQ_ApplicationPropertyDefinitions_Key] UNIQUE NONCLUSTERED ([Key] ASC),
    CONSTRAINT [CK_ApplicationPropertyDefinitions_DataType] CHECK ([DataType] IN (N'Text', N'Url', N'Multiline', N'Number'))
);
GO
