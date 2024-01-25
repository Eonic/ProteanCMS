<%@ WebHandler Language="C#" Class="ewLoadIcons" %>

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;
using System.Xml;
using Newtonsoft.Json.Linq;
using System.IO;

public class ewLoadIcons: IHttpHandler, IRequiresSessionState
{
    public void ProcessRequest(HttpContext context)
    {
        Protean.Cms oEw = new Protean.Cms();
        string jsonPath = oEw.goServer.MapPath("/ptn/core/icons/icons.json");
        JObject allIcons = JObject.Parse(File.ReadAllText(jsonPath));
        string key, svgValue, xmlList = "";
        JToken value;
        JObject token;
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        using (var stream = new StreamWriter(oEw.goServer.MapPath("/ptn/core/icons/icons.xml"), false))
        {
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("select1");
                writer.WriteStartElement("itemset");
                writer.WriteStartElement("h3");
                writer.WriteString("Web Application Icons");
                writer.WriteEndElement();
                writer.WriteStartElement("item");
                writer.WriteStartElement("label");
                writer.WriteString("none");
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                writer.WriteString("");
                writer.WriteEndElement();
                writer.WriteEndElement();
                foreach(KeyValuePair<string,JToken> kvp in allIcons)
                {
                    key = kvp.Key.ToString();
                    value = (JToken)kvp.Value;
                    token = (JObject)value.SelectToken("svg");
                    foreach (KeyValuePair<string, JToken> svgKey in token)
                    {
                        svgValue = svgKey.Key.ToString();
                        xmlList = "fa-" + svgValue + " fa-" + key;
                        writer.WriteStartElement("item");
                        writer.WriteStartElement("label");
                        writer.WriteString(xmlList);
                        writer.WriteEndElement();
                        writer.WriteStartElement("value");
                        writer.WriteString(xmlList);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        break;
                    }
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
        }

    }

    public bool IsReusable
    {
        get { return false; }
    }
}
