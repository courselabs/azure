using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AssetManager.Model;

public abstract class EntityBase
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string DocumentId { get; set; }

    public Guid Id { get; set; }

    public string GetId()
    {
        return string.IsNullOrEmpty(DocumentId) ? Id.ToString() : DocumentId;
    }
}