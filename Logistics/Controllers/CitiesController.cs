using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logistics.DAL;
using Logistics.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Logistics.Controllers
{
  [ApiController]
  [Route("cities")]
  public class CitiesController : Controller
  {
    private readonly ILogger<CitiesController> _logger;
    private readonly ICitiesDAL citiesDAL;

    public CitiesController(ILogger<CitiesController> logger, ICitiesDAL citiesDAL)
    {
      this._logger = logger;
      this.citiesDAL = citiesDAL;
    }

    /// <summary>
    /// Fetch ALL cities
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetCities()
    {
      var cities = await this.citiesDAL.GetCities();
      return new OkObjectResult(cities);
    }

    /// <summary>
    /// Fetch City by ID
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCitiesById(string Id)
    {
      var city = await this.citiesDAL.GetCityById(Id);
      if (city == null)
      {
        return new NotFoundResult();
      }

      return new OkObjectResult(city);
    } 
  }
}
