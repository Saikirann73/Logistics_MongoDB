using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logistics.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Logistics.Controllers
{
  [ApiController]
  [Route("planes")]
  public class PlanesController : Controller
  {
    private readonly ILogger<PlanesController> _logger;
    private readonly IPlanesDAL planesDAL;
    public PlanesController(ILogger<PlanesController> logger, IPlanesDAL planesDAL)
    {
      this._logger = logger;
      this.planesDAL = planesDAL;
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
    /// <param name="landed"></param>
    /// <returns></returns>
    [HttpPut("{id}/location/{location}/{heading}/{landed}")]
    public async Task<IActionResult> UpdatePlaneLocationAndLanding(string id, string location, float heading, string landed)
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

      var result = await this.planesDAL.UpdatePlaneLocationAndLanding(id, locations.ToList(), heading, landed);
      if (!result)
      {
        return new BadRequestObjectResult(this.planesDAL.getLastError());
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
        return new BadRequestObjectResult(this.planesDAL.getLastError());
      }

      return new JsonResult(result);
    }

    /// <summary>
    /// Replace a Plane's Route with a single city
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPut("{id}/route/{location}")]
    public async Task<IActionResult> UpdatePlaneRoute(string location)
    {
      return new OkResult();
    }

    /// <summary>
    /// Add a city to a Plane's Route
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPost("{id}/route/{location}")]
    public async Task<IActionResult> AddPlaneRoute(string location)
    {
      return new OkResult();
    }

    /// <summary>
    /// Add a city to a Plane's Route
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpDelete("{id}/route/destination")]
    public async Task<IActionResult> RemoveFirstPlaneRoute(string location)
    {
      return new OkResult();
    }
  }
}
