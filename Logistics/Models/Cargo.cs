using System;
using Logistics.Constants;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.Models
{
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

    [BsonElement(CargoConstants.Status)]
    public string Status { get; set; }

    [BsonElement(CargoConstants.Courier)]
    public string Courier { get; set; }
  }
}