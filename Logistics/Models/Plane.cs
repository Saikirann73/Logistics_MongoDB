using System.Collections.Generic;
using Logistics.Constants;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.Models
{
  public class Plane
  {
    [BsonElement(CommonConstants.UnderScoreId)]
    public string Name { get; set; }

    [BsonElement(PlanesConstants.CurrentLocation)]
    public List<string> CurrentLocation { get; set; }

    [BsonElement(PlanesConstants.Callsign)]
    public string Callsign { get; set; }

    [BsonElement(PlanesConstants.Heading)]
    public decimal Heading { get; set; }

    [BsonElement(PlanesConstants.Route)]
    public List<string> Route { get; set; }

    [BsonElement(PlanesConstants.Landed)]
    public string Landed { get; set; }
  }
}