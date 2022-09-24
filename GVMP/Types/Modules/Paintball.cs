using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace GVMP
{
    public static class Paintball
    {
        public static void initializePaintball(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null) return;

            dbPlayer.Client.TriggerEvent("statshud:open", 0, 0, 0);
        }

        public static void finishPaintball(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null) return;

            dbPlayer.Client.TriggerEvent("statshud:close");
        }

        public static void updatePaintballScore(this DbPlayer dbPlayer, int kills, int death)
        {
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null) return;
            float kd = 0;
            if (kills > 0 && death > 0) kd = kills / death;
            dbPlayer.Client.TriggerEvent("statshud:refreshdata", kills, death, kd);
        }
    }
}
