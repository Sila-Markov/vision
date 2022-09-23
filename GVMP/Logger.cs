using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVMP
{
    public static class Logger
    {
        public static void Print(string msg)
        {
            if (msg.Contains("EXCEPTION"))
            {
                WebhookSender.SendMessage("EXCEPTION-LOG", msg, Webhooks.exception, "EXCEPTION-LOG");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("(Vision Crimelife) " + msg);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static void Exception(System.Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine((object)ex);
            Console.ForegroundColor = ConsoleColor.Gray;
            WebhookSender.SendMessage("EXCEPTION-LOG", ex.ToString(), Webhooks.exception, "EXCEPTION-LOG");
        }
    }
}
