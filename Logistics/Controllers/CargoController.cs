using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logistics.Constants;
using Logistics.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Logistics.Controllers
{
  [ApiController]
  [Route("cargo")]
  public class CargoController : Controller
  {
    private readonly ILogger<CargoController> _logger;
    private readonly ICargoDAL cargoDAL;
    private readonly ICitiesDAL citiesDAL;

    private readonly IPlanesDAL planesDAL;

    public CargoController(ILogger<CargoController> logger, ICargoDAL cargoDAL, IPlanesDAL planesDAL, ICitiesDAL citiesDAL)
    {
      this._logger = logger;
      this.cargoDAL = cargoDAL;
      this.planesDAL = planesDAL;
      this.citiesDAL = citiesDAL;
    }

    /// <summary>
    /// Fetch Cargo by ID
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpGet("location/{location}")]
    public async Task<IActionResult> GetCargoAtLocation(string location)
    {
      if (string.IsNullOrEmpty(location))
      {
        return new BadRequestObjectResult("Location is invalid");
      }

      var cargos = await this.cargoDAL.GetAllCargosAtLocation(location);
      return new OkObjectResult(cargos);
    }

    /// <summary>
    /// Create a new cargo at "location" which needs to get to "destination" - error if neither location nor destination exist as cities. Set status to "in progress" 
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPost("{source}/to/{destination}")]
    public async Task<IActionResult> CreateCargo(string source, string destination)
    {
      if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
      {
        return new BadRequestObjectResult("Source or Destination is invalid");
      }

      var sourceCityObtained = await this.citiesDAL.GetCityById(source);
      if (sourceCityObtained == null)
      {
        return new NotFoundObjectResult("Source city not found");
      }

      var targetCityObtained = await this.citiesDAL.GetCityById(destination);
      if (targetCityObtained == null)
      {
        return new NotFoundObjectResult("Destination city not found");
      }

      var newCargo = await this.cargoDAL.CreateCargo(source, destination);
      return new OkObjectResult(newCargo);
    }

    /// <summary>
    /// Set status field to 'Delivered' - Increment some count of delivered items too.
    /// </summary>
    /// <returns></returns>
    [HttpPut("{cargoId}/delivered")]
    public async Task<IActionResult> CargoDelivered(string cargoId)
    {
      var result = true;
      var cargo = await this.cargoDAL.GetCargoById(cargoId);
      if (cargo.CourierDestination.Equals(cargo.Destination))
      {
        // case: Which means the courier has finally reached its original destination
        result = await this.cargoDAL.UpdateCargoStatus(cargoId, CargoConstants.Delivered);
        if (!result)
        {
          return new BadRequestObjectResult("Invalid CargoId");
        }
      }

      return new JsonResult(result);
    }

    /// <summary>
    /// Mark that the next time the courier (plane) arrives at the location of this package it should be onloaded by setting the courier field - courier should be a plane.
    /// </summary>
    /// <returns></returns>
    [HttpPut("{cargoId}/courier/{courierId}")]
    public async Task<IActionResult> CargoAssignCourier(string cargoId, string courierId)
    {
      var result = await this.cargoDAL.UpdateCargoCourier(cargoId, courierId);
      if (!result)
      {
        return new BadRequestObjectResult("Invalid CargoId");
      }

      return new JsonResult(result);
    }

    /// <summary>
    /// Move a piece of cargo from one location to another (plane to city or vice-versa)
    /// </summary>
    /// <param name="cargoId"></param>
    /// <param name="locationId"></param>
    /// <returns></returns>
    [HttpPut("{cargoId}/location/{locationId}")]
    public async Task<IActionResult> CargoMove(string cargoId, string locationId)
    {
      var result = await this.cargoDAL.UpdateCargoSourceLocation(cargoId, locationId);
      if (!result)
      {
        return new BadRequestObjectResult("Invalid CargoId");
      }

      return new JsonResult(result);
    }

    /// <summary>
    /// Unset the value of courier on a given piece of cargo
    /// </summary>
    /// <param name="cargoId"></param>
    /// <returns></returns>
    [HttpDelete("{cargoId}/courier")]
    public async Task<IActionResult> CargoUnsetCourier(string cargoId)
    {
      var result = await this.cargoDAL.RemoveCourier(cargoId);
      if (!result)
      {
        return new BadRequestObjectResult("Invalid CargoId");
      }

      return new JsonResult(result);
    }
  }
}
