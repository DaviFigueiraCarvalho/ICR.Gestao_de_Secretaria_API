using ICR.Domain.Model;

public class AddressDTO
{
    public string ZipCode { get; set; }
    public string Street { get; set; }
    public string Number { get; set; }
    public string City { get; set; }
    public string State { get; set; }

    // Conversão explícita da entidade
    public static AddressDTO FromEntity(Address address)
    {
        if (address == null)
            return null;

        return new AddressDTO
        {
            ZipCode = address.ZipCode,
            Street = address.Street,
            Number = address.Number,
            City = address.City,
            State = address.State
        };
    }
    public class AddressPatchDTO
    {
        public string? ZipCode { get; set; }
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }

        // Conversão explícita da entidade
        public static AddressPatchDTO FromEntity(Address address)
        {
            if (address == null)
                return null;

            return new AddressPatchDTO
            {
                ZipCode = address.ZipCode,
                Street = address.Street,
                Number = address.Number,
                City = address.City,
                State = address.State
            };
        }
    }
}
