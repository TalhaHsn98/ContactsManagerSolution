using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        //constructor
        public CountriesServiceTest()
        {

            var countriesInitialData = new List<Country>() { };

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(
                new DbContextOptionsBuilder<ApplicationDbContext>().Options
            );


            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
            ApplicationDbContext dbContext = dbContextMock.Object;

            _countriesService = new CountriesService(null);


        }


        #region AddCountry
        // Requirements
        //When CountryAddRequest is null, it should throw ArgumnetNullException
        [Fact]
        public async Task AddCountry_NullCountry() 
        {
            //Arrange
            CountryAddRequest? request = null;

           

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async() =>
            {
                //Act
                await _countriesService.AddCountry(request);
            }
            );
        }

        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameisNull()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest()
            { CountryName = null};



            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request);
            }
            );
        }

        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            //Arrange
            CountryAddRequest? request1 = new CountryAddRequest()
            { CountryName = "USA" };
            CountryAddRequest? request2 = new CountryAddRequest()
            { CountryName = "USA" };



            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            }
            );
        }

        //When you supply proper country name, it should add insert the country to the existing list of countries

        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = "Japan" };

            //Act
            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> countries_from_GetAllCountries = await _countriesService.GetAllCountries();

            //Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains(response, countries_from_GetAllCountries);
        }
        #endregion

        #region GetAllCountries

        [Fact]
        //The list of countries should be empty by default

        public async Task GetAllCountries_EmptyList()
        {
            List<CountryResponse> actual_country_response_list =
                await _countriesService.GetAllCountries();

            Assert.Empty(actual_country_response_list);
        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            List<CountryAddRequest> country_request_list = new List<CountryAddRequest>()
            {
                new CountryAddRequest() { CountryName = "US" },
                new CountryAddRequest() { CountryName = "UK" }
            };

            List<CountryResponse> countries_list_from_add_country = new List<CountryResponse>();

            foreach(CountryAddRequest country_request in country_request_list)
            {
                countries_list_from_add_country.Add( await _countriesService.AddCountry(country_request));
            }

            List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

            foreach(CountryResponse expected_country in countries_list_from_add_country)
            {
                Assert.Contains(expected_country, actualCountryResponseList);
            }

        }

        #endregion

        #region GetCountryByCountryID

        [Fact]
        public async Task GetCountryByCountryID_nullCountry() {

            //Arrange
            Guid? conID = null;

            //Act
            CountryResponse country_response_from_method = await _countriesService.GetCountryByCountryID(conID);

            //Assert
            Assert.Null(country_response_from_method);

        }

        [Fact]
        public async Task GetCountrybyCountryID_ValidCountryID()
        {
            //Arrange
            CountryAddRequest Countryadd = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse Country_from_Add_Method = await _countriesService.AddCountry(Countryadd);

            //Act
            CountryResponse Country_from_Get_Method = await _countriesService.GetCountryByCountryID(Country_from_Add_Method.CountryID);

            //Assert

            Assert.Equal(Country_from_Add_Method, Country_from_Get_Method);
        }
        #endregion


    }
}
