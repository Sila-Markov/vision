using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVMP
{
    class FraktionAPP : Script
    {

        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            Logger.Print("Fraktionsapp geladen.");
        }

        [RemoteEvent("phone:requestteamappapp")]
        public void RequestFraktionAPP(Client p)
        {
            try
            {
                DbPlayer dbPlayer = p.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                List<FactionPlayer> factionMembers = new List<FactionPlayer>();

                MySqlQuery mySqlQuery = new MySqlQuery("SELECT * FROM accounts WHERE Fraktion = @fraktion");
                mySqlQuery.AddParameter("@fraktion", dbPlayer.Faction.Id);
                MySqlResult mySqlResult = MySqlHandler.GetQuery(mySqlQuery);
                MySqlDataReader reader = mySqlResult.Reader;

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if(PlayerHandler.GetPlayer(reader.GetString("Username")) == null)
                        factionMembers.Add(new FactionPlayer(dbPlayer.Faction.Name, reader.GetInt32("Fraktionrank"), reader.GetString("Username")));
                    }
                }

                factionMembers = factionMembers.OrderBy(obj => obj.fraktionRank).ToList();
                factionMembers.Reverse();

                p.TriggerEvent("phone:loadteamappapp", dbPlayer.Faction.Name, NAPI.Util.ToJson(PlayerHandler.GetFactionAppPlayers(dbPlayer.Faction)), NAPI.Util.ToJson(/*PlayerHandler.GetOfflineFactionAppPlayers(dbPlayer.Faction)*/factionMembers));
            } catch(Exception ex) { Console.Write(ex.Message); }
        }

        [RemoteEvent("phone:leavefrak")]
        public void LeaveFrak(Client p)
        {
            try
            {
                DbPlayer dbPlayer = p.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                    return;

                dbPlayer.SetAttribute("Fraktion", 0);
                dbPlayer.SetAttribute("Fraktionrank", 0);
                dbPlayer.SendNotification("Du hast die Fraktion verlassen!", "black", 3500);
                dbPlayer.Faction = FactionModule.getFactionById(0);
                dbPlayer.Factionrank = 0;
                dbPlayer.RefreshData(dbPlayer);

                p.TriggerEvent("phone:close");


            } catch(Exception ex) { Console.Write(ex.Message); }
        }

    }
}
