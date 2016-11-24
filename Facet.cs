﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summon.Core
{
    public class Facet
    {
        public string FieldName { get; set; }
        public int PageNumber { get; set; }
        public int PageWeight { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }

        public Facet()
        {
            PageNumber = 1;
            PageWeight = 10;
            Operator = "AND";
        }

        public string ToFacetFilterString()
        {
            return string.Join(",", new[] { FieldName, Operator, PageNumber.ToString(), PageWeight.ToString() });
        }

        public string ToFacetValueFilterString()
        {
            // DUCTTAPE: I am not 100% sure what that last parameter even does...
            return string.Join(",", new[] { FieldName, Value, "false" });
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
