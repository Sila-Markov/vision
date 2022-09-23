using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVMP
{
    class SettingsApp : GVMP.Module.Module<SettingsApp>
    {
        List<Wallpaper> wallpapers = new List<Wallpaper>()
        {
            new Wallpaper(1, "Park", "https://i.imgur.com/Kw2VuCY.jpg"),
            new Wallpaper(2, "LCN", "https://i.imgur.com/FvsOEm2.png"),
            new Wallpaper(3, "LOST", "https://i.imgur.com/JbY482X.png"),
            new Wallpaper(4, "LSPD", "https://i.imgur.com/TgQwuzE.png"),
            new Wallpaper(5, "Marabunta", "https://i.imgur.com/belPu9t.png"),
            new Wallpaper(6, "Midnight", "https://i.imgur.com/JVV9wMS.png"),
            new Wallpaper(7, "Pier", "https://i.imgur.com/GQQ40BV.jpg"),
            new Wallpaper(8, "Triaden", "https://i.imgur.com/kMU9B90.png"),
            new Wallpaper(9, "Vagos", "https://i.imgur.com/TYZgwX7.png"),
            new Wallpaper(10, "YakuZa", "https://i.imgur.com/5hoqvjH.png"),
            new Wallpaper(11, "Feuerlord", "https://i.ibb.co/g6Vfh3w/milakunis.gif"),
            new Wallpaper(12, "", "https://i.ibb.co/SJmX6tR/tenor2.gif"),
            new Wallpaper(13, "Bubblebutt", "https://i.ibb.co/8PrTgHf/bubblebutt.gif"),
            new Wallpaper(14, "Ass", "https://cdn.discordapp.com/attachments/926071343324229632/966461190014382110/unknown.png"),
            new Wallpaper(15, "<3", "https://cdn.discordapp.com/attachments/842332172920946688/996148529489772655/trim_8B81EC92-B18E-4303-A82F-E95B8996BBCA_AdobeExpress.gif"),
            new Wallpaper(16, "Sexy", "https://cdn.discordapp.com/attachments/842332172920946688/996146005953233067/1648369913_1-xphoto-name-p-calvin-klein-porn-1.jpg"),
            new Wallpaper(17, "agr#0002", "https://media.discordapp.net/attachments/966714961252470794/984813643910754364/agrmeme.gif"),
            new Wallpaper(18, "agrFifa", "https://cdn.discordapp.com/attachments/842332172920946688/996150880363630673/unknown.png")
        };

        [RemoteEvent("requestWallpaperList")]
        public void requestWallpaperList(Client c)
        {
            try
            {
                if (c == null) return;
                c.TriggerEvent("componentServerEvent", "SettingsEditWallpaperApp", "responseWallpaperList",
                    NAPI.Util.ToJson(wallpapers));
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION requestWallpaperList] " + ex.Message);
                Logger.Print("[EXCEPTION requestWallpaperList] " + ex.StackTrace);
            }
        }

        public static void checkUserSettingsTable(Client c)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM phone_settings WHERE Id = @userid LIMIT 1");
                mySqlQuery.Parameters = new List<MySqlParameter>()
                {
                    new MySqlParameter("@userid", dbPlayer.Id)
                };
                MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
                try
                {
                    MySqlDataReader reader = mySqlReaderCon.Reader;
                    if (!reader.HasRows)
                    {
                        reader.Dispose();
                        mySqlQuery.Query = "INSERT INTO phone_settings (Id) VALUES (@userid)";
                        mySqlQuery.Parameters = new List<MySqlParameter>()
                        {
                            new MySqlParameter("@userid", dbPlayer.Id)
                        };
                        MySqlHandler.ExecuteSync(mySqlQuery);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION checkUserSettingsTable] " + ex.Message);
                    Logger.Print("[EXCEPTION checkUserSettingsTable] " + ex.StackTrace);
                }
                finally
                {
                    mySqlReaderCon.Connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION checkUserSettingsTable] " + ex.Message);
                Logger.Print("[EXCEPTION checkUserSettingsTable] " + ex.StackTrace);
            }
        }

        public static bool isFlugmodus(Client c)
        {
            if (c == null) return false;
            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return false;

            checkUserSettingsTable(c);
            MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM phone_settings WHERE Id = @userid LIMIT 1");
            mySqlQuery.Parameters = new List<MySqlParameter>()
            {
                new MySqlParameter("@userid", dbPlayer.Id)
            };
            MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
            try
            {
                mySqlQuery.Query = "SELECT * FROM phone_settings WHERE Id = @userid LIMIT 1";
                mySqlQuery.Parameters = new List<MySqlParameter>()
                {
                    new MySqlParameter("@userid", dbPlayer.Id)
                };
                MySqlHandler.ExecuteSync(mySqlQuery);
                MySqlDataReader reader = mySqlReaderCon.Reader;
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            return reader.GetInt32("Flugmodus") == 1;
                        }
                    }
                }
                finally
                {
                    mySqlReaderCon.Reader.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION isFlugmodus] " + ex.Message);
                Logger.Print("[EXCEPTION isFlugmodus] " + ex.StackTrace);
            }
            finally
            {
                mySqlReaderCon.Connection.Dispose();
            }
            return false;
        }

        [RemoteEvent("requestPhoneWallpaper")]
        public void requestPhoneWallpaper(Client c)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                checkUserSettingsTable(c);
                MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM phone_settings WHERE Id = @userid LIMIT 1");
                mySqlQuery.AddParameter("@userid", dbPlayer.Id);
                MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
                try
                {
                    MySqlDataReader reader = mySqlReaderCon.Reader;
                    try
                    {
                        if (!reader.HasRows)
                        {
                            mySqlQuery.Parameters.Clear();
                            mySqlQuery.Query = "INSERT INTO phone_settings (Id) VALUES (@userid)";
                            mySqlQuery.AddParameter("@userid", dbPlayer.Id);
                            MySqlHandler.ExecuteSync(mySqlQuery);
                        }
                        else
                        {
                            reader.Read();
                            Wallpaper wallpaper =
                                wallpapers.Find((Wallpaper wall) => wall.Id == reader.GetInt32("Wallpaper"));

                            if (wallpaper != null)
                                c.TriggerEvent("componentServerEvent", "HomeApp", "responsePhoneWallpaper",
                                    wallpaper.File);
                        }
                    }
                    finally
                    {
                        reader.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION requestPhoneWallpaper] " + ex.Message);
                    Logger.Print("[EXCEPTION requestPhoneWallpaper] " + ex.StackTrace);
                }
                finally
                {
                    mySqlReaderCon.Connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION requestPhoneWallpaper] " + ex.Message);
                Logger.Print("[EXCEPTION requestPhoneWallpaper] " + ex.StackTrace);
            }

        }

        [RemoteEvent("saveWallpaper")]
        public void saveWallpaper(Client c, int id)
        {
            if (c == null) return;
            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return;

            checkUserSettingsTable(c);
            try
            {
                MySqlQuery mySqlQuery = new MySqlQuery("UPDATE phone_settings SET Wallpaper = @val WHERE Id = @userid");
                mySqlQuery.AddParameter("@userid", dbPlayer.Id);
                mySqlQuery.AddParameter("@val", id);
                MySqlHandler.ExecuteSync(mySqlQuery);
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION saveWallpaper] " + ex.Message);
                Logger.Print("[EXCEPTION saveWallpaper] " + ex.StackTrace);
            }
        }

        [RemoteEvent("requestPhoneSettings")]
        public void requestPhoneSettings(Client c)
        {
            if (c == null) return;
            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return;

            MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM phone_settings WHERE Id = @userid LIMIT 1");
            mySqlQuery.AddParameter("@userid", dbPlayer.Id);
            MySqlResult mySqlReaderCon = MySqlHandler.GetQuery(mySqlQuery);
            try
            {
                MySqlDataReader reader = mySqlReaderCon.Reader;
                try
                {
                    if (!reader.HasRows)
                    {
                        mySqlQuery.Query = "INSERT INTO phone_settings (Id) VALUES (@userid)";
                        mySqlQuery.Parameters = new List<MySqlParameter>()
                        {
                            new MySqlParameter("@userid", dbPlayer.Id)
                        };
                        MySqlHandler.ExecuteSync(mySqlQuery);
                    }
                    else
                    {
                        reader.Read();
                        string boolstring = "true";
                        if (reader.GetInt32("Flugmodus") == 0)
                            boolstring = "false";

                        c.TriggerEvent("componentServerEvent", "SettingsApp", "responsePhoneSettings", boolstring, "false", "false");
                    }
                }
                finally
                {
                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION requestPhoneSettings] " + ex.Message);
                Logger.Print("[EXCEPTION requestPhoneSettings] " + ex.StackTrace);
            }
            finally
            {
                mySqlReaderCon.Connection.Dispose();
            }
        }

        [RemoteEvent("savePhoneSettings")]
        public void savePhoneSettings(Client c, bool flugmodusStatus, bool lautlosStatus, bool anrufAblehnen)
        {
            if (c == null) return;
            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return;

            checkUserSettingsTable(c);
            try
            {
                MySqlQuery mySqlQuery = new MySqlQuery("UPDATE phone_settings SET Flugmodus = @val WHERE Id = @userid");
                mySqlQuery.Parameters = new List<MySqlParameter>()
                {
                    new MySqlParameter("@userid", dbPlayer.Id),
                    new MySqlParameter("@val", Convert.ToInt32(flugmodusStatus))
                };
                MySqlHandler.ExecuteSync(mySqlQuery);
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION savePhoneSettings] " + ex.Message);
                Logger.Print("[EXCEPTION savePhoneSettings] " + ex.StackTrace);
            }
        }
    }
}
