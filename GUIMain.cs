using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;

using Terraria;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TerrariaApi.Server;
using Newtonsoft.Json.Linq;
using System.IO;
namespace ServerGUI
{
    public partial class GUIMain : Form
    {
        public static String PROGRAMNAME = "Admin Console";
        public static bool DEBUG = true;

        public static int ITEMOFFSET = 48;
        public static int MAXITEMS = 2748 + ITEMOFFSET + 1;
        public static int MAXITEMSPREFIX = 83 + 1;
        public static int MAXPREFIX = 86;
        public static int EQUIPMENTITEMS = 58;

        Bitmap[] sprites = new Bitmap[MAXITEMS];
        Bitmap item_0;
        Bitmap sprite;
        String[] inventory;

        private Item[] itemList = new Item[MAXITEMS];
        private Prefixs[] prefixList = new Prefixs[MAXPREFIX];

        DialogResult usersChoice;

        private bool playerFound = false;

        public GUIMain()
        {
            InitializeComponent();

            getServerDetails();
            getItemSpriteImage();
            loadItemNames();

            this.banDataBan.CellClick += this.banDataBan_CellClick;

            this.groupDataPermissions.CellClick += this.groupDataPermissions_CellClick;

            this.usersDataList.CellClick += this.usersDataList_CellClick;
            this.usersDataList.CellBeginEdit += this.usersDataList_CellBeginEdit;
            if (DEBUG)
                query.Visible = true;
        }

        private void tabPane_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case 2:
                    getBannedList(true);
                    break;
                case 3:
                    getUsers();
                    break;
                case 4:
                    getGroupList();
                    break;
                case 5:
                    getLog();
                    break;
                case 6:
                    setupAbout();
                    break;
            }
        }


        private void getServerDetails()
        {

            lblServerNamevalue.Text = Terraria.Netplay.serverListenIP.ToString() + ":" + TShock.Config.ServerPort;
            lblWorldName.Text = Main.worldName;
            lblWorldId.Text = Main.worldID.ToString();
            lblInvasionSizeValue.Text = Main.invasionSize.ToString();
            lblWorldSizeValue.Text = Main.maxTilesX + "*" + Main.maxTilesY;
            lblVersionValue.Text = Main.versionNumber.ToString();

            double time = Main.time / 3600.0;
            time += 4.5;
            if (!Main.dayTime)
                time += 15.0;
            time = time % 24.0;
            lblTimeValue.Text = String.Format("{0:d}:{1:##}", (int)Math.Floor(time), (int)Math.Round((time % 1.0) * 60.0));

            lblUptime.Text = (DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime).ToString(@"d'.'hh':'mm':'ss");
            lblPlayerCountValue.Text = Main.player.Where(p => null != p && p.active).Count().ToString();
            lblMaxPlayerValue.Text = TShock.Config.MaxSlots.ToString();

            serverPvPMode.Text = TShock.Config.PvPMode;
            serverSpawnProtectionRadius.Text = TShock.Config.SpawnProtectionRadius.ToString();

            serverAutoSave.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverDisableBuild.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverDisableClownBombsre.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverDisableDungeonGuardian.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverDisableInvisPvP.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverDisableSnowBalls.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverDisableTombstones.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverEnableWhitelist.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverHardcoreOnly.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverSpawnProtection.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);
            serverServerSideInventory.Image = new Bitmap(ServerGUI.Properties.Resources._unchecked);

            if (TShock.Config.AutoSave)
                serverAutoSave.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (TShock.Config.DisableBuild)
                serverDisableBuild.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (TShock.Config.DisableClownBombs)
                serverDisableClownBombsre.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (TShock.Config.DisableDungeonGuardian)
                serverDisableDungeonGuardian.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (TShock.Config.DisableInvisPvP)
                serverDisableInvisPvP.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (TShock.Config.DisableSnowBalls)
                serverDisableSnowBalls.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (TShock.Config.DisableTombstones)
                serverDisableTombstones.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (TShock.Config.EnableWhitelist)
                serverEnableWhitelist.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (TShock.Config.HardcoreOnly)
                serverHardcoreOnly.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (TShock.Config.SpawnProtection)
                serverSpawnProtection.Image = new Bitmap(ServerGUI.Properties.Resources._checked);
            if (Main.ServerSideCharacter)
                serverServerSideInventory.Image = new Bitmap(ServerGUI.Properties.Resources._checked);

            listPlayers.Items.Clear();
            var players = new ArrayList();
            foreach (TSPlayer tsPlayer in TShock.Players.Where(p => null != p))
            {
                ListViewItem item = new ListViewItem(tsPlayer.Name);
                item.SubItems.Add(tsPlayer.UserAccountName);
                item.SubItems.Add(tsPlayer.Group.ToString());
                item.SubItems.Add(tsPlayer.Index.ToString());
                item.SubItems.Add(tsPlayer.IP);
                listPlayers.Items.Add(item);
            }

            playerFound = false;
            lblServerRefresh.Text = DateTime.Now.ToString("ddd h:mm:ss tt");
        }

        private void serverRefresh_Click(object sender, EventArgs e)
        {
            getServerDetails();
        }

        private void listPlayers_Click(object sender, EventArgs e)
        {
            playerFound = true;
            tabPane.SelectedIndex = 1;
        }

        private void tabPlayer_Enter(object sender, EventArgs e)
        {
            if (!playerFound)
            {
                return;
            }

            int playerIndex = Int32.Parse(listPlayers.Items[listPlayers.FocusedItem.Index].SubItems[3].Text);
            if (TShock.Players[playerIndex].UserID < 0)
                lblPlayerName.Text = String.Format("{0} [index={1} UserId= UserAccountName=]", TShock.Players[playerIndex].Name, playerIndex);
            else
                lblPlayerName.Text = String.Format("{0} [index={1} UserId={2} UserAccountName={3}]",
          TShock.Players[playerIndex].Name, playerIndex, TShock.Players[playerIndex].UserID, TShock.Players[playerIndex].UserAccountName);
            var activeItems = TShock.Players[playerIndex].TPlayer.inventory.Where(p => p.active).ToList();
            var activeEquipmentItems = TShock.Players[playerIndex].TPlayer.armor.Where(p => p.active).ToList();
            var activeDyeItems = TShock.Players[playerIndex].TPlayer.dye.Where(p => p.active).ToList();

            String inventoryList = string.Join("~", activeItems.Select(p => (p.netID + "," + p.stack + "," + p.prefix)));
            String equipmentList = string.Join("~", activeEquipmentItems.Select(p => (p.netID + "," + p.stack + "," + p.prefix)));
            String dyeList = string.Join("~", activeDyeItems.Select(p => (p.netID + "," + p.stack + "," + p.prefix)));
            inventoryList = inventoryList + "~" + equipmentList + "~" + dyeList;
            inventory = inventoryList.Split('~');

            SetInventorySlot(this, "inventoryItem");

            panelPlayerHairColor.BackColor = getColor(TShock.Players[playerIndex].TPlayer.hairColor);
            panelPlayerSkinColor.BackColor = getColor(TShock.Players[playerIndex].TPlayer.skinColor);
            panelPlayerEyeColor.BackColor = getColor(TShock.Players[playerIndex].TPlayer.eyeColor);
            panelPlayerShirtColor.BackColor = getColor(TShock.Players[playerIndex].TPlayer.shirtColor);
            panelPlayerUnderShirtColor.BackColor = getColor(TShock.Players[playerIndex].TPlayer.underShirtColor);
            panelPlayerPantsColor.BackColor = getColor(TShock.Players[playerIndex].TPlayer.pantsColor);
            panelPlayerShoesColor.BackColor = getColor(TShock.Players[playerIndex].TPlayer.shoeColor);
        }

        private System.Drawing.Color getColor(Color value)
        {
            return System.Drawing.Color.FromArgb(value.R, value.G, value.B);
        }

        private void GUIMain_Load(object sender, EventArgs e)
        {
            this.BackColor = System.Drawing.Color.FromArgb(238, 232, 170);
        }

        private void btnKick_Click(object sender, EventArgs e)
        {
            String reason = txtKickBanReason.Text;
            if (reason.Length == 0)
            {
                MessageBox.Show("No reason has been given for the Kick action.", PROGRAMNAME, MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                return;
            }
            int playerIndex = Int32.Parse(listPlayers.Items[listPlayers.FocusedItem.Index].SubItems[3].Text);

            KickBan kb = new KickBan();
            String results = kb.KickBanPlayer("Kick", playerIndex, reason);
            lblKickBanStatus.Text = results;
        }

        private void btnBan_Click(object sender, EventArgs e)
        {
            String reason = txtKickBanReason.Text;
            if (reason.Length == 0)
            {
                MessageBox.Show("No reason has been given for the Ban action.", PROGRAMNAME, MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                return;
            }
            int playerIndex = Int32.Parse(listPlayers.Items[listPlayers.FocusedItem.Index].SubItems[3].Text);

            KickBan kb = new KickBan();
            String results = kb.KickBanPlayer("Ban", playerIndex, reason);
            lblKickBanStatus.Text = results;
        }

        public void SetInventorySlot(Control control, String group)
        {
            if (control is Label)
            {
                Label lbl = (Label)control;
                if (lbl.Name.StartsWith(group))
                {
                    int slotIndex = Int32.Parse(lbl.Name.Substring(13)) - 1;
                    String n = inventory[slotIndex];
                    int netId = Int32.Parse(n.Split(',')[0]);
                    string stacks = n.Split(',')[1];
                    int prefix = Int32.Parse(n.Split(',')[2]);
                    if (slotIndex > EQUIPMENTITEMS || netId == 0)
                        lbl.Text = "";
                    else
                    {
                        if (stacks.Length == 0)
                            lbl.Text = "";
                        else
                            lbl.Text = stacks;
                    }
                    lbl.Image = sprites[ITEMOFFSET + netId];

                    ToolTip itemTip = new ToolTip();
                    itemTip.IsBalloon = false;
                    itemTip.ShowAlways = true;
                    if (prefix > 0)
                        itemTip.SetToolTip(lbl, prefixList[prefix].Name + " " + itemList[ITEMOFFSET + netId].Name);
                    else
                        itemTip.SetToolTip(lbl, itemList[ITEMOFFSET + netId].Name);
                    /*
                    lbl.Image = item_0;
                    Graphics g = Graphics.FromImage(lbl.Image);
                    Bitmap smallImage = new Bitmap(sprites[ITEMOFFSET + netId]);
//                    g.DrawImage(smallImage, new System.Drawing.Point(0, 0));
 //                   CopyBmpRegion((Bitmap)sprites[ITEMOFFSET + netId], new Rectangle(5, 5, 100, 100), new Point(100, 100));
                    RectangleF rect = new Rectangle(0, 0, sprites[ITEMOFFSET + netId].Width, sprites[ITEMOFFSET + netId].Height);
                    g.DrawImage(smallImage, new Rectangle(0, 0, lbl.Image.Width, lbl.Image.Height), rect, GraphicsUnit.Pixel); 
                     */

                }
            }
            else
                foreach (Control child in control.Controls)
                {
                    SetInventorySlot(child, group);
                }

        }


        private void getItemSpriteImage()
        {
            item_0 = new Bitmap(ServerGUI.Properties.Resources.item_0);
            if (item_0 == null)
                Console.WriteLine("null item_0");

            sprite = new Bitmap(ServerGUI.Properties.Resources.sprite);
            if (item_0 == null)
                Console.WriteLine("null sprite");
            int width = 48;
            int height = 48;
            int rows = MAXITEMS / 100 + 1;   //we assume the no. of rows and cols are known and each chunk has equal width and height
            int cols = 100;
            sprites = new Bitmap[rows * cols];

            int lastRow = 0;
            System.Drawing.Rectangle rectangle;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols && (lastRow < itemList.Length); j++)
                {
                    try
                    {
                        rectangle = new System.Drawing.Rectangle(j * width, i * height, width, height);
                        sprites[lastRow] = (Bitmap)sprite.Clone(rectangle, sprite.PixelFormat);
                        lastRow++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("E-" + (lastRow - 1) + " " + itemList[lastRow - 1] + " " + j + "-" + (j * width) + ":" + i + "-" + (i * height));
                    }
                }
            }
        }

        private void loadItemNames()
        {
            int counter = 0;
            string line;
            string[] linearray;
            string name;
            int netId;
            int stackSize;
            int prefix;
            for (int i = 0; i < itemList.Length; i++)
                itemList[i] = new Item();
            Assembly assem = Assembly.GetExecutingAssembly();

            for (int i = 0; i < prefixList.Length; i++)
                prefixList[i] = new Prefixs();
            counter = 0;

            Assembly _assembly;
            StreamReader reader = null;
            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                reader = new StreamReader(_assembly.GetManifestResourceStream("ServerGUI.assests.itemlist.txt"));
            }
            catch
            {
                Console.WriteLine("Error accessing itemlist resource!");
            }
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    linearray = line.Split('`');
                    netId = Int32.Parse(linearray[0].Trim());
                    name = linearray[1].Trim();
                    stackSize = Int32.Parse(linearray[3].Trim());
                    prefix = Int32.Parse(linearray[2].Trim());
                    itemList[counter] = new Item(name, netId, stackSize, prefix);
                    counter++;
                }
                reader.Close();
            }
            catch
            {
                Console.WriteLine("Error prefix list text!");
            }

            for (int i = 0; i < prefixList.Length; i++)
                prefixList[i] = new Prefixs();
            counter = 0;

            reader = null;
            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                reader = new StreamReader(_assembly.GetManifestResourceStream("ServerGUI.assests.prefixlist.txt"));
            }
            catch
            {
                Console.WriteLine("Error accessing prefixlist resource!");
            }
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    linearray = line.Split(':');
                    name = linearray[0].Trim();
                    prefix = Int32.Parse(linearray[1].Trim());
                    prefixList[counter] = new Prefixs(name, prefix);
                    counter++;
                }
            }
            catch
            {
                Console.WriteLine("Error prefix list text!");
            }

        }

        /// <summary>
        /// Gives an image based on the item name given
        /// </summary>
        /// <param name="img">The name of the item to create an image for</param>
        /// <returns>Retrurns image associated with the item in question</returns>
        public System.Drawing.Image ItemImage(string img)
        {
            try
            {
                System.Reflection.Assembly thisExe;
                thisExe = System.Reflection.Assembly.GetExecutingAssembly();
                String item = "GUIMain.Resources.Item_" + img + ".png";
                System.IO.Stream file = thisExe.GetManifestResourceStream(item);
                return Image.FromStream(file);

            }
            catch
            {
                return null;
            }
        }

        #region  About Tab
        private void setupAbout()
        {
            aboutVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            aboutServerVersion.Text = TShock.VersionNum.ToString();
            aboutSQLSupport.Text = TShock.Config.StorageType + " " + TShock.Config.MySqlDbName + " " + TShock.Config.MySqlHost;
        }


        #endregion

        #region  Server Tab
        //  Server Tab


        private void stopServer_Click(object sender, EventArgs e)
        {
            TShock.Utils.StopServer();
        }


        //      Ban Tab
        /// <summary>
        /// ///////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void getBannedList(bool fullSearch)
        {
            string userId;
            bool success = false;
            bool nameSuccess = false;
            bool groupSuccess = false;
            bool IPSuccess = false;
            bool nameUsed = false;
            bool groupUsed = false;
            bool IPUsed = false;
            banUnBan.Enabled = false;
            banDataBan.Rows.Clear();
            // Instantiate the regular expression object.
            string pat = banSearchName.Text;
            Regex r = null;
            if (!fullSearch)
            {
                nameUsed = ((banSearchName.Text.Length > 0) || banFuzzyName.Checked);
                groupUsed = ((banSearchGroup.Text.Length > 0) || banFuzzyGroup.Checked);
                IPUsed = ((banSearchIP.Text.Length > 0) || banFuzzyIP.Checked);

                if (nameUsed)
                {
                    if (banFuzzyName.Checked)
                        pat = @"^.*" + banSearchName.Text + @".*$";
                    else
                        pat = @"^" + ((banSearchName.Text.Length == 0) ? ".*" : banSearchName.Text) + @"$";
                    r = new Regex(pat, RegexOptions.IgnoreCase);
                }
                if (groupUsed)
                {
                    if (banFuzzyGroup.Checked)
                        pat = @"^.*" + banSearchGroup.Text + @".*$";
                    else
                        pat = @"^" + ((banSearchGroup.Text.Length == 0) ? ".*" : banSearchGroup.Text) + @"$";
                    r = new Regex(pat, RegexOptions.IgnoreCase);
                }
                if (IPUsed)
                {
                    if (banFuzzyIP.Checked)
                        pat = @"^.*" + banSearchIP.Text + @".*$";
                    else
                        pat = @"^" + ((banSearchIP.Text.Length == 0) ? ".*" : banSearchIP.Text) + @"$";
                    r = new Regex(pat, RegexOptions.IgnoreCase);
                }
            }
            foreach (TShockAPI.DB.Ban ban in TShock.Bans.GetBans())
            {
                success = fullSearch;
                nameSuccess = true;
                groupSuccess = true;
                IPSuccess = true;
                if (fullSearch)
                    success = true;
                else
                {
                    if (nameUsed)
                    {
                        // Match the regular expression pattern against a text string.
                        Match m = r.Match(ban.Name);
                        // Here we check the Match instance.
                        nameSuccess = m.Success;
                        success = nameSuccess;
                    }
                    if (IPUsed)
                    {
                        // Match the regular expression pattern against a text string.
                        Match m = r.Match(ban.IP);
                        // Here we check the Match instance.
                        IPSuccess = m.Success;
                        success = IPSuccess;
                    }
                }
                if (success)
                {
                    TShockAPI.DB.User user = TShock.Users.GetUserByName(ban.Name);
                    userId = String.Format("{0}", user.ID);
                    string myDate = "";
                    if (ban.Date.Length > 0)
                        myDate = String.Format("{0:MM/dd/yyyy HH:mm}", DateTime.ParseExact(ban.Date.Replace("T", " "), "yyyy-MM-dd HH:mm:ss", null));
                    string myExpirationDate = "";
                    if (ban.Expiration.Length > 0)
                        myExpirationDate = String.Format("{0:MM/dd/yyyy HH:mm}", DateTime.ParseExact(ban.Expiration.Replace("T", " "), "yyyy-MM-dd HH:mm:ss", null));
                    banDataBan.Rows.Add(false, ban.Name, ban.IP, ban.Reason, ban.BanningUser, myDate, myExpirationDate);
                }
            }
        }

        #endregion

        #region  Ban Tab
        private void refreshBan_Click(object sender, EventArgs e)
        {
            banSearchName.Text = "";
            banSearchGroup.Text = "";
            banSearchIP.Text = "";
            banFuzzyName.Checked = false;
            banFuzzyGroup.Checked = false;
            banFuzzyIP.Checked = false;

            getBannedList(true);
        }
        private void banDataBan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //check if row index is not selected
            DataGridViewCheckBoxCell cbc = (DataGridViewCheckBoxCell)banDataBan.CurrentCell;
            DataGridViewCheckBoxCell unBan = new DataGridViewCheckBoxCell();
            unBan = (DataGridViewCheckBoxCell)banDataBan.Rows[e.RowIndex].Cells[0];
            if (unBan.Value == null)
                unBan.Value = false;
            switch (unBan.Value.ToString())
            {
                case "False":
                    unBan.Value = true;
                    banUnBan.Enabled = true;
                    break;
                case "True":
                    unBan.Value = false;
                    break;
            }
        }

        private void banUnBan_Click(object sender, EventArgs e)
        {
            bool someUnBanned = false;
            foreach (DataGridViewRow row in banDataBan.Rows)
            {
                if (row.Cells != null)
                {
                    if (!row.IsNewRow)
                    {
                        DataGridViewCheckBoxCell unBan = new DataGridViewCheckBoxCell();
                        unBan = (DataGridViewCheckBoxCell)row.Cells[0];
                        if (unBan.Value.ToString().Equals("True"))
                            TShock.Bans.RemoveBan(((DataGridViewCell)row.Cells[1]).Value.ToString(), true, true, true);
                        someUnBanned = true;
                    }
                }
            }
            if (someUnBanned)
                getBannedList(true);
        }

        private void banStartSearch_Click(object sender, EventArgs e)
        {
            getBannedList(false);
        }

        private void banClearSearch_Click(object sender, EventArgs e)
        {
            banSearchName.Text = "";
            banSearchGroup.Text = "";
            banSearchIP.Text = "";
            banFuzzyName.Checked = false;
            banFuzzyGroup.Checked = false;
            banFuzzyIP.Checked = false;
        }


        #endregion

        #region  Group Tab
        //     Group tab

        // Group Tab
        private void getGroupList()
        {
            groupDataList.Rows.Clear();
            groupDataPermissions.Rows.Clear();
            var groups = new ArrayList();
            foreach (TShockAPI.Group TSgroup in TShock.Groups)
            {
                groupDataList.Rows.Add(TSgroup.Name, TSgroup.ParentName, TSgroup.ChatColor, "", TSgroup.Prefix, TSgroup.Suffix);
            }
            foreach (DataGridViewRow row in groupDataList.Rows)
            {
                if (row.Cells[2] != null)
                {
                    System.Drawing.Color oldColor = row.Cells[2].Style.BackColor;
                    if (row.Cells[2].Value != null)
                        if (row.Cells[2].Value.ToString().Length > 0)
                        {
                            row.Cells[2].Style.BackColor = tabColorDecode(row.Cells[2].Value.ToString());
                            row.Cells[3].Style.BackColor = row.Cells[2].Style.BackColor;
                        }
                }
            }
            this.groupDataList.Sort(this.groupDatagroup, ListSortDirection.Ascending);

        }

        private System.Drawing.Color tabColorDecode(string colorString)
        {
            ColorDialog dlg = new ColorDialog();
            byte[] color = new Byte[] { 0, 0, 0 };
            string[] bytes = colorString.Split(',');
            for (int i = 0; i < Math.Min(bytes.Length, 3); i++)
            {
                if (bytes[i].Length > 0)
                {
                    int c = Int32.Parse(bytes[i]);
                    byte[] b = BitConverter.GetBytes(c);
                    color[i] = b[0];
                }
            }
            return System.Drawing.Color.FromArgb(color[0], color[1], color[2]);
        }
        private System.Drawing.Color tabColorPickerDialog(string colorString)
        {
            ColorDialog dlg = new ColorDialog();
            byte[] color = new Byte[] { 0, 0, 0 };
            string[] bytes = colorString.Split(',');
            for (int i = 0; i < Math.Min(bytes.Length, 3); i++)
            {
                if (bytes[i].Length > 0)
                {
                    int c = Int32.Parse(bytes[i]);
                    byte[] b = BitConverter.GetBytes(c);
                    color[i] = b[0];
                }
            }
            dlg.Color = System.Drawing.Color.FromArgb(color[0], color[1], color[2]);

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dlg.Color;
            }
            return dlg.Color;
        }

        private void tabUpdatePermissions_Click(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = groupDataList.CurrentRow;

            string group = "";
            if (selectedRow.Cells[0] != null)
                if (selectedRow.Cells[0].Value != null)
                    group = selectedRow.Cells[0].Value.ToString();

            string parent = "";
            if (selectedRow.Cells[1] != null)
                if (selectedRow.Cells[1].Value != null)
                    parent = selectedRow.Cells[1].Value.ToString();

            string chatColor = "";
            if (selectedRow.Cells[2] != null)
                if (selectedRow.Cells[2].Value != null)
                    chatColor = selectedRow.Cells[2].Value.ToString();

            string prefix = "";
            if (selectedRow.Cells[3] != null)
                if (selectedRow.Cells[3].Value != null)
                    prefix = selectedRow.Cells[3].Value.ToString();

            string suffix = "";
            if (selectedRow.Cells[4] != null)
                if (selectedRow.Cells[4].Value != null)
                    suffix = selectedRow.Cells[4].Value.ToString();

            string permissions = "";
            string comma = "";
            foreach (DataGridViewRow row in groupDataPermissions.Rows)
            {
                if (row.IsNewRow)
                    if (DEBUG)
                        Console.WriteLine("3>new");
                    else
                    {
                        if (row.Cells[1] != null)
                            if (row.Cells[1].Value != null)
                            {
                                if (row.Cells[1].Value.ToString().Equals("*"))
                                    continue;
                            }
                    }

                if (row.Cells[0] != null)
                    if (row.Cells[0].Value != null)
                    {
                        permissions = permissions + comma + row.Cells[0].Value.ToString();
                        comma = ",";
                    }
            }

            string result;
            if (group.Length == 0)
            {
                usersChoice = MessageBox.Show("No group given.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedRow.IsNewRow)
            {
                try
                {
                    result = TShock.Groups.AddGroup(group, parent, permissions, chatColor, false);
                }
                catch (GroupExistsException e1)
                {
                    Console.WriteLine("Group " + group + " already exists!");
                    usersChoice = MessageBox.Show("Group " + group + " already exists!", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (DEBUG)
                    Console.WriteLine("add g " + group + ":" + result);
                TShock.Log.ConsoleInfo(PROGRAMNAME + " added  group " + group);
            }
            try
            {
                TShock.Groups.UpdateGroup(group, parent, permissions, chatColor, suffix, prefix);
            }
            catch (GroupNotExistsException e1)
            {
                Console.WriteLine("Group " + group + " does not exist!");
                usersChoice = MessageBox.Show("Group " + group + " does not exist!", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (DEBUG)
                Console.WriteLine("update g " + group);

            if (deletedPermissions.Count > 0)
            {
                result = TShock.Groups.DeletePermissions(group, deletedPermissions);

                if (DEBUG)
                    Console.WriteLine("del p " + group + ":" + result);
            }
            getGroupList();
        }

        private void groupDataPermissions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //check if row index is not selected
            DataGridViewRow row = groupDataPermissions.Rows[e.RowIndex];

        }

        private void groupDataList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //check if row index is not selected
            DataGridViewRow row = groupDataList.Rows[e.RowIndex];

            if (e.ColumnIndex == 3) // chat color
            {
                System.Drawing.Color newColor = tabColorPickerDialog(row.Cells[2].Value.ToString());
                groupDataList.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = newColor;
                row.Cells[2].Value = newColor.R.ToString() + "," + newColor.G.ToString() + "," + newColor.B.ToString();
            }

            string name = row.Cells[0].Value.ToString();
            groupDataPermissions.Rows.Clear();
            deletedPermissions.Clear();

            TShockAPI.Group group = TShock.Groups.GetGroupByName(name);

            if (group != null)
                if (group.permissions.Count > 0)
                {
                    string[] permissions = group.Permissions.Split(',');

                    string[] negatedpermissions = group.negatedpermissions.ToArray();

                    string[] totalpermissions = group.TotalPermissions.ToArray();
                    string Inherited = "";
                    string[,] permissionList = new String[totalpermissions.Length, 2];
                    for (int i = 0; i < totalpermissions.Length; i++)
                    {
                        Inherited = "*";
                        for (int j = 0; j < permissions.Length; j++)
                        {
                            if (totalpermissions[i].Equals(permissions[j]))
                            {
                                Inherited = "";
                                break;
                            }
                        }
                        String s = totalpermissions[i];
                        permissionList[i, 0] = totalpermissions[i];
                        permissionList[i, 1] = Inherited;
                    }

                    for (int i = 0; i < totalpermissions.Length; i++)
                        groupDataPermissions.Rows.Add(permissionList[i, 0], permissionList[i, 1]);
                }
            this.groupDataPermissions.Sort(this.permissionsDataPermissons, ListSortDirection.Ascending);

        }

        private void tabGroupRefresh_Click(object sender, EventArgs e)
        {
            getGroupList();
        }
        List<string> deletedPermissions = new List<string>();
        private void groupDataPermissions_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Cells[1].Value.ToString().Equals("*"))
            {
                // Cancel the deletion if the permission belongs to parent.
                e.Cancel = true;
            }
            deletedPermissions.Add(e.Row.Cells[0].Value.ToString());
        }

        private void groupDataPermissions_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {

        }

        private void groupDataList_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {

        }

        private void groupDataList_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {

        }

        #endregion

        #region  users Tab
        //          Users Tab

        /// <summary>
        /// Gets a list of Users from db.
        /// </summary>
        private static List<UserList> FindUsers(string search, bool casesensitive = true)
        {
            UserList rec = null;
            int id = 0;
            String sql;
            int hasInventory;
            List<UserList> UserList = new List<UserList>();

            try
            {
                sql = "SELECT * FROM Users " + search + " order by UserName";
                using (var reader = TShock.DB.QueryReader(sql))
                {
                    while (reader.Read())
                    {
                        rec = new UserList(reader.Get<Int32>("Id"), reader.Get<string>("UserName"), reader.Get<string>("UserGroup"), reader.Get<string>("Registered"), reader.Get<string>("LastAccessed"), reader.Get<string>("KnownIPs"), 0);
                        id = rec.Id;
                        using (var counter = TShock.DB.QueryReader("SELECT count(*) as count FROM tsCharacter where account =@0", id))
                        {
                            if (counter.Read())
                            {
                                hasInventory = counter.Get<Int32>("count");
                                rec.InventoryCount = hasInventory;
                            }
                        }
                        UserList.Add(rec);
                    }
                }
                return UserList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return null;
        }


        private void getUsers()
        {
            string knownIPs;
            string[] IPString;
            string registeredDate = "";
            string lastAccessedDate = "";
            string[] inventoryList;
            bool hasInventory;
            String searchString;

            String whereClause = "";
            String andClause = " where ";
            String column = "username";
            searchString = "%";

            if (usersSearchName.Text != null)
                if (usersSearchName.Text.Length > 0)
                {
                    column = "UserName";
                    searchString = usersSearchName.Text;
                    if (usersFuzzyName != null && usersFuzzyName.Checked)
                        whereClause = whereClause + andClause + " UserName like '%" + usersSearchName.Text + "%'";
                    else
                        whereClause = whereClause + andClause + " UserName like '" + usersSearchName.Text + "'";
                    andClause = " and ";
                }
            if (usersSearchGroup.Text != null)
                if (usersSearchGroup.Text.Length > 0)
                {
                    column = "UserGroup";
                    searchString = usersSearchGroup.Text;
                    if (usersFuzzyGroup != null && usersFuzzyGroup.Checked)
                        whereClause = whereClause + andClause + " UserGroup like '%" + usersSearchGroup.Text + "%'";
                    else
                        whereClause = whereClause + andClause + " UserGroup like '" + usersSearchGroup.Text + "'";
                    andClause = " and ";
                }
            if (usersSearchIP.Text != null)
                if (usersSearchIP.Text.Length > 0)
                {
                    column = "KnownIPs";
                    searchString = usersSearchIP.Text;
                    if (usersFuzzyIP != null && usersFuzzyIP.Checked)
                        whereClause = whereClause + andClause + " KnownIPs like '%" + usersSearchIP.Text + "%'";
                    else
                        whereClause = whereClause + andClause + " KnownIPs like '" + usersSearchIP.Text + "'";
                    andClause = " and ";
                } query.Text = whereClause;

            List<UserList> users = FindUsers(whereClause, false);

            usersDataList.Rows.Clear();
            usersNewRow = -1;
            foreach (UserList user in users)
            {
                registeredDate = "";
                if (user.Registered != null)
                    if (user.Registered.Length > 0)
                        registeredDate = String.Format("{0:MM/dd/yyyy HH:mm}", DateTime.ParseExact(user.Registered.Replace("T", " "), "yyyy-MM-dd HH:mm:ss", null));
                lastAccessedDate = "";
                if (user.LastAccessed != null)
                    if (user.LastAccessed.Length > 0)
                        lastAccessedDate = String.Format("{0:MM/dd/yyyy HH:mm}", DateTime.ParseExact(user.LastAccessed.Replace("T", " "), "yyyy-MM-dd HH:mm:ss", null));

                knownIPs = "";
                if (user.KnownIPs != null)
                {
                    knownIPs = user.KnownIPs;
                    knownIPs = knownIPs.Replace("\"", "");
                    knownIPs = knownIPs.Replace("\r\n", "");
                    knownIPs = knownIPs.Replace("[", "");
                    knownIPs = knownIPs.Replace("]", "");
                    IPString = knownIPs.Split(',');
                    knownIPs = "";
                    String comma = "";
                    for (int j = 0; j < IPString.Length; j++)
                    {
                        knownIPs = knownIPs + comma + IPString[j].Trim();
                        comma = ", ";
                    }
                }
                hasInventory = false;
                if (user.InventoryCount > 0)
                    hasInventory = true;
                usersDataList.Rows.Add(user.UserName, user.UserGroup, registeredDate, lastAccessedDate, knownIPs, user.Id, hasInventory);
            }

            this.usersDataList.Sort(this.usersDataUser, ListSortDirection.Ascending);
            usersAddUser.Enabled = false;
        }
        private void usersDataList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //check if row index is not selected
            DataGridViewRow row = usersDataList.Rows[e.RowIndex];

            usersKnowIPs.Text = "";
            if (row.Cells[4] == null)
                return;
            if (row.Cells[4].Value == null)
                return;
            usersKnowIPs.Text = row.Cells[4].Value.ToString();

            string groupName = row.Cells[1].Value.ToString();
            usersListPermissions.Items.Clear();

            TShockAPI.Group group = TShock.Groups.GetGroupByName(groupName);

            if (group != null)
            {
                string[] permissions = group.TotalPermissions.ToArray();
                for (int i = 0; i < permissions.Length; i++)
                {
                    ListViewItem item = new ListViewItem(permissions[i]);
                    usersListPermissions.Items.Add(item);
                }
            }
        }
        private void usersDataList_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (!usersDataList.Rows[e.RowIndex].IsNewRow)
            {
                if (e.ColumnIndex == 0)
                    e.Cancel = true;
            }
            usersNewRow = e.RowIndex;
            usersAddUser.Enabled = true;
        }
        private int usersNewRow;
        private void searchUsers_Click(object sender, EventArgs e)
        {
            getUsers();
        }

        private void usersAddUser_Click(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = usersDataList.Rows[usersNewRow];

            string name = "";
            if (selectedRow.Cells[0] != null)
                if (selectedRow.Cells[0].Value != null)
                    name = selectedRow.Cells[0].Value.ToString();

            string group = "";
            if (selectedRow.Cells[1] != null)
                if (selectedRow.Cells[1].Value != null)
                    group = selectedRow.Cells[1].Value.ToString();

            string password = promptPassword("Password for " + name + "?");
            DialogResult usersChoice;
            if (password.Length == 0)
            {
                usersChoice = MessageBox.Show("Invalid password.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (name.Length == 0)
            {
                usersChoice = MessageBox.Show("No name given.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (group.Length == 0)
            {
                usersChoice = MessageBox.Show("No group given.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TShockAPI.DB.User user = new TShockAPI.DB.User();
            user.Name = name;
            user.Group = group;
            user.Password = password;

            try
            {
                TShock.Users.AddUser(user);
                TShock.CharacterDB.SeedInitialData(TShock.Users.GetUser(user));
                TShock.Log.ConsoleInfo(PROGRAMNAME + " added Account " + user.Name + " to group " + user.Group);
            }
            catch (GroupNotExistsException e1)
            {
                Console.WriteLine("Group " + user.Group + " does not exist!");
                usersChoice = MessageBox.Show("Group " + user.Group + " does not exist!", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (UserExistsException e2)
            {
                usersChoice = MessageBox.Show("User " + user.Name + " already exists!", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (UserManagerException e3)
            {
                Console.WriteLine("User " + user.Name + " could not be added, check console for details.");
                usersChoice = MessageBox.Show("User " + user.Name + " could not be added, check console for details.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            getUsers();
        }

        private void usersDataList_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {

            DataGridViewRow row = usersDataList.SelectedRows[0];
            DialogResult usersChoice =
                MessageBox.Show("Are you sure you want to delete " + row.Cells[0].Value.ToString() + "?", PROGRAMNAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // cancel the delete event
            if (usersChoice == DialogResult.No)
                e.Cancel = true;
        }

        private void usersDataList_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            DataGridViewRow row = e.Row;
            TShockAPI.DB.User user = new TShockAPI.DB.User();
            user.Name = row.Cells[0].Value.ToString();

            TShock.Users.RemoveUser(user);
            TShock.Log.ConsoleInfo(PROGRAMNAME + " successfully deleted account: " + Name + ".");
        }

        public static string promptPassword(string text)
        {
            string caption = "What is the password?";
            Form prompt = new Form();
            prompt.Width = 250;
            prompt.Height = 150;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 150 };
            Button confirmation = new Button() { Text = "Ok", Left = 75, Width = 100, Top = 75 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.ShowDialog();
            return textBox.Text;
        }

        #endregion

        #region  Log Tab
        // Log Tab

        private void getLog()
        {
            int lineCount;
            lineCount = int.Parse(logNumberOfLines.Text);

            var directory = new DirectoryInfo(TShock.Config.LogPath);

            String searchPattern = @"(19|20)\d\d[-](0[1-9]|1[012])[-](0[1-9]|[12][0-9]|3[01]).*.log";

            var log = Directory.GetFiles(TShock.Config.LogPath).Where(path => Regex.Match(path, searchPattern).Success).Last();
            String logFile = Path.GetFullPath(log);

            FileStream logFileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader logFileReader = new StreamReader(logFileStream);

            int limit = System.Convert.ToInt32(lineCount);
            var buffor = new Queue<string>(limit);
            while (!logFileReader.EndOfStream)
            {
                string line = logFileReader.ReadLine();
                if (buffor.Count >= limit)
                    buffor.Dequeue();
                buffor.Enqueue(line);
            }

            logDataList.Items.Clear();
            foreach (string line in buffor)
            {
                ListViewItem item = new ListViewItem(line.ToString());
                logDataList.Items.Add(item);

                Console.WriteLine(line.ToString());
            }
            string[] LogLinesEnd = buffor.ToArray();
            // Clean up
            logFileReader.Close();
            logFileStream.Close();

        }

        private void refreshLog_Click(object sender, EventArgs e)
        {
            getLog();
        }
        #endregion
    }
}