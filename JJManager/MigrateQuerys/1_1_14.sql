-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

--BEGIN TRANSACTION _1_1_14

declare @schema_name nvarchar(256)
declare @table_name nvarchar(256)
declare @col_name nvarchar(256)
declare @Command  nvarchar(1000)

set @schema_name = N'dbo'
set @table_name = N'analog_inputs'
set @col_name = N'axis_orientation'

select @Command = 'ALTER TABLE ' + @schema_name + '.[' + @table_name + '] DROP CONSTRAINT ' + d.name
 from sys.tables t
  join sys.default_constraints d on d.parent_object_id = t.object_id
  join sys.columns c on c.object_id = t.object_id and c.column_id = d.parent_column_id
 where t.name = @table_name
  and t.schema_id = schema_id(@schema_name)
  and c.name = @col_name

--print @Command

execute (@Command)
GO

EXEC sp_rename N'[dbo].[analog_inputs].[axis_orientation]', N'inverted_axis', 'COLUMN'
GO

ALTER TABLE analog_inputs ADD DEFAULT 0 FOR inverted_axis;
ALTER TABLE analog_inputs ALTER COLUMN type varchar(10);
UPDATE analog_inputs SET inverted_axis = 0 WHERE inverted_axis = 'normal';
UPDATE analog_inputs SET inverted_axis = 1 WHERE inverted_axis = 'inverted';
GO
ALTER TABLE analog_inputs ALTER COLUMN inverted_axis BIT;

UPDATE dbo.configs SET software_version = '1.1.14';

--COMMIT TRANSACTION _1_1_14
GO

SET ANSI_WARNINGS on
GO