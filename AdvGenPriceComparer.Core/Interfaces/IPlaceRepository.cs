using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

public interface IPlaceRepository
{
    string Add(Place place);
    bool Update(Place place);
    bool Delete(string id);
    bool SoftDelete(string id);
    Place? GetById(string id);
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