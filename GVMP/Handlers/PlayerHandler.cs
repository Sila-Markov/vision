using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GVMP
{
    public static class PlayerHandler
    {
        public static List<DbPlayer> GetPlayers()
        {
            List<DbPlayer> dbPlayers = new List<DbPlayer>();
            List<Client> clients = NAPI.Pools.GetAllPlayers();

            foreach (Client c in clients)
            {
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null || dbPlayer.Client.IsNull)
                    continue;

                dbPlayers.Add(dbPlayer);
            }

            return dbPlayers;
        }

        public static List<DbPlayer> GetAdminPlayers()
        {
            List<DbPlayer> dbPlayers = new List<DbPlayer>();
            List<Client> clients = NAPI.Pools.GetAllPlayers();

            foreach (Client c in clients)
            {
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null || dbPlayer.Client.IsNull || dbPlayer.Adminrank.Permission <= 0)
                    continue;

                dbPlayers.Add(dbPlayer);
            }

            return dbPlayers;
        }

        /**/
        public static List<DbPlayer> GetFactionPlayers(this Faction faction)
        {
            List<DbPlayer> dbPlayers = new List<DbPlayer>();
            List<Client> clients = NAPI.Pools.GetAllPlayers();

            foreach (Client c in clients)
            {
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null || dbPlayer.Client.IsNull || dbPlayer.Faction.Id != faction.Id)
                    continue;

                dbPlayers.Add(dbPlayer);
            }

            return dbPlayers;
        }
        public static List<FactionPlayer> GetOfflineFactionAppPlayers(Faction faction)
        {
            using MySqlConnection connection = new MySqlConnection(Configuration.connectionString);
            connection.Open();

            List<FactionPlayer> list = new List<FactionPlayer>();

            try
            {

                using (var cmd = new MySqlCommand($"SELECT * FROM accounts WHERE Fraktion = {faction.Id}", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString(2);
                            var rank = reader.GetDecimal(7);

                            if (GetPlayer(name) == null)
                                list.Add(new FactionPlayer(faction.Name, (int)rank, name));
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("Error while requesting offline fraktionusers: " + ex.Message);
            }
            return list;
        }

        public static List<FactionPlayer> GetFactionAppPlayers(this Faction faction)
        {
            List<FactionPlayer> dbPlayers = new List<FactionPlayer>();
            List<Client> clients = NAPI.Pools.GetAllPlayers();

            foreach (Client c in clients)
            {
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null || dbPlayer.Client.IsNull || dbPlayer.Faction.Id != faction.Id)
                    continue;

                FactionPlayer factionplayer = new FactionPlayer(dbPlayer.Faction.Name, dbPlayer.Factionrank, dbPlayer.Name);
                dbPlayers.Add(factionplayer);
            }

            return dbPlayers;
        }

        public static List<DbPlayer> GetBusinessPlayers(this Business business)
        {
            List<DbPlayer> dbPlayers = new List<DbPlayer>();
            List<Client> clients = NAPI.Pools.GetAllPlayers();

            foreach (Client c in clients)
            {
                DbPlayer dbPlayer = c.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null || dbPlayer.Client.IsNull || dbPlayer.Business.Id != business.Id)
                    continue;

                dbPlayers.Add(dbPlayer);
            }

            return dbPlayers;
        }

        public static DbPlayer GetPlayer(string Name)
        {
            DbPlayer dbPlayer = GetPlayers().FirstOrDefault((DbPlayer dbPlayer) => dbPlayer.Name == Name);

            return dbPlayer;
        }

        public static DbPlayer GetPlayer(int Id)
        {
            DbPlayer dbPlayer = GetPlayers().FirstOrDefault((DbPlayer dbPlayer) => dbPlayer.Id == Id);

            return dbPlayer;
        }
    }
}
