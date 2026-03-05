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
        public long ChurchId { get; set; }
        [ForeignKey(nameof(ChurchId))] 
        public Church? Church { get; set; }
        public long ReferenceId { get; set; }
        [ForeignKey(nameof(ReferenceId))] 
        public Reference? Reference { get; set; }
        public decimal Amount { get; set; }
        public Repass() { }

        public Repass(long churchId, long reference, decimal amount)
        {
            ChurchId = churchId;
            ReferenceId = reference;
            Amount = amount;
        }
        public void SetChurchId(long churchId)
        {
            ChurchId = churchId;
        }
        public void SetReference(long reference)
        {
            ReferenceId = reference;
        }
        public void SetAmount(decimal amount)
        {
            Amount = amount;
        }

        }
}
