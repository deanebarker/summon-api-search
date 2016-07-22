using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core
{
    public class SummonDocument
    {
        public Dictionary<string, List<string>> Fields { get; set; }

        public SummonDocument()
        {
            Fields = new Dictionary<string, List<string>>();
        }

        public static SummonDocument ParseXml(XElement docElement)
        {
            var doc = new SummonDocument();
            foreach (var fieldElement in docElement.Elements("field"))
            {
                var name = fieldElement.Attribute("name").Value;
                var values = new List<string>();
                foreach (var valueElement in fieldElement.Elements("value"))
                {
                    values.Add(valueElement.Value);
                }
                doc.Fields.Add(name, values);
            }
            doc.Link = docElement.Attribute("link").Value;
            return doc;
        }

        public string GetField(string key, string defaultValue = null)
        {
            if (!Fields.ContainsKey(key))
            {
                if(defaultValue != null)
                {
                    return defaultValue;
                }
                throw new ArgumentException("No field for key: " + key);
            }

            if (Fields[key].Count == 0)
            {
                return defaultValue;
            }

            return Fields[key].First();
        }

        public bool HasField(string key)
        {
            return Fields.ContainsKey(key);
        }

        public string Title
        {
            get
            {
                return GetField("Title", string.Empty);
            }
        }

        public string PublicationTitle
        {
            get { return GetField("PublicationTitle", string.Empty); }
        }

        public string PublicationYear
        {
            get { return GetField("PublicationYear", string.Empty); }
        }

        public string Authors
        { 
            get
            {
                if (!Fields.ContainsKey("Author") || Fields["Author"].Count == 0)
                {
                    return string.Empty;
                }

                if(Fields["Author"].Count > 4)
                {
                    return string.Join("; ", Fields["Author"].Take(4)) + " ( + " + (Fields["Author"].Count - 4) + " more)";
                }

                return string.Join("; ", Fields["Author"]);
            }
        }

        public string Link { get; private set; }

        public string Summary
        {
            get { return GetField("Snippet", string.Empty); }
        }

        public DateTime? Date
        {
            get
            {
                if (!HasField("PublicationDate"))
                {
                    return new DateTime?();
                }
                return new DateTime?(DateTime.Parse(GetField("PublicationDate")));
            }
        }

        public string Type
        {
            get
            {
                return GetField("ContentType", string.Empty);
            }
        }

        public string SmallImage
        {
            get
            {
                return GetField("thumbnail_s", string.Empty);
            }
        }

        public string MediumImage
        {
            get
            {
                return GetField("thumbnail_m", string.Empty);
            }
        }

        public string LargeImage
        {
            get
            {
                return GetField("thumbnail_l", string.Empty);
            }
        }

        public bool HasSmallImage
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SmallImage);
            }
        }

        public bool HasMediumImage
        {
            get
            {
                return !string.IsNullOrWhiteSpace(MediumImage);
            }
        }

        public bool HasLargeImage
        {
            get
            {
                return !string.IsNullOrWhiteSpace(MediumImage);
            }
        }

        public bool HasAuthors
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Authors);
            }
        }

        public bool HasSummary
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Summary);
            }
        }
    }
}
