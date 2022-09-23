using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace GVMP
{
    class AufsatzModule : GVMP.Module.Module<AufsatzModule>
    {
        public static List<Aufsatz> fest = new List<Aufsatz>();
        public static Vector3 Position = new Vector3(142.6, -750.23, 258.15);

        protected override bool OnLoad()
        {
            MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM test");
            MySqlResult mySqlResult = MySqlHandler.GetQuery(mySqlQuery);
            try
            {
                MySqlDataReader reader = mySqlResult.Reader;
                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            loadTest(reader);
                        }
                    }
                }
                finally
                {
                    reader.Dispose(); 
                }
            }
            catch (Exception ex)
            {
            Logger.Print("[EXCEPTION loadTests] " + ex.Message);
            Logger.Print("[EXCEPTION loadTests] " + ex.StackTrace);
            }
            finally
            {
                mySqlResult.Connection.Dispose();
            }

            return true;
        }


        public static void loadTest(MySqlDataReader reader)
        {
            try
            {

                Aufsatz aufsatz = new Aufsatz 
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Position = NAPI.Util.FromJson<Vector3>(reader.GetString("Position")),
                };

                fest.Add(aufsatz);
                ColShape val = NAPI.ColShape.CreateCylinderColShape(aufsatz.Position, 1.4f, 1.4f, 0);
                val.SetData("FUNCTION_MODEL", new FunctionModel("openWeaponCom", aufsatz.Name));
                val.SetData("MESSAGE", new Message("Benutze E um TEST zu öffnen.", aufsatz.Name, "orange", 5000));

                NAPI.Marker.CreateMarker(36, aufsatz.Position, new Vector3(), new Vector3 (), 1.0f, new Color(9, 114, 193), false, 0);
                NAPI.Blip.CreateBlip(549, aufsatz.Position, 1f, 0, aufsatz.Name, 255, 0.0f, true, 0, 0);
            }
            catch (Exception ex)
            {
            Logger.Print("[EXCEPTION opentest] " + ex.Message);
            Logger.Print("[EXCEPTION opentest] " + ex.StackTrace);
            }
        }
        [RemoteEvent("openWeaponCom")]

        public void openWeaponCom(Client c)
        {

        }

    }
}