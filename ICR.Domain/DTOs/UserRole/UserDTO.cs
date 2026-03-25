using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static ICR.Domain.Model.UserRoleAgreggate.User;

namespace ICR.Domain.DTOs.UserRole
{
    public class UserDTO
    {
        public long? MemberId { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public UserScope Scope { get; set; }

    }
    public class UserResponseDTO
    {
        public long Id { get; set; }
        public long? MemberId { get; set; }
        public string MemberName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public UserScope Scope { get; set; }
        // ResultMessage removed — use exceptions / ProblemDetails for errors

    }
    public class UserPatchDTO
    {
        public long? MemberId { get; set; }
        public string? Username { get; set; } = null!;
        public string? PasswordHash { get; set; } = null!;
        public UserScope? Scope { get; set; }

    }
}
