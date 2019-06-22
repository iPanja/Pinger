using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Pinger{
    public partial class Form1 : MetroFramework.Forms.MetroForm {
        Stopwatch stopwatch = new Stopwatch();
        Timer ulTimer = new Timer();
        Timer pingTimer = new Timer();
        TimeSpan? lastFail = new TimeSpan?();
        int failCounter = 0;

        //Config
        int timeout;
        int interval;
        Dictionary<string, string> servers = new Dictionary<string, string>();

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            loadCondig();            
        }
        private void settingsButton_Click(object sender, EventArgs e) {
            SettingsForm settingsForm = new SettingsForm(this);
            settingsForm.ShowDialog();
        }
        //Functions
        public void updateLabels(object sender, EventArgs e) {
            mttLabel.Text = stopwatch.Elapsed.ToString("hh\\:mm\\:ss");
            if (lastFail.HasValue) {
                lftLabel.Text = (stopwatch.Elapsed - lastFail).Value.ToString("hh\\:mm\\:ss");
                fccLabel.Text = failCounter.ToString();
            }

        }
        public void pingServers(object sender, EventArgs e) {
            handleServer(servers.ElementAt(0).Value, s1pLabel, s1Picture);
            handleServer(servers.ElementAt(1).Value, s2pLabel, s2Picture);
            handleServer(servers.ElementAt(2).Value, s3pLabel, s3Picture);
        }
        public void handleServer(string ip, MetroLabel label, PictureBox picture) {
            Ping ping = new Ping();
            ping.PingCompleted += new PingCompletedEventHandler(pingCallback);

            //Mandatory parameters that I do not care about
            byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            PingOptions options = new PingOptions(64, true);

            try {
                ping.SendAsync(ip, timeout, buffer, options, new Tuple<MetroLabel, PictureBox>(label, picture));
            }
            catch (PingException) {
                Console.Out.WriteLine("RIP");
            }
        }
        public void pingCallback(object sender, PingCompletedEventArgs e) {
            string delay = e.Reply.RoundtripTime.ToString();
            Tuple<MetroLabel, PictureBox> ui = (Tuple<MetroLabel, PictureBox>) e.UserState;
            if (e.Reply.Status == IPStatus.Success) {
                ui.Item2.BackgroundImage = Image.FromFile("checkmark.png");
                ui.Item1.Text = delay + " ms";
            }
            else {
                ui.Item2.BackgroundImage = Image.FromFile("x.png");
                ui.Item1.Text = "ERR";
                lastFail = stopwatch.Elapsed;
                failCounter++;
            }
        }
        public void loadCondig() {
            //Load Config
            timeout = Pinger.Properties.Settings.Default.timeout;
            interval = Pinger.Properties.Settings.Default.interval;

            itLabel.Text = (interval / 1000) + " sec";
            ttLabel.Text = (timeout / 1000) + " sec";

            //Setup Timers
            ulTimer.Interval = 1000;
            ulTimer.Tick += new EventHandler(updateLabels);
            stopwatch.Start();
            ulTimer.Start();

            pingTimer.Interval = interval;
            pingTimer.Tick += new EventHandler(pingServers);
            pingTimer.Start();

            try {
                servers = JsonConvert.DeserializeObject<Dictionary<string, string>>(Pinger.Properties.Settings.Default.servers);
                s1nLabel.Text = servers.ElementAt(0).Key + " (" + servers.ElementAt(0).Value + ")";
                s2nLabel.Text = servers.ElementAt(1).Key + " (" + servers.ElementAt(1).Value + ")";
                s3nLabel.Text = servers.ElementAt(2).Key + " (" + servers.ElementAt(2).Value + ")";
            }
            catch (Exception ex) {
                Console.Out.WriteLine("ERROR: Could not convert string to type Dictionary<string, string> via Newtonsoft Json Convert");
                Console.Out.WriteLine(ex.ToString());
            };
        }

        private void siteButton_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("http://www.github.com/iPanja/Pinger");
        }
    }
}
