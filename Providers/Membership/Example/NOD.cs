using Protean.Providers.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Protean.Providers.Membership
{
    public partial class NOD : IMembershipProvider
    {

        public NOD()
        {
            // do nothing
            //_AdminXforms = new DefaultProvider.AdminXForms(ref myweb);
        }
        //IMembershipProvider obj1 = new DefaultProvider();
        private IMembershipAdminXforms _AdminXforms;
        private IMembershipAdminProcess _AdminProcess;
        private IMembershipActivities _Activities;

        IMembershipAdminXforms IMembershipProvider.AdminXforms
        {
            set
            {
                _AdminXforms = value;
            }
            get
            {
                return _AdminXforms;
            }
        }
        IMembershipAdminProcess IMembershipProvider.AdminProcess
        {
            set
            {
                _AdminProcess = value;
            }
            get
            {
                return _AdminProcess;
            }
        }
        IMembershipActivities IMembershipProvider.Activities
        {
            set
            {
                _Activities = value;
            }
            get
            {
                return _Activities;
            }
        }

        public IMembershipProvider Initiate(ref Cms myWeb)
        {
            _AdminXforms = new AdminXForms(ref myWeb);
            _AdminProcess = new AdminProcess(ref myWeb);
          //  _AdminProcess.oAdXfm = _AdminXforms;
            string exception = null;
            _Activities = new Activities(exception);
            return this;
        }

        public void Dispose()
        {
            _AdminXforms = null;
            _AdminProcess = null;
            _Activities = null;
        }

    }
}
