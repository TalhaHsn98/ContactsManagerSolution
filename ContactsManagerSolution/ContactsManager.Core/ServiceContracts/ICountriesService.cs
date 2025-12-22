using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Country entity
    /// </summary>
    public interface ICountriesService
    {
        Task<CountryResponse> AddCountry(CountryAddRequest? countryAddREquest);

        /// <summary>
        /// Returns all countries from the list
        /// </summary>
        /// <returns>All countries as the list</returns>
        Task<List<CountryResponse>> GetAllCountries();

        Task<CountryResponse> GetCountryByCountryID(Guid? countryID);

        Task<int> UploadCountriesFromExcelFile(IFormFile formFile);

    }
}
