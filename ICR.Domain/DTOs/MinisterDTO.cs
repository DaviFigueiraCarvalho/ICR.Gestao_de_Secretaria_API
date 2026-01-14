using ICR.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.DTOs
{
    public class MinisterDTO
    {
        public long MemberId { get; set; }
        public long Cpf { get; set; }
        public string Email { get; set; } = null!;
        public DateTime CardValidity { get; set; }
        public DateTime PresbiterOrdinationDate { get; set; }
        public DateTime? MinisterOrdinationDate { get; set; }
        public Address Address { get; set; } = null!;

    }
    public class MinisterResponseDTO
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public string MemberName { get; set; } = null!;
        public string ChurchMemberName { get; set; } = null!;
        public string FederationMemberName { get; set; } = null!;
        public DateTime MemberBirthday { get; set; }
        public string MemberWifeName { get; set; } = null!;
        public DateTime MemberWeddingDate { get; set; }
        public long Cpf { get; set; }
        public string Email { get; set; } = null!;
        public DateTime CardValidity { get; set; }
        public DateTime PresbiterOrdinationDate { get; set; }
        public DateTime? MinisterOrdinationDate { get; set; }
        public Address Address { get; set; } = null!; public string ResultMessage { get; set; } = string.Empty;
    }

    public class MinisterPatchDTO
    {
        public long? MemberId { get; set; }
        public long? Cpf { get; set; }
        public string? Email { get; set; }
        public DateTime? CardValidity { get; set; }
        public DateTime? PresbiterOrdinationDate { get; set; }
        public DateTime? MinisterOrdinationDate { get; set; }
        public Address? Address { get; set; }
    }
    public class MinisterBirthdayDTO
    {
        public string Name { get; set; } 
        public string Type { get; set; } 
        public string? MemberWifeName { get; set; }
        public DateTime Birthday { get; set; }
    }

}
