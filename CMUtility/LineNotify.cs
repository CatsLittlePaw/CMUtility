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
        public void PostErrorMessage(string ErrorMsg, string type) 
        {
            ErrorMsg = (ErrorMsg.Length > 900) ? ErrorMsg.Substring(0, 900) : ErrorMsg;
            var ProjectName = "幫我購Service";
            var Message = $"[{ProjectName}] {DateTime.UtcNow.AddHours(8).ToLongDateString()} {DateTime.UtcNow.AddHours(8).ToLongTimeString()} 發生{type}錯誤\n" +
                $"{ErrorMsg}";

            PostLineNotify(Message);
        }
        private async void PostLineNotify(string message) 
        {
            var Keys = ConfigurationManager.AppSettings["LINE_NOTIFY"].Split('|');
            var client = new RestClient("https://notify-api.line.me/api/notify");

            foreach (var key in Keys)
            {
                var request = new RestRequest();
                request.AddHeader("Authorization", $"Bearer {key}");
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("message", message);

                RestResponse response = await client.PostAsync(request);
            }
        }
    }
}
