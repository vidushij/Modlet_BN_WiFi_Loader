using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 
using System.Threading;

namespace ThinkEco
{
    public class MainProcess
    {
        readonly MainForm mfRef;
        readonly SynchronizationContext mfSync;

        private FreescaleInterface freescaleInterface;
        private GainspanInterface gainspanInterface;

        public MainProcess(MainForm mf, SynchronizationContext sync)
        {
            mfRef = mf;
            mfSync = sync;
        }

        public void LoadFirmware()
        {
            try
            {
                Parameters.Initialize("LOAD");

                Settings.ParseSettings();

                #region Programming Freescale
                mfSync.Send(state => mfRef.ProcessRunningGui(0, "Erasing Freescale flash"), null);
                byte[] ssl = File.ReadAllBytes(Parameters.libDir + "\\" + Parameters.FSsslFilename);
                freescaleInterface = new FreescaleInterface(ssl);

                mfSync.Send(state => mfRef.ProcessRunningGui(5, "Trimming crystal"), null);
                Trimmer trimmer = new Trimmer(freescaleInterface);
                trimmer.Run();

                mfSync.Send(state => mfRef.ProcessRunningGui(10, "Programming Freescale hardware parameters"), null);
                Parameters.setFsHwParam(freescaleInterface.MC13224V);
                freescaleInterface.WriteHwParams(Parameters.fsHwParam);

                mfSync.Send(state => mfRef.ProcessRunningGui(15, "Programming Freescale firmware"), null);
                byte[] firmware = File.ReadAllBytes(Parameters.binDir + "\\" + Parameters.FSbinFilename);
                freescaleInterface.WriteFirmware(firmware);

                mfSync.Send(state => mfRef.ProcessRunningGui(25, "Freescale chip programmed successfully"), null);
                freescaleInterface.Close();
                freescaleInterface = null; 
                #endregion

                #region Programming Gainspan
                gainspanInterface = new GainspanInterface();

                gainspanInterface.SetProgramMode();
                mfSync.Send(state => mfRef.ProcessRunningGui(25, "Erasing Gainspan internal flash"), null);
                gainspanInterface.EraseInternalFlash();
                mfSync.Send(state => mfRef.ProcessRunningGui(25, "Programming Gainspan sfp WLAN binary"), null);
                gainspanInterface.ProgramWlanFw(Parameters.CurrExecDir + "\\" + Parameters.libDir + "\\" + Parameters.gsWfwProgBin);

                gainspanInterface.SetRunMode();
                mfSync.Send(state => mfRef.ProcessRunningGui(40, "Erasing external flash"), null);
                gainspanInterface.EraseExternalFlash();
                mfSync.Send(state => mfRef.ProcessRunningGui(60, "Programming file system"), null);
                gainspanInterface.ProgramSfInfo(Parameters.CurrExecDir + "\\" + Parameters.binDir + "\\" + Settings.GSSFInfoBinFilename);
                mfSync.Send(state => mfRef.ProcessRunningGui(60, "Programming webpages"), null);
                gainspanInterface.ProgramWebpages(Parameters.CurrExecDir + "\\" + Parameters.binDir + "\\" + Settings.GSWebPagesBinFilename);

                gainspanInterface.SetProgramMode();
                mfSync.Send(state => mfRef.ProcessRunningGui(70, "Erasing Gainspan flash"), null);
                gainspanInterface.EraseInternalFlash();
                mfSync.Send(state => mfRef.ProcessRunningGui(70, "Programming Gainspan WLAN firmware"), null);
                gainspanInterface.ProgramWlanFw(Parameters.CurrExecDir + "\\" + Parameters.binDir + "\\" + Settings.GSWFWBinFilename);
                mfSync.Send(state => mfRef.ProcessRunningGui(80, "Programming Gainspan APP firmware"), null);
                gainspanInterface.ProgramAppFw(Parameters.CurrExecDir + "\\" + Parameters.binDir + "\\" + Settings.GSApp1BinFilename, 
                                               Parameters.CurrExecDir + "\\" + Parameters.binDir + "\\" + Settings.GSApp2BinFilename);

                Parameters.setGsHwParam();
                mfSync.Send(state => mfRef.ProcessRunningGui(99, "Programming Gainspan factory settings"), null);
                gainspanInterface.ProgramFactDef(Parameters.CurrExecDir + "\\" + Parameters.libDir + "\\" + Parameters.facDefTmp + ".txt", 
                                                 Parameters.CurrExecDir + "\\" + Parameters.libDir + "\\" + Parameters.facDefTmp + ".bin");

                mfSync.Send(state => mfRef.ProcessRunningGui(100, "Gainspan module programmed successfully"), null);
                gainspanInterface.Close();
                gainspanInterface = null; 
                #endregion

                Parameters.LogInfo("PASS", "");
                mfSync.Send(state => mfRef.PassResultGui(), null);
            }
            catch (Exception_FAIL ex)
            {
                GracefulExit(); 

                Parameters.LogInfo("FAIL", ex.Message);
                mfSync.Send(state => mfRef.FailResultGui(ex.Message), null);
            }
            catch (Exception_STOP ex)
            {
                GracefulExit(); 

                Parameters.LogInfo("STOP", ex.Message);
                mfSync.Send(state => mfRef.StopResultGui(ex.Message), null);
            }
            catch (Exception ex)
            {
                GracefulExit(); 

                Parameters.LogInfo("STOP", ex.Message);
                mfSync.Send(state => mfRef.StopResultGui("Unknown Exception: " + ex.Message), null);
            }
        }

        public void EraseFirmware()
        {
            try
            {
                Parameters.Initialize("ERASE");

                Settings.Initialize();

                #region Erasing Freescale 
                mfSync.Send(state => mfRef.ProcessRunningGui(0, "Erasing Freescale flash"), null);
                freescaleInterface = new FreescaleInterface(null);

                mfSync.Send(state => mfRef.ProcessRunningGui(20, "Freescale flash erased successfully"), null);
                freescaleInterface.Close();
                freescaleInterface = null;
                #endregion

                #region Erasing Gainspan
                gainspanInterface = new GainspanInterface();

                gainspanInterface.SetProgramMode();
                mfSync.Send(state => mfRef.ProcessRunningGui(20, "Erasing Gainspan internal flash"), null);
                gainspanInterface.EraseInternalFlash();
                mfSync.Send(state => mfRef.ProcessRunningGui(30, "Programming Gainspan sfp WLAN binary"), null);
                gainspanInterface.ProgramWlanFw(Parameters.CurrExecDir + "\\" + Parameters.libDir + "\\" + Parameters.gsWfwProgBin);

                gainspanInterface.SetRunMode();
                mfSync.Send(state => mfRef.ProcessRunningGui(40, "Erasing Gainspan external flash"), null);
                gainspanInterface.EraseExternalFlash();

                gainspanInterface.SetProgramMode();
                mfSync.Send(state => mfRef.ProcessRunningGui(90, "Erasing Gainspan internal flash"), null);
                gainspanInterface.EraseInternalFlash();

                mfSync.Send(state => mfRef.ProcessRunningGui(100, "Gainspan flash erased successfully"), null);
                gainspanInterface.Close();
                gainspanInterface = null;
                #endregion

                Parameters.LogInfo("PASS", "");
                mfSync.Send(state => mfRef.PassResultGui(), null);
            }
            catch (Exception_FAIL ex)
            {
                GracefulExit(); 

                Parameters.LogInfo("FAIL", ex.Message);
                mfSync.Send(state => mfRef.FailResultGui(ex.Message), null);
            }
            catch (Exception_STOP ex)
            {
                GracefulExit(); 

                Parameters.LogInfo("STOP", ex.Message);
                mfSync.Send(state => mfRef.StopResultGui(ex.Message), null);
            }
            catch (Exception ex)
            {
                GracefulExit(); 

                Parameters.LogInfo("STOP", ex.Message);
                mfSync.Send(state => mfRef.StopResultGui("Unknown Exception: " + ex.Message), null);
            }
        }

        private void GracefulExit()
        {
            try
            {
                if (freescaleInterface != null) freescaleInterface.Close();
            }
            catch { }

            try
            {
                if (gainspanInterface != null) gainspanInterface.Close();
            }
            catch { }
        }
    }
}
