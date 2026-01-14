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
        [ForeignKey("ChurchId")]
        public long ChurchId { get; private set; }
        public Church? Church { get; private set; }
        [ForeignKey("ResponsibleId")]
        public long? ResponsibleId { get; private set; }
        public Member? Responsible { get; private set; }
        protected Cell() { }
        public Cell(long id, string name, long churchId, long? responsibleId)
        {
            Id = id;
            Name = name;
            ChurchId = churchId;
            ResponsibleId = responsibleId;

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
