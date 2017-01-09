using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core.Fields
{
    public class Field : IField
    {
        public string Name { get; set; }

        public string Value
        {
            get
            {
                return Values.FirstOrDefault();
            }
        }

        public XElement RawXml { get; private set; }

        public List<string> Values { get; private set; }

        public Field(XElement element)
        {
            LoadFromXml(element);
        }

        public Field(string name, string value)
        {
            Name = name;
            Values = new List<string>() { value };
        }

        public void LoadFromXml(XElement element)
        {
            RawXml = element;
            Name = element.Attribute("name").Value;

            // We have to parse the XML here, rather than doing it in the Values property getter because a simple field might not have an XML backing
            // e.g. if you create a field like "new Field(name, value)" then there is no XML "behind" the field
            Values = new List<string>();
            foreach (var valueElement in element.Elements("value"))
            {
                Values.Add(valueElement.Value);
            }
        }
    }
}
