using System;
using System.Xml;

namespace Protean
{
    public static partial class xmlTools
    {
        public class ewXmlElement
        {

            private const string mcModuleName = "xmlTools.ewXmlElement";
            private XmlElement BaseXmlElement;

            public ewXmlElement(XmlElement xmlElement)
            {
                try
                {
                    BaseXmlElement = xmlElement;
                }
                catch (Exception)
                {
                    // returnException(mcModuleName, "AddElement", ex, "", "", gbDebug)
                }
            }

            public XmlElement XmlElement
            {
                get
                {
                    return BaseXmlElement;
                }
                set
                {
                    BaseXmlElement = value;
                }
            }

            public ewXmlElement AddElement(string localName, string innerText = null)
            {
                try
                {
                    var newElmt = new ewXmlElement(BaseXmlElement.OwnerDocument.CreateElement(localName));
                    if (innerText != null)
                    {
                        newElmt.XmlElement.InnerText = innerText;
                    }
                    BaseXmlElement.AppendChild(newElmt.XmlElement);
                    return newElmt;
                }
                catch (Exception)
                {
                    // returnException(mcModuleName, "AddElement", ex, "", "", gbDebug)
                    return null;
                }
            }

            public ewXmlElement AddMenuItem(string name, string id, string url, string dislayName = null, string description = null, int contentCount = default)
            {
                try
                {
                    var newElmt = AddElement("MenuItem");
                    newElmt.XmlElement.SetAttribute("id", id);
                    newElmt.XmlElement.SetAttribute("name", name);
                    newElmt.XmlElement.SetAttribute("url", url);
                    if (contentCount != default)
                    {
                        newElmt.XmlElement.SetAttribute("contentCount", contentCount.ToString());
                    }
                    newElmt.AddElement("DisplayName", dislayName);
                    newElmt.AddElement("Description", description);
                    return newElmt;
                }
                catch (Exception)
                {
                    // returnException(mcModuleName, "AddElement", ex, "", "", gbDebug)
                    return null;
                }
            }

        }

    }
}