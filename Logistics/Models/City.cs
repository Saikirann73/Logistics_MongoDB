using System.Collections.Generic;
using Logistics.Constants;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.Models
{
  public class City
  {
    [BsonElement(CommonConstants.UnderScoreId)]
    public string Name { get; set; }

    [BsonElement(CitiesConstants.Position)]
    public List<float> Location { get; set; }

    [BsonElement(CitiesConstants.Country)]
    public string Country { get; set; }
  }
}
