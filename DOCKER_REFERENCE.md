# docker-compose.local - Quick Reference

## 🎯 Comandos Essenciais

### Iniciar (recomendado)
```powershell
.\docker-local.ps1 start
```

### Parar
```powershell
.\docker-local.ps1 stop
```

### Ver logs
```powershell
.\docker-local.ps1 logs
```

### Limpar tudo
```powershell
.\docker-local.ps1 clean
```

---

## 📦 Arquivos Criados

| Arquivo | Propósito |
|---------|-----------|
| `docker-compose.local.yml` | Composição dos containers (PostgreSQL, API, PgAdmin) |
| `.env.local` | Variáveis de ambiente (NÃO adicionar ao git) |
| `.env.example` | Template de variáveis de ambiente |
| `init-db.sql` | Script de inicialização do PostgreSQL |
| `docker-local.ps1` | Script PowerShell para gerenciamento |
| `README_LOCAL.md` | Documentação completa |
| `DOCKER_REFERENCE.md` | Este arquivo |

---

## 🔌 Serviços Disponíveis

### 1. PostgreSQL
- **Container**: postgres (icr-postgres-local)
- **Porta**: 5432
- **Usuário**: icradmin
- **Senha**: root
- **Database**: icr_connect
- **Health Check**: Automático

### 2. API .NET 10
- **Container**: icr.api (icr-api-local)
- **Porta**: 8080
- **Ambiente**: Development
- **Modo**: Watch (recompila automaticamente)

### 3. PgAdmin
- **Container**: pgadmin (icr-pgadmin-local)
- **Porta**: 5050
- **Email**: admin@localhost
- **Senha**: admin123

---

## 🚨 Troubleshooting Rápido

### Erro: "Address already in use"
**Solução**: Mudar portas em `docker-compose.local.yml`:
```yaml
ports:
  - "5433:5432"  # Use 5433 em vez de 5432
```

### Erro: "Cannot connect to the Docker daemon"
**Solução**: Iniciar Docker Desktop

### API não inicia / compila com erro
```powershell
# Reconstruir sem cache
.\docker-local.ps1 build

# Ver logs detalhados
.\docker-local.ps1 logs
```

### PostgreSQL não inicializa
```powershell
# Verificar logs do banco
docker-compose -f docker-compose.local.yml logs postgres

# Reset completo
.\docker-local.ps1 clean
.\docker-local.ps1 start
```

### Dados do banco não persistem
**Verificar**: Se o volume `postgres_data` existe
```powershell
docker volume ls | findstr icr
```

---

## 🔐 Segurança

⚠️ **NUNCA** comitar `.env.local` - está no `.gitignore`

Para produção, use:
- Senhas fortes
- Secrets no Docker Swarm/Kubernetes
- Variáveis de ambiente do SO

---

## 📊 Monitoramento

### Status dos containers
```powershell
.\docker-local.ps1 status
```

### Usar Docker Desktop GUI
- Abrir Docker Desktop
- Aba "Containers"
- Procurar por containers com nome "icr-"

### Inspecionar container
```powershell
docker inspect icr-api-local
```

---

## 🔄 Workflow de Desenvolvimento

1. **Iniciar**: `.\docker-local.ps1 start`
2. **Desenvolver**: Editar código localmente
3. **Aguardar recompilação**: O `dotnet watch` faz isso automaticamente
4. **Testar**: Acessar http://localhost:8080
5. **Debug**: Ver logs com `.\docker-local.ps1 logs`
6. **Parar**: `.\docker-local.ps1 stop`

---

## 🎓 Aprendizado

### Estrutura Docker Compose Local
```yaml
Services:
  ├── postgres (Database)
  ├── icr.api (API Development)
  └── pgadmin (Database Management UI)

Networks:
  └── icr-local-network (Bridge)

Volumes:
  ├── postgres_data (Database persistence)
  └── . (App source code - sync)
```

### Healthcheck
PostgreSQL tem healthcheck para garantir readiness:
```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U icradmin -d icr_connect"]
  interval: 10s
  timeout: 5s
  retries: 5
```

---

## ✅ Checklist Inicial

- [ ] Docker Desktop instalado e rodando
- [ ] Portas 5432, 8080, 5050 disponíveis
- [ ] `.env.local` criado (cópia de `.env.example`)
- [ ] `docker-compose.local.yml` na raiz do projeto
- [ ] Dockerfile localizado em `ICR.API/Dockerfile`

---

## 📞 Suporte

Para mais informações:
1. Ler `README_LOCAL.md`
2. Consultar `docker-compose.local.yml`
3. Verificar logs com `.\docker-local.ps1 logs`
4. Limpar e reconstruir com `.\docker-local.ps1 clean` + `start`
