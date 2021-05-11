using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logistics.DAL.Interfaces;
using Logistics.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Logistics.DAL
{
  public class CitiesDAL : ICitiesDAL
  {
    private const string collectionName = "cities";
    private const string database = "logistics";
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongodataBase;
    private readonly IMongoCollection<BsonDocument> citiesCollection;

    public CitiesDAL(IMongoClient mongoClient)
    {
      this.mongoClient = mongoClient;
      this.mongodataBase = mongoClient.GetDatabase(database);
      this.citiesCollection = this.mongodataBase.GetCollection<BsonDocument>(collectionName);
    }

    public async Task<List<City>> GetCities()
    {
      var cityDtosCursor = await this.citiesCollection.FindAsync(new BsonDocument());
      var cityDtos = cityDtosCursor.ToList();
      var cities = new List<City>();
      foreach (var cityDto in cityDtos)
      {
        var cityModel = BsonSerializer.Deserialize<City>(cityDto);
        cities.Add(cityModel);
      }

      return cities;
    }

    public async Task<City> GetCityById(string id)
    {
      var filter = new BsonDocument();
      filter["_id"] = id;
      try
      {
        var cursor = await this.citiesCollection.FindAsync(filter);
        var cities = cursor.ToList();
        if (cities.Any())
        {
          var cityModel = BsonSerializer.Deserialize<City>(cities.FirstOrDefault());
          return cityModel;
        }

      }
      catch (MongoException)
      {
        // TODO: log
        throw;
      }

      return null;
    }
  }
}
