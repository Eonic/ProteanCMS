using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    public partial class ATwiCliPhotoFile : ATwiCliFile
    {

        #region TCPF Constants
        private const string TCPF_PHOTO_STRING = "photo.jpg";
        private const string TCPF_PHOTO_EXTENSION = ".jpg";
        #endregion
        private int m_iUserTags;
        private double m_dLongitude;
        private double m_dLatitude;
        private bool m_bShowMap;
        private string m_strCameraMake;
        private string m_strCameraModel;
        public int UserTags_Count
        {
            get
            {
                return m_iUserTags;

            }
            set
            {
                m_iUserTags = value;

            }
        }

        public double Longitude
        {
            get
            {
                return m_dLongitude;

            }
            set
            {
                m_dLongitude = value;
            }
        }

        public double Latitude
        {
            get
            {
                return m_dLatitude;

            }
            set
            {
                m_dLatitude = value;

            }
        }

        public bool ShowMap
        {
            get
            {
                return m_bShowMap;

            }
            set
            {
                m_bShowMap = value;

            }
        }

        public string CameraMake
        {
            get
            {
                return m_strCameraMake;

            }
            set
            {
                m_strCameraMake = value;

            }
        }

        public string CameraModel
        {
            get
            {
                return m_strCameraModel;

            }
            set
            {
                m_strCameraModel = value;

            }
        }

        public override string GetDirectURL()
        {
            string GetDirectURLRet = default;
            GetDirectURLRet = TCF_DIRECT_URL + TCPF_PHOTO_STRING + "?id=" + this.ID + "&size=original";
            return GetDirectURLRet;
        }

        public override string GetExtension()
        {
            return TCPF_PHOTO_EXTENSION;

        }

        public string LargeURL
        {
            get
            {
                return TCF_DIRECT_URL + TCPF_PHOTO_STRING + "?id=" + this.ID + "&size=large";
            }
        }

        public string SmallURL
        {
            get
            {
                return TCF_DIRECT_URL + TCPF_PHOTO_STRING + "?id=" + this.ID + "&size=small";
            }
        }

        public string MediumURL
        {
            get
            {
                return TCF_DIRECT_URL + TCPF_PHOTO_STRING + "?id=" + this.ID + "&size=medium";
            }
        }

        public string SquareURL
        {
            get
            {
                return TCF_DIRECT_URL + TCPF_PHOTO_STRING + "?id=" + this.ID + "&size=square";
            }
        }

        public string TileURL
        {
            get
            {
                return TCF_DIRECT_URL + TCPF_PHOTO_STRING + "?id=" + this.ID + "&size=tile";
            }
        }

        public string LargestURL
        {
            get
            {
                return TCF_DIRECT_URL + TCPF_PHOTO_STRING + "?id=" + this.ID + "&size=largest_available";
            }
        }

    }

}
