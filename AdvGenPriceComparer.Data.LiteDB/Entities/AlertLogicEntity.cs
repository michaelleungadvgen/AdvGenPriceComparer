using LiteDB;
using System.Collections.Generic;

namespace AdvGenPriceComparer.Data.LiteDB.Entities;

public class AlertLogicEntity
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public List<string> Keywords { get; set; } = new List<string>();
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime UpdatedDate { get; set; }
}