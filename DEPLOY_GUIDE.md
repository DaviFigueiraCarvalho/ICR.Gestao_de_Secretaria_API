# 🚀 Guia de Deploy - Sistema Internacionalizado

## ⚡ Quick Start

### 1. **Limpar e Compilar**
```bash
cd ICR.Gestao_de_Secretaria_API

# Limpar cache
dotnet clean
dotnet nuget locals all --clear

# Compilar cada projeto individualmente
dotnet build ICR.Domain/ICR.Domain.csproj -c Release
dotnet build ICR.Application/ICR.Application.csproj -c Release
dotnet build ICR.Infrastructure/ICR.Infrastructure.csproj -c Release
dotnet build ICR.API/ICR.API.csproj -c Release
```

### 2. **Aplicar Migration**
```bash
# Via dotnet CLI
dotnet ef database update --project ICR.Infrastructure --startup-project ICR.API

# OU via Package Manager Console (Visual Studio)
Update-Database
```

### 3. **Migrar Dados Existentes**
Execute no seu banco PostgreSQL:

```sql
-- Marcar membros com telefone existente como Brasil
UPDATE members 
SET "CellPhone_Country_Code" = 'BR',
    "CellPhone_Number" = regexp_replace("PhoneNumber", '\D', '', 'g')
WHERE "CellPhone_Country_Code" IS NULL 
  AND "PhoneNumber" IS NOT NULL;

-- Marcar igrejas com endereço como Brasil
UPDATE church 
SET "Address_Country_Code" = 'BR'
WHERE "Address_Country_Code" IS NULL;

-- Marcar ministros com endereço como Brasil
UPDATE minister 
SET "Address_Country_Code" = 'BR'
WHERE "Address_Country_Code" IS NULL;
```

---

## 🧪 Testes

### Testar Telefone Brasileiro
```bash
curl -X POST http://localhost:5000/api/members \
  -H "Content-Type: application/json" \
  -d '{
    "familyId": 1,
    "name": "João Silva",
    "gender": 1,
    "birthDate": "1990-01-15T00:00:00Z",
    "hasBeenMarried": false,
    "role": 0,
    "cellPhone": {
      "countryCode": "BR",
      "number": "11999887766"
    }
  }'
```

### Testar Telefone Americano
```bash
curl -X POST http://localhost:5000/api/members \
  -H "Content-Type: application/json" \
  -d '{
    "familyId": 1,
    "name": "John Smith",
    "gender": 1,
    "birthDate": "1990-01-15T00:00:00Z",
    "hasBeenMarried": false,
    "role": 0,
    "cellPhone": {
      "countryCode": "US",
      "number": "2015550123"
    }
  }'
```

### Testar Endereço Brasileiro
```bash
curl -X POST http://localhost:5000/api/churches \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Igreja Centro",
    "federationId": 1,
    "address": {
      "countryCode": "BR",
      "postalCode": "01310100",
      "street": "Avenida Paulista",
      "number": "1000",
      "city": "São Paulo",
      "state": "SP"
    }
  }'
```

### Testar Endereço Americano
```bash
curl -X POST http://localhost:5000/api/churches \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New York Church",
    "federationId": 1,
    "address": {
      "countryCode": "US",
      "postalCode": "10001",
      "street": "Fifth Avenue",
      "number": "350",
      "city": "New York",
      "state": "NY"
    }
  }'
```

---

## 🐛 Troubleshooting

### Erro: "OutOfMemoryException"
```bash
# Solução: Limpar cache e compilar projeto por projeto
dotnet clean
Remove-Item -Path bin, obj -Recurse -Force -ErrorAction SilentlyContinue
dotnet build ICR.Domain/ICR.Domain.csproj -c Release
dotnet build ICR.Application/ICR.Application.csproj -c Release
dotnet build ICR.Infrastructure/ICR.Infrastructure.csproj -c Release
dotnet build ICR.API/ICR.API.csproj -c Release
```

### Erro: "relation already exists"
```bash
# A migration foi parcialmente aplicada. Remover a última migration e recriar:
dotnet ef migrations remove --project ICR.Infrastructure
dotnet ef migrations add AddInternationalPhoneAndAddress --project ICR.Infrastructure
dotnet ef database update --project ICR.Infrastructure --startup-project ICR.API
```

### Erro: "DbContext not found"
```bash
# Certifique-se de que está usando o projeto correto:
dotnet ef database update \
  --project ICR.Infrastructure \
  --startup-project ICR.API
```

---

## 📊 Validações por País

### Brasil (BR)
- **Telefone**: 10-11 dígitos (DDD + número)
- **CEP**: 8 dígitos
- **Exemplo**: `11999999999`, `01310100`

### Estados Unidos (US)
- **Telefone**: 10+ dígitos
- **ZIP**: 5 ou 9 dígitos
- **Exemplo**: `2015550123`, `10001` ou `10001-1234`

### Canadá (CA)
- **Telefone**: 10+ dígitos
- **Postal**: Formato A1A 1A1
- **Exemplo**: `2025550123`, `K1A 0B1`

### Portugal (PT)
- **Telefone**: 9+ dígitos
- **Postal**: 4 dígitos
- **Exemplo**: `912345678`, `1000`

### Espanha (ES)
- **Telefone**: 9+ dígitos
- **Postal**: 5 dígitos
- **Exemplo**: `912345678`, `28001`

### México (MX)
- **Telefone**: 10+ dígitos
- **Postal**: 5 dígitos
- **Exemplo**: `5512345678`, `06600`

---

## 📱 Formato de Resposta da API

### GET /api/members/1
```json
{
  "id": 1,
  "name": "João Silva",
  "cellPhone": {
    "countryCode": "BR",
    "countryName": "Brasil",
    "number": "11999999999",
    "displayFormat": "(11) 99999-9999",
    "internationalFormat": "+55 11 99999-9999",
    "e164Format": "+5511999999999",
    "isMobileNumber": true
  }
}
```

### GET /api/churches/1
```json
{
  "id": 1,
  "name": "Igreja Centro",
  "address": {
    "countryCode": "BR",
    "postalCode": "01310100",
    "street": "Avenida Paulista",
    "number": "1000",
    "city": "São Paulo",
    "state": "SP",
    "complement": null,
    "countyOrRegion": null
  }
}
```

---

## 🔄 Verificar Sucesso da Implementação

```bash
# 1. Verificar se os projetos compilam
dotnet build "ICR.Gestão_de_Secretaria.slnx" -c Release

# 2. Verificar se a migration foi aplicada
dotnet ef migrations list --project ICR.Infrastructure

# 3. Conectar ao banco e verificar as tabelas
psql -U postgres -d seu_banco -c "\d members"
psql -U postgres -d seu_banco -c "\d church"
psql -U postgres -d seu_banco -c "\d minister"

# 4. Testar a API
curl http://localhost:5000/api/v1/countries
```

---

## 📋 Checklist de Deploy

- [ ] Todos os projetos compilam com sucesso
- [ ] Migration foi criada e testada localmente
- [ ] Dados existentes foram migrados corretamente
- [ ] Testes com telefones de diferentes países passam
- [ ] Testes com endereços de diferentes países passam
- [ ] API retorna formatos corretos
- [ ] Banco de dados está atualizado
- [ ] Documentação da API foi atualizada
- [ ] Testes unitários e de integração passam

---

## 🎉 Sucesso!

Se chegou aqui, a implementação foi bem-sucedida! Seu sistema agora suporta:
- ✅ 40+ países
- ✅ Validação internacional de telefones (Google libphonenumber)
- ✅ Validação de código postal por país
- ✅ Múltiplos formatos de exibição
- ✅ Backend seguro e confiável

**Parabéns!** 🚀
