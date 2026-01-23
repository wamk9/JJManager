-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

/*
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (1, N'Mixer de �udio JJM-01', 5, 0, 0, 0, N'HID', N'JJM01')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (2, N'ButtonBox JJB-01', 2, 0, 0, 0, N'Joystick', N'JJB01')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (3, N'Streamdeck JJSD-01', 0, 12, 0, 0, N'HID', N'JJSD01')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (4, N'Streamdeck JJSD-02', 0, 100, 0, 0, N'Bluetooth', N'JJSD02')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (5, N'ButtonBox JJB-01 V2', 0, 0, 255, 0, N'HID', N'JJB01_V2')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (6, N'Painel de Leds JJQ-01', 0, 0, 0, 0, N'Bluetooth', N'JJQ01')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (7, N'ButtonBox JJBP-06', 0, 0, 0, 0, N'HID', N'JJBP06')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (8, N'ButtonBox JJB-999', 0, 0, 0, 0, N'HID', N'JJB999')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (9, N'Hub ARGB JJHL-01', 0, 0, 8, 0, N'HID', N'JJHL01')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (10, N'Hub ARGB JJHL-01 Plus', 0, 0, 8, 0, N'HID', N'JJHL01PLUS')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (11, N'Hub RGB JJHL-02', 0, 0, 8, 0, N'HID', N'JJHL02')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (12, N'Hub RGB JJHL-02 Plus', 0, 0, 8, 0, N'HID', N'JJHL02PLUS')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (13, N'Dashboard JJDB-01', 0, 0, 255, 0, N'HID', N'JJDB01')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (14, N'LoadCell JJLC-01', 0, 0, 0, 0, N'HID', N'JJLC01')
INSERT INTO [dbo].[jj_products] ([id], [product_name], [analog_inputs_qtd], [digital_inputs_qtd], [analog_outputs_qtd], [digital_outputs_qtd], [conn_type], [class_name]) VALUES (15, N'ButtonBox JJB-Slim Type A', 0, 8, 8, 0, N'HID', N'JJBSlim_A')
*/

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