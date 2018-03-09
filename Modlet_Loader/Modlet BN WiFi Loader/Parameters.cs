using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;

namespace ThinkEco
{
    class Parameters
    {
        #region Global Constants
        public const string pwd = "Amadeus1756";
        public const string binDir = "bin";
        public const string libDir = "lib";
        public const string logDir = "log";
        public const string FSsslFilename = "ModletBNWiFi_Loader_SSL_x.bin";
        public const string gsWfwProgBin = "WFW_GS1011_RECOVERY_WD_DISABLE.bin";
        public const string gsFacdefExe = "gs_facdef.exe";
        public const string gsProgExe = "gs_flashprogram.exe";
        public const string gsSfpExe = "gs_sfp.exe";
        public const string facDefTmp = "facDef"; 
        public const string logFile = "ModletBNWiFiLoadLog.csv";
        public const string bakFile = "ModletBNWiFiLoadLog.bak";
        #endregion

        #region Label Information
        public static string fsModelNum;
        public static string fsSerialNum;
        public static string fsMac;
        public static string fsMfgDate;
        public static string fsPartNum;
        public static string fsDummy;

        public static string gsModelNum;
        public static string gsSerialNum;
        public static string gsMac;
        public static string gsMfgDate;
        public static string gsPartNum;
        #endregion

        #region Runtime Parameters
        public static DateTime startTime;
        public static string operation;
        public static int coarseTrim;
        public static int fineTrim;
        public static int freqError; 
        public static string FSbinFilename;
        public static byte[] fsHwParam;
        public static string gsHwParam; 
        public static string ftdiSerialNum;
        public static string freqCounterId; 
        public static string CurrExecDir;
        #endregion
        
        public Parameters()
        {
        }

        public static void Initialize(string op)
        {
            startTime = DateTime.Now;
            operation = op;
            coarseTrim = 0;
            fineTrim = 0;
            freqError = 0; 
            FSbinFilename = "";
            fsHwParam = new byte[0];
            gsHwParam = ""; 
            ftdiSerialNum = "";
            freqCounterId = "";
            CurrExecDir = Directory.GetCurrentDirectory();
        }

        public static void setFsHwParam(bool MC13224)
        {
            byte[] pin = new byte[4];
            RandomNumberGenerator randGen = RandomNumberGenerator.Create();
            randGen.GetBytes(pin);

            FSbinFilename = (MC13224) ? Settings.FS224binFilename : Settings.FS226binFilename;

            fsHwParam = new byte[34]; 
            fsHwParam[ 0] = 0x06; // deviceID;
            fsHwParam[ 1] = (MC13224) ? (byte)0x00 : (byte)0x01; // majorVersion;
            fsHwParam[ 2] = 0x00; // minorVersion;
            fsHwParam[ 3] = Convert.ToByte(fsSerialNum.Substring(6, 2), 16);
            fsHwParam[ 4] = Convert.ToByte(fsSerialNum.Substring(4, 2), 16);
            fsHwParam[ 5] = Convert.ToByte(fsSerialNum.Substring(2, 2), 16);
            fsHwParam[ 6] = Convert.ToByte(fsSerialNum.Substring(0, 2), 16);
            fsHwParam[ 7] = (MC13224) ? (byte)0x00 : (byte)0x01; // zigbeeSocID;
            fsHwParam[ 8] = 0x00; // powerChipID;
            fsHwParam[ 9] = 0x00; // Igain[0];
            fsHwParam[10] = 0x00; // Igain[1];
            fsHwParam[11] = 0x40; // Igain[2];
            fsHwParam[12] = 0xD7; // Vgain[0];
            fsHwParam[13] = 0xA3; // Vgain[1];
            fsHwParam[14] = 0x40; // Vgain[2];
            fsHwParam[15] = 0x00; // FlashID;
            fsHwParam[16] = 0x10; // relayID;
            fsHwParam[17] = 0x00; // RLdelay[0];
            fsHwParam[18] = 0x00; // RLdelay[1];
            fsHwParam[19] = 0x00; // sslID;
            fsHwParam[20] = pin[0];
            fsHwParam[21] = pin[1];
            fsHwParam[22] = pin[2];
            fsHwParam[23] = pin[3];
            fsHwParam[24] = Convert.ToByte(fsMac.Substring(14, 2), 16);
            fsHwParam[25] = Convert.ToByte(fsMac.Substring(12, 2), 16);
            fsHwParam[26] = Convert.ToByte(fsMac.Substring(10, 2), 16);
            fsHwParam[27] = Convert.ToByte(fsMac.Substring( 8, 2), 16);
            fsHwParam[28] = Convert.ToByte(fsMac.Substring( 6, 2), 16);
            fsHwParam[29] = Convert.ToByte(fsMac.Substring( 4, 2), 16);
            fsHwParam[30] = Convert.ToByte(fsMac.Substring( 2, 2), 16);
            fsHwParam[31] = Convert.ToByte(fsMac.Substring( 0, 2), 16);
            fsHwParam[32] = (byte)coarseTrim;
            fsHwParam[33] = (byte)fineTrim;
        }

        public static void setGsHwParam()
        {
            gsHwParam =
                "1  " + Parameters.gsMac.ToUpper() + "\r\n" +                               // MAC
                "2  modletBN_WiFi_" + Parameters.gsMac.ToUpper().Substring(6) + "\r\n" +    // SSID
                "3  6\r\n" +                                                                // Channel
                "4  0\r\n" +                                                                // Security Type
                "5  1\r\n" +                                                                // WEP ID
                "6  aabbccddee\r\n" +                                                       // WEP Key
                "7  GSDemo123\r\n" +                                                        // Passphrase
                "8  admin\r\n" +                                                            // Username
                "9  admin\r\n" +                                                            // Password
                "10 GainSpan\r\n" +                                                         // Manufacturer
                "11 GS1011M\r\n" +                                                          // Model
                "12 1011\r\n" +                                                             // Model Type
                "13 GainSpan WiFi Module\r\n" +                                             // Device Name
                "14 gainspan";                                                              // Host Name

            using (StreamWriter writer = new StreamWriter(libDir + "\\" + facDefTmp + ".txt", false))
            {
                writer.Write(gsHwParam);
            }
        }

        public static void LogInfo(string result, string msg)
        {
            bool putHdrCsv = false;
            bool putHdrBak = false;

            if (!File.Exists(logDir + "\\" + logFile)) putHdrCsv = true;
            if (!File.Exists(logDir + "\\" + bakFile)) putHdrBak = true;

            string hdrtxt =
                "Program Version" + "," +
                "Computer Name" + "," +
                "Installation Directory" + "," +
                "Interface Board S/N" + "," +
                "Frequency Counter ID" + "," +
                "Operation" + "," +
                "Start Time" + "," +
                "Duration" + "," +
                "Main Label" + "," +
                "WiFi Label" + "," +
                "Gainspan wlan" + "," +
                "Gainspan app1" + "," +
                "Gainspan app2" + "," +
                "Gainspan SFinfo" + "," +
                "Gainspan WebPages" + "," +
                "Freescale Bin" + "," +
                "Freescale SSL" + "," +
                "Trimming" + "," +
                "cTrim" + "," +
                "fTrim" + "," +
                "error" + "," +
                "Freescale Hardware Parameters" + "," +
                "Gainspan Hardware Parameters" + "," +
                "Result" + "," +
                "Message" + "," +
                "Signature";

            string logtxt =
                Convert.ToString(Assembly.GetExecutingAssembly().GetName().Version.Major) + "." + Convert.ToString(Assembly.GetExecutingAssembly().GetName().Version.Minor) + "," +
                Environment.MachineName.Replace(',', ';') + "," +
                Directory.GetCurrentDirectory() + "," +
                ftdiSerialNum + "," +
                freqCounterId + "," +
                operation + "," +
                startTime.ToString() + "," +
                DateTime.Now.Subtract(startTime).TotalSeconds.ToString() + "," +
                fsModelNum + " " + fsSerialNum + " " + fsMac + " " + fsMfgDate + " " + fsPartNum + " " + fsDummy + "," +
                gsModelNum + " " + gsSerialNum + " " + gsMac + " " + gsMfgDate + " " + gsPartNum + "," +
                Settings.GSWFWBinFilename + "," +
                Settings.GSApp1BinFilename + "," +
                Settings.GSApp2BinFilename + "," +
                Settings.GSSFInfoBinFilename + "," +
                Settings.GSWebPagesBinFilename + "," +
                FSbinFilename + "," +
                FSsslFilename + "," +
                Settings.trimCrystal.ToString() + "," +
                coarseTrim.ToString() + "," +
                fineTrim.ToString() + "," +
                freqError.ToString() + ",";

            for (int i = 0; i < fsHwParam.Length; i++) logtxt += fsHwParam[i].ToString("x2");
            logtxt += ",";

            logtxt += gsHwParam;
            logtxt += ","; 

            logtxt += result + "," + msg + ",";

            logtxt = logtxt.Replace("\r", "\\r").Replace("\n", "\\n");

            MD5 md5Alg = MD5.Create();
            byte[] md5 = md5Alg.ComputeHash(Encoding.ASCII.GetBytes(logtxt));
            byte[] signature = EncDec.Encrypt(md5, pwd);
            for (int i = 0; i < signature.Length; i++) logtxt += signature[i].ToString("x2"); 

            using (StreamWriter writer = new StreamWriter(logDir + "\\" + logFile, true))
            {
                if (putHdrCsv) writer.WriteLine(hdrtxt);
                writer.WriteLine(logtxt);
            }
            using (StreamWriter writer = new StreamWriter(logDir + "\\" + bakFile, true))
            {
                if (putHdrBak) writer.WriteLine(hdrtxt);
                writer.WriteLine(logtxt);
            }
        }
    }
}
