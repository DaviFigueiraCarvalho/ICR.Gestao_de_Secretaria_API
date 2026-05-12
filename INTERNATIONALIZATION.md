# 🌍 Sistema Internacionalizado de Telefone e Endereço

## Resumo das Mudanças

A solução foi totalmente internacionalizada para suportar múltiplos países além do Brasil. Abaixo está o detalhamento completo:

### ✅ O que foi implementado:

#### 1. **Value Objects Internacionais**
- `Country.cs` - Suporta 40+ países com código ISO, nome, código de telefone e cultura
- `Phone.cs` - Validação internacional de telefones usando **libphonenumber-csharp**
- `Address.cs` - Suporte a múltiplos formatos de código postal por país

#### 2. **Países Suportados**
- Brasil (BR)
- Estados Unidos (US)
- Canadá (CA)
- México (MX)
- Portugal (PT)
- Espanha (ES)
- Argentina (AR)
- Chile (CL)
- Colômbia (CO)
- Peru (PE)
- Venezuela (VE)
- Reino Unido (GB)
- França (FR)
- Alemanha (DE)
- Itália (IT)
- Países Baixos (NL)
- Bélgica (BE)
- Suíça (CH)
- Áustria (AT)
- Polônia (PL)
- República Tcheca (CZ)
- Hungria (HU)
- Romênia (RO)
- Ucrânia (UA)
- Rússia (RU)
- Japão (JP)
- China (CN)
- Índia (IN)
- África do Sul (ZA)
- Austrália (AU)
- Nova Zelândia (NZ)
- Coreia do Sul (KR)
- Singapura (SG)
- Tailândia (TH)
- Indonésia (ID)
- Filipinas (PH)
- Israel (IL)
- Arábia Saudita (SA)
- Emirados Árabes Unidos (AE)
- Turquia (TR)
- Egito (EG)
- Nigéria (NG)
- Quênia (KE)

#### 3. **Validação de Telefone**
Cada país possui validação específica:
- **Brasil**: 10-11 dígitos com DDD válido
- **EUA/Canadá**: Mínimo 10 dígitos
- **Portugal**: Mínimo 9 dígitos
- **Suporte genérico** para outros países

Formatos suportados:
- `DisplayFormat`: Formatado para exibição (ex: "(11) 99999-9999")
- `InternationalFormat`: Formato internacional (ex: "+55 11 99999-9999")
- `E164Format`: Formato E.164 (ex: "+5511999999999")
- `Number`: Apenas dígitos para armazenamento

#### 4. **Validação de Endereço**
Cada país possui validação específica de código postal:
- Brasil: 8 dígitos (CEP)
- EUA: 5 ou 9 dígitos (ZIP)
- Canadá: Formato A1A 1A1
- Portugal: 4 dígitos
- Espanha: 5 dígitos
- México: 5 dígitos
- Argentina: Mínimo 4 dígitos
- Chile: 7 dígitos
- Colômbia: 6 dígitos
- E mais...

#### 5. **Modelos Atualizados**
- `Member` - CellPhone agora é `Phone?` (opcional, internacional)
- `Minister` - Address agora é `Address?` (opcional)
- Todas as entidades mantêm suporte a múltiplos países

#### 6. **DTOs Atualizados**
- `PhoneDTO` / `PhoneResponseDTO` - Inclui país e múltiplos formatos
- `AddressDTO` - Inclui país e campos opcionais adicionais
- `MemberDTO`, `MinisterDTO`, `ChurchDTO` - Atualizados para usar novos DTOs

#### 7. **Service de Validação**
`LocationValidationService` fornece:
- `GetSupportedCountries()` - Lista todos os países
- `GetCountryInfo(code)` - Informações de um país específico

### 🗄️ Mudanças no Banco de Dados

Será necessário criar uma migration para:
1. Adicionar coluna `Country` na tabela `address`
2. Renomear `ZipCode` → `PostalCode`
3. Adicionar colunas opcionais (`Complement`, `CountyOrRegion`)
4. Adicionar suporte a `Phone` na tabela `members`

**Comando para criar migration:**
```bash
dotnet ef migrations add AddInternationalPhoneAndAddress --project ICR.Infrastructure
dotnet ef database update
```

### 📦 NuGet Packages Adicionados
- `libphonenumber-csharp` v9.0.29 - Validação internacional de telefones (Google)

### 🔄 Migração de Dados Existentes

Para dados brasileiros existentes, execute durante a migration:
```csharp
// Todos os telefones e endereços existentes serão marcados como Brasil (BR)
UPDATE members SET Country = 'BR' WHERE Country IS NULL;
UPDATE addresses SET Country = 'BR' WHERE Country IS NULL;
```

### 💡 Exemplos de Uso

#### Criar Member com Telefone
```csharp
var phone = new Phone("BR", "11999999999");
var member = new Member(..., phone, ...);
```

#### Criar Minister com Endereço
```csharp
var address = new Address(
    "US",
    "12345",
    "Main Street",
    "123",
    "New York",
    "NY",
    "Apt 101",
    "Manhattan"
);
var minister = new Minister(..., address, ...);
```

#### Via API
```json
POST /api/members
{
  "cellPhone": {
    "countryCode": "BR",
    "number": "11999999999"
  }
}

POST /api/churches
{
  "address": {
    "countryCode": "US",
    "postalCode": "12345",
    "street": "Main Street",
    "number": "123",
    "city": "New York",
    "state": "NY"
  }
}
```

### ⚠️ Notas Importantes

1. **Phone é opcional no Member** - Use `Phone?`
2. **Address é opcional no Minister** - Use `Address?`
3. **Validação ocorre no backend** - Seguro e confiável
4. **Country não pode ser alterado** - Uma vez criado, o país da entidade é imutável
5. **Formato é automatizado** - O sistema escolhe o melhor formato para cada país

### 🚀 Próximas Etapas

1. ✅ Criar migrations
2. ✅ Atualizar controllers da API
3. ✅ Adicionar testes de validação
4. ✅ Documentar endpoints
5. ✅ Migrar dados existentes

---

**Data**: 2026
**Framework**: .NET 10
**Status**: ✅ Implementado (Aguardando migrations)
