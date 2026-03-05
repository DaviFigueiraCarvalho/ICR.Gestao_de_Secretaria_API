    using ICR.Domain.Model.ChurchAggregate;
    using ICR.Domain.Model.MemberAggregate;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
using static ICR.Domain.Model.CellAggregate.Cell;

    namespace ICR.Domain.DTOs
    {
        public class CellDTO
        {
            public string Name { get; set; }
            public CellType Type { get; set; }
            public long ChurchId { get; set; }
            public long? ResponsibleId { get; set; }

        }

        public class CellResponseDTO
        {
            public long Id { get;  set; }
            public string Name { get; set; }
            public CellType Type { get; set; }
            public string TypeName => Type switch
            {
                CellType.Celula => "Célula",
                CellType.ComunidadeMissionaria => "Comunidade Missionária",
                _ => "Desconecido"
            };
            public long ChurchId { get; set; }
            public string ChurchName { get; set; }
            public long? ResponsibleId { get; set; }
            public string? ResponsibleName { get; set; }
            // ResultMessage removed: use HTTP ProblemDetails or exceptions for failures
           

        }
        public class CellPatchDTO
        {
            public string? Name { get; set; }
            public CellType? Type { get; set; }
            public long? ChurchId { get; set; }
            public long? ResponsibleId { get; set; }

        }
}
