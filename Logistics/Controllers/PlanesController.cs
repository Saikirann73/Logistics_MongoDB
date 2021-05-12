using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logistics.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Logistics.Controllers
{
  [ApiController]
  [Route("planes")]
  public class PlanesController : Controller
  {
    private readonly ILogger<PlanesController> _logger;
    private readonly IPlanesDAL planesDAL;
    private readonly ICitiesDAL citiesDAL;

    public PlanesController(ILogger<PlanesController> logger, IPlanesDAL planesDAL, ICitiesDAL citiesDAL)
    {
      this._logger = logger;
      this.planesDAL = planesDAL;
      this.citiesDAL = citiesDAL;
    }

    /// <summary>
    /// Fetch planes
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetPlanes()
    {
      var planes = await this.planesDAL.GetPlanes();
      return new OkObjectResult(planes);
    }

    /// <summary>
    /// Fetch plane by ID
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlanesById(string Id)
    {
      var plane = await this.planesDAL.GetPlaneById(Id);
      if (plane == null)
      {
        return new NotFoundResult();
      }

      return new OkObjectResult(plane);
    }

    /// <summary>
    /// Update location, heading, and landed for a plane
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="heading"></param>
    /// <param name="city"></param>
    /// <returns></returns>
    [HttpPut("{id}/location/{location}/{heading}/{city}")]
    public async Task<IActionResult> UpdatePlaneLocationAndLanding(string id, string location, float heading, string city)
    {
      if (string.IsNullOrEmpty(location))
      {
        return new BadRequestObjectResult("Location information is invalid");
      }
      var locations = location.Split(',');
      if (locations.Count() != 2)
      {
        return new BadRequestObjectResult("Location information is invalid");
      }
      var cityObtained = await this.citiesDAL.GetCityById(city);
      if (cityObtained == null)
      {
        return new BadRequestObjectResult("Found invalid city");
      }

      var result = await this.planesDAL.UpdatePlaneLocationAndLanding(id, locations.ToList(), heading, city);
      if (!result)
      {
        return new BadRequestObjectResult(this.planesDAL.GetLastError());
      }

      return new JsonResult(result);
    }

    /// <summary>
    /// Update location and heading for a plane
    /// </summary>
    /// <param name="id"></param>
    /// <param name="location"></param>
    /// <param name="heading"></param>
    /// <returns></returns>
    [HttpPut("{id}/location/{location}/{heading}")]
    public async Task<IActionResult> UpdatePlaneLocation(string id, string location, float heading)
    {
      if (string.IsNullOrEmpty(location))
      {
        return new BadRequestObjectResult("Location information is invalid");
      }
      var locations = location.Split(',');
      if (locations.Count() != 2)
      {
        return new BadRequestObjectResult("Location information is invalid");
      }

      var result = await this.planesDAL.UpdatePlaneLocation(id, locations.ToList(), heading);
      if (!result)
      {
        return new BadRequestObjectResult(this.planesDAL.GetLastError());
      }

      return new JsonResult(result);
    }

    /// <summary>
    /// Add a city to a Plane's Route
    /// </summary>
    /// <param name="city"></param>
    /// <returns></returns>
    [HttpPost("{id}/route/{city}")]
    public async Task<IActionResult> AddPlaneRoute(string id, string city)
    {
      var cityObtained = await this.citiesDAL.GetCityById(city);
      if (cityObtained == null)
      {
        return new BadRequestObjectResult("Found invalid city");
      }

      var result = await this.planesDAL.AddPlaneRoute(id, city);
      if (!result)
      {
        return new BadRequestObjectResult(this.planesDAL.GetLastError());
      }
      return new JsonResult(result);
    }

    /// <summary>
    /// Replace a Plane's Route with a single city
    /// </summary>
    /// <param name="city"></param>
    /// <returns></returns>
    [HttpPut("{id}/route/{city}")]
    public async Task<IActionResult> UpdatePlaneRoute(string id, string city)
    {
      var cityObtained = await this.citiesDAL.GetCityById(city);
      if (cityObtained == null)
      {
        return new BadRequestObjectResult("Found invalid city");
      }

      var result = await this.planesDAL.ReplacePlaneRoutes(id, city);
      if (!result)
      {
        return new BadRequestObjectResult(this.planesDAL.GetLastError());
      }

      return new JsonResult(result);
    }

    /// <summary>
    /// Remove a city to a Plane's Route
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}/route/destination")]
    public async Task<IActionResult> RemoveFirstPlaneRoute(string id)
    {
      var result = await this.planesDAL.RemoveFirstPlaneRoute(id);
      if (!result)
      {
        return new BadRequestObjectResult(this.planesDAL.GetLastError());
      }

      return new JsonResult(result);
    }
  }
}
