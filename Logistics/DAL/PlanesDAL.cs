using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logistics.Constants;
using Logistics.DAL.Interfaces;
using Logistics.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Logistics.DAL
{
  public class PlanesDAL : IPlanesDAL
  {
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongodataBase;
    private readonly IMongoCollection<BsonDocument> planesCollection;
    private string lastError;

    public PlanesDAL(IMongoClient mongoClient)
    {
      this.mongoClient = mongoClient;
      this.mongodataBase = mongoClient.GetDatabase(CommonConstants.Database);
      this.planesCollection = this.mongodataBase.GetCollection<BsonDocument>(PlanesConstants.CollectionName);
    }

    public async Task<List<Plane>> GetPlanes()
    {
      var planeDtosCursor = await this.planesCollection.FindAsync(new BsonDocument());
      var planeDtos = planeDtosCursor.ToList();
      var planes = new List<Plane>();
      foreach (var planeDto in planeDtos)
      {
        var planeModel = BsonSerializer.Deserialize<Plane>(planeDto);
        planes.Add(planeModel);
      }

      return planes;
    }

    public async Task<Plane> GetPlaneById(string id)
    {
      var filter = new BsonDocument();
      filter[CommonConstants.UnderScoreId] = id;
      try
      {
        var cursor = await this.planesCollection.FindAsync(filter);
        var planes = cursor.ToList();
        if (planes.Any())
        {
          var planeModel = BsonSerializer.Deserialize<Plane>(planes.FirstOrDefault());
          return planeModel;
        }

      }
      catch (MongoException ex)
      {
        lastError = ex.ToString();
      }

      return null;
    }

    public async Task<bool> UpdatePlaneLocation(string id, List<string> location, float heading)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, id);
        var update = Builders<BsonDocument>.Update
                                        .Set(PlanesConstants.CurrentLocation, location)
                                        .Set(PlanesConstants.Heading, heading);
        var updatedPlaneResult = await this.planesCollection.UpdateOneAsync(filter, update);
        result = updatedPlaneResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = ex.ToString();
        result = false;
      }

      return result;
    }

    public async Task<bool> UpdatePlaneLocationAndLanding(string id, List<string> location, float heading, string city)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, id);
        var update = Builders<BsonDocument>.Update
                             .Set(PlanesConstants.CurrentLocation, location)
                             .Set(PlanesConstants.Heading, heading)
                             .Set(PlanesConstants.Landed, city);
        var updatedPlaneResult = await this.planesCollection.UpdateOneAsync(filter, update);
        result = updatedPlaneResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = ex.ToString();
        result = false;
      }

      return result;
    }

    public async Task<bool> AddPlaneRoute(string id, string city)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, id);
        var update = Builders<BsonDocument>.Update
                             .Push(PlanesConstants.Route, city);
        var updatedPlaneResult = await this.planesCollection.UpdateOneAsync(filter, update);
        result = updatedPlaneResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = ex.ToString();
        result = false;
      }
      return result;
    }

    public async Task<bool> ReplacePlaneRoutes(string id, string city)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, id);
        var update = Builders<BsonDocument>.Update
                             .Set(PlanesConstants.Route, new List<string> { city });
        var updatedPlaneResult = await this.planesCollection.UpdateOneAsync(filter, update);
        result = updatedPlaneResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = ex.ToString();
        result = false;
      }
      return result;
    }

    public async Task<bool> RemoveFirstPlaneRoute(string id)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, id);
        var update = Builders<BsonDocument>.Update
                             .PopFirst(PlanesConstants.Route);
        var updatedPlaneResult = await this.planesCollection.UpdateOneAsync(filter, update);
        result = updatedPlaneResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = ex.ToString();
        result = false;
      }
      return result;
    }

    public string GetLastError()
    {
      return lastError;
    }
  }
}