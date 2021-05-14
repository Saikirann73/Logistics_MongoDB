using System.Collections.Generic;
using Logistics.Constants;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.Models
{
  [BsonIgnoreExtraElements]
  public class Plane
  {
    [BsonElement(CommonConstants.UnderScoreId)]
    public string Callsign { get; set; }

    [BsonElement(PlanesConstants.CurrentLocation)]
    public List<double> CurrentLocation { get; set; }

    [BsonElement(PlanesConstants.Heading)]
    public decimal Heading { get; set; }

    [BsonElement(PlanesConstants.Route)]
    public List<string> Route { get; set; }

    [BsonElement(PlanesConstants.Landed)]
    public string Landed { get; set; }

    [BsonElement(PlanesConstants.Hub)]
    public string Hub { get; set; }
    
    [BsonElement(PlanesConstants.PlaneType)]
    public string PlaneType { get; set; }
  }
}