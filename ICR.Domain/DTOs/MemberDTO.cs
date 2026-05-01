using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.MemberAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICR.Domain.DTOs
{
    public class MemberDTO
    {
        public long FamilyId { get; set; }
        public string Name { get; set; } = null!;
        public GenderType Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public bool HasBeenMarried { get; set; }
        public MemberRole Role { get; set; }
        public string? CellPhone { get; set; }
    }

    public class MemberResponseDTO
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
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
        public long FamilyId { get; set; }
        public string FamilyName { get; set; }
        public string FamilyChurchName  { get; set; }
        public string FamilyCellName { get; set; }
        public DateTime BirthDate { get; set; }
        public bool HasBeenMarried { get; set; }
        public string? SpouseName { get; set; }
        public DateTime? WeddingDate { get; set; }
        public GenderType Gender { get; set; }
        public string GenderName => Gender switch
        {
            GenderType.HOMEM => "Homem",
            GenderType.MULHER => "Mulher",
            _ => "Não Informado ou Invalido"
        };
        public ClassType Class { get; set; }
        public string ClassName => Class switch
        {
            ClassType.BEBE => "Bebê",
            ClassType.CRIANCA => "Criança",
            ClassType.JUNIORES => "Juniores",
            ClassType.JUVENIS => "Juvenis",
            ClassType.JOVENS => "Jovens",
            ClassType.HOMENS => "Homens",
            ClassType.MULHERES => "Mulheres",
            _ => "Desconhecido"
        };

        public string? CellPhone { get; set; }
    }
    public class MemberPatchDTO
    {
        public long? FamilyId { get; set; }
        public string? Name { get; set; } = null!;
        public GenderType? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool? HasBeenMarried { get;  set; }
        public MemberRole? Role { get; set; }
        public string? CellPhone { get; set; }
        public ClassType? Class { get; set; }

    }
}
