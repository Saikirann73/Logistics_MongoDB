using System;
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
  public class PlanesDAL : IPlanesDAL
  {
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongodataBase;
    private readonly IMongoCollection<BsonDocument> planesCollection;

    private readonly ILogger<PlanesDAL> logger;
    private string lastError;

    public PlanesDAL(IMongoClient mongoClient, ILogger<PlanesDAL> logger)
    {
      this.mongoClient = mongoClient;
      this.mongodataBase = mongoClient.GetDatabase(CommonConstants.Database);
      this.planesCollection = this.mongodataBase.GetCollection<BsonDocument>(PlanesConstants.CollectionName);
      this.logger = logger;
    }

    public async Task<List<Plane>> GetPlanes()
    {
      var planeDtosCursor = await this.planesCollection.FindAsync(new BsonDocument());
      var planeDtos = planeDtosCursor.ToList();
      var planes = new ConcurrentBag<Plane>();
      // Parallelizing the serialization to make it faster.
      Parallel.ForEach(planeDtos, planeDto =>
      {
        var planeModel = BsonSerializer.Deserialize<Plane>(planeDto);
        planes.Add(planeModel);
      });

      return planes.ToList();
    }

    public async Task<Plane> GetPlaneById(string id)
    {
      var filter = new BsonDocument();
      filter[CommonConstants.UnderScoreId] = id;
      try
      {
        // Will use _id index
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
        lastError = $"Failed to fetch the plane by id: {id}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        throw;
      }

      return null;
    }

    public async Task<bool> UpdatePlaneLocation(string id, List<double> location, float heading)
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
        lastError = $"Failed to update the location/heading info for plane: {id}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        result = false;
      }

      return result;
    }

    public async Task<bool> UpdatePlaneLocationAndLanding(string id, List<double> location, float heading, string city)
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
        lastError = $"Failed to update the location/heading/city info for plane: {id}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
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
                             .AddToSet(PlanesConstants.Route, city);
        var updatedPlaneResult = await this.planesCollection.UpdateOneAsync(filter, update);
        result = updatedPlaneResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to add plane route : {city} for the plane: {id}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
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
        lastError = $"Failed to replace plane route : {city} for the plane: {id}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        result = false;
      }
      return result;
    }

    public async Task<bool> RemoveFirstPlaneRoute(string id)
    {
      var result = false;
      try
      {
        var plane = await this.GetPlaneById(id);
        var updatedRoute = Enumerable.Range(1, plane.Route.Count).Select(i => plane.Route[i % plane.Route.Count]).ToArray();
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, id);
        var update = Builders<BsonDocument>.Update
                             .Set(PlanesConstants.Route, updatedRoute);
        var updatedPlaneResult = await this.planesCollection.UpdateOneAsync(filter, update);
        result = updatedPlaneResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to remove the first route  for the plane: {id}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
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