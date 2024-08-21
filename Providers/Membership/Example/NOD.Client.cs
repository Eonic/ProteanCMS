using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;  // Microsoft Authentication Library (MSAL)
using System.Text.Json;
using System.Threading.Tasks;
using Protean;
using System.Web;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Net;
using System.Security;
using RestSharp.Authenticators;
using RestSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens;
using System.Xml;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Net.Mail;

namespace Protean.Providers.Membership

{ 
    public partial class NOD
{
        class Client
        {
            private NameValueCollection dynamicsCfg = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/NOD");

            public Client()
            {
               
            } 
        }
    }
}
