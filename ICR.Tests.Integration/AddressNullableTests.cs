using ICR.Domain.DTOs;
using ICR.Domain.Model;
using Xunit;

namespace ICR.Tests.Unit
{
    public class AddressNullableTests
    {
        [Fact]
        public void Address_CanBeCreatedEmpty()
        {
            // Arrange & Act - Create an address with all null/empty fields
            var address = new Address(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Assert
            Assert.Null(address.Country);
            Assert.Null(address.PostalCode);
            Assert.Null(address.Street);
            Assert.Null(address.Number);
            Assert.Null(address.City);
            Assert.Null(address.State);
            Assert.Null(address.Complement);
            Assert.Null(address.CountyOrRegion);
        }

        [Fact]
        public void Address_CanBeCreatedWithValidData()
        {
            // Arrange & Act
            var address = new Address(
                "BR",
                "12345678",
                "Rua Teste",
                "123",
                "São Paulo",
                "SP",
                "Apt 456",
                null
            );

            // Assert
            Assert.NotNull(address.Country);
            Assert.Equal("BR", address.Country.Code);
            Assert.Equal("12345678", address.PostalCode);
            Assert.Equal("Rua Teste", address.Street);
            Assert.Equal("123", address.Number);
            Assert.Equal("São Paulo", address.City);
            Assert.Equal("SP", address.State);
            Assert.Equal("Apt 456", address.Complement);
        }

        [Fact]
        public void Address_ThrowsWhenPartialData()
        {
            // Arrange & Act & Assert
            // Only providing country but not other required fields should throw
            var ex = Assert.Throws<ArgumentException>(() =>
                new Address(
                    "BR",
                    null,  // Missing postal code
                    null,  // Missing street
                    null,  // Missing number
                    null,  // Missing city
                    null,  // Missing state
                    null,
                    null
                )
            );

            Assert.Contains("obrigatório", ex.Message);
        }

        [Fact]
        public void Address_GetFormattedAddress_ReturnsEmptyWhenNull()
        {
            // Arrange
            var address = new Address(null, null, null, null, null, null, null, null);

            // Act
            var formatted = address.GetFormattedAddress();

            // Assert
            Assert.Empty(formatted);
        }

        [Fact]
        public void Address_GetFormattedAddress_ReturnsFormattedString_WhenValid()
        {
            // Arrange
            var address = new Address(
                "BR",
                "12345678",
                "Rua Teste",
                "123",
                "São Paulo",
                "SP",
                "Apt 456",
                null
            );

            // Act
            var formatted = address.GetFormattedAddress();

            // Assert
            Assert.Contains("Rua Teste", formatted);
            Assert.Contains("123", formatted);
            Assert.Contains("São Paulo", formatted);
            Assert.Contains("SP", formatted);
        }

        [Fact]
        public void Address_Equals_WorksWithNullAddresses()
        {
            // Arrange
            var address1 = new Address(null, null, null, null, null, null, null, null);
            var address2 = new Address(null, null, null, null, null, null, null, null);

            // Act & Assert
            Assert.Equal(address1, address2);
        }

        [Fact]
        public void Address_GetHashCode_WorksWithNullAddresses()
        {
            // Arrange
            var address = new Address(null, null, null, null, null, null, null, null);

            // Act & Assert
            Assert.NotEqual(0, address.GetHashCode());
        }

        [Fact]
        public void AddressDTO_FromEntity_ReturnsNull_WhenAddressIsNull()
        {
            // Arrange
            Address? address = null;

            // Act
            var dto = AddressDTO.FromEntity(address);

            // Assert
            Assert.Null(dto);
        }

        [Fact]
        public void AddressDTO_FromEntity_ReturnsNull_WhenAddressIsEmpty()
        {
            // Arrange
            var emptyAddress = new Address(null, null, null, null, null, null, null, null);

            // Act
            var dto = AddressDTO.FromEntity(emptyAddress);

            // Assert
            Assert.Null(dto);
        }

        [Fact]
        public void AddressDTO_AllFields_AreNullable()
        {
            // Arrange
            var dto = new AddressDTO();

            // Act & Assert - All properties should be nullable
            Assert.Null(dto.CountryCode);
            Assert.Null(dto.PostalCode);
            Assert.Null(dto.Street);
            Assert.Null(dto.Number);
            Assert.Null(dto.City);
            Assert.Null(dto.State);
        }
    }
}
