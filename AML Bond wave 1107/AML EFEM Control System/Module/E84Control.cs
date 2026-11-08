using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using AML;

namespace AML
{
    public class E84Control
    {
        public delegate void AlarmEvent(string Code, string Description);
        public enum E84AccessMode { Unknown, Auto, Manual };
        private enum E84AskStatusItem { InputSignal, ControlBoard, SensorStatus };
        public delegate void E84CommandReply(E84CommandItem Action, bool Result);
        public delegate void E84ReceiveEvent(E84EventItem Event, bool OnOff , byte RawEvtBye);
        public event E84CommandReply E84Reply;
        public event E84ReceiveEvent E84Event;
        public event AlarmEvent E84Alarm;
        public event ThrowOutSocketReply ThrowOutSocketReplyEvent;


        #region E84EventItem
        private enum E84ReceivedState { Idle, Receiving };
        public enum E84TpnItem { TP1 = 0x70, TP2, TP3, TP4, TP5, TP6 };

        public enum E84CommandItem
        {
            HeartBeat,
            Auto,
            Manual,
            Reset,
            Enable,
            SetTp1,
            SetTp2,
            SetTp3,
            SetTp4,
            SetTp5,
            SetTp6,
        }

        //public enum E84Timeout
        //{
        //    SetTp1,
        //    SetTp2,
        //    SetTp3,
        //    SetTp4,
        //    SetTp5,
        //    SetTp6,
        //}

        public enum E84EventItem
        {
            HandoverFinish,
            HandoffAvailable,
      
            Go,
            Cs,
            Valid,
            L_Req,
            U_Req,
            Tr_Req,
            Ready,
            Busy,
            Compt,
            Auto,
            Manual,
            Placement,
            Presence,
            Clamp,
            Alarm,
        }

        #endregion

        private SerialPort E84COM;
        private Byte RecStartByte = 0xAA;
        private Byte RecLastByte = 0xBB;
        private List<Byte> Temp = new List<byte>();

        private Thread BG_E84;
        private Thread TdInitial;
        private BlockQueue<byte[]> E84Queue = new BlockQueue<byte[]>();

        public bool HandOffAvailable = false;
        public bool Busy = false;

        public E84AccessMode E84Mode = E84AccessMode.Unknown;
        private E84AskStatusItem CurrentAskStatus;

        private E84ReceivedState ReceivedState;
        public AML_LogPage Log = new AML_LogPage();

        private void SendOutSocket(string data)
        {
            if (ThrowOutSocketReplyEvent != null)
                ThrowOutSocketReplyEvent(data);
        }
        string PortName;
        public void Initial(string _PortName,SerialPort sp, AML_LogPage _log)
        {
            PortName = _PortName;
            Log = _log;
            BG_E84 = new Thread(BG_E84_DoWork);
            BG_E84.IsBackground = true;
            BG_E84.Start();
            E84COM = sp;
           // E84COM = new SerialPort(Comport);
            E84COM.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.COM_DataRecieved);
            //E84COM.BaudRate = 115200;
            //E84COM.DataBits = 8;
            //E84COM.Parity = Parity.None;
            //E84COM.StopBits = StopBits.One;
            //sp = E84COM;    
            TdInitial = new Thread(StatusThread);
        }

        public void OpenPort()
        {
            try
            {
                E84COM.Close();
                E84COM.Open();
                TdInitial.Start();
            }
            catch { }
        }
        public void ClosePort()
        {
            try { E84COM.Close(); }
            catch { }
        }

        //=====執行緒=====
        private void BG_E84_DoWork()
        {
            byte[] RecBytes;
            List<byte[]> ReturnData;
            while (true)
            {
                Thread.Sleep(100);
                if (E84Queue == null)
                    continue;
                if (E84Queue.Count() == 0)
                    continue;
                RecBytes = E84Queue.DeQueue();
                //if (RecBytes == null)
                //{
                //    Temp.Clear();//Receive data timeout
                //    continue;
                //}
                ReturnData = DataCombine(RecBytes);
                E84Queue.Clear();
                for (int i = 0; i < ReturnData.Count; i++)
                {
                    if (ReturnData[i][0] == 0x55)
                        E84EventHandler(ReturnData[i]);
                    else
                        E84RecHandler(ReturnData[i]);
                }
            }
        }
        public void StatusThread()
        {
            try
            {
                //Thread.Sleep(1000);
                //SetManual();
                Thread.Sleep(1000);
                GetHandoffMode();
                Thread.Sleep(1000);
                GetSensorState();
            }
            catch (Exception ex)
            {
                Log.Write("E84->Host", "E84 StatusThread Exception:" + ex.Message);
            }
        }

        //=====通訊處理=====
        private void COM_DataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] raw_byte = new byte[128];
                SerialPort sp = (SerialPort)sender;
                if (sp.BytesToRead > 0)
                {
                    Int32 length = sp.Read(raw_byte, 0, raw_byte.Length);
                    Array.Resize(ref raw_byte, length);
                    E84Queue.EnQueue(raw_byte);
                }
            }
            catch (Exception ex)
            {
               Log.Write("E84->Host", "E84 COMReiceved Exception:" + ex.Message);
            }
        }
        private List<byte[]> DataCombine(byte[] raw_data)
        {
            List<byte[]> ReturnData = new List<byte[]>();
            for (int i = 0; i < raw_data.Length; i++)
            {
                switch (ReceivedState)
                {
                    case E84ReceivedState.Idle:
                        if (raw_data[i] == RecStartByte)
                            ReceivedState = E84ReceivedState.Receiving;
                        else
                        { //是不是要錯誤處理
                        }
                        break;
                    case E84ReceivedState.Receiving:
                        if (raw_data[i] != RecLastByte)
                        {
                            Temp.Add(raw_data[i]);
                            break;
                        }
                        //以下已經收到LastByte了
                        //TimeOutCheck.Enabled = false;
                        ReturnData.Add(Temp.ToArray());
                        Temp.Clear();
                        ReceivedState = E84ReceivedState.Idle;
                        break;
                }
            }
            return ReturnData;
        }
        private void E84RecHandler(byte[] RecBytes)
        {
            switch (RecBytes[0])
            {
                case 0x00:
                    SendE84Reply(E84CommandItem.HeartBeat);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (HeartBeat)");
                    break;
                case 0x01:
                    if(RecBytes[1] == 0x00)
                        E84Mode = E84AccessMode.Auto;
                    SendE84Reply(E84CommandItem.Auto, E84Mode == E84AccessMode.Auto);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Switch E84AccessMode.Auto)");
                    break;
                case 0x02:
                    if(RecBytes[1] == 0x00)
                        E84Mode = E84AccessMode.Manual;
                    SendE84Reply(E84CommandItem.Manual, E84Mode == E84AccessMode.Manual);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Switch E84AccessMode.Manual)");
                    break;
                case 0x03:
                    if (RecBytes[1] == 0x00)
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Switch E84ES.OnOff)");
                    break;
                case 0x04:
                    SendE84Reply(E84CommandItem.Reset, RecBytes[1] == 0x00);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Reset E84ControlBoard)");
                    break;
              
                
                
                case 0x08:
                    //沒啥用 等Event
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte));
                    break;
                


                case (byte)E84TpnItem.TP1: //70
                    SendE84Reply(E84CommandItem.SetTp1, RecBytes[1] == 0x00);
                    if (RecBytes[1] == 0x00)
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP1 TimeOut Done)");
                        Log.WriteSystemLog("E84", "Set TP1 TimeOut Done");
                    }
                    else
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP1 TimeOut Fail)");
                        Log.WriteSystemLog("E84", "Set TP1 TimeOut Fail");
                    }
                    break;
                case (byte)E84TpnItem.TP2:
                    SendE84Reply(E84CommandItem.SetTp2, RecBytes[1] == 0x00);
                    if (RecBytes[1] == 0x00)
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP2 TimeOut Done)");
                        Log.WriteSystemLog("E84", "Set TP2 TimeOut Done");
                    }
                    else
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP2 TimeOut Fail)");
                        Log.WriteSystemLog("E84", "Set TP2 TimeOut Fail");
                    }
                    break;
                case (byte)E84TpnItem.TP3:
                    SendE84Reply(E84CommandItem.SetTp3, RecBytes[1] == 0x00);
                    if (RecBytes[1] == 0x00)
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP3 TimeOut Done)");
                        Log.WriteSystemLog("E84", "Set TP3 TimeOut Done");
                    }
                    else
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP3 TimeOut Fail)");
                        Log.WriteSystemLog("E84", "Set TP3 TimeOut Fail");
                    }
                    break;
                case (byte)E84TpnItem.TP4:
                    SendE84Reply(E84CommandItem.SetTp4, RecBytes[1] == 0x00);
                    if (RecBytes[1] == 0x00)
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP4 TimeOut Done)");
                        Log.WriteSystemLog("E84", "Set TP4 TimeOut Done");
                    }
                    else
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP4 TimeOut Fail)");
                        Log.WriteSystemLog("E84", "Set TP4 TimeOut Fail");
                    }
                    break;
                case (byte)E84TpnItem.TP5:
                    SendE84Reply(E84CommandItem.SetTp5, RecBytes[1] == 0x00);
                    if (RecBytes[1] == 0x00)
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP5 TimeOut Done)");
                        Log.WriteSystemLog("E84", "Set TP5 TimeOut Done");
                    }
                    else
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP5 TimeOut Fail)");
                        Log.WriteSystemLog("E84", "Set TP5 TimeOut Fail");
                    }
                    break;
                case (byte)E84TpnItem.TP6:
                    SendE84Reply(E84CommandItem.SetTp6, RecBytes[1] == 0x00);
                    if (RecBytes[1] == 0x00)
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP6 TimeOut Done)");
                        Log.WriteSystemLog("E84", "Set TP6 TimeOut Done");
                    }
                    else
                    {
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Set TP6 TimeOut Fail)");
                        Log.WriteSystemLog("E84", "Set TP6 TimeOut Fail");
                    }
                    break;
                case 0x81:
                    if (CurrentAskStatus == E84AskStatusItem.InputSignal)
                    {
                        HandOffAvailable = ((RecBytes[2] & 0x40) == 0x40);
                        SendE84Event(E84EventItem.HandoffAvailable, HandOffAvailable);
                    }
                    else if (CurrentAskStatus == E84AskStatusItem.ControlBoard)
                    {
                        switch (RecBytes[1] & 0x06)
                        {
                            case 0x02:
                                E84Mode = E84AccessMode.Manual;
                                SendE84Event(E84EventItem.Auto, false);
                                break;
                            case 0x04:
                                E84Mode = E84AccessMode.Auto;
                                SendE84Event(E84EventItem.Auto, true);
                                break;
                            default:
                                E84Mode = E84AccessMode.Unknown;
                                break;
                        }
                    }
                    else if (CurrentAskStatus == E84AskStatusItem.SensorStatus)
                    {
                        byte SensorState = RecBytes[1];
                        int Sensor = Convert.ToInt32(SensorState);
                        switch (Sensor)
                        {
                            case 0:
                                SendE84Event(E84EventItem.Presence, false);
                                SendE84Event(E84EventItem.Placement, false);
                                break;
                            case 3:
                                SendE84Event(E84EventItem.Placement, true);
                                break;
                            case 128:
                                SendE84Event(E84EventItem.Presence, true);
                                break;
                            case 131:
                                SendE84Event(E84EventItem.Presence, true);
                                SendE84Event(E84EventItem.Placement, true);
                                break;
                            default:    // 其餘的不認
                                break;
                        }
                    }
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte));
                    break;
                case 0x88:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte));
                    //command_result = "[EVT] FOUP Complete Confirmed, Send BUSY OFF Signal ok";
                    break;
                case 0x89:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte));
                    //command_result = "[EVT] Continous Load Confirmed";
                    break;
            }
        }
        private void E84EventHandler(byte[] RecBytes)
        {
            byte EvtByte = RecBytes[1];
            switch (EvtByte)
            {
                case 0x09:

                    HandOffAvailable = true; //Auto
                    SendE84Event(E84EventItem.HandoffAvailable, true, EvtByte);
                    
                    E84Mode = E84AccessMode.Auto;
                    SendE84Event(E84EventItem.Auto, true, EvtByte);
                   
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (HO Available On)");
                    break;
                case 0x10:
                    SendE84Event(E84EventItem.Go, true,EvtByte);//OHT arrival
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (GO ON [OHT Arrival])");
                    break;
                case 0x11:
                    Busy = true;
                    SendE84Event(E84EventItem.Cs, true,EvtByte);    //Handshake start
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CS ON)");
                    break;
                case 0x12:
                    SendE84Event(E84EventItem.Valid, true, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (VALID ON)");
                    break;
                case 0x13:
                    SendE84Event(E84EventItem.L_Req, true, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (L_REQ ON)");
                    break;
                case 0x14:
                    SendE84Event(E84EventItem.U_Req, true, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (U_REQ ON)");
                    break;
                case 0x15:
                    SendE84Event(E84EventItem.Tr_Req, true, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TR_REQ ON)");
                    break;
                case 0x16:
                    SendE84Event(E84EventItem.Ready, true, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (READY ON)");
                    break;
                case 0x17:
                    SendE84Event(E84EventItem.Busy, true,EvtByte);    //Busy On
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (BUSY ON)");
                    break;
                case 0x18:
                    SendE84Event(E84EventItem.L_Req, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (L_REQ OFF)");
                    break;
                case 0x19:
                    SendE84Event(E84EventItem.U_Req, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (U_REQ OFF)");
                    break;
                case 0x1a:
                    SendE84Event(E84EventItem.Busy, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (BUSY OFF)");
                    break;
                case 0x1b:
                    SendE84Event(E84EventItem.Tr_Req, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TR_REQ OFF)");
                    break;
                case 0x1c:
                    SendE84Event(E84EventItem.Compt, true, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (COMPT ON)");
                    break;
                case 0x1d:
                    SendE84Event(E84EventItem.Ready, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (READY OFF)");
                    break;
                case 0x1e:
                    SendE84Event(E84EventItem.Valid, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (VALID OFF)");
                    break;
                case 0x1f:
                    SendE84Event(E84EventItem.Compt, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (COMPT OFF)");
                    break;
                case 0x20:
                    Busy = false;
                    //SendE84Event(E84EventItem.Busy, false);    //Busy Off
                    SendE84Event(E84EventItem.Cs, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CS OFF)");
                    break;
                case 0x21:
                    //Busy = false;
                    SendE84Event(E84EventItem.Go, false,EvtByte);//OHT leave
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (GO OFF [OHT Leave])");
                    break;
                case 0x24:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CONT ON)");
                    break;
                case 0x25:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CONT OFF)");
                    break;
                case 0x22:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CONTINUE HAND-OFF)");    
                    //SendE84Event(E84EventItem.L_Req, false);
                    break;
                case 0x28:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CS_1 ON)");
                    break;
                case 0x29:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CS_1 OFF)");
                    break;
                case 0x2A:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (VS_0 ON)");
                    break;
                case 0x2B:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (VS_0 OFF)");
                    break;
                case 0x2C:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (VS_1 ON)");
                    break;
                case 0x2D:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (VS_1 OFF)");
                    break;
                case 0x30:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Detect Load Start)");
                    break;
                case 0x31:
                    SendE84Event(E84EventItem.Presence, true, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (PRESENCE SENSOR ON)");
                    break;
                case 0x32:
                    SendE84Event(E84EventItem.Placement, true, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (PLACEMENT SENSOR ON)");
                    break;
                case 0x33:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (FOUP SENSOR ALL ON)");
                    break;
                case 0x37:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (FOUP SENSOR current status)");
                    break;
                case 0x38:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Detect Unload Start)");
                    break;
                case 0x39:
                    SendE84Event(E84EventItem.Placement, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (PLACEMENT SENSOR OFF)");
                    break;
                case 0x3A:
                    SendE84Event(E84EventItem.Presence, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (PRESENCE SENSOR OFF)");
                    break;
                case 0x3B:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (FOUP SENSOR ALL OFF)");
                    break;
                case 0x3D:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (PS NONE DEFINED)");
                    break;
                case 0x3E:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (PL NONE DEFINED)");
                    break;
                case 0x3F:    // 這邊需要parse燈號的部分
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CURRENT FOUP SENSOR STATUS)");
                    //  0x55 0x3F 0x02 0x00 0xBB 
                    byte SensorState = RecBytes[3];
                    int Sensor = Convert.ToInt32(SensorState);
                    switch (Sensor)
                    {
                        case 0:
                            SendE84Event(E84EventItem.Presence, false, EvtByte);
                            SendE84Event(E84EventItem.Placement, false, EvtByte);
                            break;
                        case 3:
                            SendE84Event(E84EventItem.Placement, true, EvtByte);
                            break;
                        case 128:
                            SendE84Event(E84EventItem.Presence, true, EvtByte);
                            break;
                        case 131:
                            SendE84Event(E84EventItem.Presence, true, EvtByte);
                            SendE84Event(E84EventItem.Placement, true, EvtByte);
                            break;
                        default:    // 其餘的不認
                            break;
                    }
                    break;
                case 0x58:
                    Busy = false;
                    SendE84Event(E84EventItem.Busy, false, EvtByte);                //Handover Finished 解 Busy
                    SendE84Event(E84EventItem.HandoverFinish, true, EvtByte);       //發Event取代 CSOff 的判斷
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Handover Finished)");
                    break;
                case 0x70:
                    HandOffAvailable = false;
                    SendE84Event(E84EventItem.HandoffAvailable, false, EvtByte);
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (HO Available Off)");
                    break;
                case 0x71:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (DO Off Event)");
                    break;
                case 0x72:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (KEY-RESET or DI-RESET)");
                    break;
                case 0x73:
                    Busy = false;
                    SendE84Event(E84EventItem.Busy, false,EvtByte);    //Auto Recover 解 Busy
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (AUTO RECOVER)");
                    break;
                case 0x7E:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CURRENT DI STATUS)");
                    break;
                case 0x7F:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CURRENT E84 SIGNAL INPUT STATUS)");
                    break;
              
                case 0xfd:
                    E84Mode = E84AccessMode.Manual;
                    SendE84Event(E84EventItem.Manual, true, EvtByte);
                    break;
                case 0xfe:
                    E84Mode = E84AccessMode.Auto;
                    SendE84Event(E84EventItem.Auto, true, EvtByte);
                    break;
                #region Error list 這段看能不能寫在檔案裡 不然就放在另一個cs檔
                case 0x80:
                    SendAlarm(RecBytes[1], "CS_0 timeout");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (CS_0 timeout)");
                    break;
                case 0x81:
                    SendAlarm(RecBytes[1], "TD_0 timeout");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TD_0 timeout)");
                    break;
                case 0x82:
                    SendAlarm(RecBytes[1], "TP1 timeout");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TP1 timeout)");
                    break;
                case 0x83:
                    SendAlarm(RecBytes[1], "TP2 timeout");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TP2 timeout)");
                    break;
                case 0x84:
                    SendAlarm(RecBytes[1], "TP3-load timeout");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TP3-load timeout)");
                    break;
                case 0x86:
                    SendAlarm(RecBytes[1], "TP3-Unload timeout");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TP3-Unload timeout)");
                    break;
                case 0x88:
                    SendAlarm(RecBytes[1], "TP4 timeout");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TP4 timeout)");
                    break;
                case 0x8b:
                    SendAlarm(RecBytes[1], "TP5 timeout");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TP5 timeout)");
                    break;
                case 0x8c:
                    SendAlarm(RecBytes[1], "TP6 timeout");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (TP6 timeout)");
                    break;
                case 0xef:
                    //SendAlarm(RecBytes[1], "Stan On Last none-auto-recover error");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Stan On Last none-auto-recover error)");
                    break;
                case 0xf1:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (ES ON)");
                    break;
                case 0xf2:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Clamp on)");
                    break;
                case 0xf3:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Clamp off)");
                    break;
                case 0xf4:
                    SendAlarm(RecBytes[1], "Clamp on error");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Clamp on error)");
                    break;
                case 0xf5:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Light Curtain On)");
                    break;
                case 0xf6:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Light Curtain Off)");
                    break;
                case 0xf7:
                    SendAlarm(RecBytes[1], "Light Curtain On Error");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Light Curtain On Error)");
                    break;
                case 0xf8:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (EQ_ALARM ON)");
                    break;
                case 0xf9:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (EQ_ALARM OFF)");
                    break;
                case 0xfa:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (ES On)");
                    break;
                case 0xfb:
                    SendAlarm(RecBytes[1], "ES ERROR");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (ES ERROR)");
                    break;
                case 0xfc:
                    SendAlarm(RecBytes[1], "POWER ERROR");
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (POWER ERROR)");
                    break;
                case 0xff:
                    Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Environment Signal not Ready)");
                    break;
                default:
                    if (RecBytes[1] >= 0xa0 && RecBytes[1] <= 0xc9)
                    {
                        SendAlarm(RecBytes[1], "E84 Sequence error");
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (E84 Sequence error)");
                    }
                    else if (RecBytes[1] >= 0xd0 && RecBytes[1] <= 0xe7)
                    {
                        SendAlarm(RecBytes[1], "Foup Sequence error");
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Foup Sequence error)");
                    }
                    else
                    {
                        //SendAlarm(RecBytes[1], "Unknown evnet or alarm");
                        Log.Write("E84->Host", String.Format("{0:x2} ", RecStartByte) + ByteArrayToString(RecBytes) + String.Format("{0:x2} ", RecLastByte) + " (Unknown evnet or alarm)");
                    }
                    break;
                #endregion
            }
        }
        private void SendData(bool wait_cmd, byte[] Data)
        {
            try
            {
                List<byte> DataTemp = new List<byte>();
               // DataTemp.Add(0xAA);//....
                DataTemp.Add(0x55);
                DataTemp.AddRange(Data);
                DataTemp.Add(0xBB);

               // byte[] DataTemp = { 85, 1, 0, 0xBB };

                if (E84COM.IsOpen)
                {
                    //E84COM.Write(Encoding.ASCII.GetString(DataTemp.ToArray()));    // Old
                    E84COM.Write(DataTemp.ToArray(), 0, DataTemp.ToArray().Length);    // New
                    Log.Write("Host->E84", ByteArrayToString(DataTemp.ToArray()));    //顯示RawData
                    //E84_Command_Event(e84_index, false, BitConverter.ToString(send_data).ToUpper().Replace("-", " "));
                }
                else
                {
                    //E84_Alarm_Event(e84_index, true, "Port is not open");
                    //寫Log
                }
            }
            catch (Exception ex)
            {
                Log.WriteSystemLog("E84-"+ E84COM.PortName, ex.ToString());
            }
        }
        private void SendE84Reply(E84CommandItem Reply, bool Result = true)
        {
            if (E84Reply != null)
                E84Reply(Reply, Result);
        }
        private void SendE84Event(E84EventItem Event, bool OnOff, byte RawEvtBye = 0x00)
        {
            if (E84Event != null)
                E84Event(Event, OnOff, RawEvtBye);
        }
        private void SendAlarm(byte AlarmCode, string Description)
        {
            if (E84Alarm != null)
                E84Alarm(Convert.ToInt32(AlarmCode).ToString(), Description);

            //上面用Evt統一報.... Follow Richard遺毒
            SendE84Event(E84EventItem.Alarm, false, AlarmCode);
        }

        //=====指令=====
        public void SendHeartBeat()
        {
            byte[] CommandBytes = new byte[] { 0x00, 0 };
            SendData(true, CommandBytes);
        }
        public void SetAuto()
        {
            byte[] CommandBytes = new byte[] { 0x01, 0 };
            Log.Write("E84", "Send E84 AccessMode Auto Cmd (0x01 0)");
            SendData(true, CommandBytes);
        }
        public void SetManual()
        {
            byte[] CommandBytes = new byte[] { 0x02, 0 };
            Log.Write("E84", "Send E84 AccessMode Manual Cmd (0x02 0)");
            SendData(true, CommandBytes);
        }
        public void SendReset()
        {
            Busy = false;
            //SendE84Event(E84EventItem.Busy, false);    //Busy Off
            byte[] CommandBytes = new byte[] { 0x04, 0 };
            Log.Write("E84", "Send E84 AccessMode Reset Cmd(0x04 0)");
            SendData(true, CommandBytes);
        }

        /*

        Case "Enable"
        ByteData = {170, 85, 128, 0, 187}
        sRawDataString = "170 85 128 0 187"

        Case "SetESOn"
        ByteData = {170, 85, 3, 1, 187}
        sRawDataString = "170 85 3 1 187"
        Case "SetESOff"
        ByteData = {170, 85, 3, 0, 187}
        sRawDataString = "170 85 3 0 187"

         */
        public void SetEnable()
        {
            byte[] CommandBytes = new byte[] { 0x80, 0 };
            Log.Write("E84", "Send E84 AccessMode SetEnable Cmd (0x80 0)");
            SendData(true, CommandBytes);
            SendE84Reply(E84CommandItem.Enable, E84Mode == E84AccessMode.Auto);
        }

        public void SetES(bool Value)
        {
            byte[] CommandBytes = Value ? new byte[] { 0x03, 0x01 } : new byte[] { 0x03, 0x00 };
            string CmdString = Value ? "On:0x03 0x01" : "Off:0x03 0x00";
            Log.Write("E84", "Send E84 HOAvailable " + CmdString);
            SendData(true, CommandBytes);
        }

        public void SendHandoffAvailable(bool Value)
        {
            byte[] CommandBytes = Value ? new byte[] { 0x08, 0x01 } : new byte[] { 0x08, 0x00 };
            string CmdString = Value ? "On:0x08 0x01" : "Off:0x08 0x00";
            Log.Write("E84", "Send E84 HOAvailable " + CmdString);
            SendData(true, CommandBytes);
        }
        public void SetTpTimeout(E84TpnItem TPn, byte period)
        {
            byte[] CommandBytes = new byte[] { (byte)TPn, period };
            SendData(true, CommandBytes);
        }
        public void GetControlMode()
        {
            byte[] CommandBytes = new byte[] { 0x81, 0x05 };
            CurrentAskStatus = E84AskStatusItem.ControlBoard;
            SendData(true, CommandBytes);
        }
        public void GetHandoffMode()
        {
            byte[] CommandBytes = new byte[] { 0x81, 0x02 };
            CurrentAskStatus = E84AskStatusItem.InputSignal;
            SendData(true, CommandBytes);
        }
        public void GetSensorState()
        {
            byte[] CommandBytes = new byte[] { 0x81, 0x03 };
            CurrentAskStatus = E84AskStatusItem.SensorStatus;
            SendData(true, CommandBytes);
        }
        public void RawSend(byte[] Data)
        {
            SendData(false, Data);
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);

            return hex.ToString();
        }

    }
}
