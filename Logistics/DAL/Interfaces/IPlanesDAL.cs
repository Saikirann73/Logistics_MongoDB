using System.Collections.Generic;
using System.Threading.Tasks;
using Logistics.Models;

namespace Logistics.DAL.Interfaces
{
  public interface IPlanesDAL
  {
    string getLastError();
    Task<Plane> GetPlaneById(string id);
    Task<List<Plane>> GetPlanes();
    Task<bool> UpdatePlaneLocation(string id, List<string> location, float heading);
    Task<bool> UpdatePlaneLocationAndLanding(string id, List<string> location, float heading, string landed);
  }
}