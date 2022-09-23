using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GVMP
{
    class geschenkpointModule : GVMP.Module.Module<geschenkpointModule>
    {
        public static List<string> geschenkItems = new List<string>();
        public static List<int> alreadygeschenkped = new List<int>();
        public static List<geschenkpointModel> geschenkList = new List<geschenkpointModel>();

        protected override bool OnLoad()
        {
            geschenkItems.Add("Advancedrifle");
            geschenkItems.Add("Gusenberg");
            geschenkItems.Add("Assaultrifle");

            geschenkList.Add(new geschenkpointModel
            {
                Name = "geschenkpoint",
                Position = new Vector3(-429.25, 1110.19, 327.68)
            });


            foreach (geschenkpointModel geschenkpointModel in geschenkList)
            {
                ColShape cb = NAPI.ColShape.CreateCylinderColShape(geschenkpointModel.Position, 7.4f, 2.4f, 0);
                cb.SetData("FUNCTION_MODEL", new FunctionModel("usegeschenkpoint"));
                cb.SetData("MESSAGE", new Message("Benutze E um dich auszurüsten.", "Waffengeschenk", "white", 3000));
                NAPI.Blip.CreateBlip(781, geschenkpointModel.Position, 1f, 0, "Waffengeschenk ", 255, 0.0f, true, 0, 0);
            }

            return true;
        }

        [RemoteEvent("usegeschenkpoint")]
        public static void geschenkPlayer(Client c)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                try
                {
                    if (alreadygeschenkped.Contains(dbPlayer.Id))
                    {
                        dbPlayer.SendNotification("Du hast dein geschenk für die jetzige Wende bereits abgeholt.", 3000,
                            "blue", "geschenkpoint");
                        return;
                    }

                    alreadygeschenkped.Add(dbPlayer.Id);
                    dbPlayer.SendProgressbar(5000);
                    dbPlayer.disableAllPlayerActions(true);
                    dbPlayer.PlayAnimation(33, "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 8f);
                    NAPI.Task.Run(() =>
                    {
                        var r = new Random();
                        string item = geschenkItems[r.Next(0, geschenkItems.Count)];
                        dbPlayer.UpdateInventoryItems(item, 1, false);
                        dbPlayer.StopProgressbar();
                        dbPlayer.SendNotification(
                            "Du hast dein Waffengeschenk für die jetzige Wende abgeholt. (+ 1x " + item + ")", 3000, "blue", "geschenkpoint");
                        dbPlayer.StopAnimation();
                        dbPlayer.disableAllPlayerActions(false);
                    }, 5000);
                }
                catch (Exception ex)
                {
                    Logger.Print("[EXCEPTION usegeschenkpoint] " + ex.Message);
                    Logger.Print("[EXCEPTION usegeschenkpoint] " + ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION usegeschenkpoint] " + ex.Message);
                Logger.Print("[EXCEPTION usegeschenkpoint] " + ex.StackTrace);
            }
        }
    }
}
