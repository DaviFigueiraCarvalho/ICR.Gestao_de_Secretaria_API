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
            string? cellPhone = null
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

            CalculateClass();
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
            CalculateClass();
        }

        public void SetBirthDate(DateTime date)
        {
            BirthDate = date;
            CalculateClass();
        }

        public void MarkAsMarried()
        {
            HasBeenMarried = true;
            CalculateClass();
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
        private void CalculateClass()
        {
            int age = CalculateAge(BirthDate);

            if (age <= 2)
            {
                Class = ClassType.BEBE;
                return;
            }

            if (age >= 3 && age < 7)
            {
                Class = ClassType.CRIANCA;
                return;
            }

            if (age >= 7 && age < 11)
            {
                Class = ClassType.JUNIORES;
                return;
            }

            if (age >= 11 && age < 15)
            {
                Class = ClassType.JUVENIS;
                return;
            }

            // >= 15
            if (HasBeenMarried)
            {
                Class = Gender == GenderType.HOMEM
                    ? ClassType.HOMENS
                    : ClassType.MULHERES;
            }
            else
            {
                Class = ClassType.JOVENS;
            }
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }
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
