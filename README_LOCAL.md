# Guia de Execução Local com Docker Compose

Este guia descreve como executar a aplicação ICR Gestão de Secretaria 100% localmente usando Docker Compose.

## 📋 Pré-requisitos

- Docker Desktop instalado e rodando
- Docker Compose (geralmente incluído com Docker Desktop)
- Git
- PowerShell (ou terminal compatível)

## 🚀 Quick Start

### 1. Construir e iniciar os containers

```powershell
docker-compose -f docker-compose.local.yml up -d --build
```

Isso iniciará:
- **PostgreSQL** (porta 5432)
- **API .NET 10** (porta 8080)
- **PgAdmin** (porta 5050) - para gerenciar o banco de dados

### 2. Verificar o status

```powershell
docker-compose -f docker-compose.local.yml ps
```

### 3. Acessar a aplicação

- **API**: http://localhost:8080
- **Swagger/OpenAPI**: http://localhost:8080/swagger (se disponível)
- **PgAdmin**: http://localhost:5050
  - Email: `admin@localhost`
  - Senha: `admin123`

## 🔧 Comandos Úteis

### Visualizar logs da API
```powershell
docker-compose -f docker-compose.local.yml logs -f icr.api
```

### Visualizar logs do PostgreSQL
```powershell
docker-compose -f docker-compose.local.yml logs -f postgres
```

### Visualizar todos os logs
```powershell
docker-compose -f docker-compose.local.yml logs -f
```

### Parar os containers
```powershell
docker-compose -f docker-compose.local.yml down
```

### Parar e remover volumes (limpar dados)
```powershell
docker-compose -f docker-compose.local.yml down -v
```

### Reconstruir a imagem da API
```powershell
docker-compose -f docker-compose.local.yml build --no-cache icr.api
```

### Executar comando dentro do container da API
```powershell
docker-compose -f docker-compose.local.yml exec icr.api dotnet --version
```

## 📊 Configuração do Banco de Dados no PgAdmin

1. Acesse http://localhost:5050
2. Faça login com:
   - Email: `admin@localhost`
   - Senha: `admin123`
3. Clique em "Add New Server"
4. Na aba "General", preencha:
   - Name: `ICR Local`
5. Na aba "Connection", preencha:
   - Host name/address: `postgres`
   - Port: `5432`
   - Username: `icradmin`
   - Password: `root`
   - Save password: ✓
6. Clique em "Save"

## 🔌 Conectar ao Banco de Dados Localmente

Para conectar de ferramentas externas como DBeaver, TablePlus, etc:

```
Host: localhost
Port: 5432
Database: icr_connect
Username: icradmin
Password: root
```

## 📝 Variáveis de Ambiente

As variáveis estão definidas em:
- `.env.local` - para docker-compose
- `ICR.API/appsettings.Development.json` - para execução local sem Docker

Você pode customizar:
- `POSTGRES_PASSWORD` - senha do PostgreSQL
- `JWT_KEY` - chave JWT
- `ROOTUSERNAME` / `ROOTPASSWORD` - credenciais do usuário root da aplicação
- Endpoints CORS

## 🐛 Troubleshooting

### A API não consegue conectar ao banco de dados
1. Verifique se o PostgreSQL está rodando: `docker ps`
2. Verifique os logs: `docker-compose -f docker-compose.local.yml logs postgres`
3. Verifique a string de conexão em `docker-compose.local.yml`

### Porta já está em uso
Se as portas 5432, 8080 ou 5050 estão em uso:

Edite `docker-compose.local.yml` e mude o mapeamento de portas, ex:
```yaml
ports:
  - "5433:5432"  # Use 5433 ao invés de 5432
```

### Reconstruir tudo do zero
```powershell
docker-compose -f docker-compose.local.yml down -v
docker system prune -a
docker-compose -f docker-compose.local.yml up -d --build
```

## 🔄 Volume Compartilhado

A API está configurada com um volume que sincroniza o código local:
```yaml
volumes:
  - .:/app
```

Isso permite:
- Fazer alterações no código localmente
- Ver as alterações refletidas no container (via `dotnet watch`)
- Debug mais fácil

## ⚡ Hot Reload

O Dockerfile está configurado com `dotnet watch`, que recompila automaticamente quando você muda o código.

## 📚 Estrutura do Projeto

```
.
├── docker-compose.local.yml    # Orquestração local
├── .env.local                  # Variáveis de ambiente
├── init-db.sql                 # Inicialização do banco
├── ICR.API/                    # API .NET
├── ICR.Application/            # Camada de aplicação
├── ICR.Infrastructure/         # Camada de infraestrutura
└── ICR.Domain/                 # Camada de domínio
```

## 🎯 Próximas Pastas

- [ ] Testar a API em http://localhost:8080
- [ ] Conectar o PgAdmin e verificar as tabelas
- [ ] Executar testes de integração
- [ ] Consultar documentação de migração do banco de dados

---

**Dúvidas?** Verifique os logs: `docker-compose -f docker-compose.local.yml logs -f`
