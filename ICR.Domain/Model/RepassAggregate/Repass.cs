using ICR.Domain.Model.ChurchAggregate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.Model.RepassAggregate
{
    [Table("repass")]
    public class Repass
    {
        public long Id { get; set; }
        [ForeignKey("ChurchId")]
        public long ChurchId { get; set; }
        public Church? Church { get; set; }
        public long Reference { get; set; }
        public decimal Amount { get; set; }
        public Repass() { }

        public Repass(long id, long churchId, long reference, decimal amount)
        {
            Id = id;
            ChurchId = churchId;
            Reference = reference;
            Amount = amount;
        }
        public void SetChurchId(long churchId)
        {
            ChurchId = churchId;
        }
        public void SetReference(long reference)
        {
            Reference = reference;
        }
        public void SetAmount(decimal amount)
        {
            Amount = amount;
        }

        }
}
