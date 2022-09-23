using System;
using System.Collections.Generic;
using System.Text;

namespace GVMP
{
    public class FactionPlayer
    {
        public string fraktionName { get; set; }

        public int fraktionRank { get; set; }

        public string playerName { get; set; }

        public FactionPlayer(string fraktionName, int fraktionRank, string playerName)
        {
            this.fraktionName = fraktionName;
            this.fraktionRank = fraktionRank;
            this.playerName = playerName;
        }
    }
}
