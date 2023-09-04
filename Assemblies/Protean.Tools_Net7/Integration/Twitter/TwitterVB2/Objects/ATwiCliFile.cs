using System;

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    public abstract partial class ATwiCliFile
    {
        #region TCF Constants
        public const string TCF_DIRECT_URL = "http://twic.li/api/";
        #endregion
        private string m_strID;
        private string m_strUsername;
        private string m_strUserID;
        private string m_strURL;
        private int m_iComments;
        private int m_iViews;
        private DateTime m_dtTimeStamp;

        public string UserID
        {
            get
            {
                return m_strUserID;

            }
            set
            {
                m_strUserID = value;

            }
        }
        public string ID
        {
            get
            {
                return m_strID;

            }
            set
            {
                m_strID = value;

            }
        }

        public string ScreenName
        {
            get
            {
                return m_strUsername;

            }
            set
            {
                m_strUsername = value;
            }
        }

        public string URL
        {
            get
            {
                return m_strURL;

            }
            set
            {
                m_strURL = value;
            }
        }

        public int Comments_Count
        {
            get
            {
                return m_iComments;

            }
            set
            {
                m_iComments = value;
            }
        }

        public int Views_Count
        {
            get
            {
                return m_iViews;

            }
            set
            {
                m_iViews = value;
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return m_dtTimeStamp;
            }
            set
            {
                m_dtTimeStamp = value;
            }

        }

        public void InsertUnixTime(double p_dtTimeStamp)
        {
            var dtEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            m_dtTimeStamp = dtEpoch.AddSeconds(p_dtTimeStamp);

        }
        public abstract string GetDirectURL();
        public abstract string GetExtension();

    }
}
