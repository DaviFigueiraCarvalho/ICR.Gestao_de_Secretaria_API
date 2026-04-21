# Guia de Deploy em Produção

Este documento descreve os requisitos obrigatórios e as boas práticas para executar a API em ambiente de produção.

---

## Variáveis de Ambiente Obrigatórias

Todas as variáveis abaixo **devem** ser definidas antes de iniciar a aplicação em produção.
A ausência de qualquer uma delas causará falha imediata na inicialização com uma mensagem de erro clara.

| Variável                              | Descrição                                                                                               | Exemplo                                      |
|---------------------------------------|--------------------------------------------------------------------------------------------------------|----------------------------------------------|
| `JWT_KEY`                             | Chave secreta para assinar tokens JWT. Use no mínimo 32 caracteres aleatórios.                         | _(gerado com ferramenta de secrets)_         |
| `DATABASE_URL`                        | URL de conexão do PostgreSQL em produção (Supabase).                                                    | `postgres://user:pass@host:5432/db`          |
| `ROOTUSERNAME`                        | Nome de usuário do administrador root criado na primeira inicialização (banco vazio).                   | `admin_root`                                 |
| `ROOTPASSWORD`                        | Senha do administrador root. Deve ser forte. **Nunca será logada.**                                    | _(gerado com ferramenta de secrets)_         |
| `ASPNETCORE_ENVIRONMENT`              | Ambiente de execução. Deve ser `Production` em produção.                                               | `Production`                                 |
| `Cors__AllowedOrigins__0`             | Primeira origem CORS permitida (repita com `__1`, `__2` etc. para múltiplas origens).                  | `https://meudominio.com`                     |

### Variáveis de Ambiente Opcionais

| Variável                              | Descrição                                                                                               | Padrão             |
|---------------------------------------|--------------------------------------------------------------------------------------------------------|--------------------|
| `Database__ApplyMigrationsOnStartup` | Aplica migrations automaticamente no startup. Recomendado manter `false` em produção.                  | `false`            |
| `ASPNETCORE_URLS`                     | URL(s) em que a aplicação escuta.                                                                       | `http://+:8080`    |

---

## Configuração de CORS

Os domínios do frontend permitidos devem ser fornecidos via variável de ambiente ou configuração. Use a notação de índice do ASP.NET Core para múltiplos valores:

```bash
Cors__AllowedOrigins__0=https://meudominio.com
Cors__AllowedOrigins__1=https://www.meudominio.com
```

---

## Geração de JWT_KEY Segura

Use uma ferramenta de linha de comando para gerar uma chave aleatória forte:

```bash
# Linux/macOS
openssl rand -base64 48

# PowerShell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
```

---

## Checklist de Deploy

- [ ] `ASPNETCORE_ENVIRONMENT=Production` definido
- [ ] `JWT_KEY` gerado com no mínimo 32 caracteres e armazenado em um secret manager
- [ ] `DATABASE_URL` apontando para o banco de produção (Supabase)
- [ ] `ROOTUSERNAME` e `ROOTPASSWORD` definidos para o primeiro deploy (banco vazio)
- [ ] `Cors__AllowedOrigins__0` definido com o domínio real do frontend
- [ ] Banco de dados PostgreSQL acessível e saudável antes do primeiro start
- [ ] Migrations aplicadas antes do primeiro start (`dotnet ef database update` ou pipeline de CI/CD)
- [ ] Backup do banco configurado
- [ ] Health check (`/health`) monitorado por ferramenta de observabilidade
- [ ] Logs estruturados sem dados sensíveis confirmados

---

## Comportamento no Primeiro Start (Banco Vazio)

Quando o banco de dados está vazio, a aplicação cria automaticamente um usuário root com escopo `NATIONAL` (acesso total) usando `ROOTUSERNAME` e `ROOTPASSWORD`. Após a criação, **a senha não é mais necessária no ambiente** e deve ser rotacionada via painel administrativo.

---

## Migrations em Produção

A opção `Database:ApplyMigrationsOnStartup` está desabilitada por padrão em produção. Recomenda-se aplicar migrations manualmente ou via pipeline de CI/CD antes de fazer o deploy:

```bash
dotnet ef database update --project ICR.Infrastructure --startup-project ICR.API
```

---

## Health Check

A aplicação expõe um endpoint de health check em `/health`. Use-o para:
- Readiness/liveness probes no Kubernetes
- Monitoramento externo
- Verificação pós-deploy

```bash
curl https://api.meudominio.com/health
# Resposta esperada: 200 Healthy
```

---

## Estratégia de Deploy em Produção

Em produção, este projeto **não usa `docker-compose`**.

- Frontend e API são publicados em serviços separados.
- O banco é externo (Supabase), acessado por `DATABASE_URL`.
- A comunicação entre serviços é por HTTPS/URL pública.

Exemplo de variáveis da API em produção:

```bash
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=postgres://user:pass@host:5432/db
JWT_KEY=<secret>
ROOTUSERNAME=<root-user>
ROOTPASSWORD=<root-pass>
Cors__AllowedOrigins__0=https://app.seudominio.com
```

## Desenvolvimento Local

Para desenvolvimento, use `docker-compose` local com banco PostgreSQL local (não usar banco de produção):

- `docker-compose.yml` com `icr_db` local
- API apontando para `ConnectionStrings__DefaultConnection` local
- Nunca testar local contra banco de produção
