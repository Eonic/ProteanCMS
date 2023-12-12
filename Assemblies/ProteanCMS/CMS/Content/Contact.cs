using System.Runtime.CompilerServices;

namespace Protean
{
    public partial class Cms
    {
        public override Cms.dbHelper moDbHelper
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return base.moDbHelper;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (base.moDbHelper != null)
                {
                    base.moDbHelper.OnError -= OnComponentError;
                }

                base.moDbHelper = value;
                if (base.moDbHelper != null)
                {
                    base.moDbHelper.OnError += OnComponentError;
                }
            }
        }
        public class Contact
        {
            public int nContactKey;
            public int nContactDirId;
            public int nContactCartId;
            public string cContactType;
            public string cContactName;
            public string cContactCompany;
            public string cContactAddress;
            public string cContactCity;
            public string cContactState;
            public string cContactZip;
            public string cContactCountry;
            public string cContactTel;
            public string cContactFax;
            public string cContactEmail;
            public string cContactLocationSummary;
            public string cContactXml;
            public int nAuditId;
            public string cContactForiegnRef;
            public double nLat;
            public double nLong;
            public string cContactForeignRef;
            public string cContactAddress2;
            public string cContactFirstName;
            public string cContactLastName;
            public string cContactTitle;
        }
    }
}