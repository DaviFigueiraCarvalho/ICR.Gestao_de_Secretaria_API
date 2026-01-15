using ICR.Domain.Model.MemberAggregate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ICR.Domain.Model.UserRoleAgreggate
{
    [Table("users")]
    public class User: BasicModel
    {
        public long Id { get; set; }
        // FK para Member
        public long MemberId { get; set; }
        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }
        public string Username { get; set; } = null!;
        // Hash da senha, nunca texto puro
        public string PasswordHash { get; set; } = null!;
        // Escopo macro: CHURCH, FEDERATION, NATIONAL
        public UserScope Scope { get; set; }


        public User() { }

        public User(long memberId, string username, string passwordHash, UserScope scope)
        {
            MemberId = memberId;
            Username = username;
            PasswordHash = passwordHash;
            Scope = scope;
        }

        public enum UserScope
        {
            CHURCH = 0,
            FEDERATION = 1,
            NATIONAL = 2
        }
        public void SetUsername(string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
                throw new ArgumentException("Username cannot be empty", nameof(newUsername));
            Username = newUsername;
        }
        public void SetPasswordHash(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("PasswordHash cannot be empty", nameof(newPasswordHash));
            PasswordHash = newPasswordHash;
        }
        public void SetScope(UserScope newScope)
        {
            if (!Enum.IsDefined(typeof(UserScope), newScope))
                throw new ArgumentException("Invalid scope value", nameof(newScope));
            Scope = newScope;
        }




    }
}
