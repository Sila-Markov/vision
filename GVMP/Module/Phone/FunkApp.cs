using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace GVMP
{
    class FunkApp : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            Logger.Print("Funkapp geladen.");
        }

        [RemoteEvent("phone:joinfunk")]
        public void JoinFunk(Client p, string radio)
        {
            try
            {
                bool encrypted = false;
                DbPlayer dbPlayer = p.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                foreach (Faction fraktion in FactionModule.factionList)
                {
                    try
                    {
                        if (!String.IsNullOrWhiteSpace(radio) && Convert.ToInt32(radio) == fraktion.Dimension)
                        {

                            encrypted = true;
                            if (dbPlayer.Faction == fraktion)
                            {
                                p.TriggerEvent("overlay:changeimage", "funk", "radio2.png");
                                p.Eval("mp.events.callRemote('server:joinradio', " + radio + ")");
                                return;
                            }
                        }
                    }
                    catch (Exception e3) { }
                }
                if (encrypted)
                {
                    p.TriggerEvent("overlay:changeimage", "funk", "radio3.png");
                    dbPlayer.SendNotification("Dieser Funkkanal ist verschlüsselt.", "black", 6000);
                }
                else
                {
                    p.TriggerEvent("overlay:changeimage", "funk", "radio2.png");
                    dbPlayer.SendNotification("Du bist Funkkanal " + radio + " MHz beigetreten.", "black", 6000);
                    p.Eval("mp.events.callRemote('server:joinradio', " + radio + ")");
                }
            } catch(Exception ex) { Console.Write(ex.Message); }
        }

        [RemoteEvent("phone:joinfrakfunk")]
        public void JoinFrakFunk(Client p)
        {
            try
            {
                DbPlayer dbPlayer = p.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                foreach (Faction fraktion in FactionModule.factionList)
                {
                    if (dbPlayer.Faction == fraktion)
                    {
                        p.TriggerEvent("overlay:changeimage", "funk", "radio2.png");
                        dbPlayer.SendNotification("Du bist Funkkanal " + fraktion.Dimension + " MHz beigetreten.", "black", 6000);
                        p.Eval("mp.events.callRemote('server:joinradio', " + fraktion.Dimension + ")");
                    }
                }
            } catch(Exception ex) { Console.Write(ex.Message); }
        }
    }
}
