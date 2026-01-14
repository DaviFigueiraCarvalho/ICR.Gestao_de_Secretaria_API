using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.DTOs
{
    public class RepassDTO
    {
        public long ChurchId { get; set; }
        public long Reference { get; set; }
        public decimal Amount { get; set; }

    }
    public class RepassResponseDTO
    {
        public long Id { get; set; }
        public long ChurchId { get; set; }
        public string ChurchName { get; set; }
        public long Reference { get; set; }
        public decimal Amount { get; set; }
        public string ResultMessage { get; set; }

    }
    public class RepassUpdateDTO
    {
        public long? ChurchId { get; set; }
        public long? Reference { get; set; }
        public decimal? Amount { get; set; }

    }
}
