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
  public class PlanesDAL
  {
    private const string collectionName = "planes";
    private const string database = "logistics";
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongodataBase;
    private readonly IMongoCollection<BsonDocument> planesCollection;

    public PlanesDAL(IMongoClient mongoClient)
    {
      this.mongoClient = mongoClient;
      this.mongodataBase = mongoClient.GetDatabase(database);
      this.planesCollection = this.mongodataBase.GetCollection<BsonDocument>(collectionName);
    }

    public async Task<List<City>> GetCities()
    {
      var cityDtosCursor = await this.planesCollection.FindAsync(new BsonDocument());
      var cityDtos = cityDtosCursor.ToList();
      var cities = new List<City>();
      foreach (var cityDto in cityDtos)
      {
        var cityModel = BsonSerializer.Deserialize<City>(cityDto);
        cities.Add(cityModel);
      }

      return cities;
    }
  }
}