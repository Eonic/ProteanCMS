using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using static Protean.Syndication.Distributor;

namespace Protean.Handlers
{
    /// <summary>
    /// HTTP Handler for delivering CMS pages
    /// </summary>
    public class DeliverAjaxElmt : IHttpHandler, IRequiresSessionState
    {
        private Cms oCms;

        /// <summary>
        /// Processes the incoming HTTP request and renders the CMS page
        /// </summary>
        /// <param name="context">The HTTP context</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                using (oCms = new Cms())
                {
                    NameValueCollection moConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

                    oCms.InitializeVariables();
                    oCms.Open();

                    if (moConfig["AjaxXsl"] != null && moConfig["AjaxXsl"] != "")
                    {
                        oCms.mcEwSiteXsl = moConfig["AjaxXsl"];
                    }
                    else
                    {
                        oCms.mcEwSiteXsl = "/ptn/core/ajax.xsl";
                    }

                    oCms.GetAjaxHTML();
                    moConfig = null;
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Trace.TraceError("DeliverPageHandler error: {0}", ex.ToString());

                // Return appropriate error response
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/html";
                context.Response.Write("<html><body><h1>An error occurred</h1></body></html>");
            }
            finally
            {
                // Ensure cleanup
                if (oCms != null)
                {
                    try
                    {
                        oCms.Dispose();
                    }
                    catch (Exception disposeEx)
                    {
                        System.Diagnostics.Trace.TraceError("Error disposing Cms object: {0}", disposeEx.ToString());
                    }
                    oCms = null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get { return false; } // Set to false for thread safety with session state
        }
    }
}