/* =============================================================================
   AnaliseCredito - 01 - Criação da base de dados
   -----------------------------------------------------------------------------
   Pode ser executado várias vezes: só cria a base de dados se ainda não existir.
   Executar ligado ao servidor (base de dados "master").
   ============================================================================= */

IF DB_ID(N'AnaliseCredito') IS NULL
BEGIN
    CREATE DATABASE AnaliseCredito;
END
GO
