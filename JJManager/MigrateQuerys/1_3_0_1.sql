-- Automaticamente da Rollback quando acontece um erro na transaction.
set xact_abort on
SET ANSI_WARNINGS off
GO

-- Migration 1.3.0.1: Correcoes JJLC-01
-- - Fix para perfil ativo ao conectar mantendo dados originais do firmware
-- - Fix para habilitar controles quando conecta dentro da tela de edicao

UPDATE dbo.configs SET software_version = '1.3.0.1';
GO

SET ANSI_WARNINGS on
GO
