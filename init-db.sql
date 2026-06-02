-- Inicialização do banco de dados ICR
-- Este arquivo é executado automaticamente quando o container PostgreSQL é criado

-- Criar extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Criar schema se necessário
CREATE SCHEMA IF NOT EXISTS public;

-- Conceder permissões ao usuário icradmin
GRANT ALL PRIVILEGES ON SCHEMA public TO icradmin;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO icradmin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO icradmin;

-- Adicionar permissão padrão para tabelas futuras
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO icradmin;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO icradmin;
