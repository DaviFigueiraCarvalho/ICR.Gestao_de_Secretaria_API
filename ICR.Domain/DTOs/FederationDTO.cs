using System;

namespace ICRManagement.Domain.DTOs
{
    public class FederationDTO
    { 
        public string Name { get; set; }      // Nome da comissão
        public long? MinisterId { get; set; }

    }

    public class FederationResponseDTO
    {
        public long Id { get; set; }          // Identificador único da comissão
        public string Name { get; set; }      // Nome da comissão
        public long? MinisterId { get; set; }
        public string MinisterName { get; set; }
        public string ResultMessage { get; set; }
    }
}
