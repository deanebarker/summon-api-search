using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Summon.Core.Fields
{
    public interface IField
    {
        void LoadFromXml(XElement element);
        string Value { get; }
        string Name { get; }
        XElement RawXml { get; }
        
    }
}
