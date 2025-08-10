using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Data.LiteDB.Entities;
using AdvGenPriceComparer.Data.LiteDB.Utilities;

namespace AdvGenPriceComparer.Data.LiteDB.Repositories;

public class PlaceRepository : IPlaceRepository
{
    private readonly DatabaseService _database;

    public PlaceRepository(DatabaseService database)
    {
        _database = database;
    }

    public string Add(Place place)
    {
        place.DateAdded = DateTime.UtcNow;
        var entity = PlaceEntity.FromPlace(place);
        var insertedId = _database.Places.Insert(entity);
        return insertedId.ToString();
    }

    public bool Update(Place place)
    {
        var entity = PlaceEntity.FromPlace(place);
        return _database.Places.Update(entity);
    }

    public bool Delete(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return false;
        return _database.Places.Delete(objectId);
    }

    public bool SoftDelete(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return false;
        
        var entity = _database.Places.FindById(objectId);
        if (entity == null) return false;
        
        entity.IsActive = false;
        return _database.Places.Update(entity);
    }

    public Place? GetById(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return null;
        var entity = _database.Places.FindById(objectId);
        return entity?.ToPlace();
    }

    public IEnumerable<Place> GetAll()
    {
        return _database.Places.FindAll().Where(x => x.IsActive).Select(x => x.ToPlace());
    }

    public IEnumerable<Place> SearchByName(string name)
    {
        return _database.Places
            .Find(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase) && x.IsActive)
            .Select(x => x.ToPlace());
    }

    public IEnumerable<Place> GetByChain(string chain)
    {
        return _database.Places
            .Find(x => x.Chain == chain && x.IsActive)
            .Select(x => x.ToPlace());
    }

    public IEnumerable<Place> GetBySuburb(string suburb)
    {
        return _database.Places
            .Find(x => x.Suburb == suburb && x.IsActive)
            .Select(x => x.ToPlace());
    }

    public IEnumerable<Place> GetByState(string state)
    {
        return _database.Places
            .Find(x => x.State == state && x.IsActive)
            .Select(x => x.ToPlace());
    }

    public IEnumerable<Place> GetByLocation(double latitude, double longitude, double radiusKm)
    {
        // Simple distance calculation - for more accurate results, consider using a proper geospatial library
        return _database.Places
            .Find(x => x.IsActive && x.Latitude.HasValue && x.Longitude.HasValue)
            .Where(x => 
            {
                var distance = CalculateDistance(latitude, longitude, x.Latitude!.Value, x.Longitude!.Value);
                return distance <= radiusKm;
            })
            .Select(x => x.ToPlace());
    }

    public IEnumerable<string> GetAllChains()
    {
        return _database.Places
            .Find(x => x.IsActive && !string.IsNullOrEmpty(x.Chain))
            .Select(x => x.Chain!)
            .Distinct()
            .OrderBy(x => x);
    }

    public IEnumerable<string> GetAllSuburbs()
    {
        return _database.Places
            .Find(x => x.IsActive && !string.IsNullOrEmpty(x.Suburb))
            .Select(x => x.Suburb!)
            .Distinct()
            .OrderBy(x => x);
    }

    public IEnumerable<string> GetAllStates()
    {
        return _database.Places
            .Find(x => x.IsActive && !string.IsNullOrEmpty(x.State))
            .Select(x => x.State!)
            .Distinct()
            .OrderBy(x => x);
    }

    public int GetTotalCount()
    {
        return _database.Places.Count(x => x.IsActive);
    }

    public Dictionary<string, int> GetChainCounts()
    {
        return _database.Places
            .Find(x => x.IsActive && !string.IsNullOrEmpty(x.Chain))
            .GroupBy(x => x.Chain!)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Haversine formula for calculating distance between two coordinates
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}