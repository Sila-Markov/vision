using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GVMP
{
    class GangwarModule : GVMP.Module.Module<GangwarModule>
    {
        public static List<Gangwar> GWZones = new List<Gangwar>();
        public static List<Gangwar> BlockedZones = new List<Gangwar>();
        public static Gangwar RunningGangwar = null;
        public static Blip GangwarBlip = null;
        public static Marker GangwarMarker = null;
        public override Type[] RequiredModules() => new Type[1]
        {
            typeof (FactionModule)
        };
        
        protected override bool OnLoad()
        {
            MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM gangwars");
            MySqlResult mySqlResult = MySqlHandler.GetQuery(mySqlQuery);
            MySqlDataReader reader = mySqlResult.Reader;
             
            while (reader.Read())
            {
                Gangwar gangwar = new Gangwar
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Faction = FactionModule.getFactionById(reader.GetInt32("Faction")),
                    Zone = NAPI.Util.FromJson<Vector3>(reader.GetString("Zone")),
                    Flag1 = new Flag(NAPI.Util.FromJson<Vector3>(reader.GetString("Flag1"))),
                    Flag2 = new Flag(NAPI.Util.FromJson<Vector3>(reader.GetString("Flag2"))),
                    Flag3 = new Flag(NAPI.Util.FromJson<Vector3>(reader.GetString("Flag3"))),
                    Flag4 = new Flag(NAPI.Util.FromJson<Vector3>(reader.GetString("Flag4"))),

                    PlayerSpawn1 = new Flag(NAPI.Util.FromJson<Vector3>(reader.GetString("PlayerSpawn1"))),
                    PlayerSpawn2 = new Flag(NAPI.Util.FromJson<Vector3>(reader.GetString("PlayerSpawn2"))),

                    RotationPlayerSpawn1 = reader.GetFloat("RotationPlayerSpawn1"),
                    RotationPlayerSpawn2 = reader.GetFloat("RotationPlayerSpawn1"),

                    CarSpawn1 = NAPI.Util.FromJson<Vector3>(reader.GetString("CarSpawn1")),
                    CarSpawn2 = NAPI.Util.FromJson<Vector3>(reader.GetString("CarSpawn2")),

                    Attacker = null,
                    AttackerPoints = 0,
                    FactionPoints = 0,
                    StopDate = DateTime.Now
                };
                GWZones.Add(gangwar);
                ColShape c = NAPI.ColShape.CreateCylinderColShape(gangwar.Zone, 1.4f, 1.4f, 0);
                c.SetData("FUNCTION_MODEL", new FunctionModel("StartGangwar", gangwar.Id));
                c.SetData("MESSAGE", new Message("Benutze E um einen Gangwar zu starten.", "GANGWAR", "orange", 3000));

                GangwarBlip = NAPI.Blip.CreateBlip(543, gangwar.Zone, 1.0f, (byte)gangwar.Faction.Blip, gangwar.Name + " - " + gangwar.Faction.Name, 255, 0, true, 0, uint.MaxValue);
                GangwarMarker = NAPI.Marker.CreateMarker(1, gangwar.Zone, new Vector3(), new Vector3(), 1.0f, gangwar.Faction.RGB, false, 0);
            }

            reader.Dispose();
            mySqlResult.Connection.Dispose();
            return true;
        }
        public static Gangwar FindGWById(int Id)
        {
            return GWZones.FirstOrDefault((Gangwar GWZone) => GWZone.Id == Id);
        }

        [RemoteEvent("StartGangwar")]
        public void StartGangwar(Client c, int id)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                if (dbPlayer.Faction.Id == 0) return;
                string gangwarname = "Kein gebiet";
                string gangwarowner = "Niemand";
                int gangwarid = id;


                foreach (Gangwar gangwar in GWZones)
                {
                    if (gangwar.Id == id)
                    {
                        gangwarname = gangwar.Name;
                        gangwarowner = gangwar.Faction.Name;
                    }
                }
                dbPlayer.SetData("CurrentgangwarID", id);
                dbPlayer.Client.TriggerEvent("gwstart:open", gangwarname, gangwarowner, id);
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION StartGangwar] " + ex.Message);
                Logger.Print("[EXCEPTION StartGangwar] " + ex.StackTrace);
            }
        }

        [RemoteEvent("gwhud:startgw")]
        public void Gangwar(Client c)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                int id = 0;
                id = dbPlayer.GetData("CurrentgangwarID");
                if (id == 0) return;
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null) return;
                Console.WriteLine(id.ToString());
                if (dbPlayer.Faction.Id == 0) return;

                dbPlayer.CloseNativeMenu();

                Gangwar gw = FindGWById(id);

                if (gw == null) return;
                if (dbPlayer.Faction == gw.Faction) return;

                if (gw.Faction.GetFactionPlayers().Count < 0)
                {
                    dbPlayer.SendNotification("Es müssen mindestens 3 Personen aus der anderen Fraktion online sein.", "black", 3500);
                    return;
                }
                if (BlockedZones.Contains(gw))
                {
                    dbPlayer.SendNotification("Dieses Gebiet wurde bereits angegriffen.", "black", 3500);
                    return;
                }
                if (RunningGangwar != null)
                {
                    dbPlayer.SendNotification("Es läuft bereits ein Gangwar.", "black", 3500);
                    return;
                }
                if (gw.Faction.Id == 0)
                {
                    dbPlayer.SendNotification("Du hast das Gebiet eingenommen.", "black", 3500);
                    GWZones.Remove(gw);
                    gw.Faction = dbPlayer.Faction;
                    GWZones.Add(gw);
                    MySqlQuery mySqlQuery = new MySqlQuery("UPDATE gangwars SET Faction = @faction WHERE Id = @id");
                    mySqlQuery.AddParameter("@id", gw.Id);
                    mySqlQuery.AddParameter("@faction", dbPlayer.Faction.Id);
                    MySqlHandler.ExecuteSync(mySqlQuery);
                    return;
                }
                gw.Attacker = dbPlayer.Faction;
                dbPlayer.SetDimension(FactionModule.GangwarDimension);
                dbPlayer.Position = gw.PlayerSpawn1.Position;
                c.Rotation = new Vector3(0f, 0f, gw.RotationPlayerSpawn1);

                foreach (DbPlayer dbTarget in gw.Faction.GetFactionPlayers())
                {
                        dbTarget.SendNotification($"Das Gebiet {gw.Name} wird von der Fraktion {gw.Attacker.Name} angegriffen.", "black", 6000, "GANGWAR");
                }
                foreach (DbPlayer dbTarget in gw.Attacker.GetFactionPlayers())
                {
                    if (dbTarget != null && dbTarget.IsValid(true))
                        dbTarget.SendNotification($"Deine Fraktion greift das Gebiet {gw.Name} an.", "black", 6000, "GANGWAR");
                }
                Notification.SendGlobalNotification($"Die Fraktion {gw.Attacker.Name} greift das Gebiet {gw.Name} von der Fraktion {gw.Faction.Name} an.", 8000, "orange", Notification.icon.warn);

                WebhookSender.SendMessage("Gangwar gestartet!", "Gebiet: " + gw.Name + " Angreifer: " + gw.Attacker.Name + " Verteidiger: " + gw.Faction.Name, Webhooks.gangwarlogs, "Gangwar");
                BlockedZones.Add(gw);
                RunningGangwar = gw;

                gw.StopDate = DateTime.Now.AddMinutes(45);

                ColShape col = NAPI.ColShape.CreateCylinderColShape(gw.Zone, 250, 25f, Convert.ToUInt32(FactionModule.GangwarDimension));
                col.SetData("GANGWAR", true);

                Marker m = NAPI.Marker.CreateMarker(1, gw.Zone, new Vector3(), new Vector3(), 600, new Color(240, 132, 0), false, Convert.ToUInt32(FactionModule.GangwarDimension));
                m.SetData("GANGWAR", true);

                ColShape c1 = NAPI.ColShape.CreateCylinderColShape(gw.Flag1.Position.Add(new Vector3(0, 0, 1)), 1.4f, 1.4f, Convert.ToUInt32(FactionModule.GangwarDimension));
                c1.SetData("GANGWAR_FLAG", 1);

                NAPI.Marker.CreateMarker(4, gw.Flag1.Position.Add(new Vector3(0, 0, 1)), new Vector3(), new Vector3(), 1.0f, new Color(255, 140, 0), false, Convert.ToUInt32(FactionModule.GangwarDimension));
                
                ColShape c2 = NAPI.ColShape.CreateCylinderColShape(gw.Flag2.Position.Add(new Vector3(0, 0, 1)), 1.4f, 1.4f, Convert.ToUInt32(FactionModule.GangwarDimension));
                c2.SetData("GANGWAR_FLAG", 2);

                NAPI.Marker.CreateMarker(4, gw.Flag2.Position.Add(new Vector3(0, 0, 1)), new Vector3(), new Vector3(), 1.0f, new Color(255, 140, 0), false, Convert.ToUInt32(FactionModule.GangwarDimension));
               
                ColShape c3 = NAPI.ColShape.CreateCylinderColShape(gw.Flag3.Position.Add(new Vector3(0, 0, 1)), 1.4f, 1.4f, Convert.ToUInt32(FactionModule.GangwarDimension));
                c3.SetData("GANGWAR_FLAG", 3);

                NAPI.Marker.CreateMarker(4, gw.Flag3.Position.Add(new Vector3(0, 0, 1)), new Vector3(), new Vector3(), 1.0f, new Color(255, 140, 0), false, Convert.ToUInt32(FactionModule.GangwarDimension));
                
                ColShape c4 = NAPI.ColShape.CreateCylinderColShape(gw.Flag4.Position.Add(new Vector3(0, 0, 1)), 1.4f, 1.4f, Convert.ToUInt32(FactionModule.GangwarDimension));
                c4.SetData("GANGWAR_FLAG", 4);

                NAPI.Marker.CreateMarker(4, gw.Flag4.Position.Add(new Vector3(0, 0, 1)), new Vector3(), new Vector3(), 1.0f, new Color(255, 140, 0), false, Convert.ToUInt32(FactionModule.GangwarDimension));
                dbPlayer.StopAnimation();
                dbPlayer.SendNotification("Anti-Car", "black", 3000);
            }
            catch(Exception ex)
            {
                Logger.Print("[EXCEPTION Gangwar] " + ex.Message);
                Logger.Print("[EXCEPTION Gangwar] " + ex.StackTrace);
            }
        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public void EnterGWZone(ColShape col, Client c)
        {
            try
            {
                if (c == null || col == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null) return;
                if (RunningGangwar == null) return;
                if (dbPlayer.Faction != RunningGangwar.Faction && dbPlayer.Faction != RunningGangwar.Attacker) return;
                if (col.HasData("GANGWAR"))
                {
                    dbPlayer.SetData("IN_GANGWAR", true);
                    c.GiveWeapon(WeaponHash.Gusenberg, 5000);
                    c.GiveWeapon(WeaponHash.AdvancedRifle, 0);
                    c.GiveWeapon(WeaponHash.AssaultRifle, 0);
                    c.GiveWeapon(WeaponHash.BullpupRifle, 5000);
                    c.GiveWeapon(WeaponHash.HeavyPistol, 5000);
                    c.TriggerEvent("gangwar:open", RunningGangwar.Name, (int)(RunningGangwar.StopDate - DateTime.Now).TotalMinutes+1 + " Minute/n", RunningGangwar.Attacker.Name, RunningGangwar.Attacker.Logo, RunningGangwar.AttackerPoints, RunningGangwar.Attacker.GetRGBStr(), RunningGangwar.Faction.Name, RunningGangwar.Faction.Logo, RunningGangwar.FactionPoints, RunningGangwar.Faction.GetRGBStr());
                }
                else if (col.HasData("GANGWAR_FLAG"))
                {
                    int data = (int)((dynamic)col.GetData("GANGWAR_FLAG"));
                    dbPlayer.SendNotification(string.Concat("Du hast die Flagge ", data.ToString(), " betreten."), "black", 3500);
                    switch (data)
                    {
                        case 1:
                            {
                                RunningGangwar.Flag1.Faction = dbPlayer.Faction.Id;
                                break;
                            }
                        case 2:
                            {
                                RunningGangwar.Flag2.Faction = dbPlayer.Faction.Id;
                                break;
                            }
                        case 3:
                            {
                                RunningGangwar.Flag3.Faction = dbPlayer.Faction.Id;
                                break;
                            }
                        case 4:
                            {
                                RunningGangwar.Flag4.Faction = dbPlayer.Faction.Id;
                                break;
                            }
                    }
                }
            }

            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION EnterGWZone] " + ex.Message);
                Logger.Print("[EXCEPTION EnterGWZone] " + ex.StackTrace);
            }
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public void ExitGWZone(ColShape col, Client c)
        {
            try
            {
                if (c == null || col == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null) return;
                if (RunningGangwar == null) return;
                if (dbPlayer.Faction != RunningGangwar.Faction && dbPlayer.Faction != RunningGangwar.Attacker) return;
                if (col.HasData("GANGWAR"))
                {
                    dbPlayer.ResetData("IN_GANGWAR");
                    c.TriggerEvent("gangwar:close");
                }
                else if (col.HasData("GANGWAR_FLAG"))
                {
                    int data = (int)((dynamic)col.GetData("GANGWAR_FLAG"));
                    dbPlayer.SendNotification("Du hast die Flagge " + data + " verlassen.", "black", 3500);

                    switch (data)
                    {
                        case 1:
                            {
                                RunningGangwar.Flag1.Faction = 0;
                                break;
                            }
                        case 2:
                            {
                                RunningGangwar.Flag2.Faction = 0;
                                break;
                            }
                        case 3:
                            {
                                RunningGangwar.Flag3.Faction = 0;
                                break;
                            }
                        case 4:
                            {
                                RunningGangwar.Flag4.Faction = 0;
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION ExitGWZone] " + ex.Message);
                Logger.Print("[EXCEPTION ExitGWZone] " + ex.StackTrace);
            }
        }


        public static void handleKill(DbPlayer killer)
        {
            try
            {
                //Faction fraktion = FactionModule.getFactionByName(frak);

                if (killer == null || killer.Client == null) return;
                if (!killer.HasData("IN_GANGWAR") || RunningGangwar == null) return;

                if (killer.Faction.Id == RunningGangwar.Faction.Id)
                {
                    RunningGangwar.FactionPoints += 3;
                }
                else if (killer.Faction.Id == RunningGangwar.Attacker.Id)
                {
                    RunningGangwar.AttackerPoints += 3;
                }

                killer.Faction.GetFactionPlayers()
                    .ForEach((DbPlayer dbPlayer) => dbPlayer.SendNotification("+3 Punkte für das Töten eines Gegners!", $"{killer.Faction.RGB}", 5000, $"{killer.Faction.Name}"));

                RunningGangwar.Faction.GetFactionPlayers().ForEach(e =>
                {
                    if (e.Client.Position.DistanceTo(RunningGangwar.Zone) < 250)
                        e.Client.TriggerEvent("gangwar:refreshdata",(int)(RunningGangwar.StopDate - DateTime.Now).TotalMinutes+1 + " Minute/n", RunningGangwar.AttackerPoints, RunningGangwar.FactionPoints);
                });
                RunningGangwar.Attacker.GetFactionPlayers().ForEach(e =>
                {
                    if (e.Client.Position.DistanceTo(RunningGangwar.Zone) < 250)
                        e.Client.TriggerEvent("gangwar:refreshdata",(int)(RunningGangwar.StopDate - DateTime.Now).TotalMinutes+1 + " Minute/n", RunningGangwar.AttackerPoints, RunningGangwar.FactionPoints);
                });
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION handleKill] " + ex.Message);
                Logger.Print("[EXCEPTION handleKill] " + ex.StackTrace);
            }
        }

        public override void OnFiveSecUpdate()
        {
            try
            {
                if (RunningGangwar == null) return;

                if (RunningGangwar.StopDate < DateTime.Now)
                {
                    EndGangwar();
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION Gangwar OnFiveSecUpdate] " + ex.Message);
                Logger.Print("[EXCEPTION Gangwar OnFiveSecUpdate] " + ex.StackTrace);
            }
        }

        public override void OnMinuteUpdate()
        {
            try
            {
                if (RunningGangwar == null) return;

                if (RunningGangwar.Flag1.Faction == RunningGangwar.Faction.Id)
                {
                    RunningGangwar.FactionPoints += 3;
                }
                else if (RunningGangwar.Flag1.Faction == RunningGangwar.Attacker.Id)
                {
                    RunningGangwar.AttackerPoints += 3;
                }
                if (RunningGangwar.Flag2.Faction == RunningGangwar.Faction.Id)
                {
                    RunningGangwar.FactionPoints += 3;
                }
                else if (RunningGangwar.Flag2.Faction == RunningGangwar.Attacker.Id)
                {
                    RunningGangwar.AttackerPoints += 3;
                }
                if (RunningGangwar.Flag3.Faction == RunningGangwar.Faction.Id)
                {
                    RunningGangwar.FactionPoints += 3;
                }
                else if (RunningGangwar.Flag3.Faction == RunningGangwar.Attacker.Id)
                {
                    RunningGangwar.AttackerPoints += 3;
                }
                if (RunningGangwar.Flag4.Faction == RunningGangwar.Faction.Id)
                {
                    RunningGangwar.FactionPoints += 3;
                }
                else if (RunningGangwar.Flag4.Faction == RunningGangwar.Attacker.Id)
                {
                    RunningGangwar.AttackerPoints += 3;
                }

                RunningGangwar.Faction.GetFactionPlayers().ForEach(e =>
                {
                    if (e.Client.Position.DistanceTo(RunningGangwar.Zone) < 250)
                        e.Client.TriggerEvent("gangwar:refreshdata", (int)(RunningGangwar.StopDate - DateTime.Now).TotalMinutes+1 + " Minute/n", RunningGangwar.AttackerPoints, RunningGangwar.FactionPoints);
                });

                RunningGangwar.Attacker.GetFactionPlayers().ForEach(e =>
                {
                    if (e.Client.Position.DistanceTo(RunningGangwar.Zone) < 250)
                        e.Client.TriggerEvent("gangwar:refreshdata", (int)(RunningGangwar.StopDate - DateTime.Now).TotalMinutes+1 + " Minute/n", RunningGangwar.AttackerPoints, RunningGangwar.FactionPoints);
                });
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION AddGWPoints] " + ex.Message);
                Logger.Print("[EXCEPTION AddGWPoints] " + ex.StackTrace);
            }
        }

        public static void EndGangwar()
        {
            try
            {
                if (RunningGangwar.FactionPoints == RunningGangwar.AttackerPoints)
                {
                    RunningGangwar.FactionPoints++;
                }
                foreach (DbPlayer player in PlayerHandler.GetPlayers())
                {
                    if (player.Client.Dimension == 8888)
                    {
                      player.TriggerEvent("gangwar:close");
                      if(player.Faction == RunningGangwar.Faction || player.Faction == RunningGangwar.Attacker)
                      {
                            player.TriggerEvent("gangwarstats:open", RunningGangwar.Name, RunningGangwar.Faction.Name, RunningGangwar.Attacker.Name, RunningGangwar.FactionPoints, RunningGangwar.AttackerPoints, RunningGangwar.Faction.GetRGBStr(), RunningGangwar.Attacker.GetRGBStr(), RunningGangwar.Faction.Logo, RunningGangwar.Attacker.Logo);
                            player.Client.Position = player.Faction.Storage;
                            player.Dimension = 0;
                            player.RemoveAllWeapons();
                            WeaponManager.loadWeapons(player.Client);
                      }
                    }
                }

                MySqlQuery mySqlQuery = new MySqlQuery("UPDATE gangwars SET Faction = @faction WHERE Id = @id");
                mySqlQuery.AddParameter("@id", RunningGangwar.Id);
                if (RunningGangwar.AttackerPoints > RunningGangwar.FactionPoints)
                {
                    RunningGangwar.Faction = RunningGangwar.Attacker;
                    mySqlQuery.AddParameter("@faction", RunningGangwar.Attacker.Id);
                    Notification.SendGlobalNotification(
                        $"Die Fraktion {RunningGangwar.Attacker.Name} hat den Kampf um das Gebiet {RunningGangwar.Name} gewonnen.",
                        8000, "orange", Notification.icon.warn);

                    WebhookSender.SendMessage("Gangwar zuende!", "Gebiet: " + RunningGangwar.Name + " Gewinner: " + RunningGangwar.Attacker.Name, Webhooks.gangwarlogs, "Gangwar");
                }
                else
                {
                    mySqlQuery.AddParameter("@faction", RunningGangwar.Faction.Id);
                    Notification.SendGlobalNotification(
                        $"Die Fraktion {RunningGangwar.Faction.Name} hat den Kampf um das Gebiet {RunningGangwar.Name} gewonnen.",
                        8000, "orange", Notification.icon.warn);
                    WebhookSender.SendMessage("Gangwar zuende!", $"Gebiet:  {RunningGangwar.Name } - " + 
                        $" Gewinner: " + RunningGangwar.Faction.Name, Webhooks.gangwarlogs, "Gangwar");
                }

                GangwarBlip.Delete();
                GangwarMarker.Delete();
                NAPI.Blip.CreateBlip(543, RunningGangwar.Zone, 1.0f, (byte)RunningGangwar.Attacker.Blip, RunningGangwar.Name + " - " + RunningGangwar.Attacker.Name, 255, 0, true, 0, uint.MaxValue);
                NAPI.Marker.CreateMarker(1, RunningGangwar.Zone, new Vector3(), new Vector3(), 1.0f, RunningGangwar.Attacker.RGB, false, 0);
                MySqlHandler.ExecuteSync(mySqlQuery);
                RunningGangwar.Flag1.Faction = 0;
                RunningGangwar.Flag2.Faction = 0;
                RunningGangwar.Flag3.Faction = 0;
                RunningGangwar.Flag4.Faction = 0;
                
                RunningGangwar = null;

            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION EndGangwar] " + ex.Message);
                Logger.Print("[EXCEPTION EndGangwar] " + ex.StackTrace);
            }
        }
    }
}
