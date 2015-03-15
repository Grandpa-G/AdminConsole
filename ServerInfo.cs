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
     class ServerInfo
    {
        static CommandArgs args;
        public ServerInfo(CommandArgs a)
        {
            args = a;
        }

       public static void LoadServer () {
           string formatc = "    {1} {0} {2} Ver:{3}";
                           args.Player.SendMessage(String.Format("Current Plugins ({0})", ServerApi.Plugins.Count), Color.LightSalmon);
                for (int i = 0; i < ServerApi.Plugins.Count; i++)
                {
                    PluginContainer pc = ServerApi.Plugins.ElementAt(i);
                    args.Player.SendMessage(String.Format(formatc, pc.Plugin.Name, pc.Plugin.Description, pc.Plugin.Author, pc.Plugin.Version), Color.LightSalmon);
                }

        }
    }
}
