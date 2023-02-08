using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    public partial class ATwiCliVideoFile : ATwiCliFile
    {
        #region TCVF Constants
        public const string TCVF_VIDEO_STRING = "video.flv";
        public const string TCVF_VIDEO_EXTENSION = ".flv";
        #endregion
        private string m_strThumbNailURL;

        public string ThumbNailURL
        {
            get
            {
                return m_strThumbNailURL;

            }
            set
            {
                m_strThumbNailURL = value;
            }
        }


        public override string GetDirectURL()
        {
            return TCF_DIRECT_URL + TCVF_VIDEO_STRING + "?id=" + this.ID + "&size=original";
        }

        public override string GetExtension()
        {
            return TCVF_VIDEO_EXTENSION;

        }
    }

}
