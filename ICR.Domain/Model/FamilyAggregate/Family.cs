using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.MemberAggregate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.Model.FamilyAggregate
{
    [Table("families")]
    public class Family : BasicModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        [ForeignKey(nameof(CellId))]
        public long CellId { get; set; }
        public Cell Cell { get; set; }
        [ForeignKey(nameof(ChurchId))]
        public long ChurchId { get; set; }
        public Church Church { get; set; }

        [ForeignKey(nameof(ManId))]
        public long? ManId { get; set; }
        public Member? Man { get; set; }
        [ForeignKey(nameof(WomanId))]
        public long? WomanId { get; set; }
        public Member? Woman { get; set; }

        public DateTime? WeddingDate { get; set; }


        public Family(){ }
        public Family(long id,string name, long churchId, long cellId, long? manId, long? womanId, DateTime? weddingDate)
        {
            Name = name;
            Id = id;
            ChurchId = churchId;
            CellId = cellId;
            ManId = manId;
            WomanId = womanId;
            WeddingDate = weddingDate;
        }
        public void SetName(string name)
        {
            Name = name;
        }
        public void SetChurchId(long churchId)
        {
            ChurchId = churchId;
        }
        public void SetCellId(long cellId)
        {
            CellId = cellId;
        }
        public void SetFatherId(long? manId)
        {
            ManId = manId;
        }
        public void SetMotherId(long? womanId)
        {
            WomanId = womanId;
        }
        public void SetWeddingDate(DateTime? weddingDate)
        {
            WeddingDate = weddingDate;
        }

    }
}

