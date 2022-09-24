using GTANetworkAPI;
using GVMP.Handlers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GVMP
{
    class PlayerLogin : Script
    {

        [ServerEvent(Event.PlayerConnected)]
        public void onPlayerConnect(Client c)
        {
            try
            {
                NAPI.Player.SpawnPlayer(c, new Vector3(-438.53, 1076.15, 352.4 + 168.03f));
                c.Position = new Vector3(-438.53, 1076.15, 352.4);
                c.Transparency = 255;
                c.Dimension = Convert.ToUInt32(new Random().Next(10000, 99999));

                c.TriggerEvent("freezePlayer", true);
                c.TriggerEvent("OnPlayerReady");

                c.SetSharedData("PLAYER_INVISIBLE", false);

                /* TextInputBoxObject textInputBoxObject = new TextInputBoxObject
                 {
                     Title = "Anmeldeformular",
                     Message =
                         "Gebe bitte deinen Benutzernamen ein (Beispiel: Max_Mustermann). Falls du noch nicht registriert bist, wirst du automatisch registriert.",
                     Callback = "registerUser",
                     CloseCallback = "showLoginInput"
                 };*/

                MySqlQuery query = new MySqlQuery("SELECT * FROM accounts WHERE Social = @user LIMIT 1");
                query.AddParameter("@user", c.SocialClubName);

                MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(query);
                MySqlDataReader reader = mySqlReaderCon.Reader;
                try
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        //registerUser(c, reader.GetString("Username"));
                        c.Name = reader.GetString("Username");
                        c.TriggerEvent("login:open", 0);
                        return;
                    }
                }
                catch { }
                finally
                {
                    mySqlReaderCon.Connection.Dispose();
                }


                //  c.OpenTextInputBox(textInputBoxObject);
                c.TriggerEvent("login:open", 1);
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION PlayerConnected] " + ex.Message);
                Logger.Print("[EXCEPTION PlayerConnected] " + ex.StackTrace);
            }
        }

        [RemoteEvent("login:register")]
        public static void loginregister(Client c, string username, string password)
        {
            bool flag = false;
            foreach (char c2 in username)
            {
                if (c2 == '_')
                {
                    flag = true;
                }
            }

            if (username.Length > 16)
                flag = false;

            if (username != username.RemoveSpecialCharacters())
                flag = false;

            if (!flag) return;



            c.Name = username;

            MySqlQuery query4 = new MySqlQuery("SELECT * FROM accounts WHERE Social = @social LIMIT 1");
            query4.AddParameter("@social", c.SocialClubName);
            MySqlResult mySqlReaderCon4 = MySqlHandler.GetQuery(query4);

            if (!mySqlReaderCon4.Reader.HasRows)
            {
                MySqlQuery query5 = new MySqlQuery("SELECT MAX(id) as maxId FROM accounts");
                MySqlResult mySqlReaderCon5 = MySqlHandler.GetQuery(query5);
                if (!mySqlReaderCon5.Reader.HasRows) return;
                mySqlReaderCon5.Reader.Read();
                int playerid = mySqlReaderCon5.Reader.GetInt32("maxId") + 1;

                MySqlQuery query2 =
                    new MySqlQuery(
                        "INSERT INTO `accounts` (`Id`, `Username`, `Password`, `Money`, `Social`, `Location`) VALUES (@id, @username, @password, @money, @social, @loc)");
                query2.AddParameter("@id", playerid);
                query2.AddParameter("@username", c.Name);
                query2.AddParameter("@password", password);
                query2.AddParameter("@money", 10000000);
                query2.AddParameter("@social", c.SocialClubName);
                query2.AddParameter("@loc",
                    NAPI.Util.ToJson(RandomSpawns.spawns[new Random().Next(RandomSpawns.spawns.Count)]));
                MySqlHandler.ExecuteSync(query2);

                c.TriggerEvent("login:open", 0);
            }


        }

        [RemoteEvent("login:login")]
        public static async void loginlogin(Client c, string password)
        {
            if (c == null) return;
            try
            {
                MySqlQuery query = new MySqlQuery("SELECT * FROM accounts WHERE Username = @user LIMIT 1");
                query.AddParameter("@user", c.Name);

                MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(query);
                MySqlDataReader reader = mySqlReaderCon.Reader;
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (password != reader.GetString("password")) //reader.GetString("Password") != password)
                            {
                                c.LoginStatus("Passwort falsch.");
                                return;
                            }

                            if (BanModule.isIdentifierBanned(reader.GetString("Username")))
                            {
                                c.BanKickPlayer("Banned");
                                return;
                            }

                            if (c.SocialClubName != reader.GetString("Social"))
                            {
                                c.LoginStatus("Dein SocialClub Konto stimmt nicht mit diesem Ingame Konto überrein! SocialClubName des Kontos: " + reader.GetString("Social") + "");
                                return;
                            }
                            DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(c.Name);
                            if (dbPlayer2 != null)
                            {

                                c.LoginStatus("Der Spieler ist bereits online.");
                                c.Kick();
                                return;
                            }

                            int id = reader.GetInt32("Id");
                            Adminrank adminrank = AdminrankModule.adminrankList.FirstOrDefault((Adminrank f) =>
                                f.Permission == reader.GetInt32("Adminrank"));
                            if (adminrank == null)
                            {
                                adminrank = new Adminrank()
                                {
                                    Permission = 0,
                                    ClothingId = 0,
                                    RGB = new Color(0, 0, 0),
                                    Name = "User"
                                };
                            }

                            int money = reader.GetInt32("Money");
                            int fraktionsrank = reader.GetInt32("Fraktionrank");
                            int int4 = reader.GetInt32("Businessrank");

                            c.ResetData("player");

                            Faction fraktion = FactionModule.getFactionById(0);

                            if (reader.GetInt32("Fraktion") != 0)
                                fraktion = FactionModule.getFactionById(reader.GetInt32("Fraktion"));

                            Business businessById = BusinessModule.getBusinessById(0);
                            if (reader.GetInt32("Business") != 0)
                            {
                                businessById = BusinessModule.getBusinessById(reader.GetInt32("Business"));
                            }

                            DbPlayer dbPlayer = new DbPlayer
                            {
                                Client = c,
                                Name = c.Name,
                                Id = id,
                                Password = reader.GetString("Password"),
                                Money = money,
                                AccountStatus = AccountStatus.LoggedIn,
                                Faction = fraktion,
                                /* SpielerFraktion = Convert.ToBoolean(reader.GetInt32("Fraktion")),*/
                                Business = businessById,
                                Businessrank = int4,
                                Factionrank = fraktionsrank,
                                VoiceHash = Guid.NewGuid().ToString(),
                                ForumId = 0,
                                IsCuffed = false,
                                IsFarming = false,
                                PlayerClothes = new PlayerClothes(),
                                Adminrank = adminrank,

                                LastEInteract = DateTime.Now,
                                LastInteracted = DateTime.Now,
                                OnlineSince = DateTime.Now,
                                Level = reader.GetInt32("Level"),
                                DeathData = new DeathData
                                {
                                    IsDead = false,
                                    DeathTime = new DateTime(0)
                                },
                                Loadout = new List<NXWeapon>(),
                                __ActionsDisabled = false,
                                Medic = Convert.ToBoolean(reader.GetInt32("Medic")),
                                Event = Convert.ToBoolean(reader.GetInt32("Event"))
                            };

                            c.LoginStatus("successfully");
                            c.TriggerEvent("client:respawning");

                            c.TriggerEvent("freezePlayer", false);

                            House house = HouseModule.houses.FirstOrDefault((House house2) =>
                                house2.OwnerId == dbPlayer.Id || house2.TenantsIds.Contains(dbPlayer.Id));
                            c.TriggerEvent("Hud:SetData", dbPlayer.Money, dbPlayer.Id, NAPI.Pools.GetAllPlayers().Count, dbPlayer.Name, dbPlayer.Adminrank.Name);
                            c.TriggerEvent(
                                "onPlayerLoaded",
                                c.Name.Split("_")[0],
                                c.Name.Split("_")[1],
                                reader.GetInt32("Id"),
                                0,
                                0,
                                0,
                                money,
                                0,
                                house == null ? 0 : house.Id,
                                reader.GetInt32("Fraktion"),
                                0,
                                reader.GetInt32("Level"),
                                reader.GetInt32("Death"),
                                false,
                                false,
                                false,
                                false,
                                false,
                                1,
                                FactionModule.getFactionById(reader.GetInt32("Fraktion")).Name,
                                0,
                                NAPI.Util.ToJson(Animations.animations),
                                reader.GetInt32("Adminrank"),
                                0.60f,
                                0.5f,
                                0,
                                0,
                                0
                            );

                            c.SetSharedData("CLIENT_RANGE", 2);
                            c.TriggerEvent("setVoiceType", 2.ToString());

                            c.SetSharedData("IN_CALL", "none");

                            c.SetSharedData("FUNK_TALKING", false);
                            c.SetSharedData("FUNK_CHANNEL", 0);
                            c.TriggerEvent("ConnectTeamspeak");

                            PlayerClothes playerClothes = new PlayerClothes();

                            playerClothes.Hut = new clothingPart
                            {
                                drawable = -1,
                                texture = 0
                            };
                            playerClothes.Brille = new clothingPart
                            {
                                drawable = 0,
                                texture = 0
                            };
                            playerClothes.Haare = new clothingPart
                            {
                                drawable = 0,
                                texture = 0
                            };
                            playerClothes.Maske = new clothingPart
                            {
                                drawable = 0,
                                texture = 0
                            };
                            playerClothes.Oberteil = new clothingPart
                            {
                                drawable = 1,
                                texture = 0
                            };
                            playerClothes.Unterteil = new clothingPart
                            {
                                drawable = 15,
                                texture = 0
                            };
                            playerClothes.Kette = new clothingPart
                            {
                                drawable = 14,
                                texture = 0
                            };
                            /* playerClothes.Kette = new clothingPart
                             {
                                 drawable = 15,
                                 texture = 0
                             };*/
                            playerClothes.Koerper = new clothingPart
                            {
                                drawable = 0,
                                texture = 0
                            };
                            playerClothes.Hose = new clothingPart
                            {
                                drawable = 5,
                                texture = 0
                            };
                            playerClothes.Schuhe = new clothingPart
                            {
                                drawable = 1,
                                texture = 0
                            };

                            dbPlayer.PlayerClothes = playerClothes;

                            c.SetData("player", dbPlayer);

                            dbPlayer.SetData("PBZone", null);
                            dbPlayer.SetData("PBKills", 0);
                            dbPlayer.SetData("PBDeaths", 0);
                            c.TriggerEvent("startdatime");

                            dbPlayer.SetData("IN_LABOR", false);

                            dbPlayer.SetData("MASK", true);

                            if (reader.GetString("Customization") != "")
                            {


                                CustomizeModel customizeModel =
                                    NAPI.Util.FromJson<CustomizeModel>(reader.GetString("Customization"));
                                Dictionary<int, HeadOverlay> dictionary = new Dictionary<int, HeadOverlay>();
                                if (customizeModel.customization.Appearance[9] != null)
                                    dictionary.Add(1,
                                        ClothingManager.CreateHeadOverlay(
                                            (byte)customizeModel.customization.Appearance[1].Value,
                                            (byte)customizeModel.customization.Appearance[9].Value, 0,
                                            customizeModel.customization.Appearance[1].Opacity));
                                dictionary.Add(2,
                                    ClothingManager.CreateHeadOverlay(2,
                                        (byte)customizeModel.customization.Appearance[2].Value, 0,
                                        customizeModel.customization.Appearance[2].Opacity));
                                dictionary.Add(3,
                                    ClothingManager.CreateHeadOverlay(3,
                                        (byte)customizeModel.customization.Appearance[3].Value, 0,
                                        customizeModel.customization.Appearance[3].Opacity));
                                dictionary.Add(4,
                                    ClothingManager.CreateHeadOverlay(4,
                                        (byte)customizeModel.customization.Appearance[4].Value, 0,
                                        customizeModel.customization.Appearance[4].Opacity));
                                dictionary.Add(5,
                                    ClothingManager.CreateHeadOverlay(5,
                                        (byte)customizeModel.customization.Appearance[5].Value, 0,
                                        customizeModel.customization.Appearance[5].Opacity));
                                dictionary.Add(8,
                                    ClothingManager.CreateHeadOverlay(8,
                                        (byte)customizeModel.customization.Appearance[8].Value, 0,
                                        customizeModel.customization.Appearance[8].Opacity));
                                HeadBlend val = default(HeadBlend);
                                val.ShapeFirst = (byte)customizeModel.customization.Parents.MotherShape;
                                val.ShapeSecond = (byte)customizeModel.customization.Parents.FatherShape;
                                val.ShapeThird = 0;
                                val.SkinFirst = (byte)customizeModel.customization.Parents.MotherSkin;
                                val.SkinSecond = (byte)customizeModel.customization.Parents.FatherSkin;
                                val.SkinThird = 0;
                                val.ShapeMix = customizeModel.customization.Parents.Similarity;
                                val.SkinMix = customizeModel.customization.Parents.SkinSimilarity;
                                val.ThirdMix = 0f;
                                HeadBlend val2 = val;
                                bool flag = customizeModel.customization.Gender == 0;

                                List<Decoration> list = new List<Decoration>();
                                foreach (uint tattoo in customizeModel.customization.Tattoos)
                                {
                                    if (AssetsTattooModule.AssetsTattoos.ContainsKey(tattoo))
                                    {
                                        AssetsTattoo assetsTattoo = AssetsTattooModule.AssetsTattoos[tattoo];
                                        Decoration item = default(Decoration);
                                        item.Collection = NAPI.Util.GetHashKey(assetsTattoo.Collection);
                                        item.Overlay = flag ? NAPI.Util.GetHashKey(assetsTattoo.HashMale) : NAPI.Util.GetHashKey(assetsTattoo.HashFemale);
                                        list.Add(item);
                                    }
                                }
                                c.SetCustomization(flag, val2, (byte)customizeModel.customization.EyeColor,
                                    (byte)customizeModel.customization.Hair.Color,
                                    (byte)customizeModel.customization.Hair.HighlightColor,
                                    customizeModel.customization.Features.ToArray(), dictionary,
                                    list.ToArray());

                                dbPlayer.SetClothes(2, customizeModel.customization.Hair.Hair, 0);
                                dbPlayer.SetPosition(NAPI.Util.FromJson<Vector3>(reader.GetString("Location"))
                                    .Add(new Vector3(0.0f, 0.0f, 1.0)));

                                PlayerClothes playerClothes2 =
                                    NAPI.Util.FromJson<PlayerClothes>(reader.GetString("Clothes"));
                                if (playerClothes2.Hut != null)
                                    c.SetAccessories(0, playerClothes2.Hut.drawable, playerClothes2.Hut.texture);
                                if (playerClothes2.Brille != null)
                                    c.SetAccessories(1, playerClothes2.Brille.drawable, playerClothes2.Brille.texture);
                                if (playerClothes2.Maske != null)
                                    dbPlayer.SetClothes(1, playerClothes2.Maske.drawable, playerClothes2.Maske.texture);
                                if (playerClothes2.Oberteil != null)
                                    dbPlayer.SetClothes(11, playerClothes2.Oberteil.drawable, playerClothes2.Oberteil.texture);
                                if (playerClothes2.Unterteil != null)
                                    dbPlayer.SetClothes(8, playerClothes2.Unterteil.drawable, playerClothes2.Unterteil.texture);
                                if (playerClothes2.Kette != null)
                                    dbPlayer.SetClothes(7, playerClothes2.Kette.drawable, playerClothes2.Kette.texture);
                                if (playerClothes2.Koerper != null)
                                    dbPlayer.SetClothes(3, playerClothes2.Koerper.drawable, playerClothes2.Koerper.texture);
                                if (playerClothes2.Hose != null)
                                    dbPlayer.SetClothes(4, playerClothes2.Hose.drawable, playerClothes2.Hose.texture);
                                if (playerClothes2.Schuhe != null)
                                    dbPlayer.SetClothes(6, playerClothes2.Schuhe.drawable, playerClothes2.Schuhe.texture);
                                dbPlayer.PlayerClothes = playerClothes2;

                                dbPlayer.RefreshData(dbPlayer);
                                WeaponManager.loadWeapons(c);

                                await HouseKeyHandler.Instance.LoadHouseKeys(dbPlayer);
                                await VehicleKeyHandler.Instance.LoadPlayerVehicleKeys(dbPlayer);

                                dbPlayer.SetDimension(0);

                                if (house != null)
                                    dbPlayer.TriggerEvent("createClientBlip", 40, house.Entrance, "Haus " + house.Id, 0,
                                        false, 0, 1);

                                NAPI.Task.Run(() =>
                                {
                                    if (dbPlayer.GetAttributeInt("Death") == 1)
                                        dbPlayer.SetHealth(0);
                                }, 3500);
                            }
                            else
                            {
                                NAPI.Task.Run(delegate
                                {
                                    string text =
                                        "{\"customization\":{\"Gender\":0,\"Parents\":{\"FatherShape\":0,\"MotherShape\":0,\"FatherSkin\":0,\"MotherSkin\":0,\"Similarity\":1,\"SkinSimilarity\":1},\"Features\":[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],\"Hair\":{\"Hair\":0,\"Color\":0,\"HighlightColor\":0},\"Appearance\":[{\"Value\":255,\"Opacity\":1},{\"Value\":255,\"Opacity\":1},{\"Value\":1,\"Opacity\":1},{\"Value\":5,\"Opacity\":0.4},{\"Value\":0,\"Opacity\":0},{\"Value\":0,\"Opacity\":0},{\"Value\":255,\"Opacity\":1},{\"Value\":255,\"Opacity\":1},{\"Value\":0,\"Opacity\":0},{\"Value\":255,\"Opacity\":1},{\"Value\":255,\"Opacity\":1}],\"EyebrowColor\":0,\"BeardColor\":0,\"EyeColor\":0,\"BlushColor\":0,\"LipstickColor\":0,\"ChestHairColor\":0},\"level\":0}";
                                    c.TriggerEvent("openWindow", "CharacterCreator", text);
                                    c.Position = new Vector3(402.8664, -996.4108, -99.00027);
                                    c.TriggerEvent("client:respawning");
                                    c.Eval("mp.players.local.setHeading(-185);");
                                    c.Dimension = Convert.ToUInt32(new Random().Next(10000, 99999));
                                }, 1000L);
                            }

                            if (adminrank.Permission > 0)
                            {
                                dbPlayer.SendNotification(
                                    "Ihre Rechte wurden vollständig initialisiert. (" + adminrank.Name + ")", "red", 3000, "ADMIN");

                                if (adminrank.Permission > 98)
                                {
                                    dbPlayer.TriggerEvent("createClientBlip", 73, new Vector3(448.83, -981.55, 43.69),
                                        "Donator-Kleidungsladen", 26, false, 0, 1);
                                }
                            }

                            if (adminrank.Permission < 1 && Configuration.whitelist)
                            {
                                dbPlayer.Client.SendNotification("Die Whitelist ist aktuell aktiviert!");
                                dbPlayer.Client.Kick();
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
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION PlayerLogin] " + ex.Message);
                Logger.Print("[EXCEPTION PlayerLogin] " + ex.StackTrace);
            }
        }

        [RemoteEvent("kick")]
        public void kick(Client c)
        {
            try
            {
                c.Kick();
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION kick] " + ex.Message);
                Logger.Print("[EXCEPTION kick] " + ex.StackTrace);
            }
        }
    }
}

/*
 * (c) Sila.
 */