using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protean
{
    public partial class Cms
    {

        public partial class modal
        {
            public partial class APILog
            {
                public APILog() { }
                public int nAPILogKey { get; set; }
                public int nUserId { get; set; }
                public DateTime dRequestDateTime { get; set; }
                public DateTime dResponseDateTime { get; set; }
                public string cRequestedUrl { get; set; }
                public string cMethodName { get; set; }
                public string cPayLoad { get; set; }
                public string cResponseData { get; set; }
                public string cResponseType { get; set; }
                public string cRequestType { get; set; }

                public string cSourceIP { get; set; }
                public string cUserAgent { get; set; }
            }
        }
    }
}
