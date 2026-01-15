using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace ICR.Domain.Model.RepassAggregate
{
    [Table("reference")]
    public class Reference : BasicModel
    {
        public long Id { get; set; }

        public string Name { get; private set; }          // ex: JAN.26
        public DateTime CompetenceDate { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected Reference() { }

        public Reference(DateTime competenceDate)
        {
            CompetenceDate = competenceDate;
            CreatedAt = DateTime.UtcNow;

            // Nome no padrão JAN.26
            Name = competenceDate
                .ToString("MMMyy", new System.Globalization.CultureInfo("pt-BR"))
                .ToUpper();
        }
    }
}
