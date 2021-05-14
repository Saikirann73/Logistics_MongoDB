using System.Collections.Generic;
using System.Threading.Tasks;
using Logistics.Models;
using MongoDB.Bson;

namespace Logistics.DAL.Interfaces
{
  public interface ICitiesDAL
  {
    Task<List<City>> FetchNearestCities(string cityId);
    Task<List<City>> GetCities();
    Task<City> GetCityById(string id);
    string GetLastError();
  }
}