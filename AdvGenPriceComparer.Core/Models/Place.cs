using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.Core.Models
{
    public class Place
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string LogoUrl { get; set; }
        public string Location { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string StoreState { get; set; }
        public string StoreZip { get; set; }
        public string StoreCountry { get; set; }
        public string StoreLatitude { get; set; }
        public string StoreLongitude { get; set; }
        public string StoreHours { get; set; }
        public string StoreHoursNote { get; set; }
        public Dictionary<string, string> ExtraInformation { set; get; }
    }
}
