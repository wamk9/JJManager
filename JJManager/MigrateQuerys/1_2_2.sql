-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

IF OBJECT_ID('dbo.frames', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[frames] (
        [id] BIGINT IDENTITY(1,1) NOT NULL,
        [info] TEXT NOT NULL,
        [num_order] INT NOT NULL,
        [delay] INT DEFAULT 1000 NOT NULL,
        [id_profile] INT NOT NULL,
        CONSTRAINT [PK_FRAME] PRIMARY KEY CLUSTERED ([id] ASC, [id_profile] ASC),
        CONSTRAINT [FK_PROFILE_FRAME] FOREIGN KEY ([id_profile]) REFERENCES [dbo].[profiles] ([id]) ON DELETE CASCADE ON UPDATE CASCADE
    );
END

GO

SET IDENTITY_INSERT dbo.jj_products ON;  
INSERT INTO dbo.jj_products (id, product_name, analog_inputs_qtd, digital_inputs_qtd) 
	VALUES 
	(7, 'ButtonBox JJBP-06', 0, 0),
	(8, 'ButtonBox JJB-999', 0, 0);

GO

ALTER TABLE dbo.user_products
ALTER COLUMN conn_id VARCHAR(100)

GO

ALTER TABLE dbo.profiles
ADD configs TEXT DEFAULT '{}';
GO

UPDATE dbo.profiles SET configs = '{}' WHERE configs IS NULL;
GO

UPDATE dbo.configs SET software_version = '1.2.2';
GO

SET ANSI_WARNINGS on
GO