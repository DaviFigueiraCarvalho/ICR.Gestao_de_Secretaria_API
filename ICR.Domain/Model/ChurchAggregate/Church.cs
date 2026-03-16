using ICR.Domain.Model.FederationAggregate;
using ICR.Domain.Model.MinisterAggregate;
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
        public Federation? Federation { get; set; }

        [ForeignKey("MinisterId")]
        public long? MinisterId { get; set; }
        public Minister? Minister { get; set; }

        //adicionei agora
        protected Church() { }

        public Church(string name, Address address, long federationId, long? ministerId = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            FederationId = federationId;
            MinisterId = ministerId;
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
            Address = address;
        }

        public void SetFederationId(long federationId)
        {
            FederationId = federationId;
        }

        public void SetMinisterId(long? ministerId)
        {
            MinisterId = ministerId;
        }
    }
    



}
