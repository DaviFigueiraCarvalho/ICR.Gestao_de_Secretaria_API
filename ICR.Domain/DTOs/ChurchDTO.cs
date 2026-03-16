using ICR.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static AddressDTO;

namespace ICR.Domain.DTOs
{
    public class ChurchDTO
    {
        public string Name { get; set; }
        public Address Address { get; set; }
        public long FederationId { get; set; }
        public long? MinisterId { get; set; }

    }

    public class ChurchResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public AddressDTO Address { get; set; }
        public long? FederationId { get; set; }
        public string FederationName { get; set; }
        public long? MinisterId { get; set; }
        public string MinisterName { get; set; }
    }
    public class ChurchPatchDTO
    {
        public string? Name { get; set; }
        public AddressPatchDTO? Address { get; set; }
        public long? FederationId { get; set; }
        public long? MinisterId { get; set; }
        public string? FailureMessage { get; set; }

    }
}
