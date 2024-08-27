using System.Runtime.CompilerServices;
using System.Windows.Markup;

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
        public partial class modal
        {
            public class Contact
            {
                public Contact() { }

                public Contact(int nContactKey, int nContactDirId, int nContactCartId, string cContactType, string cContactName, string cContactCompany, string cContactAddress, string cContactCity, string cContactState, string cContactZip, string cContactCountry, string cContactTel, string cContactFax, string cContactEmail, string cContactLocationSummary, string cContactXml, int nAuditId, string cContactForiegnRef, double nLat, double nLong, string cContactForeignRef, string cContactAddress2, string cContactFirstName, string cContactLastName, string cContactTitle)
                {
                    this.nContactKey = nContactKey;
                    this.nContactDirId = nContactDirId;
                    this.nContactCartId = nContactCartId;
                    this.cContactType = cContactType;
                    this.cContactName = cContactName;
                    this.cContactCompany = cContactCompany;
                    this.cContactAddress = cContactAddress;
                    this.cContactCity = cContactCity;
                    this.cContactState = cContactState;
                    this.cContactZip = cContactZip;
                    this.cContactCountry = cContactCountry;
                    this.cContactTel = cContactTel;
                    this.cContactFax = cContactFax;
                    this.cContactEmail = cContactEmail;
                    this.cContactLocationSummary = cContactLocationSummary;
                    this.cContactXml = cContactXml;
                    this.nAuditId = nAuditId;
                    this.cContactForiegnRef = cContactForiegnRef;
                    this.nLat = nLat;
                    this.nLong = nLong;
                    this.cContactForeignRef = cContactForeignRef;
                    this.cContactAddress2 = cContactAddress2;
                    this.cContactFirstName = cContactFirstName;
                    this.cContactLastName = cContactLastName;
                    this.cContactTitle = cContactTitle;
                }

                public Contact(int nContactDirId, string cContactType, string cContactName, string cContactAddress, string cContactAddress2, string cContactCity, string cContactState, string cContactZip, string cContactCountry, string cContactEmail, string cContactXml, string cContactForeignRef,  string cContactFirstName, string cContactLastName)
                {
                    this.nContactDirId = nContactDirId;
                    this.cContactType = cContactType;
                    this.cContactName = cContactName;
                    this.cContactAddress = cContactAddress;
                    this.cContactAddress2 = cContactAddress2;
                    this.cContactCity = cContactCity;
                    this.cContactState = cContactState;
                    this.cContactZip = cContactZip;
                    this.cContactCountry = cContactCountry;
                    this.cContactFirstName = cContactFirstName;
                    this.cContactLastName = cContactLastName;
                    this.cContactEmail = cContactEmail;
                    this.cContactXml = cContactXml;
                    this.cContactForeignRef = cContactForeignRef;
                }

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
}