using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    public partial class ATwiCliAudioFile : ATwiCliFile
    {
        #region TCAF Constants
        public const string TCAF_AUDIO_STRING = "audio.mp3";
        public const string TCAF_AUDIO_EXTENSION = ".mp3";
        #endregion

        public override string GetDirectURL()
        {
            return TCF_DIRECT_URL + TCAF_AUDIO_STRING + "?id=" + this.ID;
        }

        public override string GetExtension()
        {
            return TCAF_AUDIO_EXTENSION;


        }
    }
}
