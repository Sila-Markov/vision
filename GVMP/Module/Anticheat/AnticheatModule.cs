using GTANetworkAPI;
using System;
using GVMP;

namespace GVMP
{
    class AnticheatModule : GVMP.Module.Module<AnticheatModule>
    {
        [RemoteEvent("server:CheatDetection")]
        public static void CheatDetection(Client player, string Detection)
        {
            try
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null || dbPlayer.Client.IsNull || dbPlayer.DeathData.IsDead || dbPlayer.Client.Dimension >= 22750 ) return;// || dbPlayer.Client.Dimension != 0) return;
                if (player.HasData("PLAYER_ADUTY") && player.HasData("PLAYER_ADUTY") == true || player.HasData("DisableAC") && player.HasData("DisableAC") == true) return;
                PlayerHandler.GetAdminPlayers().ForEach((DbPlayer dbPlayer2) =>
                {
                    if (dbPlayer2.HasData("disablenc")) return;


                    Adminrank adminranks = dbPlayer2.Adminrank;

                    if (adminranks.Permission >= 91)
                        dbPlayer2.SendNotification($"{Detection} - {player.Name} Dimension: {player.Dimension}", 5000, "red", "Anticheat");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
