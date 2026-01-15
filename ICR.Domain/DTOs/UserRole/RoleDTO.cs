using System;
using System.Collections.Generic;
using System.Text;
using static ICR.Domain.Model.UserRoleAgreggate.User;

namespace ICR.Domain.DTOs.UserRole
{
    public class RoleDTO
    {
        public string Name { get; set; }
        public UserScope MinimalScope { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }

    }
    public class RoleResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public UserScope MinimalScope { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public string ResultMessage { get; set; }

    }

    public class RolePatchDTO
    {
        public string? Name { get; set; }
        public UserScope? MinimalScope { get; set; }
        public string? Description { get; set; }
        public bool? Active { get; set; }
    }
}
