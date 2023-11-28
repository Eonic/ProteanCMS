using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;

using System.Xml;
using System.Xml.XPath;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;


namespace Protean
{
    public class Proxy : MarshalByRefObject
    {
        public Assembly GetAssembly(string assemblyPath)
        {
            try
            {
                return Assembly.LoadFile(assemblyPath);
            }
            catch (Exception)
            {
                // throw new InvalidOperationException(ex);
                return null;
            }
        }
    }

   public static partial class stdTools
 {

        public static XmlNodeList SortedNodeList_UNCONNECTED(XmlElement oRootObject, string cXPath, string cSortItem, XmlSortOrder cSortOrder, XmlDataType cSortDataType, XmlCaseOrder cCaseOrder)
        {
            var oElmt = oRootObject.OwnerDocument.CreateElement("List");
            try
            {
                var oNavigator = oRootObject.CreateNavigator();
                var oExpression = oNavigator.Compile(cXPath);
                oExpression.AddSort(cSortItem, cSortOrder, cCaseOrder, "", cSortDataType);
                var oIterator = oNavigator.Select(oExpression);

                while (oIterator.MoveNext())
                    oElmt.InnerXml += oIterator.Current.OuterXml;

                return oElmt.ChildNodes;
            }
            catch (Exception)
            {
                return oElmt.ChildNodes;
            }
        }

        public static string removeTagFromXml(string xmlString, string tagNames)
        {

            tagNames = Strings.Replace(tagNames, " ", "");
            tagNames = Strings.Replace(tagNames, ",", "|");

            xmlString = Regex.Replace(xmlString, "<[/]?(" + tagNames + @":\w+)[^>]*?>", "", RegexOptions.IgnoreCase);

            return xmlString;

        }

        // Public Function addElement(ByRef oParent As XmlElement, ByVal cNewNodeName As String, Optional ByVal cNewNodeValue As String = "", Optional ByVal bAddAsXml As Boolean = False, Optional ByVal bPrepend As Boolean = False, Optional ByRef oNodeFromXPath As XmlNode = Nothing) As XmlElement
        // Dim parent As XmlNode = oParent
        // Return addElement(parent, cNewNodeName, cNewNodeValue, bAddAsXml, bPrepend, oNodeFromXPath)
        // End Function

        public class XmlNoNamespaceWriter : XmlTextWriter
        {

            private bool skipAttribute = false;

            public XmlNoNamespaceWriter(TextWriter writer) : base(writer)
            {
            }

            public XmlNoNamespaceWriter(Stream stream, System.Text.Encoding encoding) : base(stream, encoding)
            {
            }

            public override void WriteStartElement(string prefix, string localName, string ns)
            {
                base.WriteStartElement(null, localName, null);
            }

            public override void WriteStartAttribute(string prefix, string localName, string ns)
            {
                if (prefix.CompareTo("xmlns") == 0 || localName.CompareTo("xmlns") == 0)
                {
                    skipAttribute = true;
                }
                else
                {
                    base.WriteStartAttribute(null, localName, null);
                }
            }

            public override void WriteString(string text)
            {
                if (!skipAttribute)
                {
                    base.WriteString(text);
                }
            }

            public override void WriteEndAttribute()
            {
                if (!skipAttribute)
                {
                    base.WriteEndAttribute();
                }
                skipAttribute = false;
            }

            public override void WriteQualifiedName(string localName, string ns)
            {
                base.WriteQualifiedName(localName, null);
            }
        }


  }

}
