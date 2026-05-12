using ICR.Domain.Model;

namespace ICR.Domain.DTOs
{
    public class PhoneDTO
    {
        /// <summary>
        /// Código ISO do país (BR, US, PT, etc.)
        /// </summary>
        public string CountryCode { get; set; } = null!;

        /// <summary>
        /// Número de telefone (com ou sem formatação)
        /// </summary>
        public string Number { get; set; } = null!;
    }

    public class PhonePatchDTO
    {
        public string? CountryCode { get; set; }
        public string? Number { get; set; }
    }

    public class PhoneResponseDTO
    {
        public string CountryCode { get; set; } = null!;
        public string CountryName { get; set; } = null!;
        public string Number { get; set; } = null!;              // Apenas dígitos
        public string DisplayFormat { get; set; } = null!;       // Formatado para exibição
        public string InternationalFormat { get; set; } = null!; // Formato internacional
        public string E164Format { get; set; } = null!;          // Formato E.164
        public bool IsMobileNumber { get; set; }

        public static PhoneResponseDTO? FromEntity(Phone? phone)
        {
            if (phone is null)
                return null;

            return new PhoneResponseDTO
            {
                CountryCode = phone.Country.Code,
                CountryName = phone.Country.Name,
                Number = phone.Number,
                DisplayFormat = phone.DisplayFormat,
                InternationalFormat = phone.InternationalFormat,
                E164Format = phone.E164Format,
                IsMobileNumber = phone.IsMobileNumber()
            };
        }
    }
}
