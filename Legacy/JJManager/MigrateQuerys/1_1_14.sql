SET XACT_ABORT ON; -- Automatic rollback on error
SET ANSI_WARNINGS OFF;
GO

DECLARE @schema_name NVARCHAR(256);
DECLARE @table_name NVARCHAR(256);
DECLARE @col_name NVARCHAR(256);
DECLARE @Command NVARCHAR(MAX);

-- Step 1: Set schema, table, and column names
SET @schema_name = N'dbo';
SET @table_name = N'analog_inputs';
SET @col_name = N'axis_orientation';

-- Step 2: Dynamically drop default constraint for the column (original name)
SELECT TOP 1 @Command = 
    'ALTER TABLE ' + @schema_name + '.[' + @table_name + '] DROP CONSTRAINT [' + d.name + ']'
FROM sys.tables t
JOIN sys.default_constraints d ON d.parent_object_id = t.object_id
JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = d.parent_column_id
WHERE t.name = @table_name
  AND t.schema_id = SCHEMA_ID(@schema_name)
  AND c.name = @col_name;

-- Execute the dynamic command to drop the default constraint (if exists)
IF (@Command IS NOT NULL)
    EXEC(@Command);
GO

-- Step 3: Rename the column
EXEC sp_rename N'[dbo].[analog_inputs].[axis_orientation]', N'inverted_axis', 'COLUMN';
GO

-- Step 5: Add default constraint for the renamed column
ALTER TABLE analog_inputs ADD DEFAULT 0 FOR inverted_axis;
GO

-- Step 6: Update the column data to reflect new values
UPDATE analog_inputs SET inverted_axis = 0 WHERE inverted_axis = 'normal';
UPDATE analog_inputs SET inverted_axis = 1 WHERE inverted_axis = 'inverted';
GO
-- Step 4: Dynamically drop default constraint on the renamed column (if exists)
DECLARE @schema_name NVARCHAR(256);
DECLARE @table_name NVARCHAR(256);
DECLARE @col_name NVARCHAR(256);
DECLARE @Command NVARCHAR(MAX);
SET @schema_name = N'dbo';
SET @table_name = N'analog_inputs';
SET @col_name = N'inverted_axis';
SELECT TOP 1 @Command = 
    'ALTER TABLE ' + @schema_name + '.[' + @table_name + '] DROP CONSTRAINT [' + d.name + ']'
FROM sys.tables t
JOIN sys.default_constraints d ON d.parent_object_id = t.object_id
JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = d.parent_column_id
WHERE t.name = @table_name
  AND t.schema_id = SCHEMA_ID(@schema_name)
  AND c.name = @col_name;

-- Execute the dynamic command to drop the default constraint (if exists)
IF (@Command IS NOT NULL)
    EXEC(@Command);
GO

-- Step 7: Alter the column type to BIT
ALTER TABLE analog_inputs ALTER COLUMN inverted_axis BIT;
GO

-- Step 8: Update software version in configs table
UPDATE dbo.configs SET software_version = '1.1.14';
GO

SET ANSI_WARNINGS ON;
GO
