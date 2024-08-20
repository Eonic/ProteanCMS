using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Xml;
using static Protean.Cms;

namespace Protean.Providers.Membership
{
    public partial class NOD
        {



        public class AdminXForms : Protean.Providers.Membership.DefaultProvider.AdminXForms, IMembershipAdminXforms
        {
            private const string mcModuleName = "Protean.Providers.Membership.NOD.AdminXForms";

            object crmConfig = WebConfigurationManager.GetWebApplicationSection("protean/NOD");

            public AdminXForms(ref Cms aWeb) : base(ref aWeb)
            {
            }

           public XmlElement xFrmUserLogon(string FormName = "UserLogon")
            {
                XmlElement xMyForm = base.xFrmUserLogon(FormName);
                return xMyForm;
            }

            

        }

    }
}


