using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public string GetAuthorString(int maxCount = 9999)
        {
            var authorString = string.Join("; ", Authors.OrderBy(x => x.Sequence).Take(maxCount).Select(y => y.FullName));

            if(Authors.Count > maxCount)
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

        public List<Author> Authors
        {
            get
            {
                return RawXml.Elements("contributor").Select(x => new Author(x)).ToList();
            }
        }
    }

    public class Author
    {

        public Author(XElement element)
        {
            Sequence = int.Parse((string)element.Attribute("sequence") ?? "9999");

            GivenName = (string)element.Attribute("givenname") ?? null;
            Surname = (string)element.Attribute("surname") ?? null;
            FullName = (string)element.Attribute("fullname") ?? null;
            MiddleName = (string)element.Attribute("middlename") ?? null;

            if (element.Elements("organization").Any())
            {
                OrganizationName = element.Element("organization").Value;
            }


        }
        public string GivenName { get; private set; }
        public string Surname { get; private set; }
        public string FullName { get; private set; }
        public string MiddleName { get; private set; }
        public string OrganizationName { get; private set; }
        public int Sequence { get; private set; }
    }
}
