using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;

namespace CRUDExample.Controllers
{
    [Route("persons")]
    public class PersonsController : Controller
    {

        private readonly ICountriesService _countryService;
        private readonly IPersonsService _personsService;
        private readonly ILogger<PersonsController> _logger;

        public PersonsController(IPersonsService personsService, ICountriesService countriesService, ILogger<PersonsController> logger)
        {
            _countryService = countriesService;
            _personsService = personsService;
            _logger = logger;
        }

        [Route("[action]")]
        [Route("/")]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            //Search
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                    { nameof(PersonResponse.PersonName), "Person Name" },
                    { nameof(PersonResponse.Email), "Email" },
                    { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                    { nameof(PersonResponse.Gender), "Gender" },
                    { nameof(PersonResponse.CountryID), "Country" },
                    { nameof(PersonResponse.Address), "Address" }
            };
            List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //Sort
            List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedPersons); //Views/Persons/Index.cshtml
        }


        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            List<CountryResponse> countries = await _countryService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp => 
            new SelectListItem { Text = temp.CountryName, Value = temp.CountryID.ToString()});
            return View();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countryService.GetAllCountries();
                ViewBag.Countries = countries;

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            //call the service method
            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);

            //navigate to Index() action method (it makes another get request to "persons/index"
            return RedirectToAction("Index", "Persons");
        }


        [HttpGet]
        [Route("[action]/{PersonId}")]

        public async Task<IActionResult> Edit(Guid PersonId)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(PersonId);

            if(personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();
            List<CountryResponse> countries = await _countryService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
            new SelectListItem { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            return View(personUpdateRequest);

        }

        [HttpPost]
        [Route("[action]/{PersonId}")]
        public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest) {

            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null) { 
                
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid) 
            {
                PersonResponse updatedPerson = await _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index");
            }
            else
            {

                List<CountryResponse> countries = await _countryService.GetAllCountries();
                ViewBag.Countries = countries.Select(temp =>
                new SelectListItem { Text = temp.CountryName, Value = temp.CountryID.ToString() });

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(personResponse.ToPersonUpdateRequest());

            }
        }

        [HttpGet]
        [Route("[action]/{PersonId}")]

        public async Task<IActionResult> Delete(Guid PersonId) 
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(PersonId);

            if (personResponse == null) {
                return RedirectToAction("Index");
            }

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{PersonId}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest) {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null) {
                return RedirectToAction("Index");
            }

            await _personsService.DeletePerson(personResponse.PersonID);

            return RedirectToAction("Index");
        }

        [Route("PersonsPDF")]
        public async Task<IActionResult> PersonsPDF()
        {
            IEnumerable<PersonResponse> person = await _personsService.GetAllPersons();

            return new ViewAsPdf("PersonsPDF", person, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins() { Top = 20, Right = 20, Bottom = 20, Left = 20 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [Route("PersonsCSV")]

        public async Task<IActionResult> PersonsCSV()
        {
            MemoryStream memoryStream = await _personsService.GetPersonsCSV();

            return File(memoryStream, "application/octet-stream", "person.csv");
        }

        [Route("PersonsExcel")]

        public async Task<IActionResult> PersonsExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Talha");
            MemoryStream memoryStream = await _personsService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }



    }
}
