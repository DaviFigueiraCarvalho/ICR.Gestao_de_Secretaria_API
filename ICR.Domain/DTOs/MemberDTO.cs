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
        public bool HasBeenMarried { get; private set; }
        public string? Role { get; set; }
        public long? CellPhone { get; set; }
    }

    public class MemberResponseDTO
    {
        public long Id { get; set; }

        public long FamilyId { get; set; }
        public string FamilyName { get;  set; }

        public string Name { get; set; } = null!;
        public bool HasBeenMarried { get; set; }
        public GenderType Gender { get; set; }
        public string? Role { get; set; }
        public DateTime BirthDate { get; set; }
        public long? CellPhone { get; set; }
        public ClassType Class { get; set; }
        public string ResultMessage { get; set; } = null!;


    }
}
