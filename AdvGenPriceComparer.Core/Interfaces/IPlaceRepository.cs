using AdvGenPriceComparer.Core.Models;
using LiteDB;

namespace AdvGenPriceComparer.Core.Interfaces;

public interface IPlaceRepository
{
    ObjectId Add(Place place);
    bool Update(Place place);
    bool Delete(ObjectId id);
    bool SoftDelete(ObjectId id);
    Place? GetById(ObjectId id);
    IEnumerable<Place> GetAll();
    IEnumerable<Place> SearchByName(string name);
    IEnumerable<Place> GetByChain(string chain);
    IEnumerable<Place> GetBySuburb(string suburb);
    IEnumerable<Place> GetByState(string state);
    IEnumerable<Place> GetByLocation(double latitude, double longitude, double radiusKm);
    IEnumerable<string> GetAllChains();
    IEnumerable<string> GetAllSuburbs();
    IEnumerable<string> GetAllStates();
    int GetTotalCount();
    Dictionary<string, int> GetChainCounts();
}