using MongoDB.Bson;
using MongoDB.Driver;

namespace YourNamespace.Data
{
    /// <summary>
    /// Universeller Datenbankmanager für MongoDB – Enterprise-Ready!
    /// </summary>
    public class DBManager<T> where T : class
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<T> _collection;

        public DBManager(string databaseName, string collectionName, string connectionString = "mongodb://localhost:27017")
        {
            if (string.IsNullOrEmpty(databaseName)) throw new ArgumentNullException(nameof(databaseName));
            if (string.IsNullOrEmpty(collectionName)) throw new ArgumentNullException(nameof(collectionName));

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            _collection = _database.GetCollection<T>(collectionName);
        }

        #region CRUD OPERATIONS

        public async Task InsertAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task InsertManyAsync(IEnumerable<T> entities)
        {
            await _collection.InsertManyAsync(entities);
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
        }

        public async Task<bool> UpdateAsync(string id, T entity)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            var result = await _collection.ReplaceOneAsync(filter, entity);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        #endregion

        #region ADVANCED QUERIES

        public async Task<List<T>> FindAsync(FilterDefinition<T> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<T>> SearchAsync(string fieldName, string searchTerm)
        {
            var filter = Builders<T>.Filter.Regex(fieldName, new BsonRegularExpression(searchTerm, "i"));
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<long> CountAsync(FilterDefinition<T> filter = null)
        {
            return await _collection.CountDocumentsAsync(filter ?? Builders<T>.Filter.Empty);
        }

        public async Task<List<T>> GetPagedAsync(FilterDefinition<T> filter, int skip, int take, SortDefinition<T> sort = null)
        {
            var query = _collection.Find(filter ?? Builders<T>.Filter.Empty);
            if (sort != null) query = query.Sort(sort);
            return await query.Skip(skip).Limit(take).ToListAsync();
        }

        #endregion

        #region INDEXES & ADMIN

        public async Task CreateIndexAsync(string fieldName, bool ascending = true)
        {
            var indexKeys = ascending ?
                Builders<T>.IndexKeys.Ascending(fieldName) :
                Builders<T>.IndexKeys.Descending(fieldName);
            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(indexKeys));
        }

        public async Task<List<string>> ListIndexesAsync()
        {
            var indexes = await _collection.Indexes.ListAsync();
            var indexDocs = await indexes.ToListAsync();
            var result = new List<string>();
            foreach (var idx in indexDocs)
                result.Add(idx.ToString());
            return result;
        }

        #endregion

        #region TRANSACTIONS (für mehrere Collections)

        public async Task ExecuteInTransactionAsync(Func<IClientSessionHandle, Task> action)
        {
            var client = _database.Client;
            using (var session = await client.StartSessionAsync())
            {
                session.StartTransaction();
                try
                {
                    await action(session);
                    await session.CommitTransactionAsync();
                }
                catch
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        }

        #endregion

        #region HEALTH & UTILITIES

        public bool Ping()
        {
            try
            {
                var command = new BsonDocument("ping", 1);
                _database.RunCommand<BsonDocument>(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> ListCollectionsAsync()
        {
            var collections = await _database.ListCollectionNamesAsync();
            return await collections.ToListAsync();
        }

        public async Task DropCollectionAsync(string collectionName)
        {
            await _database.DropCollectionAsync(collectionName);
        }

        #endregion

        // Hier können beliebige weitere Methoden, z.B. für ChangeStreams, BulkWrites etc., ergänzt werden!
    }
}
