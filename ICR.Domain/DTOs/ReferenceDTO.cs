using System;

namespace ICR.Domain.DTOs
{
    public class ReferenceDTO
    {
        public DateTime CompetenceDate { get; set; }
    }

    public class ReferenceResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime CompetenceDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ResultMessage { get; set; }
    }
}
