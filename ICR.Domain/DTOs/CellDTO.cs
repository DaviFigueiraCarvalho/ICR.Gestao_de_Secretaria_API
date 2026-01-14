    using ICR.Domain.Model.ChurchAggregate;
    using ICR.Domain.Model.MemberAggregate;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    namespace ICR.Domain.DTOs
    {
        public class CellDTO
        {
            public string Name { get; private set; }
            public long ChurchId { get; private set; }
            public long? ResponsibleId { get; private set; }

        }

        public class CellResponseDTO
        {
            public long Id { get;  set; }
            public string Name { get; set; }
            public long ChurchId { get; set; }
            public string ChurchName { get; set; }
            public long? ResponsibleId { get; set; }
            public string? ResponsibleName { get; set; }
            public string? ResultMessage { get; set; }
           

        }
    }
