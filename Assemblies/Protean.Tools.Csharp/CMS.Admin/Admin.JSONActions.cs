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
using System.Web.Configuration;
using Microsoft.VisualBasic;

namespace Protean.CMS.Admin
{
    public class JSONActions
    {
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        private const string mcModuleName = "Eonic.Content.JSONActions";
        private System.Collections.Specialized.NameValueCollection moLmsConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/lms");
        private Protean.Cms myWeb;
        private Protean.Cms.Cart myCart;

        public JSONActions()
        {
            string ctest = "this constructor is being hit"; // for testing
            myWeb = new Protean.Cms();
            myWeb.InitializeVariables();
            myWeb.Open();
            myCart = new Protean.Cms.Cart(myWeb);
        }

        public string CheckRedirects(ref Protean.API myApi, ref Newtonsoft.Json.Linq.JObject jObj)
        {
            try
            {
                long PageId;
                long ArticleId;
                string NewPageName;


                string OldURL;
                string NewURL;



                // Validate that the user is an administrator with session user ID

                // Check we do not have a redirect for the OLD URL allready.

                // Determine all the paths that need to be redirected


                string JsonResult = "";
                return JsonResult;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                return ex.Message;
            }
        }
    }
}
