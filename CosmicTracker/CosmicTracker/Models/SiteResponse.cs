using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CosmicTracker.Models
{
    public class SiteResponse
    {
        public string message { get; set; }
        public long timestamp { get; set; }
        public Iss_position iss_position { get; set; }
        public Crew crew { get; set; }
    }
}
