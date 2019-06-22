using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using MetroFramework.Controls;

namespace Pinger {
    public partial class SettingsForm : MetroFramework.Forms.MetroForm {
        Form1 mainWindow;
        Dictionary<string, string> servers;
        int interval;
        int timeout;

        static Dictionary<MetroTextBox, MetroTextBox> sUI;

        public SettingsForm(Form1 mw) {
            InitializeComponent();
            mainWindow = mw;
            sUI = new Dictionary<MetroTextBox, MetroTextBox> {
                { s1nTextbox, s1IpTextbox },
                { s2nTextbox, s2IpTextbox },
                { s3nTextbox, s3IpTextbox }
            };
        }

        private void SettingsForm_Load(object sender, EventArgs e) {
            timeout = Pinger.Properties.Settings.Default.timeout;
            interval = Pinger.Properties.Settings.Default.interval;

            iTextbox.Text = (interval / 1000).ToString();
            tTextbox.Text = (timeout / 1000).ToString();
            try {
                servers = JsonConvert.DeserializeObject<Dictionary<string, string>>(Pinger.Properties.Settings.Default.servers);
            }
            catch (Exception ex) {
                Console.Out.WriteLine("ERROR: Could not convert string to type Dictionary<string, string> via Newtonsoft Json Convert");
                Console.Out.WriteLine(ex.ToString());
            };

            for(int i = 0; i < 3; i++) {
                KeyValuePair<MetroTextBox, MetroTextBox> ui = sUI.ElementAt(i);
                KeyValuePair<string, string> server = servers.ElementAt(i);

                ui.Key.Text = server.Key;
                ui.Value.Text = server.Value; 
            }
        }

        private void metroButton1_Click(object sender, EventArgs e) {
            Dictionary<string, string> servers = new Dictionary<string, string>();
            for(int i = 0; i < 3; i++) {
                KeyValuePair<MetroTextBox, MetroTextBox> ui = sUI.ElementAt(i);
                servers.Add(ui.Key.Text, ui.Value.Text);    
            }
            Pinger.Properties.Settings.Default.servers = JsonConvert.SerializeObject(servers);

            try {
                interval = Convert.ToInt32(iTextbox.Text) * 1000;
                timeout = Convert.ToInt32(tTextbox.Text) * 1000;
            }catch(Exception) {
                MessageBox.Show("The specified interval/timeout duration is not a number");
            }

            Pinger.Properties.Settings.Default.interval = interval;
            Pinger.Properties.Settings.Default.timeout = timeout;
            Pinger.Properties.Settings.Default.Save();
            MessageBox.Show("Settings Saved");
            mainWindow.loadCondig();
        }
    }
}
