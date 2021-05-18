using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
  public class CargoDAL : ICargoDAL
  {
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongodataBase;
    private readonly IMongoCollection<BsonDocument> cargoCollection;
    private readonly ILogger<CargoDAL> logger;
    private string lastError;
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    public CargoDAL(IMongoClient mongoClient, ILogger<CargoDAL> logger)
    {
      this.mongoClient = mongoClient;
      this.mongodataBase = mongoClient.GetDatabase(CommonConstants.Database);
      var databaseWithWriteConcern = this.mongodataBase.WithWriteConcern(WriteConcern.WMajority).WithReadConcern(ReadConcern.Majority);
      this.cargoCollection = databaseWithWriteConcern.GetCollection<BsonDocument>(CargoConstants.CollectionName);
      this.cargoCollection = this.mongodataBase.GetCollection<BsonDocument>(CargoConstants.CollectionName);
      this.logger = logger;
    }

    public async Task<List<Cargo>> GetAllCargosAtLocation(string location)
    {
      var cargos = new ConcurrentBag<Cargo>();
      var builder = Builders<BsonDocument>.Filter;
      var filter = builder.Ne(CargoConstants.Status, CargoConstants.Delivered) &
                   builder.Eq(CargoConstants.Location, location);
      try
      {
        // Created index with status, location and courier -> db.cargos.createIndex({status:1,location:1})
        var cursor = await this.cargoCollection.FindAsync(filter);
        var cargoDtos = cursor.ToList();

        // Parallelizing the serialization to make it faster.
        Parallel.ForEach(cargoDtos, cargoDto =>
        {
          var cargoModel = BsonSerializer.Deserialize<Cargo>(cargoDto);
          cargos.Add(cargoModel);
        });
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to fetch the cargoes at the location: {location}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
      }

      return cargos.ToList();
    }

    public async Task<Cargo> GetCargoById(string id)
    {
      var filter = new BsonDocument();
      filter[CommonConstants.UnderScoreId] = new ObjectId(id);
      try
      {
        // Will use _id index
        var cursor = await this.cargoCollection.FindAsync(filter);
        var cargos = cursor.ToList();
        if (cargos.Any())
        {
          var cargoModel = BsonSerializer.Deserialize<Cargo>(cargos.FirstOrDefault());
          return cargoModel;
        }
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to fetch the cargo by the id: {id}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
      }

      return null;
    }

    public async Task<Cargo> CreateCargo(string location, string destination)
    {
      var id = new ObjectId();
      var cargo = new BsonDocument()
       {
         {CommonConstants.UnderScoreId, id},
         {CargoConstants.Location,  location},
         {CargoConstants.Destination,  destination},
         {CargoConstants.CourierSource,  location},
         {CargoConstants.CourierDestination,  destination},
         {CargoConstants.Status, CargoConstants.InProgress},
         {CargoConstants.TransitType, CargoConstants.CargoTransitTypeRegional},
         {CargoConstants.Received, new BsonDateTime(DateTime.Now).ToUniversalTime()}
       };
      await this.cargoCollection.InsertOneAsync(cargo);
      var newCargo = await this.GetCargoById(cargo[CommonConstants.UnderScoreId].ToString());
      return newCargo;
    }

    public async Task<bool> UpdateCargoStatusDuration(Cargo cargo, string status)
    {
      var result = false;
      try
      {
        // Compute Pattern :  Calculating the duration between the cargo created and delivered date time.
        var presentDateTime = new BsonDateTime(DateTime.Now).ToUniversalTime();
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargo.Id));
        var duration = presentDateTime - cargo.Received;
        var update = Builders<BsonDocument>.Update
                             .Set(CargoConstants.Status, status)
                             .Set(CargoConstants.DeliveredAt, presentDateTime)
                             .Set(CargoConstants.Duration, duration.TotalMilliseconds);
        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to update the cargo : {cargo.Id} with status: {status}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        result = false;
      }
      return result;
    }

    public async Task<bool> UpdateCargoCourier(string cargoId, string courier)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargoId));
        var update = Builders<BsonDocument>.Update
                             .Set(CargoConstants.Courier, courier);
        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to assign courier : {courier} to the cargo : {cargoId}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        result = false;
      }
      return result;
    }

    public async Task<bool> UpdateCargoSourceLocation(string cargoId, string sourceLocation)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargoId));
        var update = Builders<BsonDocument>.Update
                             .Set(CargoConstants.Location, sourceLocation);
        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to update the source location : {sourceLocation} to the cargo : {cargoId}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        result = false;
      }
      return result;
    }

    public async Task<bool> UpdateCargoDestinationLocation(string cargoId, string destination)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargoId));
        var update = Builders<BsonDocument>.Update
                             .Set(CargoConstants.Destination, destination);
        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to update the destination location : {destination} to the cargo : {cargoId}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        result = false;
      }
      return result;
    }

    public async Task<bool> UpdateCargoRouteInfo(string cargoId, string destination, string transitType, string cargoFlight)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargoId));
        var update = Builders<BsonDocument>.Update
                    .Set(CargoConstants.Destination, destination)
                    .Set(CargoConstants.TransitType, transitType)
                    .Set(CargoConstants.Courier, cargoFlight);

        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to update the destination location : {destination} to the cargo : {cargoId}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        result = false;
      }
      return result;
    }

    public async Task<bool> RemoveCourier(string cargoId)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargoId));
        var update = Builders<BsonDocument>.Update
                             .Unset(CargoConstants.Courier);
        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to remove the courier from the cargo : {cargoId}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        result = false;
      }
      return result;
    }

    public string GetLastError()
    {
      return lastError;
    }

    public async Task<double> FetchAverageDeliveryTime()
    {
      var average = 0.0;
      var builder = Builders<BsonDocument>.Filter;
      var filter = builder.Exists(CargoConstants.Duration);
      var projection = Builders<BsonDocument>.Projection.Include(CargoConstants.Duration).Exclude(CommonConstants.UnderScoreId);
      // Created sparse index on 'duration'.-> db.cargos.createIndex({duration:1}, { sparse: true })
      // Also the following query will act as a covered query 
      var cursor = await this.cargoCollection.Find(filter)
                                             .Project(projection)
                                             .ToListAsync();
      var durations = cursor.ToList().Select(x => x.GetValue(CargoConstants.Duration).AsDouble);
      if (durations.Any())
      {
        average = durations.Sum() / durations.Count();
      }
      return average;
    }

    public async Task<bool> AddToCourierHistory(string cargoId,
                                                string status,
                                                string planeId,
                                                string locationId,
                                                DateTime time)
    {
      await this._semaphoreSlim.WaitAsync(); // Thread safe lock to synchronize the logging
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargoId));
        var historyEntry = new BsonDocument(new BsonElement(CargoConstants.Status, status))
                                       .Add(new BsonElement(CargoConstants.Plane, planeId))
                                       .Add(new BsonElement(CargoConstants.PackageLocation, locationId))
                                       .Add(new BsonElement(CargoConstants.Time, new BsonDateTime(time)));
        var update = Builders<BsonDocument>.Update.AddToSet(CargoConstants.CourierTrackingHistory, historyEntry);
        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to update the tracking history for the cargo : {cargoId}.Exception: {ex.ToString()}";
        this.logger.LogError(lastError);
        result = false;
      }

      this._semaphoreSlim.Release();
      return result;
    }
  }
}