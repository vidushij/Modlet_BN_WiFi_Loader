using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTD2XX_NET;

namespace ThinkEco
{
    enum InterfaceType
    {
        Gainspan, 
        Freescale
    };

    class InterfaceBoard
    {
        private FTDI ftdi;
        private string comPort;

        private InterfaceType interfaceType;

        public InterfaceBoard(InterfaceType i)
        {
            interfaceType = i; 
        }

        public void OpenPort()
        {
            ftdi = new FTDI();

            uint numDevices = 0;

            if (FTDI.FT_STATUS.FT_OK != ftdi.GetNumberOfDevices(ref numDevices)) 
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[numDevices];

            if (FTDI.FT_STATUS.FT_OK != ftdi.GetDeviceList(ftdiDeviceList)) 
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            uint locationID = 0;
            uint deviceCount = 0;

            for (int k = 0; k < numDevices; k++)
            {
                if (ftdiDeviceList[k].ID == 0x04036001)
                {
                    locationID = ftdiDeviceList[k].LocId;
                    deviceCount++;
                }
            }

            if (deviceCount == 0)
            {
                throw new Exception_STOP("No interface board found");
            }
            else if (deviceCount > 1)
            {
                throw new Exception_STOP("More than one interface board found");
            }

            if (FTDI.FT_STATUS.FT_OK != ftdi.OpenByLocation(locationID))
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            if (FTDI.FT_STATUS.FT_OK != ftdi.GetSerialNumber(out Parameters.ftdiSerialNum))
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            if (FTDI.FT_STATUS.FT_OK != ftdi.SetBaudRate(115200) ||
                FTDI.FT_STATUS.FT_OK != ftdi.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE) ||
                FTDI.FT_STATUS.FT_OK != ftdi.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0, 0))
            {

                throw new Exception_STOP("Problem accessing the interface board");
            }

            if (FTDI.FT_STATUS.FT_OK != ftdi.GetCOMPort(out comPort))
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            FTDI.FT232R_EEPROM_STRUCTURE data = new FTDI.FT232R_EEPROM_STRUCTURE();

            if (FTDI.FT_STATUS.FT_OK != ftdi.ReadFT232REEPROM(data))
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            if (data.Cbus0 != FTDI.FT_CBUS_OPTIONS.FT_CBUS_IOMODE ||
                data.Cbus1 != FTDI.FT_CBUS_OPTIONS.FT_CBUS_IOMODE ||
                data.Cbus2 != FTDI.FT_CBUS_OPTIONS.FT_CBUS_IOMODE ||
                data.Cbus3 != FTDI.FT_CBUS_OPTIONS.FT_CBUS_IOMODE)
            {
                throw new Exception_STOP("Interface board FTDI settings are incorrect");
            }

            if (interfaceType == InterfaceType.Freescale) 
                SelectFreescale();
            else
                SelectGainspan();
        }

        public void ClosePort()
        {
            if (FTDI.FT_STATUS.FT_OK != ftdi.Close())
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }
        }

        public string ComPortNum()
        {
            return comPort.Substring(3); 
        }

        public void SendUartBytes(byte[] data)
        {
            uint numBytesWritten = 0;

            if (FTDI.FT_STATUS.FT_OK != ftdi.Write(data, data.Length, ref numBytesWritten))
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            if (data.Length != numBytesWritten)
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }
        }

        public byte[] RecvUartBytes()
        {
            uint numBytesAvailable = 0;
            uint numBytesRead = 0; 

            if (FTDI.FT_STATUS.FT_OK != ftdi.GetRxBytesAvailable(ref numBytesAvailable))
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            byte[] data = new byte[numBytesAvailable];

            if (FTDI.FT_STATUS.FT_OK != ftdi.Read(data, numBytesAvailable, ref numBytesRead))
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            if (numBytesAvailable != numBytesRead)
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            return data;
        }

        public void ReleaseReset()
        {
            SetPin(3, 1);
        }

        public void AssertReset()
        {
            SetPin(3, 0);
        }

        public void ProgramMode()
        {
            SetPin(2, 1);
        }

        public void RunMode()
        {
            SetPin(2, 0);
        }

        private void SelectGainspan()
        {
            SetPin(1, 1);
        }

        private void SelectFreescale()
        {
            SetPin(1, 0);
        }

        public void PowerOn()
        {
            SetPin(0, 1);
        }

        public void PowerOff()
        {
            SetPin(0, 0);
        }

        private void SetPin(byte pin, byte state)
        {
            byte bitMask = 0;

            if (FTDI.FT_STATUS.FT_OK != ftdi.GetPinStates(ref bitMask))
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }

            // All CBUS outputs 
            bitMask |= (byte)0xF0;

            // Update the pin state 
            if (state != 0) bitMask |= (byte)(1 << pin);
            else            bitMask &= (byte)(~(1 << pin));

            if (FTDI.FT_STATUS.FT_OK != ftdi.SetBitMode(bitMask, 0x20))
            {
                throw new Exception_STOP("Problem accessing the interface board");
            }
        }
    }
}
