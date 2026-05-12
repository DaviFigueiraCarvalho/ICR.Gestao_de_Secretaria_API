using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace ICR.Domain.Model.RepassAggregate
{
    [Table("reference")]
    public class Reference : BasicModel
    {
        public long Id { get; set; }

        public string Name { get; private set; }          // ex: ABR-26
        public DateTime CompetenceDate { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected Reference() { }

        public Reference(DateTime competenceDate)
        {
            CompetenceDate = competenceDate;
            CreatedAt = DateTime.UtcNow;

            var referenceDate = competenceDate.AddMonths(-1);

            Name = referenceDate
                .ToString("MMMyy", new CultureInfo("pt-BR"))
                .ToUpper()
                .Replace('.', '-');
        }
    }
}
