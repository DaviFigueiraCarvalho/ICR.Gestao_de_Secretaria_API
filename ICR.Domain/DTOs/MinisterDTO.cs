using ICR.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static AddressDTO;

namespace ICR.Domain.DTOs
{
    public class MinisterRegistrationPendingDTO
    {
        public long MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string ChurchName { get; set; } = string.Empty;
        public int MemberRole { get; set; }
        public string MemberRoleName { get; set; } = string.Empty;
        public PhoneResponseDTO? Phone { get; set; }
    }

    public class MinisterDTO
    {
        public long MemberId { get; set; }
        public string Cpf { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateOnly CardValidity { get; set; }
        public DateOnly PresbiterOrdinationDate { get; set; }
        public DateOnly? MinisterOrdinationDate { get; set; }
        public AddressDTO? Address { get; set; }
    }

    public class MinisterResponseDTO
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public string MemberName { get; set; } = null!;
        public string ChurchMemberName { get; set; } = null!;
        public string FederationMemberName { get; set; } = null!;
        public DateOnly MemberBirthday { get; set; }
        public PhoneResponseDTO? MemberPhone { get; set; }
        public string MemberWifeName { get; set; } = null!;
        public DateOnly MemberWeddingDate { get; set; }
        public string Cpf { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool Insurance { get; set; }
        public DateOnly CardValidity { get; set; }
        public DateOnly PresbiterOrdinationDate { get; set; }
        public DateOnly? MinisterOrdinationDate { get; set; }
        public AddressDTO? Address { get; set; }
    }

    public class MinisterPatchDTO
    {
        public long? MemberId { get; set; }
        public string? Cpf { get; set; }
        public string? Email { get; set; }
        public bool? Insurance { get; set; }
        public DateOnly? CardValidity { get; set; }
        public DateOnly? PresbiterOrdinationDate { get; set; }
        public DateOnly? MinisterOrdinationDate { get; set; }
        public AddressPatchDTO? Address { get; set; }
    }

    public class MinisterBirthdayDTO
    {
        public string Name { get; set; } = null!;
        public string ChurchName { get; set; } = string.Empty;
        public string Type { get; set; } = null!;
        public PhoneResponseDTO? Phone { get; set; }
        public string? MemberWifeName { get; set; }
        public DateOnly Birthday { get; set; }
    }

    public class MinisterInsuredListDTO
    {
        public string FullName { get; set; } = null!;
        public DateOnly BirthDate { get; set; }
        public string Cpf { get; set; } = null!;
        public string Email { get; set; } = null!;
        public PhoneResponseDTO? Phone { get; set; }
        public bool Insurance { get; set; }
    }
}
