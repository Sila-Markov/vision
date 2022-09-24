using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*namespace GVMP
/*{
    class MK2Upgradermodule : GVMP.Module.Module<MK2Upgradermodule>
    {
        public static List<MK2Upgrader> weaponMK2Upgraders = new List<MK2Upgrader>();

        protected override bool OnLoad()
        {
            weaponMK2Upgraders.Add(new MK2Upgrader
            {
               /* Id = 124,
                WeaponName = "SpeziMK2",
                Weapon = WeaponHash.SpecialCarbineMK2,
                Position = new Vector3(1094.31, -2003.37, 29.97),
                Price = 1000000,
                RemoveCount = 30
            });

            weaponMK2Upgraders.Add(new MK2Upgrader
            {
                Id = 126,
                WeaponName = "KarabinerMK2",
                Weapon = WeaponHash.AssaultRifle,
                Position = new Vector3(1103.55, -2000.89, 28.65),
                Price = 750000,
                RemoveCount = 25
            });

            weaponMK2Upgraders.Add(new MK2Upgrader
            {
                Id = 125,
                WeaponName = "BullpuppMK2",
                Weapon = WeaponHash.Gusenberg,
                Position = new Vector3(1108.45, -2007.66, 29.9),
                Price = 1000000,
                RemoveCount = 30
            });

            foreach (MK2Upgrader weaponMK2Upgrader in weaponMK2Upgraders)
            {
                ColShape c = NAPI.ColShape.CreateCylinderColShape(weaponMK2Upgrader.Position, 20.0f, 2.4f, 0);
                c.SetData("FUNCTION_MODEL", new FunctionModel("openMK2Upgradering", weaponMK2Upgrader.Id));
                c.SetData("MESSAGE", new Message("Benutze E um Waffen herzustellen.", "Waffenherstellung", "green", 3000));

                NAPI.Blip.CreateBlip(156, weaponMK2Upgrader.Position, 1f, 0, "Waffenherstellung " + weaponMK2Upgrader.WeaponName, 255, 0, true, 0, 0);
            }

            return true;
        }

        [RemoteEvent("openMK2Upgrader")]
        public void openManufacturing(Client c, int Id)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                MK2Upgrader weaponMK2Upgrader =
                    weaponMK2Upgraders.FirstOrDefault((MK2Upgrader weaponMK2Upgrader2) => weaponMK2Upgrader2.Id == Id);
                if (weaponMK2Upgrader == null) return;

                List<NativeItem> nativeItems = new List<NativeItem>();
                nativeItems.Add(new NativeItem(
                    weaponMK2Upgrader.WeaponName + " - " + weaponMK2Upgrader.Price.ToDots() + "$ - " +
                    weaponMK2Upgrader.RemoveCount + " Waffenteile", Id.ToString()));
                NativeMenu nativeMenu = new NativeMenu("Waffenherstellung", "", nativeItems);
                dbPlayer.ShowNativeMenu(nativeMenu);
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION openMK2Upgradering] " + ex.Message);
                Logger.Print("[EXCEPTION openMK2Upgradering] " + ex.StackTrace);
            }
        }

        [RemoteEvent("nM-Waffenherstellung")]
        public void Waffenherstellung(Client c, string selection)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                int Id = 0;
                bool Id2 = int.TryParse(selection, out Id);
                if (!Id2) return;
                if (Id == 0) return;

                MK2Upgrader weaponMK2Upgrader = weaponMK2Upgraders.FirstOrDefault((MK2Upgrader weaponMK2Upgrader2) => weaponMK2Upgrader2.Id == Id);
                if (weaponMK2Upgrader == null) return;

                if (dbPlayer.GetItemAmount("Waffenteile") < weaponMK2Upgrader.RemoveCount)
                {
                    dbPlayer.SendNotification("Du besitzt zu wenig Waffenteile! Benötigt: " + weaponMK2Upgrader.RemoveCount, 3000, "red");
                    return;
                }

                if (dbPlayer.Money < weaponMK2Upgrader.Price)
                {
                    dbPlayer.SendNotification("Du besitzt zu wenig Geld! Benötigt: " + weaponMK2Upgrader.Price, 3000, "red");
                    return;
                }

                dbPlayer.CloseNativeMenu();

                if (!dbPlayer.IsFarming)
                {
                    dbPlayer.removeMoney(weaponMK2Upgrader.Price);
                    dbPlayer.UpdateInventoryItems("Waffenteile", weaponMK2Upgrader.RemoveCount, true);
                    dbPlayer.disableAllPlayerActions(true);
                    dbPlayer.SendProgressbar(30000);
                    dbPlayer.IsFarming = true;
                    dbPlayer.RefreshData(dbPlayer);
                    dbPlayer.PlayAnimation(33, "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 8f);
                    NAPI.Task.Run(delegate
                    {
                        if (c.Dimension != 0)
                        {
                            dbPlayer.SendNotification("Du kannst gerade keine Waffen herstellen.", 3000, "red");
                            return;
                        }
                        else
                        {
                            dbPlayer.GiveWeapon(weaponMK2Upgrader.Weapon, 9999, true);
                            dbPlayer.IsFarming = false;
                            dbPlayer.RefreshData(dbPlayer);
                            dbPlayer.StopAnimation();
                            dbPlayer.StopProgressbar();
                            dbPlayer.disableAllPlayerActions(false);
                        }
                    }, 30000);
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION Waffenherstellung] " + ex.Message);
                Logger.Print("[EXCEPTION Waffenherstellung] " + ex.StackTrace);
            }
        }
    }
}*/