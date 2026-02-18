-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

UPDATE dbo.configs SET software_version = '1.2.8';
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'simhub_properties' AND COLUMN_NAME = 'activate_on'
)
BEGIN
    ALTER TABLE simhub_properties 
    ADD activate_on NVARCHAR(MAX) 
        CONSTRAINT DF_SIMHUB_PROPERTIES_ACTIVATE_ON DEFAULT '{}' 
        CHECK (ISJSON(activate_on) > 0);
END

GO

UPDATE simhub_properties SET activate_on = '{"type": "number", "value": "0", "comparative": "after"}' WHERE jj_prop IN (
    'ABSLevel',
    'TCLevel',
    'BrakeOn',
    'HandbrakeOn'
);
GO

UPDATE simhub_properties SET activate_on = '{"type": "number", "value": "1", "comparative": "equals"}' WHERE jj_prop IN (
    'PSL',
    'Flag_Black',
    'Flag_Blue',
    'Flag_Checkered',
    'Flag_Green',
    'Flag_Orange',
    'Flag_White',
    'Flag_Yellow',
    'ABSActive',
    'TCActive',
    'SpotterLeft',
    'SpotterRight',
    'EngineIgnitionOn',
    'EngineStarted',
    'DrsAvailable',
    'DrsOn',
    'RedlineReached',
    'TurnRight',
    'TurnLeft'
);
GO

INSERT INTO simhub_properties (name, description, simhub_prop, jj_prop, activate_on) VALUES
('Carro - Shift Light 1', '', 'DataCorePlugin.GameData.CarSettings_RPMShiftLight1', 'RPMShiftLight1', '{"type": "number", "value": "1", "comparative": "equals"}'),
('Carro - Shift Light 2', '', 'DataCorePlugin.GameData.CarSettings_RPMShiftLight2', 'RPMShiftLight2', '{"type": "number", "value": "1", "comparative": "equals"}'),
('Carro - Push To Pass Ativo', '', 'DataCorePlugin.GameData.PushToPassActive', 'PushToPassActive', '{"type": "number", "value": "1", "comparative": "equals"}');
GO

UPDATE simhub_properties SET simhub_prop = 'DataCorePlugin.GameData.Brake' WHERE jj_prop = 'BrakeOn';
GO

UPDATE simhub_properties SET simhub_prop = 'DataCorePlugin.GameData.Handbrake' WHERE jj_prop = 'HandbrakeOn';
GO

SET IDENTITY_INSERT dbo.jj_products ON;
GO

INSERT INTO dbo.jj_products (id, product_name, analog_inputs_qtd, digital_inputs_qtd, analog_outputs_qtd, digital_outputs_qtd, conn_type, class_name) VALUES 
(15, 'ButtonBox JJB-Slim Type A', 0, 8, 8, 0, 'HID', 'JJBSlim_A');
GO

SET IDENTITY_INSERT dbo.jj_products OFF;
GO

UPDATE dbo.jj_products SET analog_outputs_qtd = 255, digital_outputs_qtd = 0 WHERE product_name = 'Dashboard JJDB-01';
GO

SET ANSI_WARNINGS on
GO