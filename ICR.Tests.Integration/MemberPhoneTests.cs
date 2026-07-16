using ICR.Domain.Model;
using ICR.Domain.Model.MemberAggregate;
using Xunit;

namespace ICR.Tests.Integration
{
    public class MemberPhoneTests
    {
        [Fact]
        public void CanCreateMemberWithEmptyPhone()
        {
            // Arrange
            var familyId = 1L;
            var name = "Test Member";
            var gender = GenderType.HOMEM;
            var birthDate = new DateOnly(1990, 1, 1);
            var hasBeenMarried = false;
            var role = MemberRole.N_A;
            Phone? emptyPhone = new Phone(null, null);  // Telefone vazio
            var classType = ClassType.HOMENS;

            // Act
            var member = new Member(
                familyId,
                name,
                gender,
                birthDate,
                hasBeenMarried,
                role,
                emptyPhone,
                classType
            );

            // Assert
            Assert.NotNull(member);
            Assert.Equal(name, member.Name);
            Assert.NotNull(member.CellPhone);
        }

        [Fact]
        public void CanCreateMemberWithNullPhone()
        {
            // Arrange
            var familyId = 1L;
            var name = "Test Member";
            var gender = GenderType.HOMEM;
            var birthDate = new DateOnly(1990, 1, 1);
            var hasBeenMarried = false;
            var role = MemberRole.N_A;
            Phone? nullPhone = null;  // Telefone nulo
            var classType = ClassType.HOMENS;

            // Act
            var member = new Member(
                familyId,
                name,
                gender,
                birthDate,
                hasBeenMarried,
                role,
                nullPhone,
                classType
            );

            // Assert
            Assert.NotNull(member);
            Assert.Equal(name, member.Name);
            Assert.Null(member.CellPhone);
        }
    }
}
