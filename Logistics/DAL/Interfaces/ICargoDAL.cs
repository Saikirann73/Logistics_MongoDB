using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logistics.Models;
using MongoDB.Bson;

namespace Logistics.DAL.Interfaces
{
  public interface ICargoDAL
  {
    Task<Cargo> CreateCargo(string location, string destination);
    Task<List<Cargo>> GetAllCargosAtLocation(string location);
    Task<Cargo> GetCargoById(string id);
    string GetLastError();
    Task<bool> RemoveCourier(string cargoId);
    Task<bool> UpdateCargoCourier(string cargoId, string courier);
    Task<bool> UpdateCargoDestinationLocation(string cargoId, string destination);
    Task<bool> UpdateCargoRouteInfo(string cargoId, string destination, string transitType, string cargoFlight);
    Task<bool> UpdateCargoSourceLocation(string cargoId, string location);
    Task<bool> UpdateCargoStatusDuration(Cargo cargo, string status);
    Task<double> FetchAverageDeliveryTime();
    Task<bool> AddToCourierHistory(string cargoId, string status, string planeId, string locationId, DateTime time);
  }
}