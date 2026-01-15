using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static ICR.Domain.Model.UserRoleAgreggate.User;

namespace ICR.Domain.Model.UserRoleAgreggate
{
    [Table("roles")]
    public class Role:BasicModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public UserScope MinimalScope { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
    

    public Role() { }

        public Role(long id, string name,UserScope minimalScope, string? description, bool active)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MinimalScope = minimalScope;
            Description = description;
            Active = active;
        }

        public void SetName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Name cannot be empty", nameof(newName));
            Name = newName;
        }
        public void SetMinimalScope(UserScope newScope)
        {
            if (!Enum.IsDefined(typeof(UserScope), newScope))
                throw new ArgumentException("Invalid scope value", nameof(newScope));
            MinimalScope = newScope;
        }
        public void SetDescription(string newDescription) {
            Description = newDescription;
        }
        public void SetActive(bool isActive) {
            Active = isActive;
        }

    } }
