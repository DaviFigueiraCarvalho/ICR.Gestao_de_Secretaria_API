using ICR.Domain.Model;

public class AddressDTO
{
    /// <summary>
    /// Código ISO do país (BR, US, PT, etc.)
    /// </summary>
    public string CountryCode { get; set; } = null!;

    public string PostalCode { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string Number { get; set; } = null!;
    public string? Complement { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string? CountyOrRegion { get; set; }

    public static AddressDTO? FromEntity(Address? address)
    {
        if (address == null)
            return null;

        return new AddressDTO
        {
            CountryCode = address.Country.Code,
            PostalCode = address.PostalCode,
            Street = address.Street,
            Number = address.Number,
            Complement = address.Complement,
            City = address.City,
            State = address.State,
            CountyOrRegion = address.CountyOrRegion
        };
    }

    public class AddressPatchDTO
    {
        public string? CountryCode { get; set; }
        public string? PostalCode { get; set; }
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? Complement { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? CountyOrRegion { get; set; }

        public static AddressPatchDTO? FromEntity(Address? address)
        {
            if (address == null)
                return null;

            return new AddressPatchDTO
            {
                CountryCode = address.Country.Code,
                PostalCode = address.PostalCode,
                Street = address.Street,
                Number = address.Number,
                Complement = address.Complement,
                City = address.City,
                State = address.State,
                CountyOrRegion = address.CountyOrRegion
            };
        }
    }
}
