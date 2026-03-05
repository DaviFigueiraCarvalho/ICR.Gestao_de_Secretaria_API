using ICR.Domain.Model;
using ICR.Domain.Model.MinisterAggregate;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICR.Domain.Model.FederationAggregate
{
    [Table("federation")]
    public class Federation : BasicModel
    {
        [Key]
        public long Id { get; set; } // Identificador único da comissão

        public string Name { get; set; } // Nome da comissão federada
        [ForeignKey("MinisterId")]
        public long? MinisterId { get; set; } //Id do pastor responsável pela comissão federada
        public Minister? Minister { get; set; } // Pastor responsável pela comissão federada
        // Construtor principal
        public Federation(string name, long? ministerId)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MinisterId= ministerId;
        }
        public void SetName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Name cannot be empty", nameof(newName));

            Name = newName;
        }
        public void SetMinisterId(long? ministerId)
        {
            MinisterId = ministerId;
        }
    }
}
