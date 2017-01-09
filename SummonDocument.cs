using Summon.Core.Fields;
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
        public Dictionary<string, IField> Fields { get; set; }
        
        public static Dictionary<string, Type> FieldTypeMap = new Dictionary<string, Type>();

        static SummonDocument()
        {
            FieldTypeMap.Add("PublicationDate_xml", typeof(DateField));
            FieldTypeMap.Add("Copyright_xml", typeof(CopyrightField));
            FieldTypeMap.Add("Author_xml", typeof(AuthorField));
        }

        public SummonDocument()
        {
            Fields = new Dictionary<string, IField>();
        }

        public static SummonDocument ParseXml(XElement docElement)
        {
            var doc = new SummonDocument();

            // We're going to treat every attribute of the root element as a pseudo-field
            foreach(var attribute in docElement.Attributes())
            {
                var fieldName = string.Concat("@", attribute.Name);
                doc.Fields.Add(fieldName, new Field(fieldName, attribute.Value));
            }

            // These are the actual fields. They are of type Field, unless another type has been mapped for the field name.
            foreach (var fieldElement in docElement.Elements("field"))
            {
                var fieldName = fieldElement.Attribute("name").Value;
                if(FieldTypeMap.ContainsKey(fieldName))
                {
                    // This is a custom IField implementation for this particular field name
                    var field = (IField)Activator.CreateInstance(FieldTypeMap[fieldName]);
                    field.LoadFromXml(fieldElement);
                    doc.Fields.Add(fieldName, field);
                }
                else
                {
                    doc.Fields.Add(fieldName, new Field(fieldElement));
                }
            }
            doc.Link = docElement.Attribute("link").Value;
            return doc;
        }

        public IField GetField(string key, string defaultValue = null)
        {
            if (!Fields.ContainsKey(key))
            {
                if(defaultValue != null)
                {
                    return new Field(key, defaultValue);
                }

                throw new ArgumentException("No field for key: " + key);
            }

            return Fields[key];
        }

        public bool HasField(string key)
        {
            return Fields.ContainsKey(key);
        }

        public string Title
        {
            get
            {
                return GetField("Title", string.Empty).Value;
            }
        }

        public string PublicationTitle
        {
            get { return GetField("PublicationTitle", string.Empty).Value; }
        }

        public string PublicationYear
        {
            get { return GetField("PublicationYear", string.Empty).Value; }
        }

        public string Link { get; private set; }

        public string Summary
        {
            get { return GetField("Snippet", string.Empty).Value; }
        }

        public DateTime? Date
        {
            get
            {
                if (!HasField("PublicationDate"))
                {
                    return new DateTime?();
                }
                return new DateTime?(DateTime.Parse(GetField("PublicationDate").Value));
            }
        }

        public string Type
        {
            get
            {
                return GetField("ContentType", string.Empty).Value;
            }
        }

        public string SmallImage
        {
            get
            {
                return GetField("thumbnail_s", string.Empty).Value;
            }
        }

        public string MediumImage
        {
            get
            {
                return GetField("thumbnail_m", string.Empty).Value;
            }
        }

        public string LargeImage
        {
            get
            {
                return GetField("thumbnail_l", string.Empty).Value;
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

        public bool HasSummary
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Summary);
            }
        }
    }
}
