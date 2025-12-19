using System;
using System.Collections.Generic;
using System.Text;

namespace ICR.Domain.Model
{
    public class Address
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string City { get; set; }       // Cidade
        public string State { get; set; }      // Estado
        public string ZipCode { get; set; }    // CEP
    }
}


