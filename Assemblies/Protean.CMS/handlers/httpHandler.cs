using System.Web;
using System.Web.SessionState;

namespace Protean
{

    public class WebHttpHandler : IHttpHandler, IRequiresSessionState
    {

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {

            var oEw = new Cms();

            if (!string.IsNullOrEmpty(context.Request["xml"]))
            {
                oEw.mbOutputXml = true;
            }

            oEw.InitializeVariables();
            context.Response.ContentType = "text/html";
            oEw.GetPageHTML();

            oEw = null;
        }


    }
}