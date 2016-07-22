using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace Summon.Core
{
    public class SummonQuery
    {
        public string AccessId { get; set; }
        public string SecretKey { get; set; }
        public string Host { get; set; }
        public string DateFormat { get; set; }
        public string AcceptHeader { get; set; }
        public string QueryPath { get; set; }
        public XDocument RawResponseXml { get; private set; }
        public int PageNumber { get; set; }
        public int PageWeight { get; set;  }
        public int TotalPages { get; private set; }
        public int TotalRecords { get; private set; }
        public List<Facet> TypeFacets { get; private set; }
        public List<SummonDocument> Documents { get; set; }

        // TODO: This shouldn't be a Dictionary. The same key can technically be added twice.
        // Should be a NameValueCollection or a List<DictionaryEntry>
        public Dictionary<string, string> Parameters { get; set; }


        public SummonQuery(string accessId, string secretKey)
        {
            AccessId = accessId;
            SecretKey = secretKey;


            Host = "api.summon.serialssolutions.com";
            DateFormat = "ddd, dd MMM yyyy HH:mm:ss 'GMT'";
            AcceptHeader = "application/xml";
            QueryPath = "/2.0.0/search";
            PageNumber = 1;
            PageWeight = 10;

            // TODO: This is pretty hacked up. I'm not sure we 100% understand how Summon references facets (shouldn't a facet have an ID or something?)
            var facetsToAdd = "Journal Article,Newspaper Article,Magazine Article,Newsletter,Book / eBook,Book Review,Reference,Trade Publication Article,Book Chapter,Pamphlet,Audio Recording,Report,Conference Proceeding,Video Recording";
            TypeFacets = new List<Facet>();
            facetsToAdd.Split(',').ToList().ForEach(x => TypeFacets.Add(new Facet() { Id = DeriveFacetIdFromName(x), Name = x }));
        }

        // A utility method to get the page number from the querystring, perform some logic, and assign
        public void SetPageNumberFromQuerystring(string key = "p")
        {
            if(HttpContext.Current == null)
            {
                return;
            }

            var querystring = HttpContext.Current.Request.QueryString;
            if(querystring[key] == null)
            {
                return;
            }
            PageNumber = int.Parse(querystring[key]);

            if(PageNumber == 0)
            {
                PageNumber = 1;
            }
        }

        public void AddParameter(string key, object value)
        {
            if(Parameters == null)
            {
                Parameters = new Dictionary<string, string>();
            }
            Parameters.Add(string.Concat("s.", key), value.ToString());
        }

        public void Execute()
        {
            // These two properties are strongly-typed so they can be used in the interface to assist in calculatin pagination
            // ---

            // If they have set a page number other than 1, pass that in
            if(PageNumber != 1)
            {
                AddParameter("pn", PageNumber);
            }

            // Pass in the page weight
            AddParameter("ps", PageWeight);

            // ---


            var date = DateTime.UtcNow.ToString(DateFormat);

            // Create the querystring from the parameters
            var queryString = string.Join("&", Parameters.OrderBy(x => x.Key).Select(x => string.Concat(x.Key, "=", x.Value)));

            // Create the ID string
            var idString = string.Concat(string.Join("\n", new[] { AcceptHeader, date, Host, QueryPath, queryString }), "\n");

            // Compute the digest od the ID string
            // First, SHA1 using the secret key
            // Second, Base64
            byte[] keyBytes = Encoding.UTF8.GetBytes(SecretKey);
            byte[] inputBytes = Encoding.UTF8.GetBytes(idString);
            var digest = string.Empty;
            using (var hmacsha1 = new HMACSHA1(keyBytes))
            {
                byte[] hashmessage = hmacsha1.ComputeHash(inputBytes);
                digest = Convert.ToBase64String(hashmessage);
            }

            // Create the auth header 
            var authHeader = string.Concat("Summon ", AccessId, ";", digest);

            // Make the request
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.Headers.Add("Accept", AcceptHeader);
            client.Headers.Add("x-summon-date", date);
            client.Headers.Add("Authorization", authHeader);
            var url = string.Concat("http://", Host, QueryPath, "?", queryString);

            // Parse the XML response           
            var doc = XDocument.Parse(client.DownloadString(url));
            RawResponseXml = doc;

            // Calc some pagination information
            TotalPages = int.Parse(doc.Root.Attribute("pageCount").Value);
            TotalRecords = int.Parse(doc.Root.Attribute("recordCount").Value);

            // Populate the facets
            // TODO: this assumes ONLY ContentType facts, which is not legit for broader applications
            foreach(var facetCount in doc.Descendants("facetCount"))
            {
                var thisFacet = TypeFacets.Where(x => x.Id == DeriveFacetIdFromName(facetCount.Attribute("value").Value)).FirstOrDefault();
                if(thisFacet != null)
                {
                    thisFacet.Count = int.Parse(facetCount.Attribute("count").Value);
                }
            }

            // Populate the documents
            Documents = new List<SummonDocument>();
            foreach (var docElement in doc.Descendants("document"))
            {
                Documents.Add(SummonDocument.ParseXml(docElement));
            }
        }

        // This gets a simplified string for easy referencing and querystring embedded of facet names (again, shouldn't facets have IDs?)
        public static string DeriveFacetIdFromName(string name)
        {
            name = Regex.Replace(name, @"\W", "-");
            name = name.Replace(" ", "-");
            name = Regex.Replace(name, "-{2,100}", "-");
            return name.ToLower().Trim();
        }

        // Pagination stuff
        // ---------------

        public bool HasNextPage
        {
            get
            {
                return (PageNumber < TotalPages);
            }
        }

        public bool HasPrevPage
        {
            get
            {
                return (PageNumber != 1);
            }
        }

        public int NextPageNumber
        {
            get
            {
                return PageNumber + 1;
            }
        }

        public int PrevPageNumber
        {
            get
            {
                return PageNumber - 1;
            }
        }

        public int FirstRecord
        {
            get
            {
                return ((PageNumber - 1) * PageWeight) + 1;
            }
        }

        public int LastRecord
        {
            get
            {
                var theoreticalLastRecord = PageNumber * PageWeight;
                return theoreticalLastRecord > TotalRecords ? TotalRecords : theoreticalLastRecord;
            }
        }


    }

    public class Facet
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int Count { get; set; }
    }
}
