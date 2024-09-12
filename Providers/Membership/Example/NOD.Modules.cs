using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;


namespace Protean.Providers.Membership
{

    public partial class NOD
    {
        public class Modules
        {
            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

            private const string mcModuleName = "Protean.NOD.Modules";

            private NameValueCollection moMailConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
            private NameValueCollection dynamicsCfg = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/NOD");


            public Modules()
            {

                // do nowt
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

        }
    }

}

