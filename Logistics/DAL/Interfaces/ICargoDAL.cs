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
    Task<bool> UpdateCargoLocation(string cargoId, string location);
    Task<bool> UpdateCargoStatus(string cargoId, string status);
  }
}