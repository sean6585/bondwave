using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Timers;

namespace AML
{
    public class RfidReader
    {
        private enum RfidReceivedState { Idle, Receiving };
        public enum RfidReaderType { Omron, Brillian};
        private RfidReceivedState ReceivedState = RfidReceivedState.Idle;
        private RfidReaderType ReaderType;

        public delegate void AreYouThereEvent(string Version);
        public delegate void ReadCompleteEvent(string ID, bool Result);
        public event AreYouThereEvent CheckComplete;
        public event ReadCompleteEvent ReadComplete;

        private Timer TimeOutCheck = new Timer();


        private SerialPort RFID_232 = new SerialPort();

        private Byte LastByte = 0x0D;
        private List<Byte> Temp = new List<byte>();

        private string DeviceId
        {
            set;
            get;
        }
        public bool PortIsOpen
        {
            set;
            get;
        }

        //=====Initial=====
        public bool Intial(RfidReaderType _type, string _deviceId, SerialPort sp)
        {
            ReaderType = _type;
            DeviceId = _deviceId;
            ClosePort();
            TimeOutCheck.Enabled = false;
            TimeOutCheck.Interval = 10000;
            TimeOutCheck.Elapsed += TimeOutCheck_Elapsed;
            switch (ReaderType)
            {
                case RfidReaderType.Omron:
                    RFID_232 = sp;
                    //RFID_232.BaudRate = 9600;
                    //RFID_232.DataBits = 8;
                    //RFID_232.Parity = Parity.Even;
                    //RFID_232.StopBits = StopBits.One;
                    RFID_232.DataReceived += RFID_232_DataReceived;
                    LastByte = 0x0D;
                   
                    break;
                case RfidReaderType.Brillian:
                    RFID_232 = sp;
                    //RFID_232.BaudRate = 9600;
                    //RFID_232.DataBits = 8;
                    //RFID_232.Parity = Parity.None;
                    //RFID_232.StopBits = StopBits.One;
                    RFID_232.DataReceived += RFID_232_DataReceived;
                    LastByte = 0x2A;
                    break;
           
            }
            sp = RFID_232;
            return  OpenPort();
        }
        public void ClosePort()
        {
            PortIsOpen = false;
            switch (ReaderType)
            {
                case RfidReaderType.Omron:
                case RfidReaderType.Brillian:
                    if (RFID_232.IsOpen)
                        RFID_232.Close();
                    break;
            
            }

        }
        public bool OpenPort()
        {
            ClosePort();
            try
            {
                switch (ReaderType)
                {
                    case RfidReaderType.Omron:
                    case RfidReaderType.Brillian:
                        RFID_232.Open();
                        break;
              
                }
                PortIsOpen = true;
                return true;
            }
            catch { }

            return false;
        }
        private void TimeOutCheck_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReceivedState = RfidReceivedState.Idle;
            Temp.Clear();
            ReadComplete("Read RFID timeout:" + TimeOutCheck.Interval.ToString() + "s", false);
            TimeOutCheck.Enabled = false;
        }

        //=====Command=====
        public bool ReadRfid()
        {
            if (ReceivedState != RfidReceivedState.Idle)
            {
                return true;
            }
            switch (ReaderType)
            {
                case RfidReaderType.Omron:
                    if (!RFID_232.IsOpen)
                    {
                        ReadComplete(RFID_232.PortName + " is not open", false);
                        return false;
                    }
                    ReceivedState = RfidReceivedState.Receiving;
                   // RFID_232.WriteLine("01100000000C");
                  //  RFID_232.WriteLine("01000000000C");

                    string s = "01000000000C";
                    
                    List<byte> DataTemp = new List<byte>();

                    for (int i = 0; i < s.Length; i++)
                        DataTemp.Add(Convert.ToByte(s[i]));
                        



                    DataTemp.Add(0x0d);

                       
                    
                    RFID_232.Write(DataTemp.ToArray(), 0, DataTemp.ToArray().Length);    // New
                         
                         
                     

                    TimeOutCheck.Enabled = true;
                    break;
                case RfidReaderType.Brillian:
                    if (!RFID_232.IsOpen)
                    {
                        ReadComplete(RFID_232.PortName + " is not open", false);
                        return false;
                    }
                    ReceivedState = RfidReceivedState.Receiving;
                    RFID_232.Write("~" + DeviceId + " RMID*");
                    TimeOutCheck.Enabled = true;
                    break;
             
            }
            return true;
        }
        public bool AreYouThere()
        {
            switch (ReaderType)
            {
                case RfidReaderType.Omron:
                    break;
                case RfidReaderType.Brillian:
                    if (!RFID_232.IsOpen)
                        return false;
                    ReceivedState = RfidReceivedState.Receiving;
                    RFID_232.Write("~" + DeviceId + " RU*");
                    break;
            }
            return true;
        }

        //=====Receive=====
        private void RFID_232_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string RecString = String.Empty;
            byte[] raw_byte = new byte[128];
            SerialPort sp = (SerialPort)sender;
            if (sp.BytesToRead > 0)
            {
                Int32 length = sp.Read(raw_byte, 0, raw_byte.Length);
                Array.Resize(ref raw_byte, length);
                RecString = DataCombine(raw_byte);
            }
            if (RecString == String.Empty)
                return;
            try
            {
                switch (ReaderType)
                {
                    case RfidReaderType.Brillian:
                        BrillianRecHandler(RecString);
                        break;
                    case RfidReaderType.Omron:
                        OmromRecHandler(RecString);
                        break;
                }
            }
            catch (Exception ex)
            { ReadComplete(ex.ToString() + "/" + RecString, false); }
        }
        private string DataCombine(byte[] raw_data)
        {
            for (int i = 0; i < raw_data.Length; i++)
            {
                switch (ReceivedState)
                {
                    case RfidReceivedState.Idle:
                        //錯誤時間收到訊號 不存
                        break;
                    case RfidReceivedState.Receiving:
                        if (raw_data[i] != LastByte)
                        {
                            Temp.Add(raw_data[i]);
                            break;
                        }
                        //以下已經收到LastByte了
                        TimeOutCheck.Enabled = false;
                        string CompleteString = Encoding.ASCII.GetString(Temp.ToArray());
                        ReceivedState = RfidReceivedState.Idle;
                        Temp.Clear();
                        return CompleteString;
                }
            }
            return String.Empty;
        }

        private void BrillianRecHandler(string RecString)
        {
            string[] FeedBack = RecString.Split(' ');
            if (FeedBack[1] == "RMIDR")
            {
                if (FeedBack[2] == "NO")
                    ReadComplete(FeedBack[3], true);
                else
                    ReadComplete("Fail," + RecString, false);
            }
            else if (FeedBack[1] == "RUR")
            {
                CheckComplete(FeedBack[2]);
            }
        }
        private void OmromRecHandler(string RecString)
        {
            //"0055555555555555550000000000000000"
          // string _temp = RecString.Substring(2, 16);


            //int length = RecString.Length / 2;
            string _result = string.Empty;
            for (int i = 0; i < RecString.Length; i +=2)
            {
                if (RecString.Substring(i, 2) == "00")
                    continue;

                int s = Convert.ToInt32(RecString.Substring(i , 2) , 16);
                string v = Char.ConvertFromUtf32(s);
                _result += v;
            }

            if (_result.Length > 0)
                ReadComplete(_result, true);
            else
                ReadComplete(_result, false);

        }
    
    }
}
