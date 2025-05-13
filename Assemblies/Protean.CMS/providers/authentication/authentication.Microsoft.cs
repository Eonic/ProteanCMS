using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web.Configuration;
using Newtonsoft.Json.Linq;
using System.Security.Authentication;
using Protean.Providers.Payment;
using System.Reflection;
using static Protean.stdTools;
using Protean.Tools;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Ajax.Utilities;
using System.Configuration.Provider;
using Microsoft.VisualBasic;
using System.Xml;
using System.Web.Security;


namespace Protean.Providers
{
    namespace authentication
    {
     
        public class Microsoft : authentication.Default, IauthenticaitonProvider
        {
            private string _Name = "Microsoft";

            public Microsoft()
            {
                // do nothing
            }


            string IauthenticaitonProvider.name
            {
                get
                {
                    return _Name;
                }
            }

            public IauthenticaitonProvider Initiate(ref Cms myWeb)
            {
                return this;
            }

            IauthenticaitonProvider IauthenticaitonProvider.Initiate(ref Cms myWeb)
            {
                throw new NotImplementedException();
            }
        }
    }
}
