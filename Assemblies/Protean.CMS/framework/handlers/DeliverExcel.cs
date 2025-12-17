using System;
using System.Web;
using System.Web.SessionState;

namespace Protean.Handlers
{
    /// <summary>
    /// HTTP Handler for delivering CMS pages
    /// </summary>
    public class DeliverExcel : IHttpHandler, IRequiresSessionState
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
                    oCms.mcEwSiteXsl = "/ptn/admin/reports/excel_2000.xsl";
                    oCms.mcContentType = "application/vnd.ms-excel";
                    context.Response.ContentType = "application/vnd.ms-excel";
                    context.Response.AddHeader("Content-Disposition", "attachment;filename=" + context.Request["ewCmd"] + ".xls");
                    oCms.GetPageHTML();
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