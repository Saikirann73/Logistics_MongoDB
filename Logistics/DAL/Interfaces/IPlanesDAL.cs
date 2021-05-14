using System.Collections.Generic;
using System.Threading.Tasks;
using Logistics.Models;

namespace Logistics.DAL.Interfaces
{
  public interface IPlanesDAL
  {
    Task<bool> AddPlaneRoute(string id, string location);
    string GetLastError();
    Task<Plane> GetPlaneById(string id);
    Task<List<Plane>> GetPlanes();
    Task<bool> RemoveFirstPlaneRoute(string id);
    Task<bool> ReplacePlaneRoutes(string id, string location);
    Task<bool> UpdatePlaneLocation(string id, List<double> location, float heading);
    Task<bool> UpdatePlaneLocationAndLanding(string id, List<double> location, float heading, string landed);
  }
}