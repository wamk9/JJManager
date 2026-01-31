-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

UPDATE dbo.jj_products SET analog_outputs_qtd = 255 WHERE id = 5; -- JJB-01 V2
UPDATE dbo.jj_products SET analog_outputs_qtd = 255 WHERE id = 7; -- JJBP-06
UPDATE dbo.jj_products SET analog_outputs_qtd = 255 WHERE id = 8; -- JJB-999
UPDATE dbo.jj_products SET analog_outputs_qtd = 255 WHERE id = 15; -- JJB-Slim Type A

-- JJSD-01: Update digital_inputs from 12 to 36 (12 buttons x 3 states: Pressed, Continuous, Hold)
UPDATE dbo.jj_products SET digital_inputs_qtd = 36 WHERE id = 3; -- JJSD-01
GO

-- Drop the foreign key constraint if it exists
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PROFILE_USER_PRODUCT')
BEGIN
    ALTER TABLE dbo.user_products DROP CONSTRAINT FK_PROFILE_USER_PRODUCT;
END

-- Alter column to allow NULL
ALTER TABLE dbo.user_products ALTER COLUMN id_profile INT NULL;

-- Re-create foreign key constraint (allowing NULL)
ALTER TABLE dbo.user_products
ADD CONSTRAINT FK_PROFILE_USER_PRODUCT
FOREIGN KEY (id_profile) REFERENCES dbo.profiles(id);
GO

UPDATE dbo.configs SET software_version = '1.3.0';
GO

SET ANSI_WARNINGS on
GO