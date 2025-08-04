namespace AdvGenPriceComparer.Core.Models;

public class PriceRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Price { get; set; }
    public required Place Place { get; set; }
    public required Item Item { get; set; }
}
