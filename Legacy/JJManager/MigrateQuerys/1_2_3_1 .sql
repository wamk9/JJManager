-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

UPDATE dbo.configs SET software_version = '1.2.3.1';
GO

SET ANSI_WARNINGS on
GO