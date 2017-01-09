using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core.Fields
{
    public class DateField : IField
    {
        public string Name { get; private set; }

        public XElement RawXml { get; private set; }

        public string Value
        {
            get
            {
                return RawXml.Element("datetime").Attribute("text").Value;
            }
        }

        public void LoadFromXml(XElement element)
        {
            RawXml = element;
            Name = element.Attribute("name").Value;
        }

        public DateTime? DateValue
        {
            get
            {
                var dateTimeElement = RawXml.Element("datetime");

                var month = int.Parse((string)dateTimeElement.Attribute("month") ?? "1");
                var year = int.Parse((string)dateTimeElement.Attribute("year") ?? DateTime.MinValue.ToString());
                var day = int.Parse((string)dateTimeElement.Attribute("day") ?? "1");
                return new DateTime(year, month, day);
            }
        }
    }
}
