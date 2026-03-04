using System.Threading.Tasks;
using AdvGenPriceComparer.WPF.Chat.Models;

namespace AdvGenPriceComparer.WPF.Chat.Services
{
    public interface IQueryRouterService
    {
        /// <summary>
        /// Route a query intent to the appropriate database and execute
        /// </summary>
        Task<ChatResponse> ExecuteQueryAsync(QueryIntent intent);
    }
}
