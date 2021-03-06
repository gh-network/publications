using MongoDB.Driver;

namespace GhostNetwork.Content.MongoDb
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase database;

        public MongoDbContext(IMongoDatabase database)
        {
            this.database = database;
        }

        public IMongoCollection<PublicationEntity> Publications =>
            database.GetCollection<PublicationEntity>("publications");

        public IMongoCollection<CommentEntity> Comments =>
            database.GetCollection<CommentEntity>("comments");

        public IMongoCollection<ReactionEntity> Reactions =>
            database.GetCollection<ReactionEntity>("reactions");
    }
}
