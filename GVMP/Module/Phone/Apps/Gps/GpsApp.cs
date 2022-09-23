using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;

namespace GVMP
{
    class GpsApp : GVMP.Module.Module<GpsApp>
    {
        [RemoteEvent("requestGpsLocations")]
        public void requestGpsLocations(Client c)
        {
        DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null)
            return;
            House house = HouseModule.houses.Find((House house) => house.OwnerId == dbPlayer.Id);

            List<GPSCategorie> cat = new List<GPSCategorie> { };

            List<GPSPosition> positionen = new List<GPSPosition> {
                new GPSPosition("Ephi Sammler", new Vector3(-500.5, 2977.22, 25.48)),
                new GPSPosition("Ephi Verarbeiter", new Vector3(163.16, 2285.88, 92.95)),
                new GPSPosition("Dealer", new Vector3(-676.69, -884.92, 23.35)),
            };

            List<GPSPosition> garagen = new List<GPSPosition> {
                new GPSPosition("Vespucci Garage", new Vector3(-1184.2845, -1509.452, 3.5480242)),
                new GPSPosition("Pillbox Garage", new Vector3(-313.81174, -1034.3071, 29.430506)),
                new GPSPosition("Meeting Point Garage", new Vector3(100.46749, -1073.2855, 28.274118)),
                new GPSPosition("Vinewood Garage", new Vector3(638.3967, 206.5143, 96.5042)),
                new GPSPosition("Universität Garage", new Vector3(638.3967, 206.5143, 96.5042)),
                new GPSPosition("Hafen Garage", new Vector3(-331.3111, -2779.0078, 4.0451927)),
                new GPSPosition("Flughafen Garage", new Vector3(-984.3403, -2640.988, 12.852915)),
                new GPSPosition("Rockford Garage", new Vector3(-728.04517, -63.06201, 40.653107)),
                new GPSPosition("Mirrorpark Garage", new Vector3(1036.261, -763.1047, 56.892986)),
            };

            List<GPSPosition> shop = new List<GPSPosition> {
                new GPSPosition("Vespucci Shop", new Vector3(-707.8701, -913.9265, 18.115591)),
                new GPSPosition("Ammunation Shop", new Vector3(21.150259, -1107.3888, 28.697025)),
                new GPSPosition("Davis Shop", new Vector3(-48.4442, -1756.734, 28.42101)),
                new GPSPosition("24/7 Shop", new Vector3(25.7567, -1346.8448, 28.397045)),
                new GPSPosition("Vinewood Shop", new Vector3(374.30573, 326.5396, 102.46638)),
            };

            List<GPSPosition> Fraktion = new List<GPSPosition> {
                new GPSPosition("MG13", new Vector3(1287.74, -1598.62, 54)),
                new GPSPosition("Vagos", new Vector3(-1103.4705, -1636.7103, 3.5)),
                new GPSPosition("Ballas", new Vector3(84.89, -1967.36, 20.75)),
                new GPSPosition("Grove", new Vector3(1379.6508, -539.149, 73.23)),
                new GPSPosition("Midnight", new Vector3(-328.1697, -2700.528, 7.549577)),
                new GPSPosition("Irish Mob", new Vector3(-1888.31, 2049.84, 140.98)),
                new GPSPosition("Triaden", new Vector3(1394.54, 1141.79, 113.61)),
                new GPSPosition("Yakuza", new Vector3(-1516.7, 851.89, 181.59)),
                new GPSPosition("LCN", new Vector3(-1535.9,97.72, 56.78)),
                new GPSPosition("Sicario", new Vector3(-3023.3, 81.74, 11.61)),
                new GPSPosition("Banditos", new Vector3(-1820.14, -1186.47, 14.3)),
                new GPSPosition("BGD", new Vector3(-310.27, 179.46, 87.92)),
                new GPSPosition("HoH", new Vector3(56.41, 3690.81, 39.92)),

            };

            cat.Add(new GPSCategorie("Farming-Routen", positionen));
            cat.Add(new GPSCategorie("Garagen", garagen));
            cat.Add(new GPSCategorie("Shops", shop));
            cat.Add(new GPSCategorie("fraktion", Fraktion));
            if (house != null)
                cat.Add(new GPSCategorie("Haus", new List<GPSPosition> {
                new GPSPosition("Haus " + house.Id, house.Entrance),
            }));

            c.TriggerEvent("componentServerEvent", "GpsApp", "gpsLocationsResponse", NAPI.Util.ToJson(cat));
        }

        [RemoteEvent("requestVehicleGps")]
        public void requestVehicleGps(Client c)
        {
            List<GPSCategorie> cat = new List<GPSCategorie> { };

            List<GPSPosition> vehicles = new List<GPSPosition> { };
            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
                return;
            cat.Add(new GPSCategorie("Ausgeparkte Fahrzeuge", vehicles));
            foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
            {
                DbVehicle dbVehicle = vehicle.GetVehicle();
                if (dbVehicle == null || !dbVehicle.IsValid() || dbVehicle.Vehicle == null)
                    continue;
                {
                if (NAPI.Pools.GetAllVehicles().Find(x => x.NumberPlate == dbVehicle.Plate) != null)
                if ((dbVehicle.OwnerId == dbPlayer.Id || dbVehicle.Keys.Contains(dbPlayer.Id)) && vehicle.Position.DistanceTo(dbPlayer.Client.Position) < 12000)
                        {
                    vehicles.Add(new GPSPosition(" [" + dbVehicle.Plate + "] " + dbVehicle.Model + "", NAPI.Pools.GetAllVehicles().Find(x => x.NumberPlate == dbVehicle.Plate).Position));
                }
            }
            }
            c.TriggerEvent("componentServerEvent", "GpsApp", "gpsLocationsResponse", NAPI.Util.ToJson(cat));
                }

        }


        public class GPSCategorie
        {
            public string name { get; set; }
            public List<GPSPosition> locations { get; set; }
            public GPSCategorie(string n, List<GPSPosition> pos)
            {
                name = n;
                locations = pos;
            }
        }

        public class GPSPosition
        {
            public string name { get; set; }
            public float x { get; set; }
            public float y { get; set; }

            public GPSPosition(string n, Vector3 pos)
            {
                name = n;
                x = pos.X;
                y = pos.Y;
            }
        }
    }
