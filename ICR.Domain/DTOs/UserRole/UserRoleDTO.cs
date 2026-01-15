using ICR.Domain.Model.UserRoleAgreggate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.DTOs.UserRole
{
    public class UserRoleDTO
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }


    }
    public class UserRoleResponseDTO
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public string ResultMessage { get; set; }

    }
}
