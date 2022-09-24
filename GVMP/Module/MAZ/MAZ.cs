﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVMP;
using MySql.Data.MySqlClient;

namespace GVMP
{
    public class MAZ : GVMP.Module.Module<MAZ>
    {
        public static List<MAZ> clothingList = new List<MAZ>();
        public static List<MAZ> BlockedZones = new List<MAZ>();
        private static MAZ awd;

        public static object mySqlReaderCon { get; private set; }

        protected override bool OnLoad()
        {
            Vector3 Position = new Vector3(2055.69, 3433.75, 43.07);

            ColShape val = NAPI.ColShape.CreateCylinderColShape(Position, 1.4f, 2.4f, 0);
            val.SetData("FUNCTION_MODEL", new FunctionModel("openMAZ"));
            val.SetData("MESSAGE", new Message("Drücke E um MAZ aufzubrechen!", "", "red", 3000));

            NAPI.Marker.CreateMarker(1, Position, new Vector3(), new Vector3(), 1.0f, new Color(27, 0, 0), false, 0);

         ;
         ;

            return true;
        }

        [RemoteEvent("openMAZ")]
        public static void openMAZ(Client client)
        {
            try
            {
                if (client == null) return;
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;
                Laboratory result = null;
                float distance = 99999;




                if (!dbPlayer.IsFarming)
                {

                    if (BlockedZones.Contains(awd))
                    {
                        dbPlayer.SendNotification("Absturz wurde bereits gemacht.", "black", 3500, "Absturz");
                        return;
                    }

                    dbPlayer.disableAllPlayerActions(true);
                        dbPlayer.SendProgressbar(150000);
                        dbPlayer.IsFarming = true;
                        Notification.SendGlobalNotification("Zentrales Flugabwehrsystem: Das Flugzeug wird aufgebrochen (Sandy Shores Kreisel)!", 10000, "white", Notification.icon.bullhorn);
                        dbPlayer.RefreshData(dbPlayer);
                        dbPlayer.SendNotification("Du brichst das MAZ auf!", "black", 6000);
                    dbPlayer.PlayAnimation(33, "amb@world_human_welding@male@idle_a", "idle_a", 8f);
                        NAPI.Task.Run(delegate
                        {
                            dbPlayer.TriggerEvent("client:respawning");
                            dbPlayer.StopProgressbar();
                            dbPlayer.UpdateInventoryItems("Advancedrifle", 15, false);
                            dbPlayer.UpdateInventoryItems("Gusenberg", 15, false);
                            dbPlayer.IsFarming = false;
                            dbPlayer.RefreshData(dbPlayer);
                            dbPlayer.disableAllPlayerActions(false);
                            dbPlayer.StopAnimation();
                            BlockedZones.Add(awd);
                        }, 150000);
                    }
            }
            catch (Exception ex)
                    {
                        Logger.Print("[EXCEPTION openMAZ] " + ex.Message);
                        Logger.Print("[EXCEPTION openMAZ] " + ex.StackTrace);
            }
        }
    }
}
