using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public PlanesController(ILogger<PlanesController> logger)
    {
      _logger = logger;
    }

    /// <summary>
    /// Fetch planes
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetPlanes()
    {
      return new OkResult();
    }

    /// <summary>
    /// Fetch plane by ID
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public IActionResult GetPlanesById(string Id)
    {
      return new OkResult();
    }

    /// <summary>
    /// Update location, heading, and landed for a plane
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="heading"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPut("{id}/location/{latitude}/{longitude}/{heading}/{location}")]
    public IActionResult UpdatePlaneLocationAndLanding(string latitude, string longitude, string heading, string location)
    {
      return new OkResult();
    }

    /// <summary>
    /// Update location and heading for a plane
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="heading"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPut("{id}/location/{latitude}/{longitude}/{heading}")]
    public IActionResult UpdatePlaneLocation(string latitude, string longitude, string heading, string location)
    {
      return new OkResult();
    }

    /// <summary>
    /// Replace a Plane's Route with a single city
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPut("{id}/route/{location}")]
    public IActionResult UpdatePlaneRoute(string location)
    {
      return new OkResult();
    }

    /// <summary>
    /// Add a city to a Plane's Route
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPost("{id}/route/{location}")]
    public IActionResult AddPlaneRoute(string location)
    {
      return new OkResult();
    }

    /// <summary>
    /// Add a city to a Plane's Route
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpDelete("{id}/route/destination")]
    public IActionResult RemoveFirstPlaneRoute(string location)
    {
      return new OkResult();
    }
  }
}
