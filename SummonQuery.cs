using Summon.Core.Faceting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        public List<SummonDocument> Documents { get; set; }

        public bool Executed { get; private set; }

        public List<FacetField> FacetFields { get; set; }
        public string ErrorResponse { get; private set; }

        public List<DictionaryEntry> Parameters { get; set; }


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
        }

        public void AddFacet(Facet facet)
        {
            // DUCTTAPE: Knowing that certain facets can only be added once. ContentType, for instance. You can add it more than once, but the query will throw an error on execution.

            if(Executed)
            {
                throw new InvalidOperationSequenceException();
            }

            AddParameter(ParameterName.FacetField, facet.ToFacetFilterString());

            if(facet.HasValue)
            {
                AddParameter(ParameterName.FacetValueFilter, facet.ToFacetValueFilterString());
            }

        }

        public void AddFacet(string fieldName, string value = null)
        {
            AddFacet(new Facet(fieldName, value));
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
            if (Executed)
            {
                throw new InvalidOperationSequenceException();
            }

            if (Parameters == null)
            {
                Parameters = new List<DictionaryEntry>();
            }
            Parameters.Add(new DictionaryEntry(string.Concat("s.", key), value.ToString()));
        }

        public List<AppliedFacet> AppliedFacets
        {
            get
            {
                // TODO: I know there's a way to LINQ this up, but I couldn't figure it out...
                var appliedFacets = new List<AppliedFacet>();
                foreach(var facetField in FacetFields)
                {
                    foreach(var facetCount in facetField.FacetCounts)
                    {
                        if(facetCount.IsApplied)
                        {
                            appliedFacets.Add(new AppliedFacet() { Name = facetField.DisplayName, Value = facetCount.Value });
                        }
                    }
                }
                return appliedFacets;
            }
        }

        public SummonDocument GetByBookmark(string bookmark)
        {
            Parameters.Clear();
            AddParameter("bookMark", bookmark);
            AddParameter("pn", "1");
            Execute();
            return Documents.FirstOrDefault();
        }

        public void Execute(string query = null)
        {
            if(query != null)
            {
                AddParameter(ParameterName.TextQuery, query);
            }

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
            XDocument doc = null;

            try
            {
                doc = XDocument.Parse(client.DownloadString(url));
            }
            catch(WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    ErrorResponse = reader.ReadToEnd();
                }
                throw;
            }

            Executed = true;
            RawResponseXml = doc;       

            // Calc some pagination information
            TotalPages = int.Parse(doc.Root.Attribute("pageCount").Value);
            TotalRecords = int.Parse(doc.Root.Attribute("recordCount").Value);

            // Populate the Documents
            Documents = new List<SummonDocument>();
            foreach (var docElement in doc.Descendants("document"))
            {
                Documents.Add(SummonDocument.ParseXml(docElement));
            }

            // Populate the FacetFields
            FacetFields = new List<FacetField>();
            foreach(var facetFieldElement in doc.Root.Element("facetFields").Elements("facetField"))
            {
                FacetFields.Add(FacetField.ParseXml(facetFieldElement));
            }

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

    public class InvalidOperationSequenceException : InvalidOperationException
    {
        public InvalidOperationSequenceException() : base("This operation cannot be performed after the query has been executed")
        {

        }
           
    }
}
