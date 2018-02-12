using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace AutomatedQA
{
    [XmlRoot(ElementName = "App")]
    public class AppElementCollection
    {
        public AppElementCollection()
        {
            Regions = new List<Region>();
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string @Type { get; set; }

        [XmlAttribute]
        public string Path { get; set; }

        [XmlElement]
        public List<Region> Regions { get; set; }

        public Region this[string regionId]
        {
            get { return Regions.FirstOrDefault(r => r.Id == regionId); }
        }

        public static AppElementCollection LoadFromConfigFile(string filename)
        {
            return CommonHelper.DeSerialize<AppElementCollection>(filename);
        }
    }
}
