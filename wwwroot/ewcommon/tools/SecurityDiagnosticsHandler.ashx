<%@ WebHandler Language="C#" Class="SecurityDiagnosticsHandler" %>

using System;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Reflection;
using System.Security.Policy;

public class SecurityDiagnosticsHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";

        context.Response.Write("=== AppDomain Security Check ===\n");
        context.Response.Write(string.Format("IsFullyTrusted: {0}\n", AppDomain.CurrentDomain.IsFullyTrusted));

            context.Response.Write("Current Identity: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name);

        try
        {
            var extensionType = typeof(Protean.xmlTools.xsltExtensions);
            Assembly asm = extensionType.Assembly;

            context.Response.Write("\n=== Extension Assembly Info ===\n");
            context.Response.Write(string.Format("Assembly Full Name: {0}\n", asm.FullName));
            context.Response.Write(string.Format("Is Fully Trusted: {0}\n", asm.IsFullyTrusted));

            Evidence evidence = asm.Evidence;
            foreach (var ev in evidence)
            {
                context.Response.Write(string.Format("Evidence: {0}\n", ev));
            }

            var zoneEvidence = evidence.GetHostEvidence<Zone>();
            if (zoneEvidence != null)
            {
                context.Response.Write(string.Format("Zone: {0}\n", zoneEvidence.SecurityZone));
            }
        }
        catch (Exception ex)
        {
            context.Response.Write("\nException during assembly check: " + ex.Message + "\n" + ex.StackTrace);
        }

context.Response.Write("\n=== XSLT Extension Test ===\n");

            
bool isFullyTrusted = AppDomain.CurrentDomain.IsFullyTrusted;
Console.WriteLine("Is Fully Trusted: " + isFullyTrusted);


try
{
    // Create test XML
    XmlDocument xmlDoc = new XmlDocument();
    xmlDoc.LoadXml("<Page><Request><ServerVariables/></Request></Page>");

    // XSLT string calling ew:cleanname
    string xsltString = @"
        <xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:ew='urn:ew'>
          <xsl:output method='xml' omit-xml-declaration='yes'/>
          <xsl:template match='/'>
            <xsl:value-of select='ew:cleanname(""Test Input!"")'/>
          </xsl:template>
        </xsl:stylesheet>";

    // Load the XSLT
    XslCompiledTransform xslt = new XslCompiledTransform();
    using (StringReader sr = new StringReader(xsltString))
    using (XmlReader xr = XmlReader.Create(sr))
    {
        xslt.Load(xr);
    }

    // Add extension object
    XsltArgumentList args = new XsltArgumentList();
    args.AddExtensionObject("urn:ew", new Protean.xmlTools.xsltExtensions());

    // Configure output to allow fragments
    XmlWriterSettings writerSettings = new XmlWriterSettings
    {
        ConformanceLevel = ConformanceLevel.Fragment
    };

    using (StringWriter sw = new StringWriter())
    using (XmlWriter xw = XmlWriter.Create(sw, writerSettings))
    {
        xslt.Transform(xmlDoc, args, xw);
        context.Response.Write("Result of ew:cleanname(): " + sw.ToString());
    }

        }
        catch (Exception ex)
        {
            context.Response.Write("XSLT extension test failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    public bool IsReusable { get { return false; } }
}
