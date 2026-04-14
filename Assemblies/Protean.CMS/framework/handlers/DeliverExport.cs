using System;
using System.Web;
using System.Web.SessionState;

namespace Protean.Handlers
{
    /// <summary>
    /// HTTP Handler for delivering CMS pages
    /// </summary>
    public class DeliverExport: IHttpHandler, IRequiresSessionState
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
                // Declarations
                string contentType = "";
                string siteXSL = "";
                string fileExtension = "";

                // Get the values from request string
                string mode = context.Request["format"] ?? "";
                string filename = context.Request["ewCmd"] + DateTime.Now.ToString("yyyyMMddhhmmss");
                string reportXsl = context.Request["reportXsl"] ?? "";

                // Customise the output
                // Default is Excel
                Protean.fsHelper ofs = new Protean.fsHelper();

                switch (mode.ToLower())
                {
                    case "txt":
                        contentType = "text/plain";
                        if (!string.IsNullOrEmpty(reportXsl))
                        {
                            siteXSL = ofs.checkCommonFilePath("/admin/reports/" + reportXsl + ".xsl");
                        }
                        else
                        {
                            siteXSL = ofs.checkCommonFilePath("/admin/reports/txt.xsl");
                        }
                        fileExtension = "txt";
                        break;

                    case "csv":
                        // contentType = "text/csv"
                        contentType = "text/plain";
                        if (!string.IsNullOrEmpty(reportXsl))
                        {
                            siteXSL = ofs.checkCommonFilePath("/admin/reports/" + reportXsl + ".xsl");
                        }
                        else
                        {
                            siteXSL = ofs.checkCommonFilePath("/admin/reports/csv.xsl");
                        }
                        fileExtension = "csv";
                        break;

                    case "xml":
                        contentType = "text/xml";
                        if (!string.IsNullOrEmpty(reportXsl))
                        {
                            siteXSL = ofs.checkCommonFilePath("/admin/reports/" + reportXsl + ".xsl");
                        }
                        else
                        {
                            siteXSL = ofs.checkCommonFilePath("/admin/reports/xml.xsl");
                        }
                        fileExtension = "xml";
                        break;

                    default:
                        contentType = "application/vnd.ms-excel";
                        if (!string.IsNullOrEmpty(reportXsl))
                        {
                            siteXSL = ofs.checkCommonFilePath("/admin/reports/" + reportXsl + ".xsl");
                        }
                        else
                        {
                            siteXSL = ofs.checkCommonFilePath("/admin/reports/formats/Report-Excel-2000.xsl");
                        }
                        fileExtension = "xlsx";
                        break;
                }

                using (oCms = new Cms())
                {
                    oCms.InitializeVariables();
                    oCms.mcEwSiteXsl = siteXSL;

                    // Determine whether to show the XML or not
                    if (context.Request["format"] == "rawxml")
                    {
                        oCms.mcContentType = "application/xml";
                        oCms.mbOutputXml = true;
                    }
                    else
                    {
                        // Set the response headers
                        oCms.mcContentType = contentType;
                        oCms.mcContentDisposition = "attachment;filename=" + filename + "." + fileExtension;
                        // oEw.mcContentType = "application/xml"
                    }

                    // Get the output
                    oCms.GetPageHTML();
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Trace.TraceError("DeliverExport error: {0}", ex.ToString());

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