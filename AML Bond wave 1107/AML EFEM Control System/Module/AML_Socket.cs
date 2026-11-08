using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AML
{
    public class SocketServer
    {
        //=====Event=====
        public delegate void DataReceived_Delegate(string Data);//接到資料
        public delegate void DataReceived_Byte_Delegate(byte[] Data);//接到資料
        public delegate void Connect_Delegate();//對方來連線
        public delegate void Close_Delegate();//自己關掉
        public delegate void Listening_Delegate();//正在Listening
        public delegate void Disconnect_Delegate();//對方關閉
        public delegate void SocketError_Delegate(string ex);//無法開啟或不明錯誤
        public event DataReceived_Delegate DataArrivalEvent;
        public event DataReceived_Byte_Delegate DataArrivalByteEvent;
        public event Connect_Delegate ConnectEvent;
        public event Close_Delegate CloseEvent;
        public event Listening_Delegate ListeningEvent;
        public event Disconnect_Delegate DisconnectEvent;
        public event SocketError_Delegate ErrorEvent;

        //=====Socket參數=====
        private Socket SckServer;
        private Socket SckClient;
        private string Server_IP = "127.0.0.1";
        private int Server_Port = 5000;
        private int ServerBufferSize = 32767;

        public const int ServerClosed = 0;
        public const int ServerListening = 1;
        public const int ServerConnect = 2;

        //======Socket======
        public SocketServer()
        {
        }
        public SocketServer(int DataBufferSize)
        {
            ServerBufferSize = DataBufferSize;
        }
        public void Listening(string IP, int port)
        {
            Socket_Close();
            Server_IP = IP;
            Server_Port = port;
            Listening();
        }
        private async void Listening()
        {
            try
            {
                Socket_Close();
                await Task.Delay(50);
                SckServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SckServer.Bind(new IPEndPoint(IPAddress.Parse(Server_IP), Server_Port));
                SckServer.Listen(1);//1
                SckServer.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 1000, 1000), null);
                Thread SckSReceiveTd = new Thread(Thread_Server);
                SckSReceiveTd.Start();
            }
            catch (SocketException ex)
            {
                ErrorEvent(ex.Message);
            }
            catch (Exception ex)
            {
                ErrorEvent(ex.ToString());//目前還沒遇到過 有遇到的跟我講要寫什麼進去
            }
        }
        public void Socket_Close()
        {
            if (SckClient != null)
                SckClient.Close();
            if (SckServer != null)
                SckServer.Close();
        }

        //======通訊相關======
        public bool SendData(string data)
        {
            try
            {
                if (SckClient == null)
                    return false;
                if (!SckClient.Connected)
                    return false;

                SckClient.Send(Encoding.ASCII.GetBytes(data));
                return true;
            }
            catch (Exception ex)//有遇到的跟我講
            {
                ErrorEvent(ex.ToString());
                return false;
            }
        }
        public bool SendData(byte[] data)
        {
            try
            {
                if (SckClient == null)
                    return false;
                if (!SckClient.Connected)
                    return false;

                SckClient.Send(data);
                return true;
            }
            catch (Exception ex)//有遇到的跟我講
            {
                ErrorEvent(ex.ToString());
                return false;
            }
        }

        private void Thread_Server()
        {
            try
            {
                byte[] data = new byte[ServerBufferSize];
                int DataCount;
                if (ListeningEvent != null)
                    ListeningEvent();

                SckClient = SckServer.Accept();//錯誤為10004 
                SckServer.Close();
                ConnectEvent();

                while (true)
                {
                    DataCount = SckClient.Receive(data);//錯誤為10053              
                    if ((SckClient.Poll(1, SelectMode.SelectRead) && SckClient.Available == 0))//有些Socket在這邊斷線(winsck)
                    {
                        ClientDisconnect();
                        break;
                    }

                    if (DataArrivalEvent != null)
                        DataArrivalEvent(Encoding.Default.GetString(data).Substring(0, DataCount));

                    Array.Resize(ref data, DataCount);
                    if (DataArrivalByteEvent != null)
                        DataArrivalByteEvent(data); //byte[] event;
                }
            }
            catch (SocketException ex)//通常出現在手動關閉 需要重新跑Listening
            {
                if (ex.ErrorCode == 10053 || ex.ErrorCode == 10004)
                    CloseEvent();
                else if ((ex.ErrorCode == 10054) || (ex.ErrorCode == 10060))//有些Socket跑來這邊
                    ClientDisconnect();
                else
                    ErrorEvent("Socket error code: " + ex.ErrorCode.ToString());//有遇到的跟我講
            }
            catch (Exception ex)
            {
                ErrorEvent("Socket error code: " + ex.ToString());//有遇到的跟我講
            }
        }
        private void ClientDisconnect()
        {
            if (DisconnectEvent != null)
                DisconnectEvent();
            // Listening();
        }
        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)//不知道幹嘛用的 知道的跟我講
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes((uint)onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }
    }

    public class SocketClient
    {
        //=====Event=====
        public delegate void DataReceived_Delegate(string Data);//接到資料
        public delegate void DataReceived_Byte_Delegate(byte[] Data);//接到資料
        public delegate void Connect_Delegate();//連線成功
        public delegate void Close_Delegate();//自己關掉
        public delegate void Disconnect_Delegate();//Server斷線、關閉、沒開
        public delegate void SocketError_Delegate(string ex);//不明Error遇到請描述狀況通知我
        public event DataReceived_Delegate DataArrivalEvent;
        public event DataReceived_Byte_Delegate DataArrivalByteEvent;
        public event Connect_Delegate ConnectEvent;
        public event Close_Delegate CloseEvent;
        public event Disconnect_Delegate DisconnectEvent;
        public event SocketError_Delegate ErrorEvent;

        //=====Socket參數=====
        private Socket SckClient;
        private string Client_IP = "127.0.0.1";
        private int Client_Port = 5000;
        private int ClientBufferSize = 32767;
        //System.Timers.Timer _timer;
        //_timer = new System.Timers.Timer(500);
        //_timer.Enabled = false;
        //_timer.Elapsed += new System.Timers.ElapsedEventHandler(ConnectTimer);

        public const int ClientDisconnect = 0;
        public const int ClientConnect = 1;


        //======Socket======
        public SocketClient()
        {

        }
        public SocketClient(string IP, int port)
        {
            Client_IP = IP;
            Client_Port = port;
            // Socket_Connect();
        }
        public SocketClient(int DataBufferSize)
        {
            ClientBufferSize = DataBufferSize;
        }

        public void Socket_Connect(string IP, int port)
        {
            Client_IP = IP;
            Client_Port = port;
            Socket_Connect();
        }
        private async void Socket_Connect()
        {
            try
            {
                Socket_Close();
                await Task.Delay(50);
                Thread SckReceiveTd = new Thread(Thread_Client);
                SckReceiveTd.IsBackground = true;
                SckReceiveTd.Priority = ThreadPriority.Highest;
                SckReceiveTd.Start();
            }
            catch (Exception ex)//目前還沒遇到過 有遇到的跟我講要寫什麼進去
            {
                ErrorEvent(ex.ToString());
            }
        }
        public void Socket_Close()
        {
            try
            {
                if (SckClient != null)
                    SckClient.Close();
            }
            catch (Exception ex)//目前還沒遇到過 有遇到的跟我講要寫什麼進去
            {
                ErrorEvent(ex.ToString());
            }
        }

        //======通訊相關======
        private void Thread_Client()
        {
            try
            {
                byte[] data = new byte[ClientBufferSize];
                int DataCount;
                SckClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SckClient.NoDelay = true;
                SckClient.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 1000, 1000), null);
                SckClient.Connect(new IPEndPoint(IPAddress.Parse(Client_IP), Client_Port));//10061

                if (ConnectEvent != null)
                    ConnectEvent();

                while (true)
                {
                    DataCount = SckClient.Receive(data);//Server:10054 Client:10053,10004
                    if ((SckClient.Poll(1, SelectMode.SelectRead) && SckClient.Available == 0))//有些Socket在這邊斷線(winsck)
                    {
                        if (DisconnectEvent != null)
                            DisconnectEvent();
                        break;
                    }

                    if (DataArrivalEvent != null)
                        DataArrivalEvent(Encoding.Default.GetString(data).Substring(0, DataCount));

                    if (DataArrivalByteEvent != null)
                    {
                        Array.Resize(ref data, DataCount);
                        DataArrivalByteEvent(data); //byte[] event;
                    }
                }
            }
            catch (SocketException ex)
            {
                Socket_Close();

                //Console.WriteLine(ex.ToString());
                if ((ex.ErrorCode == 10053) || (ex.ErrorCode == 10004))//主動關閉
                {
                    if (CloseEvent != null)
                        CloseEvent();
                }
                else if (ex.ErrorCode == 10038 || ex.ErrorCode == 10054 || ex.ErrorCode == 10061 || ex.ErrorCode == 10062 || ex.ErrorCode == 10060)//Server關閉或沒開
                {
                    if (DisconnectEvent != null)
                        DisconnectEvent();
                }
                else
                {
                    //10022
                    if (ErrorEvent != null)
                        ErrorEvent("Socket error code: " + ex.ErrorCode.ToString() + ",Error message: " + ex.Message);//有遇到的跟我講
                }
            }
            catch (Exception ex)
            {
                if (ErrorEvent != null)
                    ErrorEvent(ex.ToString());//有遇到的跟我講
            }
        }
        public bool SendData(string data)
        {
            try
            {
                if (SckClient == null)
                    return false;
                if (!SckClient.Connected)
                    return false;

                SckClient.Send(Encoding.ASCII.GetBytes(data));
                return true;
            }
            catch (Exception ex)//有遇到的跟我講
            {
                ErrorEvent(ex.ToString());
                return false;
            }
        }
        public bool SendData(byte[] data)
        {
            try
            {
                if (SckClient == null)
                    return false;
                if (!SckClient.Connected)
                    return false;

                SckClient.Send(data);
                return true;
            }
            catch (Exception ex)//有遇到的跟我講
            {
                ErrorEvent(ex.ToString());
                return false;
            }
        }


        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)//不知道幹嘛用的 知道的跟我講
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes((uint)onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }
    }
}
