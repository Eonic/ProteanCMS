using System;
using System.Web;
using System.Web.SessionState;

namespace Protean.Handlers
{
    /// <summary>
    /// HTTP Handler for delivering CMS pages
    /// </summary>
    public class Setup : IHttpHandler, IRequiresSessionState
    {
        private Protean.Setup oSetup;

        /// <summary>
        /// Processes the incoming HTTP request and renders the CMS page
        /// </summary>
        /// <param name="context">The HTTP context</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                using (oSetup = new Protean.Setup())
                {
                    oSetup.GetPageHTML();
                }
            }
            catch (Exception ex)
            {
                //ingore if error allready handled.
                if (context.Response.StatusCode != 500) {

                    // Log error
                    System.Diagnostics.Trace.TraceError("DeliverPageHandler error: {0}", ex.ToString());

                    // Return appropriate error response
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/html";
                    context.Response.Write("<html><body><h1>ProteanCMS Handler Error</h1><P>" + ex.Message + "</P></body></html>");
                }
            }
            finally
            {
                // Ensure cleanup
                if (oSetup != null)
                {
                    try
                    {
                        oSetup.Dispose();
                    }
                    catch (Exception disposeEx)
                    {
                        System.Diagnostics.Trace.TraceError("Error disposing oSetup object: {0}", disposeEx.ToString());
                    }
                    oSetup = null;
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