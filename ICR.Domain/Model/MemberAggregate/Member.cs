using ICR.Domain.Model.FamilyAggregate;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICR.Domain.Model.MemberAggregate
{
    [Table("members")]
    public class Member : BasicModel
    {
        public long Id { get; set; }

        public long FamilyId { get; private set; }
        public Family? Family { get; private set; }

        public string Name { get; set; } = null!;
        public bool HasBeenMarried { get; private set; }
        public GenderType Gender { get; private set; }
        public MemberRole Role { get; private set; }
        public DateTime BirthDate { get; private set; }
        public string? CellPhone { get; private set; }
        public ClassType Class { get; private set; }

        protected Member() { } // EF

        // criação: classe automática
        public Member(
            long id,
            long familyId,
            string name,
            GenderType gender,
            DateTime birthDate,
            bool hasBeenMarried,
            MemberRole role,
            string? cellPhone,
            ClassType classes 
        )
        {
            Id = id;
            FamilyId = familyId;
            SetName(name);
            Gender = gender;
            BirthDate = birthDate;
            HasBeenMarried = hasBeenMarried;
            Role = role;
            CellPhone = cellPhone;
            Class = classes;

        }
       
        
        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty");
            Name = name;
        }

        public void SetFamily(long familyId)
        {
            FamilyId = familyId;
        }

        public void SetGender(GenderType gender)
        {
            Gender = gender;
        }

        public void SetBirthDate(DateTime date)
        {
            BirthDate = date;
        }

        public void MarkAsMarried()
        {
            HasBeenMarried = true;
        }

        public void SetRole(MemberRole role)
        {
            Role = role;
        }

        public void SetCellPhone(string? phone)
        {
            CellPhone = phone;
        }

        public void SetClass(ClassType manualClass)
        {
            Class = manualClass;
        }

        // regra central de classificação
    }

    public enum GenderType
    {
        HOMEM = 1,
        MULHER = 2
    }

    public enum ClassType
    {
        BEBE = 0,
        CRIANCA = 1,
        JUNIORES = 2,
        JUVENIS = 3,
        JOVENS = 4,
        HOMENS = 5,
        MULHERES = 6,
    }

    public enum MemberRole
    {
        Pastor = 1,
        Presbitero = 2,
        Diacono = 3,
        Obreiro = 4,
        Midias = 5,
        Louvor = 6,
        Som_Projecao = 7,
        Secretaria_Integracao = 8,
        Ensino = 9,
        Evangelizacao_Social = 10,
        Familias = 11,
        Outros = 12
    }

}
