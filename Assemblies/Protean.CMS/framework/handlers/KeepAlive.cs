using System;
using System.Web;
using System.Web.SessionState;

namespace Protean.Handlers
{
    /// <summary>
    /// HTTP Handler for delivering CMS pages
    /// </summary>
    public class KeepAlive : IHttpHandler, IRequiresSessionState
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
                    oCms.InitializeVariables();

                    //if (oCms.gbSingleLoginSessionPerUser)
                  //  {
                        oCms.Open();
                        oCms.mbSuppressLastPageOverrides = true;
                        oCms.LogSingleUserSession();
                  //  }                              

                    string timeoutSec = Convert.ToString(((context.Session.Timeout / 2) * 60) - 60);

                    context.Response.ContentType = "text/html";
                    context.Response.AddHeader("Refresh", timeoutSec);
                    // This overcomes a problem with zero length content not giving out a content type, which can cause browser to treat the file as a download
                    context.Response.Write("refresh every " + timeoutSec + "secs");

                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Trace.TraceError("KeepAlive error: {0}", ex.ToString());

                // Return appropriate error response
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/html";
                context.Response.Write("<html><body><h1>An error occurred</h1></body></html>");
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