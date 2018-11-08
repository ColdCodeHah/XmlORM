using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADS.Utilities.Xml
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ADSXmlNode:Attribute
    {
        public string NodeName { get; set; }

        public ADSXmlNode(string nodeName)
        {
            this.NodeName = nodeName;
        }
    }
}
