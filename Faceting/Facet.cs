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

        public Facet(string fieldName, string value = null, bool negate = false)
        {
            PageNumber = 1;
            PageWeight = 10;
            Operator = "OR";

            FieldName = fieldName;
            Value = string.IsNullOrWhiteSpace(value) ? null : value;
            Negate = negate;
        }

        public string ToFacetFilterString()
        {
            return string.Join(",", new[] { FieldName, Operator, PageNumber.ToString(), PageWeight.ToString() });
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
