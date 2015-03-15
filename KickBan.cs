using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using TShockAPI;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TerrariaApi.Server;
using Newtonsoft.Json.Linq;

namespace ServerGUI
{
    class KickBan
    {
        public KickBan()
        {

        }
        public String KickBanPlayer(String action, int playerIndex, String reason)
        {
            String playerName = TShock.Players[playerIndex].Name;
            if (playerName.Length == 0)
                return "Error in finding player " + playerName;

            if(action.Equals("Ban"))
                TShock.Bans.AddBan(TShock.Players[playerIndex].IP, playerName, "", reason);
            TShock.Utils.ForceKick(TShock.Players[playerIndex], reason, false, true);
            return "Player " + playerName + " was " + action + "ed";
        }
    }

}
