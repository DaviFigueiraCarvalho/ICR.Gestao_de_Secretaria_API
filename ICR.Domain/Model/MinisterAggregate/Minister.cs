using ICR.Domain.Model.MemberAggregate;
using ICR.Domain.Model;
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
        public string Cpf { get; set; } = null!;

        public string Email { get; set; } = null!;

        // Validade da carteirinha ministerial
        public DateOnly CardValidity { get; set; }

        // Datas de ordenação
        public DateOnly PresbiterOrdinationDate { get; set; }
        public DateOnly? MinisterOrdinationDate { get; set; }
        public bool Insurance { get; private set; }

        // Endereço ministerial (opcional no Minister)
        public Address? Address { get; set; }

        // Construtor padrão pro EF parar de chorar
        protected Minister() { }

        // Construtor principal
        public Minister(
            long memberId,
            string cpf,
            string email,
            DateOnly cardValidity,
            DateOnly presbiterOrdinationDate,
            DateOnly? ministerOrdinationDate,
            Address? address,
            bool insurance)
        {
            MemberId = memberId;
            Cpf = cpf ?? throw new ArgumentNullException(nameof(cpf));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            CardValidity = cardValidity;
            PresbiterOrdinationDate = presbiterOrdinationDate;
            MinisterOrdinationDate = ministerOrdinationDate;
            Address = address;
            Insurance = insurance;
        }

        // Métodos de ajuste 
        public void SetMemberId(long memberId)
        {
            MemberId = memberId;
        }

        public void SetCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                throw new ArgumentException("CPF cannot be empty", nameof(cpf));
            Cpf = cpf;
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            Email = email;
        }

        public void SetCardValidity(DateOnly validity)
        {
            CardValidity = validity;
        }

        public void SetPresbiterOrdinationDate(DateOnly presbiterordinationdate)
        {
            PresbiterOrdinationDate = presbiterordinationdate;
        }

        public void SetMinisterOrdinationDate(DateOnly? ministerordinationdate)
        {
            MinisterOrdinationDate = ministerordinationdate;
        }

        public void SetMinisterAddress(Address? address)
        {
            Address = address;
        }

        public void SetInsurance(bool insurance)
        {
            Insurance = insurance;
        }
    }
}
