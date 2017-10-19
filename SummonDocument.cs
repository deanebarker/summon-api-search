using Summon.Core.Fields;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core
{
    public class SummonDocument
    {
        private Dictionary<string, IField> fields { get; set; }
        public ReadOnlyDictionary<string, IField> Fields { get { return new ReadOnlyDictionary<string, IField>(fields); } }
        
        public static Dictionary<string, Type> FieldTypeMap = new Dictionary<string, Type>();

        static SummonDocument()
        {
            FieldTypeMap.Add("PublicationDate_xml", typeof(DateField));
            FieldTypeMap.Add("Copyright_xml", typeof(CopyrightField));
            FieldTypeMap.Add("Author", typeof(AuthorField));
        }

        public SummonDocument()
        {
            fields = new Dictionary<string, IField>();
        }

        public static SummonDocument ParseXml(XElement docElement)
        {
            var doc = new SummonDocument();

            // We're going to treat every attribute of the root element as a pseudo-field
            foreach(var attribute in docElement.Attributes())
            {
                var fieldName = string.Concat("@", attribute.Name);
                doc.AddField(fieldName, new Field(fieldName, attribute.Value));
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
                    doc.AddField(fieldName, field);
                }
                else
                {
                    doc.AddField(fieldName, new Field(fieldElement));
                }
            }
            doc.Link = docElement.Attribute("link").Value;
            return doc;
        }

        public void AddField(string key, IField field)
        {
            //DUCT TAPE: This ignores subsequent fields of the same key. This will effectively hide the second, third, etc. fields with the same key.
            if(Fields.ContainsKey(key))
            {
                return;
            }
            fields.Add(key, field);
        }

        public T GetField<T>(string key, string defaultValue = null) where T : IField
        {
            return (T)GetField(key, defaultValue);
        }

        public IField GetField(string key, string defaultValue = null)
        {
            if (!Fields.ContainsKey(key))
            {
                if(defaultValue != null)
                {
                    return new Field(key, defaultValue);
                }

                return null;
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
