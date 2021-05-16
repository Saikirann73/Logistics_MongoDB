using System;
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

namespace Logistics
{
  public class CargoChangeStream
  {
    private readonly IMongoClient mongoClient;
    private readonly IMongoDatabase mongodataBase;
    private readonly IMongoCollection<BsonDocument> cargoCollection;
    private readonly IPlanesDAL planesDAL;
    private readonly ICargoDAL cargoDAL;
    private readonly ICitiesDAL citiesDAL;
    private readonly ILogger<CargoChangeStream> logger;
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    public CargoChangeStream(IMongoClient mongoClient,
                             IPlanesDAL planesDAL,
                             ICargoDAL cargoDAL,
                             ICitiesDAL citiesDAL,
                             ILogger<CargoChangeStream> logger)
    {
      this.mongoClient = mongoClient;
      this.mongodataBase = mongoClient.GetDatabase(CommonConstants.Database);
      this.cargoCollection = this.mongodataBase.GetCollection<BsonDocument>(CargoConstants.CollectionName);
      this.planesDAL = planesDAL;
      this.cargoDAL = cargoDAL;
      this.citiesDAL = citiesDAL;
      this.logger = logger;
    }

    public void Init()
    {
      new Thread(async () => await ObserveNewCargos()).Start();
      new Thread(async () => await ObserveCargoDeliveries()).Start();
    }

    /// <summary>
    /// Change stream to watch the creation of the cargoes and assigns the plane
    /// </summary>
    private async Task ObserveNewCargos()
    {
      var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
                            .Match(x => x.OperationType == ChangeStreamOperationType.Insert);
      var changeStreamOptions = new ChangeStreamOptions
      {
        FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
      };

      using (var cursor = await this.cargoCollection.WatchAsync(pipeline, changeStreamOptions))
      {
        await cursor?.ForEachAsync(async change =>
        {
          if (change == null)
          {
            return;
          }
          try
          {
            var newCargo = BsonSerializer.Deserialize<Cargo>(change.FullDocument);
            this.logger.LogInformation($"Got a new cargo : {newCargo.Id} from source: {newCargo.CourierSource} to destination: {newCargo.CourierDestination}");
            var allPlanes = await this.planesDAL.GetPlanes();
            var nearestRegionalPlane = this.FetchNearestRegionalPlane(newCargo.Location, allPlanes);
            if (nearestRegionalPlane == null)
            {
              this.logger.LogWarning($"Could not find any nearest plane to pick the cargo: {newCargo.Id}");
              return;
            }
            await ValidateDestination(newCargo, allPlanes, nearestRegionalPlane);
          }
          catch (Exception ex)
          {
            System.Console.WriteLine("Exception in New Cargoes watcher. Exception:" + ex.ToString());
            this.logger.LogError("Exception in New Cargoes watcher. Exception:" + ex.ToString());
          }
        });
      }
    }

    /// <summary>
    /// Change stream to watch for 'location' update and then finds the correct plane
    /// </summary>
    private async Task ObserveCargoDeliveries()
    {
      var updateSourceLocationPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
                                  .Match(x => x.OperationType == ChangeStreamOperationType.Update);
      var updateSourceLocationChangeStreamOptions = new ChangeStreamOptions
      {
        FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
      };

      using (var cursor = await this.cargoCollection.WatchAsync(updateSourceLocationPipeline, updateSourceLocationChangeStreamOptions))
      {
        await this._semaphoreSlim.WaitAsync();
        await this.WatchCargoUpdates(cursor);
        this._semaphoreSlim.Release();
      }
    }

    public Plane FetchNearestRegionalPlane(string sourceCity, List<Plane> allPlanes)
    {
      var plane = allPlanes.FirstOrDefault(x => x.Route.Contains(sourceCity) && x.PlaneType.ToLower() == PlanesConstants.PlaneTypeRegional.ToLower());
      return plane;
    }

    private async Task ValidateDestination(Cargo newCargo, List<Plane> allPlanes, Plane nearestRegionalPlane)
    {
      if (nearestRegionalPlane.Route.Contains(newCargo.Destination))
      {
        // case: The created courier is in the same routes of the plane
        this.logger.LogInformation($"Assigning the cargo : {newCargo.Id} to the plane: {nearestRegionalPlane.Callsign} and the destination is  {newCargo.Destination}");
        await this.cargoDAL.UpdateCargoCourier(newCargo.Id, nearestRegionalPlane.Callsign);
      }
      else
      {
        // case: The created courier is an international package, so assigning to hub
        this.logger.LogInformation($"Assigning the cargo : {newCargo.Id} to the plane: {nearestRegionalPlane.Callsign} and the destination has been set to {nearestRegionalPlane.Hub} hub");
        //await this.AssignCargo(newCargo);
        await this.cargoDAL.UpdateCargoRouteInfo(newCargo.Id, nearestRegionalPlane.Hub, CargoConstants.CargoTransitTypeRegional, nearestRegionalPlane.Callsign);
      }
      await this.cargoDAL.AddToCourierHistory(newCargo.Id,
                                              CargoConstants.Created,
                                              nearestRegionalPlane.Callsign,
                                              newCargo.Location,
                                              DateTime.UtcNow);
    }

    private async Task WatchCargoUpdates(IChangeStreamCursor<ChangeStreamDocument<BsonDocument>> cursor)
    {
      await cursor?.ForEachAsync(async change =>
      {
        if (change == null)
        {
          return;
        }
        try
        {
          Cargo deliveredCargo;
          var result = change.UpdateDescription.UpdatedFields.Contains(CargoConstants.Location);
          if (!result)
          {
            await WatchDeliveredStatus(change);
            // Update event is not for 'location' field. So ignoring.
            return;
          }

          deliveredCargo = BsonSerializer.Deserialize<Cargo>(change.FullDocument);
          await this.AssignCargo(deliveredCargo);
        }
        catch (Exception ex)
        {
          System.Console.WriteLine("Exception in Updated Cargoes watcher. Exception:" + ex.ToString());
          this.logger.LogError("Exception in Updated Cargoes watcher. Exception:" + ex.ToString());
        }
      });
    }

    private async Task WatchDeliveredStatus(ChangeStreamDocument<BsonDocument> change)
    {
      var statusResult = change.UpdateDescription.UpdatedFields.TryGetValue(CargoConstants.Status, out BsonValue statusElement);
      if (statusResult)
      {
        if (statusElement.ToString() == CargoConstants.Delivered)
        {
          Cargo deliveredCargo = BsonSerializer.Deserialize<Cargo>(change.FullDocument);
          await this.cargoDAL.AddToCourierHistory(deliveredCargo.Id,
                                                  CargoConstants.Delivered,
                                                  deliveredCargo.Courier ?? string.Empty,
                                                  deliveredCargo.Location,
                                                  DateTime.UtcNow);
        }
      }
    }

    private async Task AssignCargo(Cargo cargo)
    {
      var allPlanes = await this.planesDAL.GetPlanes();
      var plane = allPlanes.FirstOrDefault(x => x.Callsign == cargo.Location);
      if (plane != null)
      {
        // Currently the courier is in plane and not at any city.So dont assign anything here
        this.logger.LogInformation($"{plane.Callsign} has been picked up the cargo");
        await this.cargoDAL.AddToCourierHistory(cargo.Id,
                                                CargoConstants.InProgress,
                                                plane.Callsign,
                                                cargo.Location,
                                                DateTime.UtcNow);
        return;
      }
      var nearestCitiesToDestination = await this.citiesDAL.FetchNearestCities(cargo.CourierDestination);
      var planesWithDestinationRoute = allPlanes.Where(x => x.Route.Contains(cargo.Destination));
      foreach (var nearestCity in nearestCitiesToDestination)
      {
        Plane planeToAssign = planesWithDestinationRoute?.FirstOrDefault(x => x.Route.Contains(nearestCity.Name));
        if (planeToAssign != null && planeToAssign.Callsign != cargo.Location)
        {
          this.logger.LogInformation($"Assigning the cargo : {cargo.Id} to the plane: {planeToAssign.Callsign} and the destination has been set to {nearestCity.Name}");
          await this.cargoDAL.UpdateCargoRouteInfo(cargo.Id, nearestCity.Name, CargoConstants.CargoTransitTypeInternational, planeToAssign.Callsign);
          await this.cargoDAL.AddToCourierHistory(cargo.Id,
                                                  CargoConstants.InProgress,
                                                  planeToAssign.Callsign,
                                                  cargo.Location,
                                                  DateTime.UtcNow);
          break;
        }
      }
    }
  }
}