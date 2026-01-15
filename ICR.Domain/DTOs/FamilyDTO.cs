using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.DTOs
{
    public class FamilyDTO
    {
        public string Name { get; set; }
        public long ChurchId { get; set; }
        public long CellId { get; set; }
        public long? ManId { get; set; }
        public long? WomanId { get; set; }
        public DateTime? WeddingDate { get; set; }

    }

    public class ResponseFamilyDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long churchId { get; set; }
        public string ChurchName { get; set; }
        public long CellId { get; set; }
        public string CellName { get; set; }
        public long? ManId { get; set; }
        public string ManName { get; set; }
        public long? WomanId { get; set; }
        public string WomanName { get; set; }
        public DateTime? WeddingDate { get; set; }
        public string ResultMessage { get; set; }
    }
    public class FamilyPatchDTO
    {
        public string? Name { get; set; }
        public long? ChurchId { get; set; }
        public long? CellId { get; set; }
        public long? ManId { get; set; }
        public long? WomanId { get; set; }
        public DateTime? WeddingDate { get; set; }

    }
}
