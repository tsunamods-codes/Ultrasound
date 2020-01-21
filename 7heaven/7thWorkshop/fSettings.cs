/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Iros._7th.Workshop {
    public partial class fSettings : Form {
        public fSettings() {
            InitializeComponent();
        }

        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private bool WriteLinkReg(Microsoft.Win32.RegistryKey key) {
            string app = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //Create Prog_ID in Registry so we can associate file types
            //TODO: Add additional subkeys to define an "Unpack" option for IROs
            var progid = key.CreateSubKey("7thHeaven");
            if (progid == null) return false;
            var icon = progid.CreateSubKey("DefaultIcon");
            var shell = progid.CreateSubKey("shell");
            var open = shell.CreateSubKey("open");
            var command = open.CreateSubKey("command");
            progid.SetValue(String.Empty, "7thHeaven Mod File");
            icon.SetValue(String.Empty, "\"" + app + "\"");
            command.SetValue(String.Empty, "\"" + app + "\" /OPENIRO:\"%1\"");

            //Associate .iro mod files with 7H's Prog_ID- .IRO extension
            var iroext = key.CreateSubKey(".iro");
            if (iroext == null) return false;
            iroext.SetValue(String.Empty, "7thHeaven");

            //Refresh Shell/Explorer so icon cache updates
            //do this now because we don't care so much about assoc. URL if it fails
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

            //Associate iros:// URL with 7H
            var iros = key.CreateSubKey("iros");
            if (iros == null) return false;
            icon = iros.CreateSubKey("DefaultIcon");
            shell = iros.CreateSubKey("shell");
            open = shell.CreateSubKey("open");
            command = open.CreateSubKey("command");
            iros.SetValue(String.Empty, "7H Catalog Subscription");
            icon.SetValue(String.Empty, "\"" + app + "\"");
            command.SetValue(String.Empty, "\"" + app + "\" \"%1\"");
            return true;
        }

        private bool WriteLinkReg() {
            var key = Microsoft.Win32.Registry.ClassesRoot;
            bool global = WriteLinkReg(key);
            if (!global) {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes");
                global = WriteLinkReg(key);
            }
            return global;
        }

        private void fSettings_Load(object sender, EventArgs e) {

            String registry_path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Square Soft, Inc.\Final Fantasy VII";

            if (String.IsNullOrEmpty(Sys.Settings.FF7Exe)) {
                if (MessageBox.Show("Would you like to load default settings and autodetect your FF7 game folders?", "No Settings Configured", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    string ff7 = (string)Microsoft.Win32.Registry.GetValue(registry_path, "AppPath", null);

                    if (!String.IsNullOrEmpty(ff7))
                    {
                        // ff7 = Regex.Escape(ff7);
                        Sys.Settings.AaliFolder = ff7 + @"mods\Textures\";
                        Sys.Settings.FF7Exe = ff7 + @"FF7.exe";

                        Sys.Settings.MovieFolder = (string)Microsoft.Win32.Registry.GetValue(registry_path, "MoviePath", null);
                   
                        Sys.Settings.LibraryLocation = ff7 + @"mods\7th Heaven\";

                        Sys.Settings.ExtraFolders.Add("direct");
                        Sys.Settings.ExtraFolders.Add("music");

                    }
                    else
                    {
                        MessageBox.Show("Could not determine ff7.exe path. Is the game installed?", "Error");
                    }
                }
            }
            
            txtSubscriptions.Lines = Sys.Settings.SubscribedUrls.ToArray();
            txtExtraFolders.Lines = Sys.Settings.ExtraFolders.ToArray();
            txtLibrary.Text = Sys.Settings.LibraryLocation;
            txtFF7.Text = Sys.Settings.FF7Exe;
            txtMovie.Text = Sys.Settings.MovieFolder;
            txtAlsoLaunch.Lines = Sys.Settings.AlsoLaunch.ToArray();
            txtAali.Text = Sys.Settings.AaliFolder;

            for (int i = 0; i < clOptions.Items.Count; i++)
                clOptions.SetItemChecked(i, (((int)Sys.Settings.Options) & (1 << i)) != 0);

            if (Sys.Settings.VersionUpgradeCompleted < Sys.Version) {
                if (String.IsNullOrWhiteSpace(Sys.Settings.MovieFolder)) {
                    Sys.Settings.MovieFolder = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Square Soft, Inc.\Final Fantasy VII", "MoviePath", null);
                }

                if (MessageBox.Show("Would you like 7th Heaven to import IROs you open from Windows and iros:// catalog subscription links from the web?", "Associate Files and Links", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    try {
                        if (!WriteLinkReg()) throw new Exception("Could not create keys");
                    } catch (Exception ex) {
                        MessageBox.Show("Unable to register links: " + ex.ToString());
                    }
                }
            }

            txtMovie.Text = Sys.Settings.MovieFolder;
            Sys.Settings.VersionUpgradeCompleted = Sys.Version;
        }

        private void bOK_Click(object sender, EventArgs e) {

            Sys.Settings.SubscribedUrls = txtSubscriptions.Lines.ToList();

            //Convert Extra Folders textbox to a list for comparison and editing
            List<string> ExtraFoldersList = txtExtraFolders.Lines.ToList();

            //Make sure that the direct and music folders are always present
            if (!ExtraFoldersList.Contains("direct", StringComparer.InvariantCultureIgnoreCase)) ExtraFoldersList.Add("direct");
            if (!ExtraFoldersList.Contains("music", StringComparer.InvariantCultureIgnoreCase)) ExtraFoldersList.Add("music");
            
            //Save the Extra Folders and remove any duplicate entries for best performance
            Sys.Settings.ExtraFolders = ExtraFoldersList.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();

            Sys.Settings.AlsoLaunch = txtAlsoLaunch.Lines.ToList();

            Sys.Settings.FF7Exe = txtFF7.Text;
            Sys.Settings.AaliFolder = txtAali.Text;
            Sys.Settings.LibraryLocation = txtLibrary.Text;
            Sys.Settings.MovieFolder = txtMovie.Text;

            int opts = 0;
            for (int i = 0; i < clOptions.Items.Count; i++)
                if (clOptions.GetItemChecked(i)) opts |= (1 << i);
            Sys.Settings.Options = (GeneralOptions)opts;

            // Clear EXE compatibility flags if user opts out
            if (!Sys.Settings.Options.HasFlag(GeneralOptions.SetEXECompatFlags))
            {
                RegistryKey ff7CompatKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                if (ff7CompatKey.GetValue(Sys.Settings.FF7Exe) != null) ff7CompatKey.DeleteValue(Sys.Settings.FF7Exe);
            }

            if (!Sys.Settings.FF7Exe.Any())
            {
                MessageBox.Show("Missing exe path");
            }

            if (!Sys.Settings.AaliFolder.Any())
            {
                MessageBox.Show("Missing Aali OpenGL");
            }

            if (!Sys.Settings.MovieFolder.Any())
            {
                MessageBox.Show("Missing Movie path");
            }

            if (!Sys.Settings.LibraryLocation.Any())
            {
                MessageBox.Show("Missing Library path");
            }
            else
            {
                System.IO.Directory.CreateDirectory(Sys.Settings.LibraryLocation);
            }

        }

        private void bLibrary_Click(object sender, EventArgs e) {
            if (flBrowser.ShowDialog() == DialogResult.OK)
                txtLibrary.Text = flBrowser.SelectedPath;
        }

        private void bAali_Click(object sender, EventArgs e) {
            if (flBrowser.ShowDialog() == DialogResult.OK)
                txtAali.Text = flBrowser.SelectedPath;
        }

        private void bFF7_Click(object sender, EventArgs e) {
            if (ofExe.ShowDialog() == DialogResult.OK)
                txtFF7.Text = ofExe.FileName;
        }

        private void bAlsoLaunch_Click(object sender, EventArgs e) {
            if (ofExe.ShowDialog() == DialogResult.OK)
                txtAlsoLaunch.Text = ofExe.FileName;
        }

        private void bMovie_Click(object sender, EventArgs e) {
            if (flBrowser.ShowDialog() == DialogResult.OK)
                txtMovie.Text = flBrowser.SelectedPath;
        }
    }
}
