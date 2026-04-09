<%@ WebHandler Language="C#" Class="Handler" %>

using System;
using System.Web;
using Microsoft.VisualBasic;
using Protean;
   using System.Drawing;
    using Imazen.WebP;
using System.IO;


public class Handler : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        context.Response.Write("Hello World");

                string cVirtualPath = "/ptn/admin/skin/images/logosquare.png";
                string webpFileName = "/ptn/admin/skin/images/logosquare.webp";
                string newFilepath = string.Empty;
                var oEw = new Cms();
                oEw.InitializeVariables();
                try
                {
                   // oEw.moFSHelper.DeleteFile(webpFileName);
                }
                catch {
                };
    
                short WebPQuality = 60;
                using (var bitMap = new Bitmap(oEw.goServer.MapPath(cVirtualPath)))
                        {
                            var saveImageStream = File.Open(oEw.goServer.MapPath(webpFileName), FileMode.Create);
                            var encoder = new Imazen.WebP.SimpleEncoder();
                            encoder.Encode(bitMap, saveImageStream, WebPQuality);
                            encoder = null;
                            context.Response.Write("Protean Logo converted to WebP <img src='" + webpFileName + "'/>");                            
                        }          


    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}