using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core.Recommendations
{
	public class Recommendation
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public decimal Score { get; set; }
		public Uri Link { get; set; }

		public static Recommendation ParseXml(XElement element)
		{
			return new Recommendation()
			{
				Title = element.Attribute("title") != null ? element.Attribute("title").Value : null,
				Description = element.Attribute("description") != null ? element.Attribute("description").Value : null,
				Link = element.Attribute("link") != null ? new Uri(element.Attribute("link").Value) : null,
				Score = element.Attribute("score") != null ? Decimal.Parse(element.Attribute("score").Value) : 0
			};
		}
	}
}
