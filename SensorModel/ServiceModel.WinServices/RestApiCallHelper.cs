using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace SensorModel.WinServices
{

    /// <summary>
    /// thingspeak'den veri çekmek için kullanılan kütüphanedir. Get yaparak veriler çekilir. Çekilen veriler json olduğu için json deserialize edilerek modele çevrilmiş olur.
    /// </summary>
    public class RestApiCallHelper
    {
        public static string BaseUri = "https://api.thingspeak.com/channels/1608942/feeds.json?api_key=1UDNUHM2PPSAJFVS&offset=3&results=8000";
        public static async Task<TResponse> Get<TResponse>()
        {
            ServicePointManager.ServerCertificateValidationCallback = new
            RemoteCertificateValidationCallback(delegate { return true; });
            using (HttpClient client = new HttpClient())
            {
                string stringData = client.GetStringAsync(BaseUri).Result;
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                TResponse response = JsonConvert.DeserializeObject<TResponse>(stringData, settings);
                return response;
            }

        }
    }
}
