using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summon.Core
{
    public class TextQuery
    {
        public List<DictionaryEntry> Fields { get; set; }

        public static implicit operator string(TextQuery o) { return o.ToString(); }

        public string Query { get; set; }
        public TextQuery(string query = null)
        {
            Fields = new List<DictionaryEntry>();
            Query = query;
        }

        public void AddField(string fieldName, string value)
        {
            Fields.Add(new DictionaryEntry(fieldName, value));
        }

        public override string ToString()
        {
            var queryString = string.Join(" AND ", Fields.Select(x => string.Concat(x.Key, ":", x.Value)));

            if(Query != null)
            {
                queryString = string.Concat(queryString, " AND ", Query);
            }

            return queryString;
        }
    }
}
