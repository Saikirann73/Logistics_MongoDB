using System;
using Logistics.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.Models
{
  [BsonIgnoreExtraElements]
  public class Cargo
  {
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    [BsonElement(CargoConstants.Location)]
    public string Location { get; set; }

    [BsonElement(CargoConstants.Destination)]
    public string Destination { get; set; }

    [BsonElement(CargoConstants.Received)]
    public DateTime Received { get; set; }

    [BsonElement(CargoConstants.DeliveredAt)]
    public DateTime DeliveredAt { get; set; }

    [BsonElement(CargoConstants.Status)]
    public string Status { get; set; }

    [BsonElement(CargoConstants.Courier)]
    public string Courier { get; set; }

    [BsonElement(CargoConstants.CourierSource)]
    public string CourierSource { get; set; }

    [BsonElement(CargoConstants.CourierDestination)]
    public string CourierDestination { get; set; }

    [BsonElement(CargoConstants.TransitType)]
    public string TransitType { get; set; }
  }
}