using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core.Faceting
{
    public class FacetCount
    {
        public long Count { get; set; }
        public string Value { get; set; }
        public bool IsApplied { get; set; }

        public static FacetCount ParseXml(XElement facetCountElement)
        {
            var facetCount = new FacetCount()
            {
                Value = facetCountElement.Attribute("value").Value,
                Count = long.Parse(facetCountElement.Attribute("count").Value),
                IsApplied = bool.Parse(facetCountElement.Attribute("isApplied").Value)
            };

            return facetCount;
        }
    }
}
