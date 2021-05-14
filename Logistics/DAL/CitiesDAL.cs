using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logistics.Constants;
using Logistics.DAL.Interfaces;
using Logistics.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Logistics.DAL
{
  public class CitiesDAL : ICitiesDAL
  {
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongodataBase;
    private readonly IMongoCollection<BsonDocument> citiesCollection;
    private readonly ILogger<CitiesDAL> logger;
    private string lastError;
    public CitiesDAL(IMongoClient mongoClient, ILogger<CitiesDAL> logger)
    {
      this.mongoClient = mongoClient;
      this.mongodataBase = mongoClient.GetDatabase(CommonConstants.Database);
      this.citiesCollection = this.mongodataBase.GetCollection<BsonDocument>(CitiesConstants.CollectionName);
      this.logger = logger;
    }

    public async Task<List<City>> GetCities()
    {
      var sort = Builders<BsonDocument>.Sort.Ascending(CommonConstants.UnderScoreId);
      var findOptions = new FindOptions<BsonDocument, BsonDocument>()
      {
        // Sort is to display the city names in order in the front end
        Sort = sort
      };
      var cityDtosCursor = await this.citiesCollection.FindAsync(new BsonDocument(), findOptions);
      var cityDtos = cityDtosCursor.ToList();
      var cities = new ConcurrentBag<City>();
      // Parallelizing the serialization to make it faster.
      Parallel.ForEach(cityDtos, cityDto =>
      {
        var cityModel = BsonSerializer.Deserialize<City>(cityDto);
        cities.Add(cityModel);
      });

      return cities.ToList();
    }

    public async Task<City> GetCityById(string id)
    {
      var filter = new BsonDocument();
      filter[CommonConstants.UnderScoreId] = id;
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
      catch (MongoException ex)
      {
        lastError = $"Failed to fetch the city by id: {id} Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        throw;
      }

      return null;
    }

    public async Task<List<City>> FetchNearestCities(string cityId)
    {
      var nearestCitiesSorted = new List<City>();
      var city = await this.GetCityById(cityId);
      var filter = new BsonDocument("position", new BsonDocument("$near", new BsonArray
        {
           city.Location[0],
           city.Location[1]
        }));
      // Todo use projection and include only city name 
      try
      {
        // Created legacy 2d index for 'position', location and courier -> db.cities.createIndex({'position': '2d'})
        var cityDtosCursor = await this.citiesCollection.FindAsync(filter);
        var cityDtos = cityDtosCursor.ToList();
        foreach (var cityDto in cityDtos)
        {
          var cityModel = BsonSerializer.Deserialize<City>(cityDto);
          nearestCitiesSorted.Add(cityModel);
        }

        return nearestCitiesSorted;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to fetch all the cities. Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        throw;
      }
    }

    public string GetLastError()
    {
      return lastError;
    }
  }
}
