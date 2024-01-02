<%@ WebHandler Language="C#" Class="TestErrorAlert" %>
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Web;
using System.Xml;

public class TestErrorAlert : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        try
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            throw new System.Exception("this is a test");
        }
        catch (Exception ex)
        {
            string testTitle = "";
            Protean.stdTools.returnException(ref testTitle, "", "", ex,null,"",false,"New Error");
            context.Response.Write("<h1>Error Raised</h1>");
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}
