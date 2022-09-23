using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace GVMP
{
    class Handy : Script
    {

        [RemoteEvent("phone:open")]
        public void PhoneOpen(Client p)
        {
            DbPlayer dbPlayer = p.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return;

            if (!p.IsInVehicle)
            {
                NAPI.Player.PlayPlayerAnimation(p, 49, "amb@code_human_wander_texting@male@base", "static", 8f);

            }
            p.TriggerEvent("phone:openphone", dbPlayer.Faction.Name, dbPlayer.Factionrank);
        }
        [RemoteEvent("phone:close")]
        public void PhoneClose(Client p)
        {
            if (!p.IsInVehicle)
            {
                p.StopAnimation();
            }
        }

    }
}