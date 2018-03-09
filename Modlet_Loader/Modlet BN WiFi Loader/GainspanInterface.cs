using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace ThinkEco
{
    class GainspanInterface
    {
        private enum GS_FLASHPROGRAM_STATUS
        {
            GS_SUCCESS = 8,
            GS_INVALID_PARAM = 1,
            GS_NO_DEVICE = -1
        }

        private enum GS_SFP_STATUS
        {
            GS_SUCCESS = 0,
            GS_INVALID_PARAM = 1,
            GS_NO_DEVICE = 1
        }

        private InterfaceBoard interfaceBoard;

        public GainspanInterface()
        { 
            interfaceBoard = new InterfaceBoard(InterfaceType.Gainspan);

            interfaceBoard.OpenPort();

            #region Power up
            interfaceBoard.AssertReset();
            Thread.Sleep(10);
            interfaceBoard.PowerOn();
            Thread.Sleep(200);
            #endregion

            interfaceBoard.ClosePort();
        }

        public void Close()
        {
            interfaceBoard.OpenPort(); 

            #region Power down
            interfaceBoard.AssertReset();
            Thread.Sleep(10);
            interfaceBoard.PowerOff();
            Thread.Sleep(200);
            #endregion

            interfaceBoard.ClosePort();
        }

        public void SetProgramMode()
        {
            interfaceBoard.OpenPort();

            #region Set Program Mode
            interfaceBoard.AssertReset();
            Thread.Sleep(10);
            interfaceBoard.ProgramMode();
            Thread.Sleep(10);
            interfaceBoard.ReleaseReset();
            Thread.Sleep(10);
            #endregion

            interfaceBoard.ClosePort();
        }

        public void SetRunMode()
        {
            interfaceBoard.OpenPort();

            #region Set Run Mode
            interfaceBoard.AssertReset();
            Thread.Sleep(10);
            interfaceBoard.RunMode();
            Thread.Sleep(10);
            interfaceBoard.ReleaseReset();
            Thread.Sleep(10);
            #endregion

            interfaceBoard.ClosePort();
        }

        public void EraseInternalFlash()
        {
            string command = Parameters.libDir + "\\" + Parameters.gsProgExe;
            string args = "-ew -e0 -e1 -S" + interfaceBoard.ComPortNum() + " -v";

            if ((int)GS_FLASHPROGRAM_STATUS.GS_SUCCESS != ExecuteShellCommand(command, args))
            {
                throw new Exception_FAIL("Cannot erase internal flash on Gainspan"); 
            }
        }

        public void ProgramWlanFw(string filePath)
        {
            string command = Parameters.libDir + "\\" + Parameters.gsProgExe;
            string args = "-w " + "\"" + filePath + "\"" + " -S" + interfaceBoard.ComPortNum() + " -v";

            if ((int)GS_FLASHPROGRAM_STATUS.GS_SUCCESS != ExecuteShellCommand(command, args))
            {
                throw new Exception_FAIL("Cannot program WLAN firmware on Gainspan"); 
            }
        }

        public void ProgramAppFw(string app1Path, string app2Path)
        {
            string command = Parameters.libDir + "\\" + Parameters.gsProgExe;
            string args = "-0 " + "\"" + app1Path + "\"" + " -1 " + "\"" + app2Path + "\"" + " -S" + interfaceBoard.ComPortNum() + " -v";

            if ((int)GS_FLASHPROGRAM_STATUS.GS_SUCCESS != ExecuteShellCommand(command, args))
            {
                throw new Exception_FAIL("Cannot program APP firmware"); 
            }
        }

        public void ProgramFactDef(string txtPath, string binPath)
        {
            string command = Parameters.libDir + "\\" + Parameters.gsFacdefExe;
            string args = "\"" + txtPath + "\"" + " " + "\"" + binPath + "\"" ;

            if (0 != ExecuteShellCommand(command, args)) // @@ verify 
            {
                throw new Exception_STOP("Cannot generate Gainspan factory defaults"); 
            }

            command = Parameters.libDir + "\\" + Parameters.gsProgExe;
            args = "-S" + interfaceBoard.ComPortNum() + " -a 0x0801F800:" + "\"" + binPath + "\"" ;

            if ((int)GS_FLASHPROGRAM_STATUS.GS_SUCCESS != ExecuteShellCommand(command, args))
            {
                throw new Exception_FAIL("Cannot program Gainspan factory defaults");
            }
        }

        public void EraseExternalFlash()
        {
            string command = Parameters.libDir + "\\" + Parameters.gsSfpExe;
            string args = "sfe  -S" + interfaceBoard.ComPortNum() + " -v";

            if ((int)GS_SFP_STATUS.GS_SUCCESS != ExecuteShellCommand(command, args))
            {
                throw new Exception_FAIL("Cannot erase the external flash"); 
            }
        }

        public void ProgramSfInfo(string filePath)
        {
            string command = Parameters.libDir + "\\" + Parameters.gsSfpExe;
            string args = "sfw -f " + "\"" + filePath + "\"" + " -S" + interfaceBoard.ComPortNum() + " -v";

            if ((int)GS_SFP_STATUS.GS_SUCCESS != ExecuteShellCommand(command, args))
            {
                throw new Exception_FAIL("Cannot program SfInfo in external flash");
            }
        }

        public void ProgramWebpages(string filePath)
        {
            string command = Parameters.libDir + "\\" + Parameters.gsSfpExe;
            string args = "sfw -f " + "\"" + filePath + "\"" + " 0xf0000 -S" + interfaceBoard.ComPortNum() + " -v";

            if ((int)GS_SFP_STATUS.GS_SUCCESS != ExecuteShellCommand(command, args))
            {
                throw new Exception_FAIL("Cannot program webpages in external flash");
            }
        }

        private int ExecuteShellCommand(string command, string args)
        {
            try
            {
                Process proc = new Process();

                proc.StartInfo.FileName = command;
                proc.StartInfo.Arguments = args;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();
                proc.WaitForExit();

                return proc.ExitCode;
            }
            catch (Exception ex)
            {
                throw new Exception_STOP("Shell command " + command + " " + args + "threw exception " + ex.Message);
            }
        }
    }
}
