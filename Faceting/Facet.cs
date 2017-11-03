using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summon.Core.Faceting
{
    public class Facet
    {
        public string FieldName { get; set; }
        public int PageNumber { get; set; }
        public int PageWeight { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public bool Negate { get; set; }

        public IList<string> WhiteList { get; set; }

        public Facet(string fieldName, string value = null, bool negate = false, int pageNumber = 1, int pageWeight = 10, IList<string> whiteList = null)
        {
            PageNumber = pageNumber;
            PageWeight = pageWeight;
            Operator = "OR";

            FieldName = fieldName;
            Value = string.IsNullOrWhiteSpace(value) ? null : value;
            Negate = negate;
            WhiteList = whiteList != null ? whiteList : new List<string>();
        }

        public string ToFacetFilterString()
        {
            return string.Join(",", new[] { FieldName, Operator, (WhiteList != null ? string.Join(":", WhiteList) : PageNumber.ToString()), PageWeight.ToString() });
        }

        public string ToFacetValueFilterString()
        {
            return string.Join(",", new[] { FieldName, Value, Negate.ToString().ToLower() });
        }

        public bool HasValue
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Value);
            }
        }
    }
}
