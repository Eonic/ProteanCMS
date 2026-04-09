<%@ WebHandler Language="C#" Class="TestBundleCSS" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Xml;

public class TestBundleCSS : IHttpHandler, IRequiresSessionState
{
    public void ProcessRequest(HttpContext context)
    {  
        Protean.Cms oEw = new Protean.Cms();
        Protean.stdTools.gbDebug = false;
        Protean.xmlTools.xsltExtensions xsltExt = new Protean.xmlTools.xsltExtensions(ref oEw);

        context.Response.Write(xsltExt.BundleCSS("/ewthemes/IntoTheBlue2019/css/bootstrapBase.less","/bundles/test").ToString());
	    context.Response.Write("<br/>");
	    context.Response.Write("<h1>Complete</h1>");
      
    }

    public bool IsReusable
    {
        get { return false; }
    }
}