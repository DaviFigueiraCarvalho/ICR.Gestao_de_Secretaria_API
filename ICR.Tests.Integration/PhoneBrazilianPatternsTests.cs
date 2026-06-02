using ICR.Domain.Model;
using Xunit;

namespace ICR.Tests.Integration
{
    public class PhoneBrazilianPatternsTests
    {
        [Theory]
        [InlineData("(11)3456-7890", "BR")]  // Telefone fixo real
        [InlineData("(11)99999-9999", "BR")] // Telefone celular
        [InlineData("11999999999", "BR")]    // Sem formatação celular
        [InlineData("1134567890", "BR")]     // Sem formatação fixo
        [InlineData("11 99999-9999", "BR")]  // Com espaço celular
        [InlineData("(11) 99999-9999", "BR")] // Com espaço e parênteses celular
        [InlineData("(21) 3333-4444", "BR")] // Rio de Janeiro fixo
        [InlineData("(85) 98888-7777", "BR")] // Ceará celular
        public void CanCreatePhoneWithBrazilianPatterns(string phoneNumber, string countryCode)
        {
            // Act
            var phone = new Phone(countryCode, phoneNumber);

            // Assert
            Assert.NotNull(phone);
            Assert.NotEmpty(phone.Number);
            Assert.NotEmpty(phone.E164Format);
            Assert.NotEmpty(phone.DisplayFormat);
        }

        [Theory]
        [InlineData("(11)3456-7890")]  // Telefone fixo
        [InlineData("(11)99999-9999")] // Telefone celular
        [InlineData("(21)2222-3333")]  // Rio - fixo
        [InlineData("(21)98888-7777")] // Rio - celular
        public void BothFixedAndMobileNumbersPatternsAreValid(string phoneNumber)
        {
            // Act
            var phone = new Phone("BR", phoneNumber);

            // Assert
            Assert.NotNull(phone);
            Assert.Equal("BR", phone.Country.Code);
            Assert.True(phone.IsPossible());
        }

        [Theory]
        [InlineData("(11)0000-0000", "BR")]  // Número inválido (todos zeros)
        [InlineData("(11)9999999999", "BR")] // Muito dígitos para fixo
        [InlineData("(00)99999-9999", "BR")] // DDD inválido (00)
        public void ShouldThrowExceptionForInvalidPhoneNumbers(string phoneNumber, string countryCode)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Phone(countryCode, phoneNumber));
        }

        [Fact]
        public void ShouldNormalizeFormattedPhoneNumbers()
        {
            // Arrange - two different formats of the same number
            var formattedPhone = new Phone("BR", "(11) 99999-9999");
            var unformattedPhone = new Phone("BR", "11999999999");

            // Act & Assert - both should result in the same E164 format
            Assert.Equal(formattedPhone.E164Format, unformattedPhone.E164Format);
            Assert.Equal(formattedPhone.Number, unformattedPhone.Number);
        }
    }
}

