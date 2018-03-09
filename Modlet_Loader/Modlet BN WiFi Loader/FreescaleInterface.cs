using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ThinkEco
{
    class FreescaleInterface
    {
        #region SSL constants
        private const byte SOF = 0x55;

        private enum EngCommandHeader
        {
            engReadReq = 0x01,
            engReadResp = 0x02,
            engWriteReq = 0x03,
            engCommitReq = 0x04,
            engEraseReq = 0x05,
            engExReadReq_c = 0x11,
            engExReadResp_c = 0x12,
            engExWriteReq_c = 0x13,
            engExCommitReq_c = 0x14,
            engExEraseReq_c = 0x15,
            engExUnprotectReq_c = 0x16,
            engExJedecIdReq_c = 0x17,
            engExJedecIdResp_c = 0x18,
            engTrimReq_c = 0x20,
            engDeviceIdReq_c = 0x21,
            engDeviceIdResp_c = 0x22,
            engCmdCnf = 0xF0
        };

        private enum EngCmdStatus
        {
            gEngValidReq_c = 0x00,
            gEngInvalidReq_c,
            gEngSuccessOp_c,
            gEngWriteError_c,
            gEngReadError_c,
            gEngCRCError_c,
            gEngCommError_c,
            gEngExecError_c,
            gEngNoConfirm_c
        };

        private enum EngSecureOption
        {
            engSecured_c = 0xC3,
            engUnsecured_c = 0x3C
        };

        private const int sslEngBufSize = 0x200;
        #endregion

        private InterfaceBoard interfaceBoard;
        private Queue<byte> receiveQueue;
        public bool MC13224V; 

        public FreescaleInterface(byte[] ssl)
        {
            interfaceBoard = new InterfaceBoard(InterfaceType.Freescale);

            receiveQueue = new Queue<byte>();

            MC13224V = true;

            interfaceBoard.OpenPort();

            #region Power up
            interfaceBoard.AssertReset();
            Thread.Sleep(10);
            interfaceBoard.PowerOn();
            Thread.Sleep(200);
            #endregion

            #region Erase flash
            interfaceBoard.ProgramMode();
            Thread.Sleep(10);
            interfaceBoard.ReleaseReset();
            Thread.Sleep(2000);
            #endregion

            #region Execute ROM
            interfaceBoard.AssertReset();
            Thread.Sleep(10);
            interfaceBoard.RunMode();
            Thread.Sleep(10);
            interfaceBoard.ReleaseReset();
            Thread.Sleep(100);
            #endregion

            BootSsl(ssl);
        }

        public void Close()
        {
            #region Power down
            interfaceBoard.AssertReset();
            Thread.Sleep(10);
            interfaceBoard.PowerOff();
            Thread.Sleep(200);
            #endregion

            interfaceBoard.ClosePort();
        }

        public void SetXtalTrim(int coarse, int fine)
        {
            if (coarse < 0 || coarse > 15 || fine < 0 || fine > 31)
            {
                throw new Exception_FAIL("Invalid crystral trim values");
            }

            SendTrimPacket((byte)coarse, (byte)fine);
        }

        public void WriteHwParams(byte[] hwParams)
        {
            if (hwParams.Count() > 0x1000)
            {
                throw new Exception_STOP("Oversized hardware parameters"); 
            }

            WriteBlock(0x18000, hwParams);
        }

        public void WriteFirmware(byte[] firmware)
        {
            if (firmware.Count() > 0x18000 - 8)
            {
                throw new Exception_STOP("Oversized firmware image"); 
            }

            WriteBlock(8, firmware);

            SendCommitPacket(firmware.Count()); 
        }

        private void WriteBlock(int addr, byte[] data)
        {
            int length = data.Count(); 
            int offset = 0; 

            while (offset < length)
            {
                int chunkSize = (length - offset < sslEngBufSize) ? length - offset : sslEngBufSize;

                byte[] chunk = new byte[chunkSize]; 

                Buffer.BlockCopy(data, offset, chunk, 0, chunkSize); 

                SendWritePacket(addr + offset, chunk);

                offset += chunkSize; 
            }
        }

        private void BootSsl(byte[] ssl)
        {
            // Send null byte 
            interfaceBoard.SendUartBytes(new byte[1] { 0 });

            // Give time to process
            Thread.Sleep(100);

            // Look for "CONNECT" response 
            byte[] connectResponse = interfaceBoard.RecvUartBytes();

            if (!connectResponse.SequenceEqual(Encoding.ASCII.GetBytes("CONNECT")))
            {
                Close(); 
                throw new Exception_FAIL("Cannot connect to Freescale chip");
            }

            // Only connect? 
            if (ssl == null) return; 

            // Send SSL size 
            interfaceBoard.SendUartBytes(BitConverter.GetBytes(ssl.Count()));

            // Send SSL file 
            interfaceBoard.SendUartBytes(ssl);

            // Allow time to run 
            Thread.Sleep(100);

            // Look for "READY" indication 
            byte[] sslLoadResponse = interfaceBoard.RecvUartBytes();

            if (!sslLoadResponse.SequenceEqual(Encoding.ASCII.GetBytes("READY")))
            {
                Close(); 
                throw new Exception_FAIL("Cannot boot Freescale chip");
            }

            // Get device type
            GetDeviceType(); 
        }

        private void SendTrimPacket(byte coarse, byte fine)
        {
            byte[] trim = new byte[3];

            trim[0] = (byte)EngCommandHeader.engTrimReq_c;
            trim[1] = fine;
            trim[2] = coarse;

            if ((int)EngCmdStatus.gEngSuccessOp_c != SendAckPacket(trim))
            {
                throw new Exception_FAIL("Error sending trim packet");
            }
        }

        private void SendWritePacket(int addr, byte[] data)
        {
            byte[] write = new byte[data.Count() + 7];

            write[0] = (byte)EngCommandHeader.engWriteReq;
            write[1] = (byte)((addr & 0x000000FF) >> 0);
            write[2] = (byte)((addr & 0x0000FF00) >> 8);
            write[3] = (byte)((addr & 0x00FF0000) >> 16);
            write[4] = (byte)((addr & 0xFF000000) >> 24); 
            write[5] = (byte)((data.Count() & 0x00FF) >> 0); 
            write[6] = (byte)((data.Count() & 0xFF00) >> 8); 
            Buffer.BlockCopy(data, 0, write, 7, data.Count());

            if ((int)EngCmdStatus.gEngSuccessOp_c != SendAckPacket(write))
            {
                throw new Exception_FAIL("Error sending write packet");
            }
        }

        private void SendCommitPacket(int size)
        {
            byte[] commit = new byte[6];

            commit[0] = (byte)EngCommandHeader.engCommitReq;
            commit[1] = (byte)((size & 0x000000FF) >> 0);
            commit[2] = (byte)((size & 0x0000FF00) >> 8);
            commit[3] = (byte)((size & 0x00FF0000) >> 16);
            commit[4] = (byte)((size & 0xFF000000) >> 24);
            commit[5] = (byte)EngSecureOption.engSecured_c; 

            if ((int)EngCmdStatus.gEngSuccessOp_c != SendAckPacket(commit))
            {
                throw new Exception_FAIL("Error sending commit packet"); 
            }
        }

        private int SendAckPacket(byte[] payload)
        {
            SendPacket(payload);

            byte[] ack = RecvPacket();

            if (ack == null)
            {
                throw new Exception_STOP("Timeout waiting for SSL confirm");
            }

            if (ack.Count() != 2 || ack[0] != (byte)EngCommandHeader.engCmdCnf)
            {
                return (int)EngCmdStatus.gEngNoConfirm_c;
            }

            return (int)ack[1];
        }

        private void GetDeviceType()
        {
            SendPacket(new byte[1] { (byte)EngCommandHeader.engDeviceIdReq_c });

            byte[] res = RecvPacket();

            if (res == null)
            {
                throw new Exception_STOP("Timeout waiting for device ID response");
            }

            if (res.Count() != 5 || res[0] != (byte)EngCommandHeader.engDeviceIdResp_c)
            {
                throw new Exception_STOP("Error: did not receive device ID response");
            }

            if (res[1] == 0x09 && res[2] == 0x00 && res[3] == 0x00 && res[4] == 0x00)
            {
                MC13224V = true;
            }
            else if (res[1] == 0x11 && res[2] == 0x00 && res[3] == 0x00 && res[4] == 0x00)
            {
                MC13224V = false;
            }
            else
            {
                throw new Exception_FAIL("Unknown Freescale device ID");
            }

            if (MC13224V && Parameters.fsSerialNum[1] != '0' || !MC13224V && Parameters.fsSerialNum[1] != '1')
            {
                throw new Exception_STOP("S/N digit inconsistent with device ID");
            }
        }

        private void SendPacket(byte[] payload)
        {
            int size = payload.Count();

            if (size > sslEngBufSize + 7) // Max payload is engWriteReq (cmdID[1] + addr[4] + size[2] + data[sslEngBufSize])
            {
                throw new Exception_STOP("Invalid SSL payload size to send");
            }

            byte[] packet = new byte[size + 4];

            packet[0] = SOF;                                        // SOF
            packet[1] = (byte)((size & 0x00FF) >> 0);               // Payload size low byte
            packet[2] = (byte)((size & 0xFF00) >> 8);               // Payload size high byte
            Buffer.BlockCopy(payload, 0, packet, 3, size);          // Payload
            packet[size + 3] = (byte)(payload.Sum(x => (int)x));    // CRC

            interfaceBoard.SendUartBytes(packet);
        }

        private byte[] RecvPacket()
        {
            int timeout = 1000;
            int polling = 10; 
            int size;

            while (true)
            {
                // Keep polling 
                Thread.Sleep(polling);

                // Hit timeout? 
                if ((timeout -= polling) < 0)
                {
                    return null;
                }

                // Receive any data on UART
                byte[] data = interfaceBoard.RecvUartBytes();

                // Append in receive queue 
                for (int i = 0; i < data.Count(); i++) receiveQueue.Enqueue(data[i]);

                // Drop anything preceeding SOF
                while (receiveQueue.Count() > 0)
                {
                    if (receiveQueue.ElementAt(0) == SOF) break;

                    receiveQueue.Dequeue();
                }

                // Need at least 3 bytes to know payload size 
                if (receiveQueue.Count() < 3) continue;

                // Work out payload size 
                size = (int)receiveQueue.ElementAt(1) + 0x100 * (int)receiveQueue.ElementAt(2); 

                // Is size valid? 
                if (size > sslEngBufSize)
                {
                    throw new Exception_STOP("Invalid SSL payload size received");
                }

                // Wait to receive full packet 
                if (receiveQueue.Count() < size + 4) continue;

                // We got a packet
                break;
            }

            // Buffer to hold payload 
            byte[] payload = new byte[size];

            // Dequeue packet 
            receiveQueue.Dequeue();                                             // SOF
            receiveQueue.Dequeue();                                             // Payload size low byte 
            receiveQueue.Dequeue();                                             // Payload size high byte 
            for (int i = 0; i < size; i++) payload[i] = receiveQueue.Dequeue(); // Payload 
            byte CRC = receiveQueue.Dequeue();                                  // CRC

            // Check CRC 
            if (CRC != (byte)(payload.Sum(x => (int)x)))
            {
                throw new Exception_STOP("SSL packet CRC error");
            }

            // Here it is
            return payload;
        }
    }
}
