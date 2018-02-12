using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomatedQA
{
    public class Element
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public string Signature { get; set; }
    }
}
