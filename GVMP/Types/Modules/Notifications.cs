using System;
using System.Collections.Generic;
using System.Text;

namespace GVMP
{
    public static class Notifications
    {
        public static void SendNotification(this DbPlayer dbPlayer, string text, string color, int time, string textfrom = "")
        {
            dbPlayer.Client.TriggerEvent("client:sendnotify", text, color, time, textfrom);
        }
    }
}
