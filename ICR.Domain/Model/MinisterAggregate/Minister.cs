using ICR.Domain.Model.MemberAggregate;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICR.Domain.Model.MinisterAggregate
{
    [Table("minister")]
    public class Minister : BasicModel
    {
        [Key]
        public long Id { get; set; }

        // FKs
        [ForeignKey("MemberId")]
        public long MemberId { get; set; }
        public Member? Member { get; set; }

        // CPF do ministro
        public string Cpf { get; set; }

        public string Email { get; set; } = null!;

        // Validade da carteirinha ministerial
        public DateTime CardValidity { get; set; }

        // Datas de ordenação
        public DateTime PresbiterOrdinationDate { get; set; }
        public DateTime? MinisterOrdinationDate { get; set; }

        // Endereço ministerial (value object, imagino)
        public Address Address { get; set; } = null!;

        // Construtor padrão pro EF parar de chorar
        protected Minister() { }

        // Construtor principal
        public Minister(
            long memberId,
            string cpf,
            string email,
            DateTime cardValidity,
            DateTime presbiterOrdinationDate,
            DateTime? ministerOrdinationDate,
            Address address)
        {
            MemberId = memberId;
            Cpf = cpf;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            CardValidity = cardValidity;
            PresbiterOrdinationDate = presbiterOrdinationDate;
            MinisterOrdinationDate = ministerOrdinationDate;
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        // Métodos de ajuste 
        public void SetMemberId(long memberId)
        {
            MemberId = memberId;
        }
        public void SetCpf(string cpf)
        {
            Cpf = cpf;
        }
        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            Email = email;
        }
        public void SetCardValidity(DateTime validity)
        {
            CardValidity = validity;
        }
        public void SetPresbiterOrdinationDate (DateTime presbiterordinationdate)
        {
            PresbiterOrdinationDate = presbiterordinationdate;
        }
        public void SetMinisterOrdinationDate(DateTime? ministerordinationdate)
        {
            MinisterOrdinationDate = ministerordinationdate;
        }
        public void SetMinisterAddress(Address address)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }
    }
}
