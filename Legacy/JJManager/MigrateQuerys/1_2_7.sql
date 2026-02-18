-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

UPDATE dbo.configs SET software_version = '1.2.7';
GO

SET IDENTITY_INSERT dbo.jj_products ON;
GO

INSERT INTO dbo.jj_products (id, product_name, analog_inputs_qtd, digital_inputs_qtd, analog_outputs_qtd, digital_outputs_qtd, conn_type, class_name) VALUES 
(14, 'LoadCell JJLC-01', 0, 0, 0, 0, 'HID', 'JJLC01');
GO

UPDATE dbo.jj_products set digital_outputs_qtd = 255 WHERE product_name = 'Dashboard JJDB-01';
GO

SET IDENTITY_INSERT dbo.jj_products OFF;
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'device_outputs')
BEGIN
    CREATE TABLE [dbo].[device_outputs] (
        [id]         INT          NOT NULL,
        [name]       VARCHAR (255) NULL,
        [data]       TEXT         NULL,
        [type]       VARCHAR (30) NOT NULL,
        [mode]       VARCHAR (30) NOT NULL,
        [id_profile] INT          NOT NULL,
        CONSTRAINT [PK_DEVICE_OUTPUT] PRIMARY KEY CLUSTERED ([id] ASC, [id_profile] ASC),
        CONSTRAINT [FK_PROFILE_DEVICE_OUTPUT] FOREIGN KEY ([id_profile]) REFERENCES [dbo].[profiles] ([id]) ON DELETE CASCADE ON UPDATE CASCADE
    );
END;
GO

DROP TABLE IF EXISTS [dbo].[analog_inputs];
GO
DROP TABLE IF EXISTS [dbo].[digital_inputs];
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'simhub_properties')
BEGIN
    CREATE TABLE [dbo].[simhub_properties] (
        [id]			INT IDENTITY(1,1)          NOT NULL,
        [name]			VARCHAR (255) NULL,
        [description]	TEXT         NULL,
        [simhub_prop]	VARCHAR (255) NOT NULL,
        [jj_prop]		VARCHAR (255) NOT NULL,
        CONSTRAINT [PK_SIMHUB_PROPERTIES] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

MERGE INTO simhub_properties AS target
USING (VALUES 
('Carro - Pit Limiter', '', 'DataCorePlugin.GameData.PitLimiterOn', 'PSL'),
('Bandeira - Preta', '', 'DataCorePlugin.GameData.Flag_Black', 'Flag_Black'),
('Bandeira - Azul', '', 'DataCorePlugin.GameData.Flag_Blue', 'Flag_Blue'),
('Bandeira - Chegada', '', 'DataCorePlugin.GameData.Flag_Checkered', 'Flag_Checkered'),
('Bandeira - Verde', '', 'DataCorePlugin.GameData.Flag_Green', 'Flag_Green'),
('Bandeira - Laranja', '', 'DataCorePlugin.GameData.Flag_Orange', 'Flag_Orange'),
('Bandeira - Branca', '', 'DataCorePlugin.GameData.Flag_White', 'Flag_White'),
('Bandeira - Amarelo', '', 'DataCorePlugin.GameData.Flag_Yellow', 'Flag_Yellow'),
('Carro - ABS (Nível)', '', 'DataCorePlugin.GameData.ABSLevel', 'ABSLevel'),
('Carro - ABS (Ativo)', '', 'DataCorePlugin.GameData.ABSActive', 'ABSActive'),
('Carro - TC (Nível)', '', 'DataCorePlugin.GameData.TCLevel', 'TCLevel'),
('Carro - TC (Ativo)', '', 'DataCorePlugin.GameData.TCActive', 'TCActive'),
('Corrida - Carro à esquerda', '', 'DataCorePlugin.GameData.SpotterCarLeft', 'SpotterLeft'),
('Corrida - Carro à direita', '', 'DataCorePlugin.GameData.SpotterCarRight', 'SpotterRight'),
('Carro - Alerta de ignição ativa', '', 'DataCorePlugin.GameData.EngineIgnitionOn', 'EngineIgnitionOn'),
('Carro - Alerta de motor ativo', '', 'DataCorePlugin.GameData.EngineStarted', 'EngineStarted'),
('Carro - DRS Disponível', '', 'DataCorePlugin.GameData.DRSAvailable', 'DrsAvailable'),
('Carro - DRS Ativo', '', 'DataCorePlugin.GameData.DRSEnabled', 'DrsOn'),
('Carro - Limite de RPM Alcançado', '', 'DataCorePlugin.GameData.CarSettings_RPMRedLineReached', 'RedlineReached'),
('Carro - Freio Ativo', '', 'DataCorePlugin.GameData.CarSettings_RPMRedLineReached', 'BrakeOn'),
('Carro - Freio de Mão Ativo', '', 'DataCorePlugin.GameData.CarSettings_RPMRedLineReached', 'HandbrakeOn'),
('Carro - Indicador Direito', '', 'DataCorePlugin.GameData.TurnIndicatorRight', 'TurnRight'),
('Carro - Indicador Esquerdo', '', 'DataCorePlugin.GameData.TurnIndicatorLeft', 'TurnLeft')
) AS source (name, description, simhub_prop, jj_prop)
ON target.name = source.name
WHEN NOT MATCHED THEN
    INSERT (name, description, simhub_prop, jj_prop)
    VALUES (source.name, source.description, source.simhub_prop, source.jj_prop);
GO

SET ANSI_WARNINGS on
GO