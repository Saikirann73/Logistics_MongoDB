using MongoDB.Bson.Serialization.Attributes;

namespace Logistics.Models
{
  public class Plane
  {
    [BsonElement("_id")]
    public string Name { get; set; }

    [BsonElement("position")]
    public List<string> Location { get; set; }
  }
}