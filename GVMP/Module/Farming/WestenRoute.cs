using GTANetworkAPI;
using Crashkopf.Inventar;
using Crashkopf.Fraktion;
using Crashkopf.Player;
using Crashkopf.Animation;
using System;

namespace Vision.Routen
{
    class WestenRoute : Script
    {
        public static Vector3 westenKiste = new Vector3(1012.51, 1626.124, 177.4367);
        public static bool westeKistenSammeln(Client client, bool pressed)
        {
            try
            {
                if (client.Position.DistanceTo(westenKiste) <= 25)
                {
                    if (pressed)
                    {
                        if (client.GetSharedData("isFarming") == false || !client.HasSharedData("isFarming"))
                        {
                            int timer = 181;
                            AnimationDictionary.callAnimation(client, Convert.ToInt32(AnimationDictionary.animationType.leer));
                            AnimationDictionary.callAnimation(client, Convert.ToInt32(AnimationDictionary.animationType.snowball));
                            client.TriggerEvent("client:disableinv");
                            Main.freezePlayer(client, true);
                            client?.SetSharedData("isFarming", true);
                            FraktionHandler fraktion = PlayerHelper.getFraktion(client);
                            switch (fraktion.fraktion)
                            {
                                case "zivi":
                                    client?.TriggerEvent("client:startFarming", "Du hast eine Westenkiste eingesteckt.", 180);
                                    timer = 180;
                                    break;
                                default:
                                    client?.TriggerEvent("client:startFarming", "Du hast eine Westenkiste eingesteckt.", 181);
                                    timer = 181;
                                    break;
                            }
                            Item westenkiste = new Westenkiste();

                            NAPI.Task.Run(() =>
                            {
                                AnimationDictionary.callAnimation(client, Convert.ToInt32(AnimationDictionary.animationType.leer));
                                client.TriggerEvent("client:enableinv");
                                InventorySystem.AddItem(client, westenkiste.Item_Name, 1, westenkiste.maxItemQuantity);
                                client?.SetSharedData("isFarming", false);
                                Main.freezePlayer(client, false);
                            }, delayTime: timer * 1000);
                            client.TriggerEvent("client:disableinv");
                            return false;

                        }
                    }
                }
                else
                {
                    return false;
                }
                return false;
            }
            catch { return false; }
        }
    }
}


