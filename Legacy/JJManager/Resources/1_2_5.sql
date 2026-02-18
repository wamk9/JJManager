-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

ALTER TABLE dbo.jj_products
ADD analog_outputs_qtd TINYINT NOT NULL DEFAULT(0),
    digital_outputs_qtd TINYINT NOT NULL DEFAULT(0),
    conn_type VARCHAR (10) NULL,
    class_name VARCHAR (10) NULL;
GO

UPDATE dbo.jj_products SET conn_type = 'HID', class_name = 'JJM01' WHERE id = 1; -- product_name = 'Mixer de Áudio JJM-01';
GO
UPDATE dbo.jj_products SET conn_type = 'Joystick', class_name = 'JJB01' WHERE product_name = 'ButtonBox JJB-01';
GO
UPDATE dbo.jj_products SET conn_type = 'HID', class_name = 'JJSD01' WHERE product_name = 'Streamdeck JJSD-01';
GO
UPDATE dbo.jj_products SET conn_type = 'Bluetooth', class_name = 'JJSD02' WHERE product_name = 'Streamdeck JJSD-02';
GO
UPDATE dbo.jj_products SET conn_type = 'HID', class_name = 'JJB01_V2' WHERE product_name = 'ButtonBox JJB-01 V2';
GO
UPDATE dbo.jj_products SET conn_type = 'Bluetooth', class_name = 'JJQ01' WHERE product_name = 'Painel de Leds JJQ-01';
GO
UPDATE dbo.jj_products SET conn_type = 'HID', class_name = 'JJBP06' WHERE product_name = 'ButtonBox JJBP-06';
GO
UPDATE dbo.jj_products SET conn_type = 'HID', class_name = 'JJB999' WHERE product_name = 'ButtonBox JJB-999';
GO

SET IDENTITY_INSERT dbo.jj_products ON;
GO

INSERT INTO dbo.jj_products (id, product_name, analog_inputs_qtd, digital_inputs_qtd, analog_outputs_qtd, digital_outputs_qtd, conn_type, class_name) VALUES 
(9, 'Hub ARGB JJHL-01', 0, 0, 8, 0, 'HID', 'JJHL01'),
(10, 'Hub ARGB JJHL-01 Plus', 0, 0, 8, 0, 'HID', 'JJHL01PLUS'),
(11, 'Hub RGB JJHL-02', 0, 0, 8, 0, 'HID', 'JJHL02'),
(12, 'Hub RGB JJHL-02 Plus', 0, 0, 8, 0, 'HID', 'JJHL02PLUS'),
(13, 'Dashboard JJDB-01', 0, 0, 0, 0, 'HID', 'JJDB01');
GO

SET IDENTITY_INSERT dbo.jj_products OFF;
GO

UPDATE dbo.configs SET software_version = '1.2.5';
GO

SET ANSI_WARNINGS on
GO