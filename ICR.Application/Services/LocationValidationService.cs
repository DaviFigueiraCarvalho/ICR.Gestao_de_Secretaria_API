using ICR.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICR.Application.Services
{
    public interface ILocationValidationService
    {
        IEnumerable<CountryInfoDTO> GetSupportedCountries();
        CountryInfoDTO GetCountryInfo(string countryCode);
    }

    public class LocationValidationService : ILocationValidationService
    {
        public IEnumerable<CountryInfoDTO> GetSupportedCountries()
        {
            return Country.GetAllSupported()
                .Select(c => new CountryInfoDTO
                {
                    Code = c.Code,
                    Name = c.Name,
                    PhoneCountryCode = c.PhoneCountryCode,
                    CultureCode = c.CultureCode
                });
        }

        public CountryInfoDTO GetCountryInfo(string countryCode)
        {
            var country = Country.GetByCode(countryCode);
            return new CountryInfoDTO
            {
                Code = country.Code,
                Name = country.Name,
                PhoneCountryCode = country.PhoneCountryCode,
                CultureCode = country.CultureCode
            };
        }
    }

    public class CountryInfoDTO
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PhoneCountryCode { get; set; } = null!;
        public string CultureCode { get; set; } = null!;
    }
}
