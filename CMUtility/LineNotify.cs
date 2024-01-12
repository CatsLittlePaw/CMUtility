using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMUtility
{
    public class LineNotify
    {
        public static void PostLineNotify(string msg, string type = "") 
        {
            try
            {
                msg = (msg.Length > 900) ? msg.Substring(0, 900) : msg;
                var time = string.Concat(DateTime.UtcNow.AddHours(8).ToLongDateString(), DateTime.UtcNow.AddHours(8).ToLongTimeString());
                var message = $"\n{msg}";

                PostMsg(message);
            }
            catch(Exception ex)
            {

            }
        }

        private static void PostMsg(string message) 
        {
            var keys = ConfigurationManager.AppSettings["LINE_NOTIFY"].Split("|");

            foreach (var key in keys)
            {
                var options = new RestClientOptions("https://notify-api.line.me/api/notify")
                {
                    MaxTimeout = -1
                };

                var client = new RestClient(options);
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {key}");
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("message", message);

                client.Post(request);
            }           
        }
    }
}
