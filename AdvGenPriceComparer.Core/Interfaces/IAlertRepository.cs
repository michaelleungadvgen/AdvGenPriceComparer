using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

public interface IAlertRepository
{
    string Add(AlertLogicEntity alert);
    bool Update(AlertLogicEntity alert);
    bool Delete(string id);
    AlertLogicEntity? GetById(string id);
    IEnumerable<AlertLogicEntity> GetAll();
    IEnumerable<AlertLogicEntity> GetActiveAlerts();
    IEnumerable<AlertLogicEntity> GetUnreadAlerts();
    IEnumerable<AlertLogicEntity> GetAlertsByItem(string itemId);
    IEnumerable<AlertLogicEntity> GetAlertsByPlace(string placeId);
    IEnumerable<AlertLogicEntity> GetTriggeredAlerts(DateTime since);
    int GetUnreadCount();
    bool MarkAsRead(string id);
    bool MarkAllAsRead();
    bool Dismiss(string id);
    bool DismissAllRead();
}
