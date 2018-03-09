using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Globalization; 

namespace ThinkEco
{
    public partial class MainForm : Form
    {
        static MainForm mainFormRef;
        static SynchronizationContext mainFormSync;

        public MainForm()
        {
            mainFormRef = this;
            mainFormSync = SynchronizationContext.Current;

            InitializeComponent();

            selectMode.SelectedIndex = 0;

            ScanMainLabelGui("Insert a modlet BN Wifi into the fixture. Then scan the main label"); 
        }

        public void ScanMainLabelGui(string info)
        {
            selectMode.Enabled = true;

            mainLabel.Text = "";
            mainLabel.Enabled = true;
            mainLabel.Focus();

            wifiLabel.Text = "";
            wifiLabel.Enabled = false;

            progressBar.Value = 0;

            userInfo.Text = info;

            labelPASS.BackColor = Color.LightGray;
            labelPASS.Enabled = false;

            labelFAIL.BackColor = Color.LightGray;
            labelFAIL.Enabled = false;
            labelFAIL.Text = "FAIL";
        }

        public void ScanWiFiLabelGui()
        {
            selectMode.Enabled = true;

            mainLabel.Enabled = false;

            wifiLabel.Text = "";
            wifiLabel.Enabled = true;
            wifiLabel.Focus();

            progressBar.Value = 0;

            userInfo.Text = "Now scan the WiFi label";

            labelPASS.BackColor = Color.LightGray;
            labelPASS.Enabled = false;

            labelFAIL.BackColor = Color.LightGray;
            labelFAIL.Enabled = false;
            labelFAIL.Text = "FAIL";
        }

        public void ProcessRunningGui(int progress, string info)
        {
            selectMode.Enabled = false;

            mainLabel.Enabled = false;

            wifiLabel.Enabled = false;

            progressBar.Value = progress;

            userInfo.Text = info;

            labelPASS.BackColor = Color.LightGray;
            labelPASS.Enabled = false;

            labelFAIL.BackColor = Color.LightGray;
            labelFAIL.Enabled = false;
            labelFAIL.Text = "FAIL";
        }

        public void FailResultGui(string info)
        {
            selectMode.Enabled = true;

            mainLabel.Text = "";
            mainLabel.Enabled = true;
            mainLabel.Focus();

            wifiLabel.Text = "";
            wifiLabel.Enabled = false;

            progressBar.Value = 0;

            userInfo.Text = info;

            labelPASS.BackColor = Color.LightGray;
            labelPASS.Enabled = false;

            labelFAIL.BackColor = Color.Red;
            labelFAIL.Enabled = true;
            labelFAIL.Text = "FAIL";
        }

        public void StopResultGui(string info)
        {
            selectMode.Enabled = true;

            mainLabel.Text = "";
            mainLabel.Enabled = true;
            mainLabel.Focus();

            wifiLabel.Text = "";
            wifiLabel.Enabled = false;

            progressBar.Value = 0;

            userInfo.Text = info;

            labelPASS.BackColor = Color.LightGray;
            labelPASS.Enabled = false;

            labelFAIL.BackColor = Color.Yellow;
            labelFAIL.Enabled = true;
            labelFAIL.Text = "STOP";
        }

        public void PassResultGui()
        {
            selectMode.Enabled = true;

            mainLabel.Text = "";
            mainLabel.Enabled = true;
            mainLabel.Focus();

            wifiLabel.Text = "";
            wifiLabel.Enabled = false;

            progressBar.Value = 0;

            userInfo.Text = "Insert the next modlet BN Wifi into the fixture. Then scan the main label";

            labelPASS.BackColor = Color.LimeGreen;
            labelPASS.Enabled = true;

            labelFAIL.BackColor = Color.LightGray;
            labelFAIL.Enabled = false;
            labelFAIL.Text = "FAIL";
        }

        private void selectModeChanged(object sender, EventArgs e)
        {
            ScanMainLabelGui("Insert the next modlet BN Wifi into the fixture. Then scan the main label");
        }

        private void mainLabelHandler(object sender, KeyPressEventArgs e)
        {
            // Example string: "TE5010 10000000 804F580000600123 20140506 0050.0001.0000 00000000"

            // Wait until we have full string 
            if (e.KeyChar == ' ' || mainLabel.Text.Count(x => x != ' ') + 1 < 60) return;

            // Last char is not yet contained in text box 
            string allText = mainLabel.Text + e.KeyChar.ToString();

            // Split string into constituent fields 
            string[] fields = allText.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Must have 6 fields 
            if (fields.Count() != 6)
            {
                ScanMainLabelGui("Invalid main label");
                return;
            }

            // Verify model number is valid 
            if (fields[0] != "TE5010" && fields[0] != "TE5011" && fields[0] != "TE5012" && fields[0] != "TE5013")
            {
                ScanMainLabelGui("Invalid main label model number");
                return;
            }

            // Verify S/N is 8 digit uppercase hex string 
            if (fields[1].Count() != 8 || !fields[1].All(c => c >= '0' && c <= '9' || c >= 'A' && c <= 'F'))
            {
                ScanMainLabelGui("Invalid main label serial number");
                return;
            }

            // Check the manufacturer digit in the S/N 
            if (fields[1][0] < '1' || fields[1][0] > '6')
            {
                ScanMainLabelGui("Invalid manufacturer digit in S/N");
                return;
            }

            // Check the Freescale chip digit in the S/N
            if (fields[1][1] != '0' && fields[1][1] != '1')
            {
                ScanMainLabelGui("Invalid Freescale chip digit in S/N");
                return;
            }

            // For manufacturer 6 we switched to "decimal" S/N 
            if (fields[1][0] == '6' && !fields[1].All(char.IsDigit))
            {
                ScanMainLabelGui("Invalid (non-decimal) digits in S/N");
                return;
            }

            // Verify ZigBee MAC is 16 digits, in the ThinkEco IEEE domain & BN Wifi and a caps hex string
            if (fields[2].Count() != 16 || !fields[2].StartsWith("804F5800006") || !fields[2].All(c => c >= '0' && c <= '9' || c >= 'A' && c <= 'F'))
            {
                ScanMainLabelGui("Invalid main label ZigBee MAC");
                return;
            }

            // Verify date field length 
            if (fields[3].Count() != 8)
            {
                ScanMainLabelGui("Invalid main label date code");
                return;
            }

            // Verify ThinkEco P/N 
            if (fields[4] != "0050.000" + fields[1][0] + ".0" + fields[0][5] + "00")
            {
                ScanMainLabelGui("Invalid main label part number");
                return;
            }

            // Check dummy field is zero
            if (fields[5] != "00000000")
            {
                ScanMainLabelGui("Invalid main label dummy field");
                return;
            }

            // Accept product main label string 
            Parameters.fsModelNum = fields[0];
            Parameters.fsSerialNum = fields[1];
            Parameters.fsMac = fields[2];
            Parameters.fsMfgDate = fields[3];
            Parameters.fsPartNum = fields[4];
            Parameters.fsDummy = fields[5];

            // GUI for WiFi scan
            ScanWiFiLabelGui(); 
        }

        private void wifiLabelHandler(object sender, KeyPressEventArgs e)
        {
            // Example string: "GS1011MEPS 00000000 20F85EA1137D 20140506 0100.1085.1000"

            // Wait until we have full string 
            if (e.KeyChar == ' ' || wifiLabel.Text.Count(x => x != ' ') + 1 < 52) return;

            // Last char is not yet contained in text box 
            string allText = wifiLabel.Text + e.KeyChar.ToString();

            // Split string into constituent fields 
            string[] fields = allText.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Must have 5 fields 
            if (fields.Count() != 5)
            {
                ScanMainLabelGui("Invalid WiFi label"); 
                return;
            }

            // Verify model number is valid 
            if (fields[0] != "GS1011MEPS")
            {
                ScanMainLabelGui("Invalid WiFi label model number");
                return;
            }

            // Unused S/N should be zero 
            if (fields[1] != "00000000")
            {
                ScanMainLabelGui("Invalid WiFi label serial number"); 
                return;
            }

            // Verify WiFi MAC is a 12 digit uppercase hex string 
            if (fields[2].Count() != 12 || !fields[2].All(c => c >= '0' && c <= '9' || c >= 'A' && c <= 'F'))
            {
                ScanMainLabelGui("Invalid WiFi label WiFi MAC"); 
                return;
            }

            // Verify date field length 
            if (fields[3].Count() != 8)
            {
                ScanMainLabelGui("Invalid WiFi label date code"); 
                return;
            }

            // Verify ThinkEco P/N is correct
            if (fields[4] != "0100.1085.1000")
            {
                ScanMainLabelGui("Invalid WiFi label part number"); 
                return;
            }

            // Accept product WiFi label string 
            Parameters.gsModelNum = fields[0];
            Parameters.gsSerialNum = fields[1];
            Parameters.gsMac = fields[2];
            Parameters.gsMfgDate = fields[3];
            Parameters.gsPartNum = fields[4];

            // Progress GUI 
            ProcessRunningGui(0, ""); 
            
            // Start the process 
            MainProcess mainProcess = new MainProcess(mainFormRef, mainFormSync);

            if (selectMode.SelectedIndex == 0)
            {
                Thread thread = new Thread(mainProcess.LoadFirmware);
                thread.Start();
            }
            else
            {
                Thread thread = new Thread(mainProcess.EraseFirmware);
                thread.Start();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                /* All form closing activity can go here */
            }
            catch
            {
                /* Any exceptions that happen on cleanup */
            }

            if (e.CloseReason == CloseReason.TaskManagerClosing) return;
            if (e.CloseReason == CloseReason.WindowsShutDown) return;
        }
    }
}
