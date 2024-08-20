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

        public class Activities : Protean.Providers.Membership.DefaultProvider.Activities, IMembershipActivities
        {
            private const string mcModuleName = "Protean.Providers.Membership.NOD.Activities";

            public Activities(string exception) : base()
            {

            }

        }

    }
}


