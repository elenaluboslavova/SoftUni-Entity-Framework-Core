using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQL
{
    class Article : BsonDocument
    {
        [BsonElement]
        public string Name { get; set; }
    }
}
