using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.Models
{
  public class City
  {
    [BsonElement("_id")]
    public string Name { get; set; }

    [BsonElement("position")]
    public List<string> Location { get; set; }

    [BsonElement("country")]
    public string Country { get; set; }
  }
}
