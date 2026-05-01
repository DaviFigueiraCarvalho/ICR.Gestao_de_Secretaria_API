using ICR.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static AddressDTO;

namespace ICR.Domain.DTOs
{
    public class MinisterDTO
    {
        public long MemberId { get; set; }
        public string Cpf { get; set; }
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
        public string? MemberPhone { get; set; }
        public MemberRole MemberRole { get; set; }
        public string MemberRoleName => MemberRole switch
        {
            MemberRole.N_A => "N/A",
            MemberRole.Pastor => "Pastor",
            MemberRole.Presbitero => "Presbítero",
            MemberRole.Diacono => "Diácono",
            MemberRole.Obreiro => "Obreiro",
            MemberRole.Midias => "Mídias",
            MemberRole.Louvor => "Louvor",
            MemberRole.Som_Projecao => "Som e Projeção",
            MemberRole.Secretaria_Integracao => "Secretaria e Integração",
            MemberRole.Ensino => "Ensino",
            MemberRole.Evangelizacao_Social => "Evangelização e Social",
            MemberRole.Familias => "Famílias",
            MemberRole.Outros => "Outros",
            _ => "Desconhecido"
        };
        public string MemberWifeName { get; set; } = null!;
        public DateTime MemberWeddingDate { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; } = null!;
        public bool Insurance { get; set; }
        public DateTime CardValidity { get; set; }
        public DateTime PresbiterOrdinationDate { get; set; }
        public DateTime? MinisterOrdinationDate { get; set; }
        public AddressDTO Address { get; set; } = null!;
    }

    public class MinisterPatchDTO
    {
        public long? MemberId { get; set; }
        public string? Cpf { get; set; }
        public string? Email { get; set; }
        public bool? Insurance { get; set; }
        public DateTime? CardValidity { get; set; }
        public DateTime? PresbiterOrdinationDate { get; set; }
        public DateTime? MinisterOrdinationDate { get; set; }
        public AddressPatchDTO? Address { get; set; }
    }
    public class MinisterBirthdayDTO
    {
        public string Name { get; set; } 
        public string Type { get; set; } 
        public string? Phone { get; set; }
        public string? MemberWifeName { get; set; }
        public DateTime Birthday { get; set; }
    }

    public class MinisterInsuredListDTO
    {
        public string FullName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public string Cpf { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public MemberRole Role { get; set; }
        public string RoleName => Role switch
        {
            MemberRole.N_A => "N/A",
            MemberRole.Pastor => "Pastor",
            MemberRole.Presbitero => "Presbítero",
            MemberRole.Diacono => "Diácono",
            MemberRole.Obreiro => "Obreiro",
            MemberRole.Midias => "Mídias",
            MemberRole.Louvor => "Louvor",
            MemberRole.Som_Projecao => "Som e Projeção",
            MemberRole.Secretaria_Integracao => "Secretaria e Integração",
            MemberRole.Ensino => "Ensino",
            MemberRole.Evangelizacao_Social => "Evangelização e Social",
            MemberRole.Familias => "Famílias",
            MemberRole.Outros => "Outros",
            _ => "Desconhecido"
        };
        public bool Insurance { get; set; }
    }

}
