using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace GVMP
{
    class ProfilApp : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            Logger.Print("Profilapp geladen.");
        }

        [RemoteEvent("phone:requestprofilapp")]
        public void RequestProfilApp(Client player)
        {
            try
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                player.TriggerEvent("phone:loadprofilapp", dbPlayer.Name, dbPlayer.Factionrank, 0, dbPlayer.Faction.Name, dbPlayer.Factionrank, 0, dbPlayer.Level);
            }
            catch (Exception ex) { Console.Write(ex.Message); }
        }

    }
}
