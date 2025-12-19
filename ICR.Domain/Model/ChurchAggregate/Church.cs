using ICRManagement.Domain.Model.FederationAggregate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.Model.ChurchAggregate
{
    [Table("church")]
    public class Church: BasicModel
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        [ForeignKey("FederationId")]
        public long FederationId { get; set; }

        [ForeignKey("PastorId")]
        public long? PastorId { get; set; }
    
        public Church(string name, Address address, long federationId, long? pastorId = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            FederationId = federationId;
            PastorId = pastorId;
        }

        public void SetName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Name cannot be empty", nameof(newName));
            Name = newName;
        }

        public void SetAddress(Address address)
        {
            foreach (var prop in typeof(Address).GetProperties())
            {
                var value = prop.GetValue(address);
                if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                {
                    throw new ArgumentException($"Address property {prop.Name} cannot be null or empty");
                }
            }
        }

        public void SetFederationId(long federationId)
        {
            FederationId = federationId;
        }

        public void SetPastorId(long? pastorId)
        {
            PastorId = pastorId;
        }
    }
    



}
