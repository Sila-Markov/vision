using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace GVMP
{
    class LeaderApp : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            Logger.Print("Leaderapp geladen.");
        }

        [RemoteEvent("phone:uprank")]
        public void PhoneUprank(Client p, string name)
        {
            try
            {
                DbPlayer dbPlayer = p.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                if (dbPlayer.Factionrank > 9)
                {
                    if (PlayerHandler.GetPlayer(name) != null)
                    {
                        if (PlayerHandler.GetPlayer(name).Faction == dbPlayer.Faction)
                        {
                            if (PlayerHandler.GetPlayer(name).Factionrank < dbPlayer.Factionrank)
                            {
                                PlayerHandler.GetPlayer(name).Factionrank = PlayerHandler.GetPlayer(name).Factionrank + 1;
                                PlayerHandler.GetPlayer(name).RefreshData(PlayerHandler.GetPlayer(name));
                                PlayerHandler.GetPlayer(name).SendNotification("Du hast den Spieler " + name + " auf Rang " + PlayerHandler.GetPlayer(name).Factionrank + " upranked.", "black", 3500);
                                MySqlQuery mySqlQuery =
                                new MySqlQuery("UPDATE accounts SET Fraktionrank = @rang WHERE Id = @id");
                                mySqlQuery.AddParameter("@id", PlayerHandler.GetPlayer(name).Id);
                                mySqlQuery.AddParameter("@rang", PlayerHandler.GetPlayer(name).Factionrank);
                                MySqlHandler.ExecuteSync(mySqlQuery);

                                if (PlayerHandler.GetPlayer(name) != null)
                                {
                                    PlayerHandler.GetPlayer(name).SendNotification("Dein Rang wurde auf " + PlayerHandler.GetPlayer(name).Factionrank + " upranked.", "black", 3500);
                                }
                            }
                            else
                            {
                                dbPlayer.SendNotification("Du hast keine Berechtigung, um die Rechte für diesen Spieler zu verändern.", "black", 3500);
                            }
                        }
                        else
                        {
                            dbPlayer.SendNotification("Dieser Spieler ist nicht in deiner Fraktion.", "black", 3500);
                        }
                    }
                    else
                    {
                        dbPlayer.SendNotification("Dieser Spieler existiert nicht.", "black", 3500);
                    }

                }
                else
                {
                    dbPlayer.SendNotification("Du hast dazu keine Berechtigung.", "black", 3500);
                }
            } catch(Exception ex) { Console.Write(ex.Message); }
        }

        [RemoteEvent("phone:downrank")]
        public void PhoneDownrank(Client p, string name)
        {
            try
            {
                DbPlayer dbPlayer = p.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                if (dbPlayer.Factionrank > 9)
                {
                    if (PlayerHandler.GetPlayer(name) != null)
                    {
                        if (PlayerHandler.GetPlayer(name).Faction == dbPlayer.Faction)
                        {
                            if (PlayerHandler.GetPlayer(name).Factionrank < dbPlayer.Factionrank)
                            {
                                PlayerHandler.GetPlayer(name).Factionrank = PlayerHandler.GetPlayer(name).Factionrank - 1;
                                PlayerHandler.GetPlayer(name).RefreshData(PlayerHandler.GetPlayer(name));
                                PlayerHandler.GetPlayer(name).SendNotification("Du hast den Spieler " + name + " auf Rang " + PlayerHandler.GetPlayer(name).Factionrank + " gederanked.", "black", 3500);
                                MySqlQuery mySqlQuery =
                                new MySqlQuery("UPDATE accounts SET Fraktionrank = @rang WHERE Id = @id");
                                mySqlQuery.AddParameter("@id", PlayerHandler.GetPlayer(name).Id);
                                mySqlQuery.AddParameter("@rang", PlayerHandler.GetPlayer(name).Factionrank);
                                MySqlHandler.ExecuteSync(mySqlQuery);

                                if (PlayerHandler.GetPlayer(name) != null)
                                {
                                    PlayerHandler.GetPlayer(name).SendNotification("Dein Rang wurde auf " + PlayerHandler.GetPlayer(name).Factionrank + " gederanked.", "black", 3500);
                                }
                            }
                            else
                            {
                                dbPlayer.SendNotification("Du hast keine Berechtigung, um die Rechte für diesen Spieler zu verändern.", "black", 3500);
                            }
                        }
                        else
                        {
                            dbPlayer.SendNotification("Dieser Spieler ist nicht in deiner Fraktion.", "black", 3500);
                        }
                    }
                    else
                    {
                        dbPlayer.SendNotification("Dieser Spieler existiert nicht.", "black", 3500);
                    }

                }
                else
                {
                    dbPlayer.SendNotification("Du hast dazu keine Berechtigung.", "black", 3500);
                }
            }
            catch (Exception ex) { Console.Write(ex.Message); }
        }

        [RemoteEvent("phone:invite")]
        public void PhoneInvite(Client c, string name)
        {
            if (c == null) return;
            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return;
            try
            {
                if (dbPlayer.Faction == null || dbPlayer.Faction.Id == 0)
                    return;

                if (dbPlayer.Factionrank > 9)
                {
                    DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);

                    if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                    {
                        dbPlayer.SendNotification("Spieler nicht online!", "black", 3500);
                        return;
                    }

                    /* List<DbPlayer> list = dbPlayer.Faction.GetFactionPlayers().FindAll((DbPlayer player) => player.SpielerFraktion);
                     if (list.Count >= 35)
                     {
                         dbPlayer.SendNotification("Deine Fraktion hat mehr als 35 Member, daher kannst du keinen Mehr einladen!", "black", 3500, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                     }*/

                    /* List<DbPlayer> factionPlayers = dbPlayer.Faction.GetFactionPlayers();

                     if (factionPlayers.FindAll(dbPlayer2.Id).Count >= 35)
                     {
                         dbPlayer.SendNotification("Deine Fraktion hat mehr als 35 Member, daher kannst du keinen Mehr einladen!", "black", 3500, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                     }*/


                    if (dbPlayer2.Faction.Id == dbPlayer.Faction.Id)
                    {
                        dbPlayer.SendNotification("Der Spieler ist bereits in deiner Fraktion.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);
                    }
                    else
                    {
                        if (dbPlayer2.Faction.Id == 0)
                        {
                            dbPlayer2.TriggerEvent("invite:open", dbPlayer.Faction.Name);
                            dbPlayer.SendNotification("Du hast " + name + " eine Einladung gesendet.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);
                        }
                        else
                        {
                            dbPlayer.SendNotification("Dieser Spieler ist bereits in einer Fraktion.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);
                        }
                    }
                }
                else
                {
                    dbPlayer.SendNotification("Du hast dazu keine Berechtigung.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION addPlayer] " + ex.Message);
                Logger.Print("[EXCEPTION addPlayer] " + ex.StackTrace);
            }
        }


        [RemoteEvent("Accept:Invite")]
        public void PhoneJoinfrak(Client c, string frak)
        {
            if (c == null) return;
            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return;

            try
            {
                Faction fraktion = FactionModule.getFactionByName(frak);
                dbPlayer.Faction = fraktion;
                dbPlayer.Factionrank = 0;
                dbPlayer.RefreshData(dbPlayer);

                dbPlayer.SetAttribute("Fraktion", fraktion.Id);
                dbPlayer.SetAttribute("Fraktionrank", 0);
                dbPlayer.SetAttribute("Medic", 0);

                c.TriggerEvent("updateTeamId", fraktion.Id);
                c.TriggerEvent("updateTeamRank", 0);
                c.TriggerEvent("updateJob", fraktion.Name);

                foreach (DbPlayer target in fraktion.GetFactionPlayers())
                {
                    target.SendNotification("" + c.Name + " ist jetzt ein Mitglied", fraktion.GetRGBStr(), 3000,  fraktion.Name);
                }

                dbPlayer.SendNotification("Du bist der Fraktion " + fraktion.Name + " beigetreten.", fraktion.GetRGBStr(), 3000,  fraktion.Name);
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION acceptInvite] " + ex.Message);
                Logger.Print("[EXCEPTION acceptInvite] " + ex.StackTrace);
            }
        }

        [RemoteEvent("phone:uninvite")]
        public void PhoneUninvite(Client p, string name)
        {
            if (p == null) return;
            DbPlayer dbPlayer = p.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return;

            try
            {

                if (dbPlayer.Faction == null || dbPlayer.Faction.Id == 0)
                    return;

                if (dbPlayer.Factionrank > 9)
                {
                    DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                    if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                    {
                        dbPlayer.SendNotification("Spieler nicht online!", "black", 3500);
                        return;
                    }

                    if (dbPlayer2.Faction.Id == dbPlayer.Faction.Id)
                    {
                        if (dbPlayer2.Factionrank < dbPlayer.Factionrank)
                        {
                            dbPlayer.SendNotification("Du hast den Spieler " + name + " uninvited.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);

                            dbPlayer2.SetAttribute("Fraktion", 0);
                            dbPlayer2.SetAttribute("Fraktionrank", 0);
                            dbPlayer2.SetAttribute("Medic", 0);

                            dbPlayer2.TriggerEvent("updateTeamId", 0);
                            dbPlayer2.TriggerEvent("updateTeamRank", 0);
                            dbPlayer2.TriggerEvent("updateJob", "Zivilist");

                            dbPlayer2.Faction = FactionModule.getFactionById(0);
                            dbPlayer2.Factionrank = 0;
                            dbPlayer2.RefreshData(dbPlayer2);
                            dbPlayer2.SendNotification("Du wurdest aus der Fraktion " + dbPlayer.Faction.Name + " gekickt.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);
                        }
                        else
                        {
                            dbPlayer.SendNotification("Du hast keine Berechtigung, um diesen Spieler zu uninviten.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);
                        }
                    }
                    else
                    {
                        dbPlayer.SendNotification("Dieser Spieler ist nicht in deiner Fraktion.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);
                    }
                }
                else
                {
                    dbPlayer.SendNotification("Du hast dazu keine Berechtigung.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION kickMember] " + ex.Message);
                Logger.Print("[EXCEPTION kickMember] " + ex.StackTrace);
            }
        }

        [RemoteEvent("phone:parkcars")]
        public void PhoneParkcars(Client c)
        {
            if ((Entity)(object)c == null)
            {
                return;
            }
            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(ignorelogin: true) || (Entity)(object)dbPlayer.Client == null || !dbPlayer.CanInteractAntiFlood(1))
            {
                return;
            }
            try
            {
                if (dbPlayer.Faction != null && dbPlayer.Faction.Id != 0)
                {
                    dbPlayer.Faction.GetFactionPlayers().ForEach(delegate (DbPlayer target)
                    {
                        target.SendNotification("Alle Fraktionsfahrzeuge wurden von " + c.Name + " eingeparkt.", dbPlayer.Faction.GetRGBStr(), 3000, dbPlayer.Faction.Name);
                    });
                    NAPI.Pools.GetAllVehicles().FindAll((Vehicle veh) => veh.GetVehicle() != null && veh.GetVehicle().Fraktion != null && veh.GetVehicle().Fraktion.Id == dbPlayer.Faction.Id).ForEach(delegate (Vehicle veh)
                    {
                        ((Entity)veh).Delete();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION parkCars] " + ex.Message);
                Logger.Print("[EXCEPTION parkCars] " + ex.StackTrace);
            }
        }
    }
}
