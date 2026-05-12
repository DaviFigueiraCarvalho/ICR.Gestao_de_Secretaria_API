using PhoneNumbers;
using System;

namespace ICR.Domain.Model
{
    public class Phone
    {
        private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

        public Country Country { get; private set; }
        public string Number { get; private set; }              // Apenas dígitos: "11999999999"
        public string DisplayFormat { get; private set; }       // Formato visual: "(11) 99999-9999"
        public string InternationalFormat { get; private set; } // Formato internacional: "+55 11 99999-9999"
        public string E164Format { get; private set; }          // Formato E.164: "+5511999999999"

        protected Phone() { }

        public Phone(string countryCode, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ArgumentException("Código do país é obrigatório", nameof(countryCode));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Número de telefone é obrigatório", nameof(phoneNumber));

            Country = Country.GetByCode(countryCode);
            ValidateAndParsePhone(phoneNumber);
        }

        private void ValidateAndParsePhone(string phoneNumber)
        {
            try
            {
                // Parse do número de telefone
                var parsed = PhoneUtil.Parse(phoneNumber, Country.Code);

                // Valida se é um número válido para o país
                if (!PhoneUtil.IsValidNumberForRegion(parsed, Country.Code))
                    throw new ArgumentException(
                        $"Número de telefone inválido para {Country.Name}",
                        nameof(phoneNumber));

                // Extrai apenas os dígitos
                Number = PhoneUtil.Format(parsed, PhoneNumberFormat.E164).Replace("+", "").Replace(" ", "-").Trim('-');

                // Gera os diferentes formatos
                DisplayFormat = PhoneUtil.Format(parsed, PhoneNumberFormat.NATIONAL);
                InternationalFormat = PhoneUtil.Format(parsed, PhoneNumberFormat.INTERNATIONAL);
                E164Format = PhoneUtil.Format(parsed, PhoneNumberFormat.E164);
            }
            catch (NumberParseException ex)
            {
                throw new ArgumentException(
                    $"Erro ao validar telefone para {Country.Name}: {ex.Message}",
                    nameof(phoneNumber),
                    ex);
            }
        }

        /// <summary>
        /// Verifica se um número é do tipo celular
        /// </summary>
        public bool IsMobileNumber()
        {
            try
            {
                var parsed = PhoneUtil.Parse(E164Format, Country.Code);
                var type = PhoneUtil.GetNumberType(parsed);
                return type == PhoneNumberType.MOBILE || type == PhoneNumberType.FIXED_LINE_OR_MOBILE;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida se um telefone é em potencial válido mesmo que não reconheça o padrão exato
        /// </summary>
        public bool IsPossible()
        {
            try
            {
                var parsed = PhoneUtil.Parse(E164Format, Country.Code);
                return PhoneUtil.IsPossibleNumber(parsed);
            }
            catch
            {
                return false;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Phone phone &&
                   Country.Code == phone.Country.Code &&
                   E164Format == phone.E164Format;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Country.Code, E164Format);
        }

        public override string ToString()
        {
            return DisplayFormat;
        }
    }
}
