using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protean.Tools.Integration.Twitter.TwitterVB2.Objects
{
    public partial class TwitterTrendLocation
    {
        private string _ID = string.Empty;
        private string _LocationName = string.Empty;
        private string _PlaceTypeCode = string.Empty;
        private string _PlaceTypeName = string.Empty;
        private string _CountryName = string.Empty;
        private string _CountryCode = string.Empty;
        private string _URL = string.Empty;


        public string ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }


        public string LocationName
        {
            get
            {
                return _LocationName;
            }
            set
            {
                _LocationName = value;
            }
        }


        public string PlaceTypeCode
        {
            get
            {
                return _PlaceTypeCode;
            }
            set
            {
                _PlaceTypeCode = value;
            }
        }


        public string PlaceTypeName
        {
            get
            {
                return _PlaceTypeName;
            }
            set
            {
                _PlaceTypeName = value;
            }
        }


        public string CountryName
        {
            get
            {
                return _CountryName;
            }
            set
            {
                _CountryName = value;
            }
        }


        public string CountryCode
        {
            get
            {
                return _CountryCode;
            }
            set
            {
                _CountryCode = value;
            }
        }


        public string URL
        {
            get
            {
                return _URL;
            }
            set
            {
                _URL = value;
            }
        }


        public TwitterTrendLocation(System.Xml.XmlNode SearchResultNode)
        {
            _ID = SearchResultNode["woeid"].InnerText;

            if (SearchResultNode["name"] != null)
            {
                _LocationName = SearchResultNode["name"].InnerText;
            }

            if (SearchResultNode["placeTypeName"] != null)
            {
                _PlaceTypeName = SearchResultNode["placeTypeName"].InnerText;
            }

            var PTNList = SearchResultNode.SelectNodes("placeTypeName");

            if (PTNList[0].Attributes["code"] != null)
            {
                _PlaceTypeCode = PTNList[0].Attributes["code"].Value;
            }

            if (SearchResultNode["country"] != null)
            {
                _CountryName = SearchResultNode["country"].InnerText;
            }

            var CountryList = SearchResultNode.SelectNodes("country");

            if (CountryList[0].Attributes["code"] != null)
            {
                _CountryCode = CountryList[0].Attributes["code"].Value;
            }

            if (SearchResultNode["url"] != null)
            {
                _URL = SearchResultNode["url"].InnerText;
            }
        }
    }
}
