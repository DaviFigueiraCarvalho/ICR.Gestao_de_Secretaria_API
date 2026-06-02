# 🚀 Guia de Início Rápido - Docker Compose Local

## ⚡ Primeiros Passos (5 minutos)

### 1️⃣ Verifique os pré-requisitos
```powershell
# Verificar Docker
docker --version

# Verificar Docker Compose
docker-compose --version
```

Se falhar, instale [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

### 2️⃣ Prepare o ambiente

```powershell
# Vá para a raiz do projeto
cd C:\Users\Federação ICR\source\repos\ICR.Gestao_de_Secretaria_API

# Verifique se os arquivos foram criados
ls docker-compose.local.yml
ls .env.local
ls docker-local.ps1
```

---

### 3️⃣ Inicie a aplicação

**Opção A: Usando PowerShell Script (Recomendado)**
```powershell
# Dar permissão de execução (primeira vez)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Iniciar
.\docker-local.ps1 start
```

**Opção B: Usando docker-compose diretamente**
```powershell
docker-compose -f docker-compose.local.yml up -d --build
```

---

### 4️⃣ Verifique se está rodando

```powershell
# Ver status
.\docker-local.ps1 status

# Ou
docker-compose -f docker-compose.local.yml ps
```

**Esperado:**
```
NAME                COMMAND             SERVICE      STATUS      PORTS
icr-postgres-local  postgres            postgres     running     0.0.0.0:5432->5432/tcp
icr-api-local       dotnet watch ...    icr.api      running     0.0.0.0:8080->8080/tcp
icr-pgadmin-local   /entrypoint.sh      pgadmin      running     0.0.0.0:5050->80/tcp
```

---

### 5️⃣ Acesse os serviços

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **API** | http://localhost:8080 | - |
| **Swagger** | http://localhost:8080/swagger | - |
| **PgAdmin** | http://localhost:5050 | admin@localhost / admin123 |
| **Banco** | localhost:5432 | icradmin / root |

---

## 🎯 Próximos Passos

### ✅ Testar a API
```powershell
# Abrir em um navegador
start http://localhost:8080

# Ou fazer um request
curl http://localhost:8080
```

### ✅ Conectar ao Banco de Dados
1. Abra http://localhost:5050 (PgAdmin)
2. Login: `admin@localhost` / `admin123`
3. Adicionar servidor (ver `README_LOCAL.md` para detalhes)

### ✅ Ver logs em tempo real
```powershell
.\docker-local.ps1 logs

# Ou específico
docker-compose -f docker-compose.local.yml logs -f icr.api
```

---

## 🔧 Comandos Úteis

```powershell
# Status
.\docker-local.ps1 status

# Parar
.\docker-local.ps1 stop

# Reiniciar
.\docker-local.ps1 restart

# Logs
.\docker-local.ps1 logs

# Limpar (reset completo)
.\docker-local.ps1 clean

# Reconstruir API
.\docker-local.ps1 build

# Acessar shell do container
.\docker-local.ps1 shell
```

---

## 🐛 Se algo não funcionar

### 1. Limpar e recomeçar
```powershell
.\docker-local.ps1 clean
.\docker-local.ps1 start
```

### 2. Ver logs detalhados
```powershell
.\docker-local.ps1 logs
# ou específico para API
docker-compose -f docker-compose.local.yml logs -f icr.api
```

### 3. Verificar portas
```powershell
# Listar portas em uso
netstat -ano | findstr :5432
netstat -ano | findstr :8080
netstat -ano | findstr :5050
```

### 4. Reiniciar Docker Desktop
- Abrir Docker Desktop
- Clicar em "Restart"

---

## 📚 Documentação Completa

Para mais detalhes, consulte:
- **`README_LOCAL.md`** - Guia completo com troubleshooting
- **`DOCKER_REFERENCE.md`** - Referência rápida
- **`docker-compose.local.yml`** - Configuração dos containers

---

## ✨ Características

✅ PostgreSQL 16 Alpine  
✅ API .NET 10 com hot reload (`dotnet watch`)  
✅ PgAdmin para gerenciar banco  
✅ Volumes sincronizados com código local  
✅ Health checks automáticos  
✅ Rede isolada Docker  
✅ Credenciais simples para desenvolvimento  

---

## 🎓 Estrutura

```
Seu Projeto
├── docker-compose.local.yml    ← Orquestração
├── .env.local                  ← Variáveis (não commit)
├── init-db.sql                 ← Inicialização DB
├── docker-local.ps1            ← Script de gerenciamento
├── README_LOCAL.md             ← Guia completo
├── DOCKER_REFERENCE.md         ← Referência rápida
├── QUICK_START.md              ← Este arquivo
└── ICR.API/Dockerfile          ← Imagem da API
```

---

## 💡 Dicas

1. **Desenvolvimento**: Edite o código localmente, `dotnet watch` recompila automaticamente
2. **Debug**: Use `.\docker-local.ps1 logs` para ver tudo
3. **Banco de dados**: Use PgAdmin em http://localhost:5050 para gerenciar dados
4. **Reset**: Se algo quebrou, `.\docker-local.ps1 clean` e `start` novamente

---

**Sucesso! 🎉 Sua aplicação agora está rodando 100% localmente!**

Dúvidas? Consulte os arquivos de documentação ou veja os logs.
