using System;
using System.Web;
using System.Web.SessionState;
using System.IO;

namespace Protean.Handlers
{
    /// <summary>
    /// HTTP Handler for delivering CMS pages
    /// </summary>
    public class UploadFiles : IHttpHandler, IRequiresSessionState
    {
        private Cms oCms;

        public void ProcessRequest(HttpContext context)
        {

            // First lets check for the expected session variable so we don't get hacked
            if (context.Session["allowUpload"] != null && context.Session["allowUpload"].ToString() == "True")
            {
                context.Response.Write("Upload result:<br>"); // At least one symbol should be sent to response!!!

                string folderToSave = context.Server.MapPath("\\" + context.Request["targetUrl"]);

                // Eonic.fsHelper fsHelper = new Eonic.fsHelper();

                if (context.Request.Files.Count > 0)
                {
                    HttpPostedFile myFile = context.Request.Files[0];
                    if (myFile != null && !string.IsNullOrEmpty(myFile.FileName))
                    {
                        string fileName = Path.GetFileName(myFile.FileName);
                        myFile.SaveAs(Path.Combine(folderToSave, fileName));
                        context.Response.Write("File " + myFile.FileName + " succesfully saved.<br>");
                    }
                    else
                    {
                        context.Response.Write("No files sent. Script is OK!"); // Say to Flash that script exists and can receive files
                    }
                }
                else
                {
                    context.Response.Write("No files sent. Script is OK!");
                }
            }
            else
            {
                context.Response.Write("No files sent. Session Invalid!");
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