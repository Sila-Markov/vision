using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using static System.Net.Mime.MediaTypeNames;

namespace GVMP
{
    public class Main : Script
    {
        public static int timeToRestart;
        public void InitGameMode()
        {
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetCommandErrorMessage(" ");
            NAPI.Server.SetGlobalServerChat(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);

            Modules.Instance.LoadAll();
            MySqlHandler.ExecuteSync(new MySqlQuery("UPDATE vehicles SET Parked = 1"));
            Logger.Print("Alle Autos Wurden Eingeparkt!");
            Console.WriteLine();

            Logger.Print("Vision Crimelife Wurde Erfolgreich Gestartet Und Geladen");
            Console.WriteLine();
        }
        public bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Print("[EXCEPTION UnauthorizedAccessException] " + ex.Message);
                Logger.Print("[EXCEPTION UnauthorizedAccessException] " + ex.StackTrace);
                isAdmin = false;
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION IsAdminMain] " + ex.Message);
                Logger.Print("[EXCEPTION IsAdminMain] " + ex.StackTrace);
                isAdmin = false;
            }
            return isAdmin;
        }

        public static void OnSecHandler()
        {
            foreach (DbPlayer dbPlayer in PlayerHandler.GetPlayers())
            {
                Client client = dbPlayer.Client;
                if (client == null) return;

                if (dbPlayer.DeathData.IsDead)
                {
                    NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Client, WeaponHash.Unarmed);
                }
            }
        }
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStartHandler()
        {
            InitGameMode();
            timeToRestart = 10000;
            SyncThread.Init();
            SyncThread.Instance.Start();
        }



        public static void OnHourHandler()
        {
            try
            {
                foreach (DbPlayer dbPlayer in PlayerHandler.GetPlayers())
                {
                    if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                        continue;

                    dbPlayer.SetAttribute("XP", (int)dbPlayer.GetAttributeInt("XP") + 1);
                    dbPlayer.XP = dbPlayer.XP + 1;
                    dbPlayer.RefreshData(dbPlayer);

                    if ((int)dbPlayer.GetAttributeInt("XP") >= dbPlayer.Level * 4)
                    {
                        dbPlayer.SetAttribute("Level", (int)dbPlayer.GetAttributeInt("Level") + 1);
                        dbPlayer.Level = dbPlayer.Level + 1;
                        dbPlayer.RefreshData(dbPlayer);
                        dbPlayer.SendNotification("Glueckwunsch, Sie haben nun Level " + dbPlayer.Level + " erreicht!", "black", 6000, "Level aufgestiegen!");
                        dbPlayer.SendNotification("Durch Ihr Levelup haben Sie " + dbPlayer.Level + " erhalten!", "black", 3500, "#2f2f30");
                    }

                    House house = HouseModule.houses.FirstOrDefault((House house2) => house2.TenantsIds.Contains(dbPlayer.Id));

                    if (house != null)
                    {
                        int price = 0;

                        if (house.TenantPrices.ContainsKey(dbPlayer.Id))
                            price = house.TenantPrices[dbPlayer.Id];

                        dbPlayer.SendNotification("Dir wurde dein Mietpreis abgezogen! -" + price.ToDots() + "$", "black", 6000);
                        dbPlayer.removeMoney(price);
                    }

                    dbPlayer.addMoney(2500000);
                    dbPlayer.SendNotification("Sie haben ihren Payday erhalten! +2.500.000$", "black", 6000, "KONTOVERAENDERUNG");
                    Adminrank adminranks = dbPlayer.Adminrank;

                    if (adminranks.Permission >= 91)
                    {
                        dbPlayer.SendNotification("Da du ein Teamler bekommst du einen extra PayDay! +1.000.000$", "black", 6000, "KONTOVERAENDERUNG");
                        dbPlayer.addMoney(1000000);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION OnHourSpent] " + ex.Message);
                Logger.Print("[EXCEPTION OnHourSpent] " + ex.StackTrace);
            }
        }


        public static void OnMinHandler()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(Configuration.connectionString);
                con.ClearAllPoolsAsync();
                con.Dispose();



                foreach (DbPlayer dbPlayer in PlayerHandler.GetPlayers())
                {
                    if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null || dbPlayer.Client.IsNull)
                        continue;

                    dbPlayer.Client.TriggerEvent("Hud:UpdatePlayers", NAPI.Pools.GetAllPlayers().Count);
                    if (dbPlayer.DeathData.IsDead)
                    {
                        if (dbPlayer.Client == null) return;
                        DeathData deathData = dbPlayer.DeathData;
                        DateTime dateTime = deathData.DeathTime;
                        dbPlayer.disableAllPlayerActions(true);
                        dbPlayer.SetInvincible(true);
                        dbPlayer.StopAnimation();
                        dbPlayer.PlayAnimation(33, "missarmenian2", "corpse_search_exit_ped", 8f);

                        if (DateTime.Now.Subtract(dateTime).TotalMinutes >= 2)
                        {
                            dbPlayer.DeathData = new DeathData
                            {
                                IsDead = false,
                                DeathTime = new DateTime(0)
                            };

                            if (dbPlayer.Faction.Id == 0)
                                dbPlayer.SpawnPlayer(new Vector3(298.08, -584.53, 43.26));
                            else
                                dbPlayer.SpawnPlayer(dbPlayer.Faction.Spawn);

                            dbPlayer.disableAllPlayerActions(false);
                            dbPlayer.StopAnimation();
                            dbPlayer.TriggerEvent("toggleBlurred", false);
                            //dbPlayer.TriggerEvent("toggleBlurred", false);
                            dbPlayer.SendNotification("Du wurdest wiederbelebt!", "black", 6000);
                            dbPlayer.SetAttribute("Death", 0);
                            dbPlayer.SetInvincible(false);
                            dbPlayer.SetHealth(200);
                            dbPlayer.SetArmor(0);

                            if (dbPlayer.Client.Dimension == FactionModule.GangwarDimension)
                            {
                                NAPI.Task.Run(delegate
                                {
                                    dbPlayer.disableAllPlayerActions(false);
                                    dbPlayer.StopAnimation();
                                    dbPlayer.TriggerEvent("toggleBlurred", false);
                                    //dbPlayer.TriggerEvent("toggleBlurred", false);
                                    dbPlayer.SendNotification("Gangwar-Deathbug", "black", 6000);
                                    dbPlayer.SetAttribute("Death", 0);
                                    dbPlayer.SetInvincible(false);
                                }, 3500);
                            }

                            if (dbPlayer.Client.Dimension != FactionModule.GangwarDimension)
                            {
                                dbPlayer.GetInventoryItems().ForEach((ItemModel itemModel) => dbPlayer.UpdateInventoryItems(itemModel.Name, itemModel.Amount, true));
                                dbPlayer.RemoveAllWeapons(true);
                            }
                        }
                    }

                    MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Id = @userId LIMIT 1");
                    mySqlQuery.AddParameter("@userId", dbPlayer.Id);
                    MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
                    MySqlDataReader reader = mySqlReaderCon.Reader;
                    try
                    {
                        /*if (!reader.HasRows)
                        {
                            dbPlayer.Client.SendNotification("Ungültiger Account!");
                            dbPlayer.Client.Kick();
                            continue;
                        }
                        else*/
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (reader.GetInt32("Fraktion") != dbPlayer.Faction.Id)
                                {
                                    Faction oldfraktion = dbPlayer.Faction;
                                    Faction newfraktion = FactionModule.getFactionById(reader.GetInt32("Fraktion"));

                                    dbPlayer.Faction = newfraktion;
                                    dbPlayer.RefreshData(dbPlayer);
                                }

                                if (reader.GetInt32("Fraktionrank") != dbPlayer.Factionrank)
                                {
                                    dbPlayer.Factionrank = reader.GetInt32("Fraktionrank");
                                    dbPlayer.RefreshData(dbPlayer);
                                }
                                if (reader.GetInt32("Business") != dbPlayer.Business.Id)
                                {
                                    Business businessById = BusinessModule.getBusinessById(reader.GetInt32("Business"));
                                    dbPlayer.Business = businessById;
                                    dbPlayer.RefreshData(dbPlayer);
                                }
                                if (reader.GetInt32("Businessrank") != dbPlayer.Businessrank)
                                {
                                    dbPlayer.Businessrank = reader.GetInt32("Businessrank");
                                    dbPlayer.RefreshData(dbPlayer);
                                }
                                if (reader.GetInt32("Adminrank") != dbPlayer.Adminrank.Permission)
                                {
                                    dbPlayer.Adminrank = AdminrankModule.getAdminrank(reader.GetInt32("adminrank"));
                                    dbPlayer.RefreshData(dbPlayer);
                                }

                                if (reader.GetInt32("Money") != dbPlayer.Money)
                                {
                                    dbPlayer.Money = reader.GetInt32("Money");
                                    dbPlayer.RefreshData(dbPlayer);
                                }

                            }
                        }
                    }
                    finally
                    {
                        reader.Dispose();
                        mySqlReaderCon.Connection.Dispose();
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION OnMinuteSpent] " + ex.Message);
                Logger.Print("[EXCEPTION OnMinuteSpent] " + ex.StackTrace);
            }
        }
    }
}