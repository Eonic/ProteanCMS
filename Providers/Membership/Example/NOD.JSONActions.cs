using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Protean.Cms;
using static Protean.Syndication.Distributor;
using System.Runtime.Remoting;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Diagnostics;
using System.Web;

namespace Protean.Providers.Membership {
        public partial class NOD{
        public class JSONActions : rest.JsonActions
        {
            private Cms myWeb;
            private Protean.Cms.Cart myCart;
            public JSONActions()
            {
                myWeb = new Cms();
                myWeb.InitializeVariables();
                myWeb.Open();
            }

        }

    }
}
