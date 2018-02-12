using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomatedQA
{
    [Serializable]
    public class Region
    {
        public Region()
        {
            Elements = new List<Element>();
        }

        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string ReferRegions { get; set; }

        private List<string> _referRegions;

        public List<string> ReferRegionList {
            get
            {
                if (string.IsNullOrEmpty(ReferRegions))
                    return null;
                return _referRegions ?? (_referRegions = ReferRegions.Split(';').ToList());
            }
        }

        [XmlElement("Element")]
        public List<Element> Elements { get; set; }

        public Element this[string elementId]
        {
            get { return Elements.FirstOrDefault(e => e.Id == elementId); }
        }
    }
}
