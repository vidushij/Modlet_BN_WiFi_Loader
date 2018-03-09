using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace ThinkEco
{
    class Settings
    {
        public static string GSWFWBinFilename = "";
        public static string GSApp1BinFilename = "";
        public static string GSApp2BinFilename = "";
        public static string GSSFInfoBinFilename = "";
        public static string GSWebPagesBinFilename = "";

        public static string FS224binFilename = "";
        public static string FS226binFilename = "";

        public static int  coarseTrimDefaultVal = 0;
        public static int  fineTrimDefaultVal = 0;

        public static bool trimCrystal = false;
        public static int  gpibInterfaceID = 0;
        public static byte freqCounterPrimaryAddr = 0;
        public static byte freqCounterSecondaryAddr = 0;

        public Settings()
        {
        }

        public static void Initialize()
        {
            GSWFWBinFilename = "";
            GSApp1BinFilename = "";
            GSApp2BinFilename = "";
            GSSFInfoBinFilename = "";
            GSWebPagesBinFilename = "";

            FS224binFilename = "";
            FS226binFilename = "";

            coarseTrimDefaultVal = 0;
            fineTrimDefaultVal = 0;

            trimCrystal = false;
            gpibInterfaceID = 0;
            freqCounterPrimaryAddr = 0;
            freqCounterSecondaryAddr = 0; 
        }

        public static void ParseSettings()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Settings.xml");
            XmlNode xmlNode;

            Initialize(); 

            #region Gainspan
            xmlNode = doc.SelectSingleNode("/Settings/Gainspan/Firmware/wlan");
            if (xmlNode != null)
            {
                GSWFWBinFilename = xmlNode.Attributes["name"].Value;
                VerifyMD5(GSWFWBinFilename, xmlNode.Attributes["md5"].Value);
            }

            xmlNode = doc.SelectSingleNode("/Settings/Gainspan/Firmware/app1");
            if (xmlNode != null)
            {
                GSApp1BinFilename = xmlNode.Attributes["name"].Value;
                VerifyMD5(GSApp1BinFilename, xmlNode.Attributes["md5"].Value); 
            }
            xmlNode = doc.SelectSingleNode("/Settings/Gainspan/Firmware/app2");
            if (xmlNode != null)
            {
                GSApp2BinFilename = xmlNode.Attributes["name"].Value;
                VerifyMD5(GSApp2BinFilename, xmlNode.Attributes["md5"].Value); 
            }
            xmlNode = doc.SelectSingleNode("/Settings/Gainspan/SFinfo");
            if (xmlNode != null)
            {
                GSSFInfoBinFilename = xmlNode.Attributes["name"].Value;
                VerifyMD5(GSSFInfoBinFilename, xmlNode.Attributes["md5"].Value); 
            }
            xmlNode = doc.SelectSingleNode("/Settings/Gainspan/WebPages");
            if (xmlNode != null)
            {
                GSWebPagesBinFilename = xmlNode.Attributes["name"].Value;
                VerifyMD5(GSWebPagesBinFilename, xmlNode.Attributes["md5"].Value); 
            }
            #endregion

            #region Freescale
            xmlNode = doc.SelectSingleNode("/Settings/Freescale/MC13224V");
            if (xmlNode != null)
            {
                FS224binFilename = xmlNode.Attributes["name"].Value;
                VerifyMD5(FS224binFilename, xmlNode.Attributes["md5"].Value); 
            }
            xmlNode = doc.SelectSingleNode("/Settings/Freescale/MC13226V");
            if (xmlNode != null)
            {
                FS226binFilename = xmlNode.Attributes["name"].Value;
                VerifyMD5(FS226binFilename, xmlNode.Attributes["md5"].Value);
            }
            #endregion

            #region Crystal
            xmlNode = doc.SelectSingleNode("/Settings/Crystal");
            if (xmlNode != null)
            {
                coarseTrimDefaultVal = Convert.ToInt32(xmlNode.Attributes["coarse"].Value);
                fineTrimDefaultVal = Convert.ToInt32(xmlNode.Attributes["fine"].Value);
            }
            xmlNode = doc.SelectSingleNode("/Settings/Crystal/FrequencyCounter");
            if (xmlNode != null)
            {
                trimCrystal = true;
                gpibInterfaceID = Convert.ToInt32(xmlNode.Attributes["GPIBInterfaceID"].Value);
                freqCounterPrimaryAddr = Convert.ToByte(xmlNode.Attributes["primaryAddr"].Value);
                freqCounterSecondaryAddr = Convert.ToByte(xmlNode.Attributes["secondaryAddr"].Value);
            }
            #endregion
        }

        private static void VerifyMD5(string file, string md5)
        {
            MD5 md5Alg = MD5.Create();
            byte[] data = File.ReadAllBytes(Parameters.binDir + "\\" + file);
            byte[] md5Data = md5Alg.ComputeHash(data);

            string md5String = "";
            for (int i = 0; i < md5Data.Length; i++) md5String += md5Data[i].ToString("x2");

            if (!string.Equals(md5, md5String))
            {
                throw new Exception_STOP("The file " + file + " is corrupted!");
            }
        }
    }
}
