using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jwt;
using Newtonsoft.Json;

namespace Protean.Tools.IdentityModel.Tokens
{
    public class JwtHelper
    {
        public string GenerateJwt(string apiKey, string apiIdentifier, string orgUnitId,  Dictionary<string, object> Payload)

        {
            Dictionary<string, object> PayloadContainer = (Dictionary<string, object>)Payload["Payload"];
          //  List<Dictionary<string, object>> PayloadContents = (List<Dictionary<string, object>>)PayloadContainer.Values;

            var NewPayload = new Dictionary<string, object>
            {
                {"jti", Guid.NewGuid()},
                {"iat", DateTime.UtcNow.ToUnixTime()},
                {"exp", DateTime.UtcNow.AddDays(365).ToUnixTime()},
                {"iss", apiIdentifier},
                {"OrgUnitId", orgUnitId},
                {"Payload", PayloadContainer}, 
                {"ObjectifyPayload", true}
            };
            var jsonstr = JsonConvert.SerializeObject(NewPayload);

            return JsonWebToken.Encode(NewPayload, apiKey, JwtHashAlgorithm.HS256);
        }


    }
}
