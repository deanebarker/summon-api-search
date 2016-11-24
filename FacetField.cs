using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core
{
    public class FacetField
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public List<FacetCount> FacetCounts { get; set; }

        public static FacetField ParseXml(XElement facetFieldElement)
        {
            var facetField = new FacetField()
            {
                Name = facetFieldElement.Attribute("fieldName").Value,
                DisplayName = facetFieldElement.Attribute("displayName").Value,
            };

            facetField.FacetCounts = new List<FacetCount>();

            foreach(var facetCountElement in facetFieldElement.Descendants("facetCount"))
            {
                facetField.FacetCounts.Add(FacetCount.ParseXml(facetCountElement));
            }

            return facetField;

        }
    }
}
