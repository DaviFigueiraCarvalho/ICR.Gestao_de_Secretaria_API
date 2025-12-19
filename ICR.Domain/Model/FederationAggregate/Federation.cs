using ICR.Domain.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICRManagement.Domain.Model.FederationAggregate
{
    [Table("federation")]
    public class Federation : BasicModel
    {
        [Key]
        public long Id { get; set; } // Identificador único da comissão

        public string Name { get; set; } // Nome da comissão federada
        [ForeignKey("PastorId")]
        public long? PastorId { get; set; }

        // Construtor principal
        public Federation(string name, long id, long? pastorId)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PastorId = pastorId;
        }
        public void SetName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Name cannot be empty", nameof(newName));

            Name = newName;
        }
        public void SetPastorId(long? pastorId)
        {
            PastorId = pastorId;
        }
    }
}
