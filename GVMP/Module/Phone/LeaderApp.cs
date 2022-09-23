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
            Console.Write("Leaderapp geladen.");
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
                                PlayerHandler.GetPlayer(name).SendNotification("Du hast den Spieler " + name + " auf Rang " + PlayerHandler.GetPlayer(name).Factionrank + " upranked.");
                                MySqlQuery mySqlQuery =
                                new MySqlQuery("UPDATE accounts SET Fraktionrank = @rang WHERE Id = @id");
                                mySqlQuery.AddParameter("@id", PlayerHandler.GetPlayer(name).Id);
                                mySqlQuery.AddParameter("@rang", PlayerHandler.GetPlayer(name).Factionrank);
                                MySqlHandler.ExecuteSync(mySqlQuery);

                                if (PlayerHandler.GetPlayer(name) != null)
                                {
                                    PlayerHandler.GetPlayer(name).SendNotification("Dein Rang wurde auf " + PlayerHandler.GetPlayer(name).Factionrank + " upranked.");
                                }
                            }
                            else
                            {
                                dbPlayer.SendNotification("Du hast keine Berechtigung, um die Rechte für diesen Spieler zu verändern.");
                            }
                        }
                        else
                        {
                            dbPlayer.SendNotification("Dieser Spieler ist nicht in deiner Fraktion.");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNotification("Dieser Spieler existiert nicht.");
                    }

                }
                else
                {
                    dbPlayer.SendNotification("Du hast dazu keine Berechtigung.");
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
                                PlayerHandler.GetPlayer(name).SendNotification("Du hast den Spieler " + name + " auf Rang " + PlayerHandler.GetPlayer(name).Factionrank + " gederanked.");
                                MySqlQuery mySqlQuery =
                                new MySqlQuery("UPDATE accounts SET Fraktionrank = @rang WHERE Id = @id");
                                mySqlQuery.AddParameter("@id", PlayerHandler.GetPlayer(name).Id);
                                mySqlQuery.AddParameter("@rang", PlayerHandler.GetPlayer(name).Factionrank);
                                MySqlHandler.ExecuteSync(mySqlQuery);

                                if (PlayerHandler.GetPlayer(name) != null)
                                {
                                    PlayerHandler.GetPlayer(name).SendNotification("Dein Rang wurde auf " + PlayerHandler.GetPlayer(name).Factionrank + " gederanked.");
                                }
                            }
                            else
                            {
                                dbPlayer.SendNotification("Du hast keine Berechtigung, um die Rechte für diesen Spieler zu verändern.");
                            }
                        }
                        else
                        {
                            dbPlayer.SendNotification("Dieser Spieler ist nicht in deiner Fraktion.");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNotification("Dieser Spieler existiert nicht.");
                    }

                }
                else
                {
                    dbPlayer.SendNotification("Du hast dazu keine Berechtigung.");
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
                        dbPlayer.SendNotification("Spieler nicht online!", 3000, "red");
                        return;
                    }

                    /* List<DbPlayer> list = dbPlayer.Faction.GetFactionPlayers().FindAll((DbPlayer player) => player.SpielerFraktion);
                     if (list.Count >= 35)
                     {
                         dbPlayer.SendNotification("Deine Fraktion hat mehr als 35 Member, daher kannst du keinen Mehr einladen!", 5000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                     }*/

                    /* List<DbPlayer> factionPlayers = dbPlayer.Faction.GetFactionPlayers();

                     if (factionPlayers.FindAll(dbPlayer2.Id).Count >= 35)
                     {
                         dbPlayer.SendNotification("Deine Fraktion hat mehr als 35 Member, daher kannst du keinen Mehr einladen!", 5000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                     }*/


                    if (dbPlayer2.Faction.Id == dbPlayer.Faction.Id)
                    {
                        dbPlayer.SendNotification("Der Spieler ist bereits in deiner Fraktion.", 3000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                    }
                    else
                    {
                        if (dbPlayer2.Faction.Id == 0)
                        {
                            dbPlayer2.TriggerEvent("invite:open", dbPlayer.Faction.Name);
                            dbPlayer.SendNotification("Du hast " + name + " eine Einladung gesendet.", 3000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                        }
                        else
                        {
                            dbPlayer.SendNotification("Dieser Spieler ist bereits in einer Fraktion.", 3000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                        }
                    }
                }
                else
                {
                    dbPlayer.SendNotification("Du hast dazu keine Berechtigung.", 3000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
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
                    target.SendNotification("" + c.Name + " ist jetzt ein Mitglied", 3000, fraktion.GetRGBStr(), fraktion.Name);
                }

                dbPlayer.SendNotification("Du bist der Fraktion " + fraktion.Name + " beigetreten.", 3000, fraktion.GetRGBStr(), fraktion.Name);
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
                        dbPlayer.SendNotification("Spieler nicht online!", 3000, "red");
                        return;
                    }

                    if (dbPlayer2.Faction.Id == dbPlayer.Faction.Id)
                    {
                        if (dbPlayer2.Factionrank < dbPlayer.Factionrank)
                        {
                            dbPlayer.SendNotification("Du hast den Spieler " + name + " uninvited.", 3000,
                                dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);

                            dbPlayer2.SetAttribute("Fraktion", 0);
                            dbPlayer2.SetAttribute("Fraktionrank", 0);
                            dbPlayer2.SetAttribute("Medic", 0);

                            dbPlayer2.TriggerEvent("updateTeamId", 0);
                            dbPlayer2.TriggerEvent("updateTeamRank", 0);
                            dbPlayer2.TriggerEvent("updateJob", "Zivilist");

                            dbPlayer2.Faction = FactionModule.getFactionById(0);
                            dbPlayer2.Factionrank = 0;
                            dbPlayer2.RefreshData(dbPlayer2);
                            dbPlayer2.SendNotification("Du wurdest aus der Fraktion " + dbPlayer.Faction.Name + " gekickt.", 3000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                        }
                        else
                        {
                            dbPlayer.SendNotification("Du hast keine Berechtigung, um diesen Spieler zu uninviten.", 3000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                        }
                    }
                    else
                    {
                        dbPlayer.SendNotification("Dieser Spieler ist nicht in deiner Fraktion.", 3000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
                    }
                }
                else
                {
                    dbPlayer.SendNotification("Du hast dazu keine Berechtigung.", 3000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
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
                        target.SendNotification("Alle Fraktionsfahrzeuge wurden von " + c.Name + " eingeparkt.", 3000, dbPlayer.Faction.GetRGBStr(), dbPlayer.Faction.Name);
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
