-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

CREATE TABLE [dbo].[device_inputs] (
    [id]         INT          NOT NULL,
    [name]       VARCHAR (15) NULL,
    [data]       TEXT         NULL,
    [type]       VARCHAR (30) NOT NULL,
	[mode]       VARCHAR (30) NOT NULL,
    [id_profile] INT          NOT NULL,
    CONSTRAINT [PK_DEVICE_INPUT] PRIMARY KEY CLUSTERED ([id] ASC, [id_profile] ASC),
    CONSTRAINT [FK_PROFILE_DEVICE_INPUT] FOREIGN KEY ([id_profile]) REFERENCES [dbo].[profiles] ([id]) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

UPDATE dbo.configs SET software_version = '1.2.4';
GO

SET ANSI_WARNINGS on
GO