using System;
using System.Runtime.Serialization;
using System.Xml;
using static Protean.Tools.Database;
using static Protean.Tools.Xml;


namespace Protean.Models
{

   public class Order
    {
        private XmlElement OrderElmt;
        public System.Collections.Specialized.NameValueCollection moConfig;
        // private System.Web.HttpServerUtility moServer;  //never used

        private double nFirstPayment;
        private double nRepeatPayment;
        private string sRepeatInterval;
        private int nRepeatLength;
        private bool bDelayStart;
        private DateTime dStartDate;

        private string sPaymentMethod;
        private string sTransactionRef = string.Empty;
        private string sDescription;

        //private string sGivenName;
        //private string sBillingAddress1;  //never used all strings
        //private string sBillingAddress2;
        //private string sBillingTown;
        //private string sBillingCounty;
        //private string sBillingPostcode;
        //private string sEmail;

        public XmlDocument moPageXml;

        public Order()
        {
            moPageXml = new XmlDocument();
            OrderElmt = moPageXml.CreateElement("Order");
        }

        public Order(ref XmlDocument PageDocument)
        {
            moPageXml = PageDocument;
            OrderElmt = moPageXml.CreateElement("Order");
        }

        public XmlElement xml
        {
            get
            {
                return OrderElmt;
            }
        }

        public string PaymentMethod
        {
            get
            {
                return sPaymentMethod;
            }
            set
            {
                sPaymentMethod = value;
                OrderElmt.SetAttribute("paymentMethod", value);
            }
        }

        public double firstPayment
        {
            get
            {
                return nFirstPayment;
            }
            set
            {
                nFirstPayment = value;
                OrderElmt.SetAttribute("total", Convert.ToString(value));
            }
        }

        public double repeatPayment
        {
            get
            {
                return nRepeatPayment;
            }
            set
            {
                nRepeatPayment = value;
                OrderElmt.SetAttribute("repeatPrice", Convert.ToString(value));
            }
        }

        public string repeatInterval
        {
            get
            {
                return sRepeatInterval;
            }
            set
            {
                sRepeatInterval = value;
                OrderElmt.SetAttribute("repeatInterval", value);
            }
        }

        public int repeatLength
        {
            get
            {
                return nRepeatLength;
            }
            set
            {
                nRepeatLength = value;
                OrderElmt.SetAttribute("repeatLength", Convert.ToString(value));
            }
        }

        public bool delayStart
        {
            get
            {
                return bDelayStart;
            }
            set
            {
                bDelayStart = value;
                if (value)
                    OrderElmt.SetAttribute("delayStart", "true");
                else
                    OrderElmt.SetAttribute("delayStart", "false");
            }
        }

        public DateTime startDate
        {
            get
            {
                return dStartDate;
            }
            set
            {
                dStartDate = value;
                OrderElmt.SetAttribute("startDate", XmlDate(dStartDate));
            }
        }

        public string TransactionRef
        {
            get
            {
                return sTransactionRef;
            }
            set
            {
                string sTransactionRef = value;
                OrderElmt.SetAttribute("transactionRef", sTransactionRef);
            }
        }


        public string description
        {
            get
            {
                return sDescription;
            }
            set
            {
                sDescription = value;
                XmlElement descElmt = moPageXml.CreateElement("Description");
                descElmt.InnerText = sDescription;
                OrderElmt.AppendChild(descElmt);
            }
        }

        public void SetAddress(string GivenName, string Email, string Telephone, string TelephoneCountryCode, string Company, string Street, string City, string State, string PostalCode, string Country)
        {
            XmlElement addElmt = moPageXml.CreateElement("Contact");
            addElmt.SetAttribute("type", "Billing Address");
            Protean.Tools.Xml.addElement(ref addElmt, "GivenName", GivenName);
            Protean.Tools.Xml.addElement(ref addElmt, "Email", Email);
            Protean.Tools.Xml.addElement(ref addElmt, "Telephone", Telephone);
            Protean.Tools.Xml.addElement(ref addElmt, "TelephoneCountryCode", TelephoneCountryCode);
            Protean.Tools.Xml.addElement(ref addElmt, "Street", Street);
            Protean.Tools.Xml.addElement(ref addElmt, "City", City);
            Protean.Tools.Xml.addElement(ref addElmt, "State", State);
            Protean.Tools.Xml.addElement(ref addElmt, "PostalCode", PostalCode);
            Protean.Tools.Xml.addElement(ref addElmt, "Country", Country);
            OrderElmt.AppendChild(addElmt);
        }
   
        public int? id { get; set; }

        public Contact BillingAddress { get; set; }

        public Contact DeliveryAddress { get; set; }

        public Order Duplicate(XmlElement orderXml)
        {
                        return new Order();
        }

    }
}
