using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace NoSQL
{
    class Program
    {
        static void Main(string[] args)
        {
            //, date , name ,  rating "60"
            MongoClient client = new MongoClient(
    "mongodb://127.0.0.1:27017"
);

            IMongoDatabase database = client.GetDatabase("NoSQL");

            var collection = database.GetCollection<BsonDocument>("softuniArticles");

            //collection.InsertOne(new BsonDocument 
            //{
            //    { "author", "Steve Jobs" },
            //    { "date", "05-05-2005"},
            //    {"name", "The story of Apple" },
            //    { "rating", "60"} 
            //});

            List<BsonDocument> allArticles = collection.Find(new BsonDocument { }).ToList();

            //var deleteQuery = Builders<BsonDocument>.Filter.Lt("rating", 50);
            //collection.DeleteMany(deleteQuery);

            foreach (var article in allArticles)
            {
                string name = article.GetElement("name").Value.AsString;

                int newRating = int.Parse(article.GetElement("rating").Value.AsString) + 10;

                var filterQuery = Builders<BsonDocument>.Filter.Eq("_id", article.GetElement("_id").Value);

                var updateQuery = Builders<BsonDocument>.Update.Set("rating", newRating.ToString());

                //collection.UpdateOne(filterQuery, updateQuery);

                Console.WriteLine($"{name} : rating {article.GetElement("rating").Value}");
                
            }
        }
    }
}
