using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AML
{
    public partial class OCR : UserControl
    {
        public OCR()
        {
            InitializeComponent();
        }

        public bool ExitFunction = false;
        private Thread RefreshUI;

        public event ThrowOutSocketReply ThrowOutSocketReplyEvent;
        SocketClient Client;
        string IP;
        int Port;
        string OCRName;
        public AML_LogPage Log = new AML_LogPage();
        bool _Connected;
        public bool Connected
        {
            get
            {
                return _Connected;
            }
        }

        string _ocr_result = null;

        public string OCR_Result
        {
            get
            {
                return _ocr_result;
            }
        }
        private void SendOutSocket(string data)
        {
            if (ThrowOutSocketReplyEvent != null)
                ThrowOutSocketReplyEvent(data);
        }
        public void Init(string _OCRName, string _IP, int _Port, AML_LogPage _log)
        {
            OCRName = _OCRName;
            IP = _IP;
            Port = _Port;
            Log = _log;
            groupBox1.Text = OCRName;
            Client = new SocketClient(_IP, _Port);
            Client.ConnectEvent += Client_ConnectEvent;
            Client.CloseEvent += Client_CloseEvent;
            Client.DataArrivalEvent += Client_DataArrivalEvent;
            Client.DisconnectEvent += Client_DisconnectEvent;
            Client.ErrorEvent += Client_ErrorEvent;
            //timer1sec.Enabled = true;
            label_IP.Text = "IP:"+ IP;
            label_Port.Text = "Port:" + Port;
            Client.Socket_Connect(IP, Port);

            ExitFunction = false;
            RefreshUI = new Thread(RefreshUIControl);
            RefreshUI.IsBackground = true;
            RefreshUI.Start();
        }

        private void Client_ErrorEvent(string ex)
        {
            _Connected = false;
            Log.Write(OCRName, "Socket Error:" + ex.ToString());
        }

        private void Client_DisconnectEvent()
        {
            _Connected = false;
            Log.Write(OCRName, "Socket Disconnect");
        }

        private void Client_DataArrivalEvent(string Data)
        {
           // Log.Write(OCRName, "Recieve:" + Data);
            _ocr_result = Data.Substring(0, Data.Length - 2);
            Log.Write(OCRName, "Result:" + OCR_Result);
            SendOutSocket("ReadWaferID," + OCRName + "," + OCR_Result);
        }

        private void Client_CloseEvent()
        {
            _Connected = false;
            Log.Write(OCRName, "Socket Close");
        }

        private void Client_ConnectEvent()
        {
            _Connected = true;
            Log.Write(OCRName, "Socket Connect");
        }

        public bool ReadOCR(bool from_socket)
        {
            
            // string _result = null;
            if (Connected == false)
            {
                return false;
            }

            _ocr_result = null;
            //"read" + Chr(13) + Chr(10);
            List<byte> Command_Byte = new List<Byte>(Encoding.ASCII.GetBytes("read"));
            Command_Byte.Add(0x0d);
            Command_Byte.Add(0x0a);
            byte[] Dbyte = Command_Byte.ToArray();
            Client.SendData(Dbyte);
            return true;
        }

        private void timer1sec_Tick(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if(Connected)
                {
                    label_Connected.Text = "Conneted";
                    label_Connected.BackColor = Color.Lime;

                    if(OCR_Result != null)
                    {
                        label_OCR.Text = OCR_Result;
                        label_OCR.BackColor = Color.Lime;
                    }
                    else
                    {
                        label_OCR.Text = "NULL";
                        label_OCR.BackColor = Color.Salmon;
                    }
                }
                else
                {
                    label_Connected.Text = "Disconneted";
                    label_Connected.BackColor = Color.Red;
                }
            }
        }
        private void RefreshUIControl(object obj)
        {
            while (true)
            {
                if (ExitFunction)
                    return;
                Thread.Sleep(1000);
                if (this.Visible)
                {
                    //if (Connected)
                    //{
                    //    label_Connected.Text = "Conneted";
                    //    label_Connected.BackColor = Color.Lime;

                    //    if (OCR_Result != null)
                    //    {
                    //        label_OCR.Text = OCR_Result;
                    //        label_OCR.BackColor = Color.Lime;
                    //    }
                    //    else
                    //    {
                    //        label_OCR.Text = "NULL";
                    //        label_OCR.BackColor = Color.Salmon;
                    //    }
                    //}
                    //else
                    //{
                    //    label_Connected.Text = "Disconneted";
                    //    label_Connected.BackColor = Color.Red;
                    //}
                }
            }
        }

        private void button_ReConnect_Click(object sender, EventArgs e)
        {
            Client.Socket_Close();
            Client.Socket_Connect(IP, Port);
        }

        private void button_ReadOCR_Click(object sender, EventArgs e)
        {
            ReadOCR( false);
        }
    }
}
