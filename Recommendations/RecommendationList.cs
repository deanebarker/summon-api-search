using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core.Recommendations
{
	public class RecommendationList
	{
		public string Type { get; set; }

		public List<Recommendation> Recommendations { get; set; }

		public RecommendationList()
		{
			Recommendations = new List<Recommendation>();
		}

		public static RecommendationList ParseXml(XElement doc)
		{
			var list = new RecommendationList()
			{
				Type = doc.Attribute("type").Value
			};
			foreach (var recommendationListElement in doc.Descendants("recommendation"))
			{
				list.Recommendations.Add(Recommendation.ParseXml(recommendationListElement));
			}
			return list;
		}
	}
}
