using GTANetworkAPI;
using System;

namespace GVMP
{
    internal class DeathHandler : Script
    {
        private void handleDeath(DbPlayer dbPlayer)
        {
           // dbPlayer.TriggerEvent("toggleBlurred", false);
            dbPlayer.SpawnPlayer(dbPlayer.Client.Position);
            dbPlayer.disableAllPlayerActions(true);
            dbPlayer.TriggerEvent("toggleBlurred", true);
            dbPlayer.PlayAnimation(33, "missarmenian2", "corpse_search_exit_ped", 8f);
            dbPlayer.SetInvincible(true);
            dbPlayer.SetSharedData("FUNK_CHANNEL", 0);
            dbPlayer.SetSharedData("FUNK_TALKING", false);

        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Client c, Client k, uint reason)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                PaintballModel paintballModel = dbPlayer.GetData("PBZone");

                if (k != null && k.Exists)
                {
                    DbPlayer dbKiller = k.GetPlayer();
                    WebhookSender.SendMessage("Spieler wird geötet", "Der Spieler " + dbPlayer.Name + " wurde von " + dbKiller.Name + " getötet." + "Waffe: " + dbKiller.Client.CurrentWeapon, Webhooks.killWebhook, "Kill");
                }


                if (paintballModel != null)
                {
                    if (k != null && k.Exists)
                    {
                        DbPlayer dbKiller = k.GetPlayer();
                        if (dbKiller == null || !dbKiller.IsValid(true))
                            return;
                        //Paintball
                        dbPlayer.SendNotification("Du wurdest von " + dbKiller.Name + " getötet!", "black", 5000);
                        dbKiller.SendNotification("Du hast " + dbPlayer.Name + " getötet! +30.000$", "black", 5000);
                        dbKiller.addMoney(30000);

                        dbKiller.SetHealth(200);
                        dbKiller.SetArmor(100);

                        dbPlayer.disableAllPlayerActions(true);
                        dbPlayer.SpawnPlayer(dbPlayer.Client.Position);
                        dbPlayer.PlayAnimation(33, "missarmenian2", "corpse_search_exit_ped", 8f);
                        dbPlayer.SetInvincible(true);



                        NAPI.Task.Run(() => { PaintballModule.PaintballDeath(dbPlayer, dbKiller); }, 5000);

                        return;
                    }

                }

                if (dbPlayer.HasData("IN_GANGWAR"))
                {
                    dbPlayer.TriggerEvent("toggleBlurred", true);
                    dbPlayer.DeathData = new DeathData
                    {
                        IsDead = true,
                        DeathTime = new DateTime(0)
                    };
                    if (k != null && k.Exists)
                    {
                        DbPlayer dbKiller = k.GetPlayer();
                        if (dbKiller == null || !dbKiller.IsValid(true))
                            return;

                        if (dbKiller.Faction.Id != dbPlayer.Faction.Id)
                            GangwarModule.handleKill(dbKiller);
                        //Gangwar
                        dbPlayer.SendNotification("Du wurdest von " + dbKiller.Name + " getötet!", "black", 5000);
                        dbKiller.SendNotification("Du hast " + dbPlayer.Name + " getötet! +30.000$", "black", 5000);
                        dbKiller.addMoney(30000);
                    }
                    else
                    {
                        dbPlayer.SendNotification("Du bist gestorben!", "black", 5000);
                    }

                    NAPI.Task.Run(() => handleDeath(dbPlayer), 5000);
                }
                else if (paintballModel == null)
                {
                    dbPlayer.TriggerEvent("toggleBlurred", true);
                    dbPlayer.DeathData = new DeathData { IsDead = true, DeathTime = DateTime.Now };
                    if (k != null && k.Exists)
                    {
                        DbPlayer dbKiller = k.GetPlayer();
                        if (dbKiller == null || !dbKiller.IsValid(true))
                            return;
                        //Normal
                        dbPlayer.SendNotification("Du wurdest von " + dbKiller.Name + " getötet!", "black", 5000);
                        dbKiller.SendNotification("Du hast " + dbPlayer.Name + " getötet! +30.000$", "black", 5000);
                        dbKiller.addMoney(30000);
                    }
                    else
                    {
                        dbPlayer.SendNotification("Du bist gestorben!", "black", 5000);
                    }

                    dbPlayer.SetSharedData("FUNK_CHANNEL", 0);
                    dbPlayer.SetSharedData("FUNK_TALKING", false);
                    dbPlayer.SetAttribute("Death", 1);

                    NAPI.Task.Run(() => handleDeath(dbPlayer), 5000);
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION PlayerDeath] " + ex.Message);
                Logger.Print("[EXCEPTION PlayerDeath] " + ex.StackTrace);
            }
        }
    }
}