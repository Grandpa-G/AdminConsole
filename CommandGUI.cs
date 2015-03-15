using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;

using Terraria;
using TShockAPI;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TerrariaApi.Server;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;

namespace ServerGUI
{
    [ApiVersion(1, 17)]
    public class CommandGUIMain : TerrariaPlugin
    {
        Thread thread;
        GUIMain guiForm;

        public override string Name
        {
            get { return "AdminConsole"; }
        }
        public override string Author
        {
            get { return "Granpa-G"; }
        }
        public override string Description
        {
            get { return "Provides an interactive GUI interface management tool for a TShock server"; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        public CommandGUIMain(Main game)
            : base(game)
        {
            Order = -1;
        }
        public override void Initialize()
        {
            //            ServerApi.Hooks.NetGetData.Register(this, GetData);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += OnLogin;

            Commands.ChatCommands.Add(new Command("AdminConsole.allow", CommandGUI, "adminconsole"));
            Commands.ChatCommands.Add(new Command("AdminConsole.allow", CommandGUI, "adminc"));
            CommandArgs c = new CommandArgs("main", null, null);
 //           CommandGUI(c);

        }
  
        protected override void Dispose(bool disposing)
        {
            if(guiForm != null)
            guiForm.Close();
 
            if (disposing)
            {
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= OnLogin;
            }
            base.Dispose(disposing);
        }
        private static void OnLeave(LeaveEventArgs args)
        {
         }

        private void OnLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs args)
        {
        }

        private void CommandGUI(CommandArgs args)
        {

            thread = new Thread(new ThreadStart(() =>
            {
                // this code is going to be executed in a separate thread
                ServerInfo serverInfo = new ServerInfo(args);
                 guiForm = new GUIMain();
                Application.Run(guiForm);
             }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
