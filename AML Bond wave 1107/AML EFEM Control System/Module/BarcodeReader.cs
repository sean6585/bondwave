using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;

namespace AML
{
    class BarcodeReader
    {
        public delegate void ReadBarcodeEvent(string Code, bool Result);
        public event ReadBarcodeEvent ReadBarcodeComplete;

        private SerialPort BarcodeCOM;

        private string Temp = String.Empty;
        private string Barcode;
        private bool Success;
        private bool Reading;

        private int RetryLimit = 3;
        private int TimeoutLimit = 1000;

        /*
        private BarcodeReader CodeReader = new BarcodeReader();
        CodeReader.ReadBarcodeComplete += CodeReader_ReadBarcodeComplete;
        CodeReader.Initial(20000, 3, BarcodeCOM);
        CodeReader.ReadBarcodeAsync();
        */

        public BarcodeReader()
        {
        }
        public bool Initial(int timeout_minisecond, int retry, string BarcodeComport)
        {
            try
            {
                BarcodeCOM = new SerialPort();

                BarcodeCOM.DataReceived += BarcodeCOM_DataReceived;
                BarcodeCOM.PortName = BarcodeComport;
                BarcodeCOM.BaudRate = 9600;
                BarcodeCOM.DataBits = 8;
                BarcodeCOM.Parity = Parity.None;
                BarcodeCOM.StopBits = StopBits.One;
                BarcodeCOM.Open();

                BarcodeCOM.DtrEnable = true;
                BarcodeCOM.RtsEnable = true;

                TimeoutLimit = timeout_minisecond;
                RetryLimit = retry;
                StopRead();
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }
        public void OpenPort()
        {
            try
            {
                BarcodeCOM.Close();
                BarcodeCOM.Open();
            }
            catch { }
        }
        public void ClosePort()
        {
            try { BarcodeCOM.Close(); }
            catch { }
        }

        private void BarcodeCOM_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!Reading)
                return;
            byte[] raw_byte = new byte[128];
            SerialPort sp = (SerialPort)sender;
            if (sp.BytesToRead > 0)
            {
                Int32 length = sp.Read(raw_byte, 0, raw_byte.Length);
                Array.Resize(ref raw_byte, length);
                string Data = Temp + System.Text.Encoding.ASCII.GetString(raw_byte.ToArray());
                string[] DataComponent = Data.Split('\r');
                if (Data.Contains("\r"))//成功取得字串
                {
                    for (int i = 0; i < DataComponent.Length; i++)
                    {
                        if (DataComponent[i].ToUpper().Contains("ERR") || DataComponent[i] == string.Empty)
                            continue;

                        Barcode = DataComponent[i];
                        Success = true;
                        break;
                    }
                }
                else
                    Temp = Data;
            }
        }
        public bool ReadBarcode(out string ReturnBarcode)
        {
            int RetryCount = 0;
            while (RetryCount < RetryLimit)
            {
                int WaitTime = 0;
                BarcodeCOM.DiscardInBuffer();
                StartRead();
                while (!Success && (WaitTime < TimeoutLimit))
                {
                    Thread.Sleep(100);
                    WaitTime += 100;
                }
                RetryCount++;

                StopRead();

                if (Success)
                    break;

                Thread.Sleep(100);
            }
            ReturnBarcode = Barcode;
            return Success;
        }
        public async void ReadBarcodeAsync()
        {
            string TempBarcode;
            await Task.Run(delegate()
            {
                bool Result = ReadBarcode(out TempBarcode);
                if (ReadBarcodeComplete != null)
                    ReadBarcodeComplete(TempBarcode, Result);
            });
        }
        private void StartRead()
        {
            if (Reading)
                return;
            Barcode = String.Empty;
            Temp = String.Empty;
            Success = false;
            Reading = true;
            //BarcodeCOM.Write("LON\r");
            byte[] temp = new byte[5] { 2, 76, 79, 78, 3 };
            BarcodeCOM.Write(temp, 0, 5);
        }
        private void StopRead()
        {
            Reading = false;
            //BarcodeCOM.Write("LOFF\r");
            byte[] temp = new byte[6] { 2, 76, 79, 70, 70, 3 };
            BarcodeCOM.Write(temp, 0, 6);
        }
    }
}
