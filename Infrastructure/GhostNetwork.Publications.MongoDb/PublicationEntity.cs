using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostNetwork.Publications.MongoDb
{
    public class PublicationEntity
    {
        public ObjectId Id { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("tags")]
        public IList<string> Tags { get; set; }

        [BsonElement("author")]
        public UserInfoEntity Author { get; set; }

        [BsonElement("createOn")]
        public long CreateOn { get; set; }

        [BsonElement("updateOn")]
        public long UpdateOn { get; set; }

        [BsonElement("images")]
        public string ImagesUrl { get; set; }
    }
}