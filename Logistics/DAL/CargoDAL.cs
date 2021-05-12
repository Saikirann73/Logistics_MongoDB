using System;
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
  public class CargoDAL : ICargoDAL
  {
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongodataBase;
    private readonly IMongoCollection<BsonDocument> cargoCollection;
    private string lastError;
    public CargoDAL(IMongoClient mongoClient)
    {
      this.mongoClient = mongoClient;
      this.mongodataBase = mongoClient.GetDatabase(CommonConstants.Database);
      this.cargoCollection = this.mongodataBase.GetCollection<BsonDocument>(CargoConstants.CollectionName);
    }

    public async Task<List<Cargo>> GetAllCargosAtLocation(string location)
    {
      var cargos = new List<Cargo>();
      var builder = Builders<BsonDocument>.Filter;
      var filter = builder.Ne(CargoConstants.Status, CargoConstants.Delivered) & builder.Eq(CargoConstants.Location, location);
      try
      {
        var cursor = await this.cargoCollection.FindAsync(filter);
        var cargoDtos = cursor.ToList();
        foreach (var cargoDto in cargoDtos)
        {
          var cargoModel = BsonSerializer.Deserialize<Cargo>(cargoDto);
          cargos.Add(cargoModel);
        }
      }
      catch (MongoException)
      {
        // TODO: log
        throw;
      }

      return cargos;
    }

    public async Task<Cargo> GetCargoById(ObjectId id)
    {
      var filter = new BsonDocument();
      filter[CommonConstants.UnderScoreId] = id;
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
        lastError = ex.ToString();
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
         {CargoConstants.Status, "in progress"},
         {CargoConstants.Received, new BsonDateTime(DateTime.Now)}
       };
      await this.cargoCollection.InsertOneAsync(cargo);
      var newCargo = await this.GetCargoById(cargo[CommonConstants.UnderScoreId].AsObjectId);
      return newCargo;
    }

    public async Task<bool> UpdateCargoStatus(string cargoId, string status)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargoId));
        var update = Builders<BsonDocument>.Update
                             .Set(CargoConstants.Status, status);
        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = ex.ToString();
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
        lastError = ex.ToString();
        result = false;
      }
      return result;
    }

    public async Task<bool> UpdateCargoLocation(string cargoId, string location)
    {
      var result = false;
      try
      {
        var filter = Builders<BsonDocument>.Filter.Eq(CommonConstants.UnderScoreId, new ObjectId(cargoId));
        var update = Builders<BsonDocument>.Update
                             .Set(CargoConstants.Location, location);
        var updatedCargoResult = await this.cargoCollection.UpdateOneAsync(filter, update);
        result = updatedCargoResult.IsAcknowledged;
      }
      catch (MongoException ex)
      {
        lastError = ex.ToString();
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