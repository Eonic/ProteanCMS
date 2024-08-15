using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Protean.Cms;
using System.Xml;

namespace Protean.Providers.Membership
{ 
        public partial class NOD
        {

        public class AdminProcess : Protean.Providers.Membership.DefaultProvider.AdminProcess, IMembershipAdminProcess
        {
            private const string mcModuleName = "Protean.Providers.Membership.Dynamics365.AdminProcess";

            public AdminProcess(ref Cms aWeb) : base(ref aWeb)
            {
            }


        }

    }
}


