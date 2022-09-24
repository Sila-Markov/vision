using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace GVMP
{
    class WebhookSender : GVMP.Module.Module<WebhookSender>
    {
        public static async void SendMessage(string title, string msg, string webhook, string type)
        {
            try
            {
                DateTime now = DateTime.Now;
                string[] strArray = new string[20];
                strArray[0] = "{\"username\":\"VISION. CRIMELIFE.\",\"avatar_url\":\"https://cdn.discordapp.com/attachments/982620830351638538/982629428666458154/server_banner.gif\",\"content\":\"\",\"embeds\":[{\"author\":{\"name\":\"Crimelife.\",\"url\":\"https://discord.gg/tMRXHKWwF2\",\"icon_url\":\"https://cdn.discordapp.com/icons/959806126159962142/a_dbd4efaa0393c50628089928b3059ead.gif?\"},\"title\":\"" + type + "\",\"thumbnail\":{\"url\":\"https://cdn.discordapp.com/icons/959806126159962142/a_dbd4efaa0393c50628089928b3059ead.gif?\"},\"url\":\"https://discord.gg/tMRXHKWwF2\",\"description\":\"Es wurde am **";
                int num = now.Day;
                strArray[1] = num.ToString();
                strArray[2] = ".";
                num = now.Month;
                strArray[3] = num.ToString();
                strArray[4] = ".";
                num = now.Year;
                strArray[5] = num.ToString();
                strArray[6] = " | ";
                num = now.Hour;
                strArray[7] = num.ToString();
                strArray[8] = ":";
                num = now.Minute;
                strArray[9] = num.ToString();
                strArray[10] = "** ein " + type + " ausgelöst.\",\"color\":1127128,\"fields\":[{\"name\":\"";
                strArray[11] = title;
                strArray[12] = "\",\"value\":\"";
                strArray[13] = msg;
                strArray[14] = "\",\"inline\":true}],\"footer\":{\"text\":\" Vison. Crimelife | " + type + " (c) Sila_Kashlikov 2022\"}}]}";
                string stringPayload = string.Concat(strArray);
                StringContent httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(webhook, (HttpContent)httpContent);
                }
                stringPayload = (string)null;
                httpContent = (StringContent)null;
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION SendMessage] " + ex.Message);
                Logger.Print("[EXCEPTION SendMessage] " + ex.StackTrace);
            }
        }
    }
}
