-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

ALTER TABLE simhub_properties
ALTER COLUMN simhub_prop VARCHAR(255) NULL;
GO

IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'simhub_properties'
      AND COLUMN_NAME = 'activated_on'
)
BEGIN
    ALTER TABLE simhub_properties
    DROP CONSTRAINT DF_SIMHUB_PROPERTIES_ACTIVATED_ON;

    ALTER TABLE simhub_properties
    DROP CONSTRAINT CK__simhub_pr__activ__66EA454A;

    ALTER TABLE simhub_properties
    DROP COLUMN activated_on;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.objects
    WHERE type = 'UQ'
      AND name = 'UQ_SIMHUB_PROPERTIES_JJ_PROP'
)
BEGIN
    ALTER TABLE simhub_properties
    ADD CONSTRAINT UQ_SIMHUB_PROPERTIES_JJ_PROP UNIQUE (jj_prop);
END
GO

MERGE INTO simhub_properties AS target
USING (VALUES
    ('Carro - RPM (10%)', '', NULL, 'RPM_10', '{"type": "number", "value": "1", "comparative": "equals"}'),
    ('Carro - RPM (20%)', '', NULL, 'RPM_20', '{"type": "number", "value": "1", "comparative": "equals"}'),
    ('Carro - RPM (30%)', '', NULL, 'RPM_30', '{"type": "number", "value": "1", "comparative": "equals"}'),
    ('Carro - RPM (40%)', '', NULL, 'RPM_40', '{"type": "number", "value": "1", "comparative": "equals"}'),
    ('Carro - RPM (50%)', '', NULL, 'RPM_50', '{"type": "number", "value": "1", "comparative": "equals"}'),
    ('Carro - RPM (60%)', '', NULL, 'RPM_60', '{"type": "number", "value": "1", "comparative": "equals"}'),
    ('Carro - RPM (70%)', '', NULL, 'RPM_70', '{"type": "number", "value": "1", "comparative": "equals"}'),
    ('Carro - RPM (80%)', '', NULL, 'RPM_80', '{"type": "number", "value": "1", "comparative": "equals"}'),
    ('Carro - RPM (90%)', '', NULL, 'RPM_90', '{"type": "number", "value": "1", "comparative": "equals"}'),
    ('Carro - RPM (100%)', '', NULL, 'RPM_100', '{"type": "number", "value": "1", "comparative": "equals"}')
) AS source (name, description, simhub_prop, jj_prop, activate_on)
ON target.jj_prop = source.jj_prop
WHEN MATCHED THEN
    UPDATE SET 
        target.name = source.name,
        target.description = source.description,
        target.simhub_prop = source.simhub_prop,
        target.activate_on = source.activate_on
WHEN NOT MATCHED THEN
    INSERT (name, description, simhub_prop, jj_prop, activate_on)
    VALUES (source.name, source.description, source.simhub_prop, source.jj_prop, source.activate_on);
GO

UPDATE dbo.configs SET software_version = '1.2.9';
GO

SET ANSI_WARNINGS on
GO