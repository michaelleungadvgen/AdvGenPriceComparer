using LiteDB;

namespace AdvGenPriceComparer.Data.LiteDB.Utilities;

public static class ObjectIdHelper
{
    public static bool TryParseObjectId(string? id, out ObjectId objectId)
    {
        objectId = ObjectId.Empty;
        
        if (string.IsNullOrEmpty(id))
            return false;
            
        try
        {
            objectId = new ObjectId(id);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public static ObjectId ParseObjectIdOrDefault(string? id)
    {
        return TryParseObjectId(id, out var objectId) ? objectId : ObjectId.NewObjectId();
    }
}