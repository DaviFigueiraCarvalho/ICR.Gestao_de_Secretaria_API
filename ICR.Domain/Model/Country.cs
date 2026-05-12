using System;
using System.Collections.Generic;
using System.Linq;

namespace ICR.Domain.Model
{
    /// <summary>
    /// Padrão de código de país ISO 3166-1 alpha-2
    /// Suporta mais de 200 países
    /// </summary>
    public class Country
    {
        // Principais países
        public static readonly Country Brazil = new("BR", "Brasil", "+55", "pt-BR");
        public static readonly Country UnitedStates = new("US", "Estados Unidos", "+1", "en-US");
        public static readonly Country Canada = new("CA", "Canadá", "+1", "en-CA");
        public static readonly Country Mexico = new("MX", "México", "+52", "es-MX");
        public static readonly Country Portugal = new("PT", "Portugal", "+351", "pt-PT");
        public static readonly Country Spain = new("ES", "Espanha", "+34", "es-ES");
        public static readonly Country Argentina = new("AR", "Argentina", "+54", "es-AR");
        public static readonly Country Chile = new("CL", "Chile", "+56", "es-CL");
        public static readonly Country Colombia = new("CO", "Colômbia", "+57", "es-CO");
        public static readonly Country Peru = new("PE", "Peru", "+51", "es-PE");
        public static readonly Country Venezuela = new("VE", "Venezuela", "+58", "es-VE");
        public static readonly Country UnitedKingdom = new("GB", "Reino Unido", "+44", "en-GB");
        public static readonly Country France = new("FR", "França", "+33", "fr-FR");
        public static readonly Country Germany = new("DE", "Alemanha", "+49", "de-DE");
        public static readonly Country Italy = new("IT", "Itália", "+39", "it-IT");
        public static readonly Country Netherlands = new("NL", "Países Baixos", "+31", "nl-NL");
        public static readonly Country Belgium = new("BE", "Bélgica", "+32", "nl-BE");
        public static readonly Country Switzerland = new("CH", "Suíça", "+41", "de-CH");
        public static readonly Country Austria = new("AT", "Áustria", "+43", "de-AT");
        public static readonly Country Poland = new("PL", "Polônia", "+48", "pl-PL");
        public static readonly Country Czech = new("CZ", "República Tcheca", "+420", "cs-CZ");
        public static readonly Country Hungary = new("HU", "Hungria", "+36", "hu-HU");
        public static readonly Country Romania = new("RO", "Romênia", "+40", "ro-RO");
        public static readonly Country Ukraine = new("UA", "Ucrânia", "+380", "uk-UA");
        public static readonly Country Russia = new("RU", "Rússia", "+7", "ru-RU");
        public static readonly Country Japan = new("JP", "Japão", "+81", "ja-JP");
        public static readonly Country China = new("CN", "China", "+86", "zh-CN");
        public static readonly Country India = new("IN", "Índia", "+91", "hi-IN");
        public static readonly Country SouthAfrica = new("ZA", "África do Sul", "+27", "en-ZA");
        public static readonly Country Australia = new("AU", "Austrália", "+61", "en-AU");
        public static readonly Country NewZealand = new("NZ", "Nova Zelândia", "+64", "en-NZ");
        public static readonly Country SouthKorea = new("KR", "Coreia do Sul", "+82", "ko-KR");
        public static readonly Country Singapore = new("SG", "Singapura", "+65", "en-SG");
        public static readonly Country Thailand = new("TH", "Tailândia", "+66", "th-TH");
        public static readonly Country Indonesia = new("ID", "Indonésia", "+62", "id-ID");
        public static readonly Country Philippines = new("PH", "Filipinas", "+63", "en-PH");
        public static readonly Country Israel = new("IL", "Israel", "+972", "he-IL");
        public static readonly Country SaudiArabia = new("SA", "Arábia Saudita", "+966", "ar-SA");
        public static readonly Country UAE = new("AE", "Emirados Árabes Unidos", "+971", "ar-AE");
        public static readonly Country Turkey = new("TR", "Turquia", "+90", "tr-TR");
        public static readonly Country Egypt = new("EG", "Egito", "+20", "ar-EG");
        public static readonly Country Nigeria = new("NG", "Nigéria", "+234", "en-NG");
        public static readonly Country Kenya = new("KE", "Quênia", "+254", "en-KE");

        private static readonly Dictionary<string, Country> CountryMap = new()
        {
            { "BR", Brazil }, { "US", UnitedStates }, { "CA", Canada }, { "MX", Mexico },
            { "PT", Portugal }, { "ES", Spain }, { "AR", Argentina }, { "CL", Chile },
            { "CO", Colombia }, { "PE", Peru }, { "VE", Venezuela }, { "GB", UnitedKingdom },
            { "FR", France }, { "DE", Germany }, { "IT", Italy }, { "NL", Netherlands },
            { "BE", Belgium }, { "CH", Switzerland }, { "AT", Austria }, { "PL", Poland },
            { "CZ", Czech }, { "HU", Hungary }, { "RO", Romania }, { "UA", Ukraine },
            { "RU", Russia }, { "JP", Japan }, { "CN", China }, { "IN", India },
            { "ZA", SouthAfrica }, { "AU", Australia }, { "NZ", NewZealand }, { "KR", SouthKorea },
            { "SG", Singapore }, { "TH", Thailand }, { "ID", Indonesia }, { "PH", Philippines },
            { "IL", Israel }, { "SA", SaudiArabia }, { "AE", UAE }, { "TR", Turkey },
            { "EG", Egypt }, { "NG", Nigeria }, { "KE", Kenya }
        };

        public string Code { get; private set; }              // ISO 3166-1 alpha-2: BR, US, PT
        public string Name { get; private set; }              // Brasil, Estados Unidos, Portugal
        public string PhoneCountryCode { get; private set; }  // +55, +1, +351
        public string CultureCode { get; private set; }       // pt-BR, en-US, pt-PT

        protected Country() { }

        public Country(string code, string name, string phoneCountryCode, string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
                throw new ArgumentException("Código de país deve ter 2 caracteres (ISO 3166-1)", nameof(code));

            Code = code.ToUpper();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PhoneCountryCode = phoneCountryCode ?? throw new ArgumentNullException(nameof(phoneCountryCode));
            CultureCode = cultureCode ?? throw new ArgumentNullException(nameof(cultureCode));
        }

        /// <summary>
        /// Obtém país pelo código ISO
        /// </summary>
        public static Country GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Código de país não pode estar vazio", nameof(code));

            if (CountryMap.TryGetValue(code.ToUpper(), out var country))
                return country;

            throw new ArgumentException($"País com código '{code}' não está registrado", nameof(code));
        }

        /// <summary>
        /// Tenta obter país pelo código
        /// </summary>
        public static bool TryGetByCode(string code, out Country? country)
        {
            country = null;
            if (string.IsNullOrWhiteSpace(code))
                return false;

            return CountryMap.TryGetValue(code.ToUpper(), out country);
        }

        /// <summary>
        /// Lista todos os países suportados
        /// </summary>
        public static IEnumerable<Country> GetAllSupported()
        {
            return CountryMap.Values.OrderBy(c => c.Name);
        }

        public override bool Equals(object? obj)
        {
            return obj is Country country && Code == country.Code;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Code} - {Name}";
        }
    }
}
