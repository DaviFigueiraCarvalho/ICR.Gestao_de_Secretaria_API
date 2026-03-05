using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.MemberAggregate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.Model.CellAggregate
{
    [Table("cells")]
    public class Cell:BasicModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public CellType Type { get; set; }
        [ForeignKey("ChurchId")]
        public long ChurchId { get; private set; }
        public Church? Church { get; private set; }
        [ForeignKey("ResponsibleId")]
        public long? ResponsibleId { get; private set; }
        public Member? Responsible { get; private set; }
        protected Cell() { }
        public Cell(string name, CellType type, long churchId, long? responsibleId )
        {
            Name = name;
            Type = type;
            ChurchId = churchId;
            ResponsibleId = responsibleId;
        }

        public enum CellType 
        {
            Celula = 0,
            ComunidadeMissionaria = 1
        }
        public void SetName(string name)
        {
            Name = name;
        }
        public void SetResponsible(long? responsibleId)
        {
            ResponsibleId = responsibleId;
        }

        public void SetChurch(long churchId)
        {
            ChurchId = churchId;
        }

    }
}
