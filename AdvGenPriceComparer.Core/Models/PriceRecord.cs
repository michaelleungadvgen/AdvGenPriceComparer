using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.Core.Models
{
    public class PriceRecord
    {
        public Guid Id { get; set; }    
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public Place Place { get; set; }
        public Item Item { get; set; }
    }
}
