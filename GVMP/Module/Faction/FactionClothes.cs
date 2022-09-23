/*using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GVMP
{
    class FactionClothes : Script
    {
        [RemoteEvent]
        public void FactionClothesPuton(Client c, int state)
        {

            if (c == null) return;
            DbPlayer dbPlayer = c.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid(true) || dbPlayer.Client == null)
            return;
            if (dbPlayer.Faction.Id == 3) return;

            if (state == 3) // FACTION OUTFIT
            {
                switch (dbPlayer.Faction.Id)
                {
                    case 1:
                        dbPlayer.SetClothes(1, 111, 3);
                        dbPlayer.SetClothes(3, 3, 0);
                        dbPlayer.SetClothes(4, 24, 5);
                        dbPlayer.SetClothes(11, 111, 3);
                        dbPlayer.SetClothes(6, 6, 0);
                        break;
                    case 2:
                        dbPlayer.SetClothes(11, 1, 0);
                        dbPlayer.SetClothes(2, 1, 0);
                        dbPlayer.SetClothes(3, 1, 0);
                        dbPlayer.SetClothes(4, 1, 0);
                        dbPlayer.SetClothes(5, 1, 0);
                        break;
                    default:
                        break;

                }
            }
            else // KRIEG OUTFIT
            {
                switch (dbPlayer.Faction.Id)
                {
                    case 1:
                        dbPlayer.SetClothes(1, 51, 8);
                        dbPlayer.SetClothes(3, 1, 0);
                        dbPlayer.SetClothes(4, 17, 2);
                        dbPlayer.SetClothes(11, 111, 3);
                        dbPlayer.SetClothes(6, 1, 0);
                        break;
                    case 2:
                        dbPlayer.SetClothes(11, 1, 0);
                        dbPlayer.SetClothes(2, 1, 0);
                        dbPlayer.SetClothes(3, 1, 0);
                        dbPlayer.SetClothes(4, 1, 0);
                        dbPlayer.SetClothes(5, 1, 0);
                        break;
                    default:
                        break;

                }
            }

        }
    }
}*/
