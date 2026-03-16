using System;
using System.Collections.Generic;
using System.Text;
namespace ICR.Domain.Model
{
    public class Address
    {
        public string ZipCode { get; set; }    // CEP
        public string Street { get; set; }
        public string Number { get; set; }
        public string City { get; set; }       // Cidade
        public string State { get; set; }      // Estado
    

    protected Address() { }
        public Address(string zipCode, string street, string number, string city, string state )
        {
            ZipCode = zipCode;
            Street = street ;
            Number = number;
            City = city ;
            State = state;
        }
        public void SetZipCode(string zipCode) => ZipCode = zipCode;
        public void SetStreet(string street) => Street = street;
        public void SetNumber(string number) => Number = number;
        public void SetCity(string city) => City = city;
        public void SetState(string state) => State = state;
    }
}


