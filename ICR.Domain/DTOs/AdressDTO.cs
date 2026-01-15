using ICR.Domain.Model;

public class AdressDTO
{
    public string ZipCode { get; set; }
    public string Street { get; set; }
    public string Number { get; set; }
    public string City { get; set; }
    public string State { get; set; }

    // Conversão explícita da entidade
    public static AdressDTO FromEntity(Address address)
    {
        if (address == null)
            return null;

        return new AdressDTO
        {
            ZipCode = address.ZipCode,
            Street = address.Street,
            Number = address.Number,
            City = address.City,
            State = address.State
        };
    }
}
