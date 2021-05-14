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
  public class CargoDAL : ICargoDAL
  {
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongodataBase;
    private readonly IMongoCollection<BsonDocument> cargoCollection;
    private readonly ILogger<CargoDAL> logger;
    private string lastError;
    public CargoDAL(IMongoClient mongoClient, ILogger<CargoDAL> logger)
    {
      this.mongoClient = mongoClient;
      this.mongodataBase = mongoClient.GetDatabase(CommonConstants.Database);
      this.cargoCollection = this.mongodataBase.GetCollection<BsonDocument>(CargoConstants.CollectionName);
      this.logger = logger;
    }

    public async Task<List<Cargo>> GetAllCargosAtLocation(string location)
    {
      var cargos = new ConcurrentBag<Cargo>();
      var builder = Builders<BsonDocument>.Filter;
      var filter = builder.Ne(CargoConstants.Status, CargoConstants.Delivered) &
                   builder.Or(builder.Eq(CargoConstants.Location, location), builder.Eq(CargoConstants.Courier, location));
      try
      {
        // Created index with status, location and courier -> db.cargos.createIndex({status:1,location:1,courier:1})
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
        throw;
      }

      return cargos.ToList();
    }

    public async Task<Cargo> GetCargoById(string id)
    {
      var filter = new BsonDocument();
      filter[CommonConstants.UnderScoreId] = new ObjectId(id);
      try
      {
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
         {CargoConstants.Received, new BsonDateTime(DateTime.Now)}
       };
      await this.cargoCollection.InsertOneAsync(cargo); // Todo: check about write concern
      var newCargo = await this.GetCargoById(cargo[CommonConstants.UnderScoreId].ToString());
      return newCargo;
    }

    public async Task<bool> UpdateCargoStatus(string cargoId, string status)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargoId));
        var update = Builders<BsonDocument>.Update
                             .Set(CargoConstants.Status, status)
                             .Set(CargoConstants.DeliveredAt, new BsonDateTime(DateTime.Now));
        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = $"Failed to update the cargo : {cargoId} with status: {status}.Exception: {ex.ToString()}";
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
  }
}