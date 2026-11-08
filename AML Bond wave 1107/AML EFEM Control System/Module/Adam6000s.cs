using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using Advantech.Adam;
using System.Threading.Tasks;

namespace AML
{
    public class Adam6000s
    {
        private object LockConnect = new object();
        public delegate void DI_ChangeEvent(string AdamName, int Index, bool Result);
        public delegate void AdamCmdResult(string AdamName, string AdamCmd, bool Result);
        public event DI_ChangeEvent DI_Change;
        public event AdamCmdResult AdamResult;

        private AdamSocket ADAM6000s;
        private Thread AdamControl;
        private AML_LogPage Log;
        bool firstlink = true;

        private bool[] DI_data;
        private bool[] DO_data;
        private int[] AI_data;
        private int[] AO_data;
        public bool[] DI
        {
            get { return DI_data; }
        }
        public bool[] DO
        {
            get { return DO_data; }
        }
        public double[] AI
        {
            get
            {
                double[] OutputAI = new double[AI_data.Length];
                for (int i = 0; i < AI_data.Length; i++)
                {
                    if (AITypes[i] == AnalogIO_Type.Voltage)
                        OutputAI[i] = (double)(AI_data[i] * 20.0 / 65535.0) - 10;
                    else if (AITypes[i] == AnalogIO_Type.Current_0_20mA)
                        OutputAI[i] = (double)(AI_data[i] * 20.0 / 65535.0);
                    else
                        OutputAI[i] = (double)(AI_data[i] * 16.0 / 65535.0) + 4;
                }
                return OutputAI;
            }
        }
        public double[] AO
        {
            get
            {
                double[] OutputAO = new double[AO_data.Length];
                for (int i = 0; i < AO_data.Length; i++)
                {
                    if (AOTypes[i] == AnalogIO_Type.Voltage)
                        OutputAO[i] = (double)(AO_data[i] * 10.0 / 4095.0);
                    else if (AOTypes[i] == AnalogIO_Type.Current_0_20mA)
                        OutputAO[i] = (double)(AO_data[i] * 20.0 / 4095.0);
                    else
                        OutputAO[i] = (double)(AO_data[i] * 16.0 / 4095.0) + 4;
                }
                return OutputAO;
            }
        }
        public bool IsConnect
        {
            get { return ADAM6000s.Connected; }
        }

        public bool LastResult = false;

        private int DIStartAddress = 1;
        private int DOStartAddress = 17;
        private int AIStartAddress = 1;
        private int AOStartAddress = 11;

        private int AdamPort = 502;
        public string AdamIP = "192.168.0.1";

        public enum AnalogIO_Type { Voltage, Current_0_20mA, Current_4_20mA }
        public enum AdammSeries { Adam6050, Adam6052, Adam6024, Adam6066 }

        public AdammSeries AdamType;
        public string AdamName = "AdamIO";//寫Log & Event
        private AnalogIO_Type[] AITypes;
        private AnalogIO_Type[] AOTypes;

        public Adam6000s(AdammSeries Adam_Series, AML_LogPage log, string IO_Name, string IP, int Port = 502, int SetTimeout = 500)
        {
            AdamType = Adam_Series;
            AdamIP = IP;
            AdamPort = Port;
            Log = log;

            int DOCount;
            int DICount;
            int AICount;
            int AOCount;

            switch (Adam_Series)
            {
                case AdammSeries.Adam6024:
                    DOCount = 2;
                    DICount = 2;
                    AICount = 6;
                    AOCount = 2;
                    break;
                case AdammSeries.Adam6050:
                    DOCount = 6;
                    DICount = 12;
                    AICount = 0;
                    AOCount = 0;
                    break;
                case AdammSeries.Adam6052:
                    DOCount = 8;
                    DICount = 8;
                    AICount = 0;
                    AOCount = 0;
                    break;
                case AdammSeries.Adam6066:
                    DOCount = 6;
                    DICount = 6;
                    AICount = 0;
                    AOCount = 0;
                    break;
                default:
                    return;
            }
            AdamName = IO_Name;
            DI_data = new bool[DICount];
            DO_data = new bool[DOCount];
            AI_data = new int[AICount];
            AO_data = new int[AOCount];

            AITypes = new AnalogIO_Type[AICount];
            AOTypes = new AnalogIO_Type[AOCount];
            for (int i = 0; i < AICount; i++)
                AITypes[i] = AnalogIO_Type.Voltage;//目前為暫定
            for (int i = 0; i < AOCount; i++)
                AOTypes[i] = AnalogIO_Type.Voltage;//目前為暫定

            ADAM6000s = new Advantech.Adam.AdamSocket(Advantech.Adam.AdamType.Adam6000);
            ADAM6000s.SetTimeout(SetTimeout, SetTimeout, SetTimeout);

            AdamControl = new Thread(BGAdamControl_DoWork);
            AdamControl.Name = IO_Name + " thread";
        }

        void Connect_and_Start()
        {
            try
            {
                AdamConnect();
                AdamControl.Start();
            }
            catch
            {

            }
        }

        public async void StartAdam()
        {
            await Task.Run(() => Connect_and_Start());
        }

        /*
        public void StartAdam()
        {
            try
            {
                AdamConnect();
                AdamControl.Start();
            }
            catch { }
        }*/
        public void AdamConnect()
        {
            if (ADAM6000s.Connected)
                ADAM6000s.Disconnect();
            ADAM6000s.Connect(AdamIP, System.Net.Sockets.ProtocolType.Tcp, AdamPort);
        }
        public void AdamConnect(string IP, int Port = 502)
        {
            AdamIP = IP;
            AdamPort = Port;
            AdamConnect();
        }

        //=====執行緒=====
        private void BGAdamControl_DoWork()
        {
            while (true)
            {
                ReadAO();
                ReadDO();

                while (ADAM6000s.Connected)
                {
                    try
                    {
                        ReadDI();
                        ReadAI();
                    }
                    catch (Exception ex)
                    { 
                        Log.WriteSystemLog("ADAM" + AdamName, ex.ToString());
                    }

                    Thread.Sleep(500);
                }
                AdamConnect();
                firstlink = true;
                Thread.Sleep(1000);
            }
        }

        //=====Function=====
        public bool WriteDO(int DO_Index, bool value)
        {
            if (!ADAM6000s.Connected)
                return false;
            if (DO_Index<0 || DO_Index > DO_data.Length)
                return false;
            try
            {
                lock (LockConnect)
                {
                    LastResult = ADAM6000s.Modbus().ForceSingleCoil(DOStartAddress + DO_Index, value);
                    if (LastResult)
                        DO_data[DO_Index] = value;
                    return LastResult;
                }
            }
            catch (Exception ex)
            { Log.WriteSystemLog("ADAM" + AdamName, "SetDO:" + DO_Index.ToString() + "," + value.ToString() + "," + ex.ToString()); }
            return false;
        }
        public bool WriteAllDO(bool[] value)
        {
            if (!ADAM6000s.Connected)
                return false;
            if (value.Length != DO_data.Length)
                return false;
            try
            {
                lock (LockConnect)
                {
                    LastResult = ADAM6000s.Modbus().ForceMultiCoils(DOStartAddress, value);
                    if (LastResult)
                        DO_data = value;
                    return LastResult;
                }
            }
            catch (Exception ex)
            { Log.WriteSystemLog("ADAM" + AdamName, "SetDO:" + ex.ToString()); }
            return false;
        }
        public bool WriteAO(int AO_Index, int value)
        {
            if (!ADAM6000s.Connected)
                return false;
            if (AO_Index < 0 || AO_Index >= AO_data.Length)
                return false;
            if (value < 0)
                value = 0;
            else if (value > 4095)
                value = 4095;

            try
            {
                lock (LockConnect)
                {
                    LastResult = ADAM6000s.Modbus().PresetSingleReg(AOStartAddress + AO_Index, value);
                    if (LastResult)
                        AO_data[AO_Index] = value;
                    return LastResult;
                }
            }
            catch (Exception ex)
            { Log.WriteSystemLog("ADAM" + AdamName, "SetAO:" + AO_Index.ToString() + "," + value.ToString() + "," + ex.ToString()); }
            return false;
        }

        //=====抓狀態=====
        private void ReadDI()
        {
            if (!ADAM6000s.Connected)
                return;
            if (DI_data.Length == 0)
                return;
            bool[] Old_data = DI_data;
            bool[] New_data;

            lock (LockConnect)
            {
                LastResult = ADAM6000s.Modbus().ReadInputStatus(DIStartAddress, DI_data.Length, out New_data);
                if (LastResult)
                    DI_data = New_data;
            }
            for (int i = 0; i < DI_data.Length; i++)
            {
                if (firstlink) //重新連線後強制報DI Change Event
                {
                    //firstlink = false;
                    //if (DI_Change != null & !DI_data[i] ) //false才報
                    if (DI_Change != null)
                        DI_Change(AdamName, i, DI_data[i]);
                }
                else
                {
                    if (Old_data[i] != DI_data[i])
                        if (DI_Change != null)
                            DI_Change(AdamName, i, DI_data[i]);
                }
            }

            if (firstlink) //重新連線後強制報DI Change Event
            {
                firstlink = false;
            }
        }
        private void ReadDO()
        {
            if (!ADAM6000s.Connected)
                return;
            if (DO_data.Length == 0)
                return;
            bool[] New_data;

            lock (LockConnect)
            {
                LastResult = ADAM6000s.Modbus().ReadCoilStatus(DOStartAddress, DO_data.Length, out New_data);
                if (LastResult)
                    DO_data = New_data;
            }
        }
        private void ReadAI()
        {
            if (!ADAM6000s.Connected)
                return;
            if (AI_data.Length == 0)
                return;
            int[] New_data;

            lock (LockConnect)
            {
                LastResult = ADAM6000s.Modbus().ReadHoldingRegs(AIStartAddress, AI_data.Length, out New_data);
                if (LastResult)
                    AI_data = New_data;
            }
        }
        private void ReadAO()
        {
            if (!ADAM6000s.Connected)
                return;
            if (AO_data.Length == 0)
                return;
            int[] New_data;

            lock (LockConnect)
            {
                LastResult = ADAM6000s.Modbus().ReadInputRegs(AOStartAddress, AO_data.Length, out New_data);
                if (LastResult)
                    AI_data = New_data;
            }
        }
    }
}
