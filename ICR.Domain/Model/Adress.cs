using System;
using System.Text.RegularExpressions;

namespace ICR.Domain.Model
{
    public class Address
    {
        // Propriedade interna para EF Core identificar quando Address existe
        public int? Id { get; set; }

        public Country? Country { get; private set; }
        public string? PostalCode { get; private set; }      // CEP, ZIP, Código Postal, etc.
        public string? Street { get; private set; }
        public string? Number { get; private set; }
        public string? Complement { get; private set; }     // Apartamento, bloco, etc.
        public string? City { get; private set; }
        public string? State { get; private set; }           // Estado, Província, Região, etc.
        public string? CountyOrRegion { get; private set; } // Condado, Região (opcional)

        protected Address() { }

        public Address(
            string? countryCode,
            string? postalCode,
            string? street,
            string? number,
            string? city,
            string? state,
            string? complement = null,
            string? countyOrRegion = null)
        {
            // Se todos os campos obrigatórios forem nulos/vazios, criar um endereço vazio (nullable)
            if (string.IsNullOrWhiteSpace(countryCode) &&
                string.IsNullOrWhiteSpace(postalCode) &&
                string.IsNullOrWhiteSpace(street) &&
                string.IsNullOrWhiteSpace(number) &&
                string.IsNullOrWhiteSpace(city) &&
                string.IsNullOrWhiteSpace(state))
            {
                // Todos nulos - endereço vazio permitido
                Country = null;
                PostalCode = null;
                Street = null;
                Number = null;
                City = null;
                State = null;
                Complement = null;
                CountyOrRegion = null;
                return;
            }

            // Se alguns campos estão preenchidos, todos obrigatórios devem estar
            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ArgumentException("Código do país é obrigatório quando endereço é fornecido", nameof(countryCode));

            Country = Country.GetByCode(countryCode);
            PostalCode = ValidatePostalCode(postalCode);
            Street = street?.Trim() ?? throw new ArgumentException("Rua é obrigatória", nameof(street));
            Number = number?.Trim() ?? throw new ArgumentException("Número é obrigatório", nameof(number));
            City = city?.Trim() ?? throw new ArgumentException("Cidade é obrigatória", nameof(city));
            State = state?.Trim() ?? throw new ArgumentException("Estado/Região é obrigatório", nameof(state));
            Complement = complement?.Trim();
            CountyOrRegion = countyOrRegion?.Trim();
        }

        private string? ValidatePostalCode(string? postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
                return null;

            if (Country == null)
                return postalCode;

            return Country.Code switch
            {
                "BR" => ValidateBrazilianCEP(postalCode),
                "US" => ValidateUSZIP(postalCode),
                "CA" => ValidateCanadianPostalCode(postalCode),
                "PT" => ValidatePortuguesePostalCode(postalCode),
                "ES" => ValidateSpanishPostalCode(postalCode),
                "MX" => ValidateMexicanPostalCode(postalCode),
                "AR" => ValidateArgentinianPostalCode(postalCode),
                "CL" => ValidateChileanPostalCode(postalCode),
                "CO" => ValidateColombianPostalCode(postalCode),
                "GB" => ValidateBritishPostalCode(postalCode),
                "FR" => ValidateFrenchPostalCode(postalCode),
                "DE" => ValidateGermanPostalCode(postalCode),
                "IT" => ValidateItalianPostalCode(postalCode),
                "AU" => ValidateAustralianPostalCode(postalCode),
                "JP" => ValidateJapanesePostalCode(postalCode),
                _ => SanitizePostalCode(postalCode)
            };
        }

        private string ValidateBrazilianCEP(string cep)
        {
            var cleaned = Regex.Replace(cep, @"\D", "");
            if (cleaned.Length != 8)
                throw new ArgumentException("CEP brasileiro deve ter 8 dígitos (formato: 12345-678)", nameof(cep));
            return cleaned;
        }

        private string ValidateUSZIP(string zip)
        {
            var cleaned = Regex.Replace(zip, @"\D", "");
            if (cleaned.Length != 5 && cleaned.Length != 9)
                throw new ArgumentException("ZIP code americano deve ter 5 ou 9 dígitos (formato: 12345 ou 12345-6789)", nameof(zip));
            return cleaned;
        }

        private string ValidateCanadianPostalCode(string postal)
        {
            if (!Regex.IsMatch(postal, @"^[A-Z]\d[A-Z]\s?\d[A-Z]\d$", RegexOptions.IgnoreCase))
                throw new ArgumentException("Código postal canadense inválido (formato: A1A 1A1)", nameof(postal));
            return postal.ToUpper().Replace(" ", "");
        }

        private string ValidatePortuguesePostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 4)
                throw new ArgumentException("Código postal português deve ter 4 dígitos (formato: 1234-567)", nameof(postal));
            return cleaned;
        }

        private string ValidateSpanishPostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 5)
                throw new ArgumentException("Código postal espanhol deve ter 5 dígitos", nameof(postal));
            return cleaned;
        }

        private string ValidateMexicanPostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 5)
                throw new ArgumentException("Código postal mexicano deve ter 5 dígitos", nameof(postal));
            return cleaned;
        }

        private string ValidateArgentinianPostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length < 4)
                throw new ArgumentException("Código postal argentino deve ter no mínimo 4 dígitos", nameof(postal));
            return cleaned;
        }

        private string ValidateChileanPostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 7)
                throw new ArgumentException("Código postal chileno deve ter 7 dígitos", nameof(postal));
            return cleaned;
        }

        private string ValidateColombianPostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 6)
                throw new ArgumentException("Código postal colombiano deve ter 6 dígitos", nameof(postal));
            return cleaned;
        }

        private string ValidateBritishPostalCode(string postal)
        {
            if (string.IsNullOrWhiteSpace(postal) || postal.Length < 6 || postal.Length > 8)
                throw new ArgumentException("Código postal britânico inválido", nameof(postal));
            return postal.ToUpper();
        }

        private string ValidateFrenchPostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 5)
                throw new ArgumentException("Código postal francês deve ter 5 dígitos", nameof(postal));
            return cleaned;
        }

        private string ValidateGermanPostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 5)
                throw new ArgumentException("Código postal alemão deve ter 5 dígitos", nameof(postal));
            return cleaned;
        }

        private string ValidateItalianPostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 5)
                throw new ArgumentException("Código postal italiano deve ter 5 dígitos", nameof(postal));
            return cleaned;
        }

        private string ValidateAustralianPostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 4)
                throw new ArgumentException("Código postal australiano deve ter 4 dígitos", nameof(postal));
            return cleaned;
        }

        private string ValidateJapanesePostalCode(string postal)
        {
            var cleaned = Regex.Replace(postal, @"\D", "");
            if (cleaned.Length != 7)
                throw new ArgumentException("Código postal japonês deve ter 7 dígitos", nameof(postal));
            return cleaned;
        }

        private string SanitizePostalCode(string postalCode)
        {
            return Regex.Replace(postalCode, @"\s+", "").Trim();
        }

        public void SetPostalCode(string? postalCode) => PostalCode = ValidatePostalCode(postalCode);
        public void SetStreet(string? street) => Street = street?.Trim();
        public void SetNumber(string? number) => Number = number?.Trim();
        public void SetCity(string? city) => City = city?.Trim();
        public void SetState(string? state) => State = state?.Trim();
        public void SetComplement(string? complement) => Complement = complement?.Trim();
        public void SetCountyOrRegion(string? county) => CountyOrRegion = county?.Trim();

        public override bool Equals(object? obj)
        {
            if (obj is not Address address)
                return false;

            if (Country == null || address.Country == null)
                return Country == address.Country;

            return Country.Code == address.Country.Code &&
                   PostalCode == address.PostalCode &&
                   Street == address.Street &&
                   Number == address.Number;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Country?.Code ?? "", PostalCode ?? "", Street ?? "", Number ?? "");
        }

        public string GetFormattedAddress()
        {
            if (Country == null || string.IsNullOrWhiteSpace(Street))
                return string.Empty;

            var address = $"{Street}, {Number}";
            if (!string.IsNullOrWhiteSpace(Complement))
                address += $" - {Complement}";
            address += $"\n{City}, {State} {PostalCode}";
            if (!string.IsNullOrWhiteSpace(CountyOrRegion))
                address += $"\n{CountyOrRegion}";
            address += $"\n{Country.Name}";
            return address;
        }
    }
}


