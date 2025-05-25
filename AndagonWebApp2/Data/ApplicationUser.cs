using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AndagonWebApp2.Data
{
    public class ApplicationUser : IdentityUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    }
}
