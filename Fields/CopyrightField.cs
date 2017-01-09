using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core.Fields
{
    public class CopyrightField : IField
    {
        public string Name { get; private set; }

        public XElement RawXml { get; private set; }

        public string Value
        {
            get
            {
                return Values.FirstOrDefault();
            }
        }

        public void LoadFromXml(XElement element)
        {
            RawXml = element;
            Name = element.Attribute("name").Value;
        }

        public List<string> Values
        {
            get
            {
                return RawXml.Elements("copyright").Select(x => x.Attribute("notice").Value).ToList();
            }
        }
    }
}
