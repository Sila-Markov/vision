using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVMP
{
    class ShopModule : GVMP.Module.Module<ShopModule>
    {
        public static List<Shop> shopList = new List<Shop>();

        protected override bool OnLoad()
        {
            MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM shops");
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
                            Shop shop = new Shop
                            {
                                Id = reader.GetInt32("Id"),
                                Blip = reader.GetInt32("Blip"),
                                BlipColor = reader.GetInt32("BlipColor"),
                                Position = NAPI.Util.FromJson<Vector3>(reader.GetString("Position")),
                                Title = reader.GetString("Title"),
                                Items = NAPI.Util.FromJson<List<BuyItem>>(reader.GetString("Items"))
                            };
                            shopList.Add(shop);
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
                Logger.Print("[EXCEPTION loadShops] " + ex.Message);
                Logger.Print("[EXCEPTION loadShops] " + ex.StackTrace);
            }
            finally
            {
                mySqlResult.Connection.Dispose();
            }

            foreach (Shop shop in shopList)
            {
                NAPI.Marker.CreateMarker(1, shop.Position, new Vector3(), new Vector3(), 1.0f, new Color(255, 165, 0), false, 0);

                ColShape val = NAPI.ColShape.CreateCylinderColShape(shop.Position, 1.4f, 1.4f, 0);
                val.SetData("FUNCTION_MODEL", new FunctionModel("openShop", NAPI.Util.ToJson(shop)));
                val.SetData("MESSAGE", new Message("Benutze E um den Shop zu öffnen.", shop.Title, "black", 3000));

            }

            return true;
        }

        [RemoteEvent("openShop")]
        public void openShop(Client c, string shopModel)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                if (shopModel == null)
                    return;

                c.TriggerEvent("openWindow", "Shop", shopModel);
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION openShop] " + ex.Message);
                Logger.Print("[EXCEPTION openShop] " + ex.StackTrace);
            }
        }

        [RemoteEvent("shopBuy")]
        public void shopBuy(Client c, string json)
        {
            try
            {
                if (c == null) return;
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                List<basketItems> basket = NAPI.Util.FromJson<basketShop>(json).basket;
                List<ItemModel> list = new List<ItemModel>();

                int num = 0;

                foreach (basketItems basketItems in basket)
                {
                    if (basketItems.price > 0)
                    {
                        Item item = ItemModule.itemRegisterList.FirstOrDefault((Item item) =>
                            item.Name == basketItems.itemId);
                        if (item == null) return;
                        ItemModel itemModel = new ItemModel
                        {
                            Amount = basketItems.count,
                            Id = item.Id,
                            Name = basketItems.itemId,
                            Slot = 0,
                            Weight = 0,
                            ImagePath = item.ImagePath
                        };

                        num += basketItems.price;
                        list.Add(itemModel);
                    }
                }

                if (dbPlayer.Money >= num)
                {
                    dbPlayer.removeMoney(Convert.ToInt32(num));
                    dbPlayer.SendNotification("Du hast einige Items gekauft.", "black", 3500, "SHOP");
                    WebhookSender.SendMessage("Spieler kauft was im Shop",
                        "Der Spieler " + dbPlayer.Name + " hat folgende Items gekauft: " + NAPI.Util.ToJson(basket),
                        Webhooks.shoplogs, "Shop");
                    foreach (ItemModel itemModel in list)
                    {
                        dbPlayer.UpdateInventoryItems(itemModel.Name, itemModel.Amount, false);
                    }

                }
                else
                {
                    dbPlayer.SendNotification("Du hast zu wenig Geld, um diese Items zu kaufen.", "black", 3500,
                        "SHOP");
                }
            }
            catch (Exception ex)
            {
                Logger.Print("[EXCEPTION shopBuy] " + ex.Message);
                Logger.Print("[EXCEPTION shopBuy] " + ex.StackTrace);
            }
        }
    }


}
