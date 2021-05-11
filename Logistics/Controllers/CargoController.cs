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
  [Route("cargo")]
  public class CargoController : Controller
  {

    private readonly ILogger<CargoController> _logger;

    public CargoController(ILogger<CargoController> logger)
    {
      _logger = logger;
    }

    /// <summary>
    /// Fetch Cargo by ID
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpGet("location/{location}")]
    public IActionResult GetCargoAtLocation(string location)
    {
      return new OkResult();
    }

    /// <summary>
    /// Create a new cargo at "location" which needs to get to "destination" - error if neither location nor destination exist as cities. Set status to "in progress" 
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPost("{source}/to/{destination}")]
    public IActionResult CreateCargo(string source, string destination)
    {
      return new OkResult();
    }

    /// <summary>
    /// Set status field to 'Delivered' - Increment some count of delivered items too.
    /// </summary>
    /// <returns></returns>
    [HttpPut("{cargoId}/delivered")]
    public IActionResult CargoDelivered(string cargoId)
    {
      return new OkResult();
    }

    /// <summary>
    /// Mark that the next time the courier (plane) arrives at the location of this package it should be onloaded by setting the courier field - courier should be a plane.
    /// </summary>
    /// <returns></returns>
    [HttpPut("{cargoId}/courier/{courierId}")]
    public IActionResult CargoAssignCourier(string cargoId, string courierId)
    {
      return new OkResult();
    }

    /// <summary>
    /// Move a piece of cargo from one location to another (plane to city or vice-versa)
    /// </summary>
    /// <param name="cargoId"></param>
    /// <param name="locationId"></param>
    /// <returns></returns>
    [HttpPut("{cargoId}/location/{locationId}")]
    public IActionResult CargoMove(string cargoId, string locationId)
    {
      return new OkResult();
    }

    /// <summary>
    /// Unset the value of courier on a given piece of cargo
    /// </summary>
    /// <param name="cargoId"></param>
    /// <returns></returns>
    [HttpDelete("{cargoId}/courier")]
    public IActionResult CargoUnsetCourier(string cargoId)
    {
      return new OkResult();
    }
  }
}
