using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Summon.Core.Fields
{
    public class AuthorField : IField
    {
        public string Name { get; private set; }

        public XElement RawXml { get; private set; }

        public string Value
        {
            get
            {
                return GetAuthorString();
            }
        }

        public string GetAuthorString(int maxCount = 9999, bool includeMoreAuthorCount = true)
        {
            var authorString = string.Join("; ", Authors.Take(maxCount));

            if(Authors.Count > maxCount && includeMoreAuthorCount)
            {
                authorString = string.Concat(authorString, "; and ", (Authors.Count - maxCount), " more");
            }

            return authorString;
        }

        public void LoadFromXml(XElement element)
        {
            RawXml = element;
            Name = element.Attribute("name").Value;
        }

        public List<string> Authors
        {
            get
            {
                return RawXml.Elements("value").Select(x => x.Value).ToList();
            }
        }
    }
}
