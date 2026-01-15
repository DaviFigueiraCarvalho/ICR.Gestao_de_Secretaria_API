using ICR.Domain.Model.UserRoleAgreggate;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICR.Domain.DTOs
{

    public class AuthRequestDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }


        public class AuthResponseDTO
        {
            public long Id { get; set; }
            public string Username { get; set; } = null!;
            public User.UserScope Scope { get; set; }
            public string Token { get; set; } = null!;
        }

    }
