using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using GTANetworkAPI;
using GVMP;
using GVMP.Module;
using MySql.Data.MySqlClient;

namespace GVMP
{
    class Wishreq : Script
    {
        [RemoteEvent("")]
        public void sendwishreq(Client dbPlayer, object nummernschild)
        {
            MySqlQuery mySqlQuery = new MySqlQuery("UPDATE vehicles SET Plate = @plate");
            mySqlQuery.AddParameter("@plate", nummernschild);
            MySqlHandler.ExecuteSync(mySqlQuery);
            dbPlayer.SendNotification("Kennzeichen geändert!");
        }
    }
}


/*[RemoteEvent("")]
public void sendMietvertrag(Client c, string text)
{
    DbPlayer dbPlayer = c.GetPlayer();
    if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
        return;

    if (text == dbPlayer.Name) return;

    House house = HouseModule.houses.FirstOrDefault((House house2) => house2.OwnerId == dbPlayer.Id);
    if (house == null) return;

    DbPlayer dbPlayer2 = PlayerHandler.GetPlayer(text);
    if (dbPlayer2 != null && dbPlayer.IsValid(true))
    {
        House house2 = HouseModule.houses.FirstOrDefault((House house2) => house2.OwnerId == dbPlayer2.Id || house2.TenantsIds.Contains(dbPlayer2.Id));
        WebhookSender.SendMessage("TEXTINPUTBOX", "" + dbPlayer.Name + " + " + text + " - Für Entwicklungs. - MIETVERTRAG", Webhooks.shoplogs, "Shop");
        if (house2 == null)
        {
            dbPlayer2.OpenConfirmation(new ConfirmationObject
            {
                Title = "Mietvertrag",
                Message = "Hiermit schließen Sie einen Mietvertrag mit dem Vermieter: " + dbPlayer.Name,
                Callback = "acceptMietvertrag",
                Arg1 = dbPlayer.Id.ToString(),
                Arg2 = house.Id.ToString()
            });
        }
        else
        {
            dbPlayer.SendNotification("Der Spieler besitzt bereits ein Haus!", 3000, "red");
        }
    }
    else
    {
        dbPlayer.SendNotification("Spieler nicht gefunden!");
    }
}*/