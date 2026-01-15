using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.Model.UserRoleAgreggate
{
    [Table("user_role")]
    public class UserRole
    {
        [ForeignKey("UserId")]
        public long UserId { get; set; }
        public User User { get; set; } 
        [ForeignKey("RoleId")]
        public long RoleId { get; set; }
        public Role Role { get; set; }

        public UserRole() { }
        public UserRole(long id, long userId, long roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }

}
