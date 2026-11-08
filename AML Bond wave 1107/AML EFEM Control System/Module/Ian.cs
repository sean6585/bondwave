using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.IO.Compression;

using System.Windows.Forms;
using System.Management;
using System.Net.Mail;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
//using Excel = Microsoft.Office.Interop.Excel;
//using Word = Microsoft.Office.Interop.Word;


//using System.DirectoryServices;
//using System.DirectoryServices.Protocols;
//using System.Security.Permissions;
//加入DLL參考:System.DirectoryServices, System.DirectoryServices.Protocols
using Microsoft.VisualBasic;
using System.Data;
using System.IO.Ports;

namespace AML
{
    public static class Ian
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        //[System.Serializable]
        //public class ManagementClass : System.Management.Instrumentation;

        public static void bringToFront(string title)
        {
            // Get a handle to the Calculator application.
            IntPtr handle = FindWindow(null, title);
            // Verify that Calculator is a running process.
            if (handle == IntPtr.Zero)
            {
                return;
            }
            BringWindowToTop(handle); // 將視窗浮在最上層
            // ShowWindow(handle, 3); // 將視窗最大化
        }
       
        public static string Replay_SHA256(string str)
        {
            string PrivateSHAKey = "十萬青年十萬肝";
            SHA256 sha256 = new SHA256CryptoServiceProvider();//建立一個SHA256
            byte[] source = Encoding.Default.GetBytes(str + PrivateSHAKey);//將字串轉為Byte[]
            byte[] crypto = sha256.ComputeHash(source);//進行SHA256加密
            string result = Convert.ToBase64String(crypto);//把加密後的字串從Byte[]轉為字串
            return result;
        }

        public static void Sleep(ref Object obj,int minisecond)
        {
            int i = 0;
            int th = minisecond / 100;
            while (obj == null)
            {
                System.Threading.Thread.Sleep(100);
                i++;
                if (i > th)
                    break;
            }
        }

        public static bool Timeout(ref bool flag, int timeout_s = 2)
        {

            int timeout_ms = timeout_s * 1000;

            while (true)
            {
                for (int i = 0; i < timeout_ms / 50; i++)
                {
                    System.Threading.Thread.Sleep(50);
                    if (flag == true)
                        return true;
                }
                return false;
            }

        }

        public static bool SetComPortProperty(out SerialPort com, string setting_str)
        {
            string[] token = setting_str.Split(',');


   
            com = new SerialPort();


            //	<Main_COM vale = "COM1,9600,8,N,1" />

            com.PortName = token[0];
            com.BaudRate = Convert.ToInt32(token[1]);
            
            switch(token[3].ToUpper())
            {
                case "M":
                    com.Parity = Parity.Mark;
                    break;
                case "S":
                    com.Parity = Parity.Space;
                    break;
                case "E":
                    com.Parity = Parity.Even;
                    break;
                case "O":
                    com.Parity = Parity.Odd;
                    break;
                case "N":
                    com.Parity = Parity.None;
                    break;
                default:
                    com.Parity = Parity.None;
                    break;
            }

            com.DataBits = Convert.ToInt32(token[2]);

            double stpb = Convert.ToDouble(token[4]);

            if (stpb == 1)
                com.StopBits = StopBits.One;
            else if (stpb == 0)
                com.StopBits = StopBits.None;
            else if (stpb == 1.5)
                com.StopBits = StopBits.OnePointFive;
            else if (stpb == 2)
                com.StopBits = StopBits.Two;
            else
                com.StopBits = StopBits.One;
            
            com.RtsEnable = true;
            com.DtrEnable = true;

          

                return true;
        }

        //private static void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        public class BasicAlarm
        {
            public BasicAlarm(int _alid, string _altx)
            {
                alid = _alid;
                altx = _altx;
            }

            private int alid;
            private string altx;

            public int ALID
            {
                get
                {
                    return alid;
                }
            }
            public string ALTX
            {
                get
                {
                    return altx;
                }
            }
        }

        public class BasicEvent
        {
            public BasicEvent(int _ceid, string _cetx)
            {
                ceid = _ceid;
                cetx = _cetx;
            }

            private int ceid;
            private string cetx;

            public int CEID
            {
                get
                {
                    return ceid;
                }
            }
            public string CETX
            {
                get
                {
                    return cetx;
                }
            }
        }

        public static bool DeleteLineInText(string Path, string delete_contains)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(@Path);
                var sb = new StringBuilder();

                using (var sr = new StreamReader(@Path))
                {
                    //    Node id=2 pose=-3.386 0.608 0 links=1 ~
                    string line;
                    int delete_line = -1;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        line = sr.ReadLine();

                        if (line.Contains(delete_contains))
                        {
                            delete_line = i;
                            break;
                        }
                        sb.AppendLine(line);
                    }

                    if (delete_line != -1)
                    {
                        for (int i = delete_line; i < lines.Length; i++)
                        {
                            if (i == delete_line)
                                continue;

                            line = sr.ReadLine();
                            sb.AppendLine(line);
                        }
                    }

                    sr.Close();
                }

                using (var sr = new StreamWriter(@Path))
                {
                    sr.Write(sb.ToString());
                    sr.Close();
                }

                return true;
            }
            catch
            {
                return false;
            }

        }
   
        public static bool FixLineInText(string Path,string compare_contains,string fix_line)
        {
            try
            {

                //string strData = System.IO.File.ReadAllText("Test.txt");
                //strData = strData.Replace("apple", "APPLE");
                //System.IO.File.WriteAllText("Test.txt", strData);

                string[] lines = System.IO.File.ReadAllLines(@Path);
                var sb = new StringBuilder();

                //using (var sr = new StreamReader(@Path))
                //{
                //    //    Node id=2 pose=-3.386 0.608 0 links=1 ~
                //    //string line;

                //    //for (int i = 0; i < lines.Length; i++)
                //    //{
                //    //    line = sr.ReadLine();

                //    //    if (line.Contains(compare_contains))
                //    //        sb.AppendLine(fix_line);
                //    //    else
                //    //        sb.AppendLine(line);
                //    //}

                //    sr.Close();
                //}

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(compare_contains))
                        sb.AppendLine(fix_line);
                    else
                        sb.AppendLine(lines[i]);
                }

                using (var sr = new StreamWriter(@Path))
                {
                    sr.Write(sb.ToString());
                    sr.Close();
                }


                return true;
            }
            catch
            {
                return false;
            }
        }
  
        public static string SearchLineInText(string Path,string compare_contains)
        {
            string _result = null;
            try
            {
                string[] lines = System.IO.File.ReadAllLines(@Path);
                //using (var sr = new StreamReader(@Path))
                //{
                //    //string line;
                //    for (int i = 0; i < lines.Length; i++)
                //    {
                //        //line = sr.ReadLine();

                //        //if (line.Contains(compare_contains))
                //        //    _result = line;


                //        if (lines[i].Contains(compare_contains))
                //            _result = lines[i];

                //    }

                //    sr.Close();
                //}

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(compare_contains))
                        _result = lines[i];

                }


                return _result;
            }
            catch
            {
                return null;
            }
        }

        //public static string SearchLineInTextAll(string Path, string compare_contains)
        //{
        //    string _result = null;
        //    try
        //    {
        //        string[] lines = System.IO.File.ReadAllLines(@Path);
        //        using (var sr = new StreamReader(@Path))
        //        {
        //            string line;
        //            for (int i = 0; i < lines.Length; i++)
        //            {
        //                line = sr.ReadLine();

        //                if (line.Contains(compare_contains))
        //                    _result = line;

        //            }

        //            sr.Close();
        //        }

        //        return _result;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}


        public static bool FtpUpload( string from_path, string to_path,string url,string user, string password,int port = 21,bool copy_upload = false)
        {
            try
            {

                if (copy_upload)
                {
                    string[] tokens = from_path.Split('\\');

                    string filename = tokens[tokens.Length - 1];

                    string destFile = "D:\\Temp\\" + filename;

                    if (!Directory.Exists(@"D:\\Temp"))
                        Directory.CreateDirectory(@"D:\\Temp");


                    File.Copy(from_path, destFile,true);

                    FtpUpload(destFile, to_path, url, user, password, port, false);

                    File.Delete(destFile);

                }
                else
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + user + "@" + url + ":" + port + "/" + to_path);  //  ftp.XXXX.com.tw/ExcelFile.xls");

                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    // FTP的帳密.
                    request.Credentials = new NetworkCredential(user, password);

                    // 要上傳的檔案
                    StreamReader sourceStream = new StreamReader(from_path);
                    byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    sourceStream.Close();
                    request.ContentLength = fileContents.Length;
                    //request.Timeout = 200;
                    request.UsePassive = true;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(fileContents, 0, fileContents.Length);
                    requestStream.Close();

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                    response.Close();

                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }

        public static bool FtpCreatFolder(string path, string url, string user, string password,int port = 21)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + user + "@" + url + ":" + port + "/" + path);  //  ftp.XXXX.com.tw/ExcelFile.xls");
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(user, password);
                request.Timeout = 1000;
                request.UsePassive = true;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }

        public static bool FtpDownload(string to_path, string from_path, string url, string user, string password, int port = 21, System.IO.FileMode filemode = FileMode.Create)
        {
            try
            {
                string ftpURL = "ftp://" + url + ":" + port + "/";
                string ftpUserID = user;
                string ftpUserPWD = password;
                string ftpFilePath = ftpURL + from_path;
                string tempStoragePath = to_path;

                //FtpWebRequest
                FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpFilePath);
                NetworkCredential ftpCredential = new NetworkCredential(ftpUserID, ftpUserPWD);
                ftpRequest.Credentials = ftpCredential;
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpRequest.UsePassive = true;
                //FtpWebResponse
                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                //Get Stream From FtpWebResponse
                Stream ftpStream = ftpResponse.GetResponseStream();

                using (FileStream fileStream = new FileStream(tempStoragePath, filemode))
                {
                    int bufferSize = 2048;
                    int readCount;
                    byte[] buffer = new byte[bufferSize];

                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                    while (readCount > 0)
                    {
                        fileStream.Write(buffer, 0, readCount);
                        readCount = ftpStream.Read(buffer, 0, bufferSize);
                    }
                }
                ftpStream.Close();
                ftpResponse.Close();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static List<string> FtpGetFileList(string from_path, string url, string user, string password,int port = 21)
        {

            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + url + ":" + port + "/" + from_path);
            List<string> filestxt = new List<string>();

            try
            {
                ftpRequest.Credentials = new NetworkCredential(user, password);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.UsePassive = true;

                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();

                StreamReader MyStreamReader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);
                StreamReader streamReader = new StreamReader(response.GetResponseStream());

                string returnmsg = response.BannerMessage;
                string returncode = response.StatusCode.ToString();

                string[] result = streamReader.ReadToEnd().Replace("\r\n", "\r").Split('\r');

                streamReader.Close();


                for (int i = 0; i < result.Length; i++)
                {
                    filestxt.Add(result[i]);
                }

                filestxt.Remove(string.Empty);
            }
            catch
            {

            }
            return filestxt;
        }

        public static bool SevenZip_Unzip(string filepath,string destination)
        {
            bool _result = false;
            try
            {
                var processStartInfo = new ProcessStartInfo();
               // processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.FileName = @"C:\Program Files\7-Zip\7z.exe";
                processStartInfo.Arguments = "x " + filepath + " -o" + destination;        //@"e C:\test.7z";
                Process.Start(processStartInfo);
                _result = true;
            }
            catch
            {

            }

            return _result;
        }

        public static void OpenExplorer(string path)
        {
            string file = @"C:\Windows\explorer.exe";
            string argument = @"/select, " + path; //d:\MyFolder\456.txt";
            //string argument = @"/select, d:\MyFolder\456.txt";
           Process.Start(file, argument);
        }
    
        public static bool IPCheck(string IP)
        {
            bool _result = true;

            string[] token = IP.Split('.');

            if (token.Length != 4)
            {
                _result = false;

            }
            else
            {
                try
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int num = Convert.ToInt32(token[i]);

                        if (!(num >= 0 && num <= 255))
                        {
                            _result = false;
                        }

                    }
                }
                catch
                {
                    _result = false;
                }
            }
            return _result;

        }

   
        public static bool DeleteOldFileByFolder(string dirName, int days = 90)
        {
            string[] files = Directory.GetFiles(dirName);

            try
            {

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastAccessTime < DateTime.Now.AddDays(-days))
                        fi.Delete();
                }

                return true;
            }
            catch
            {

                return false;
            }
        }

        public static bool DeleteOldFolderByFolder(string dirName, int days = 90)
        {
            //   string[] folders = Directory(dirName);

            try
            {
                string[] folders = Directory.GetDirectories(dirName);
                for (int i = 0; i < folders.Length; i++)
                {
                    if (Directory.GetCreationTime(folders[i]) < DateTime.Now.AddDays(-days))
                    {

                        Directory.Delete(folders[i], true);

                        //string[] files = Directory.GetFiles(folders[i]);
                        //foreach (string file in files)
                        //{
                        //    FileInfo fi = new FileInfo(file);
                        //    fi.Delete();
                        //}
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        public static string ListLatestFileName(string DirectoryPath)
        {
            DirectoryInfo info = new DirectoryInfo(DirectoryPath);
            FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            string _result_str = null;

            if(files.Length > 0)
            {
                _result_str = files[files.Length - 1].Name;
            }

            return _result_str;
        }

        public static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public static double ConvertToDegree(double angle)
        {
            return (angle / Math.PI) * 180;
        }

      
        public static bool IsFileLocked(string file)
        {
            try
            {
                using (File.Open(file, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException exception)
            {
                var errorCode = Marshal.GetHRForException(exception) & 65535;
                return errorCode == 32 || errorCode == 33;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SendAutomatedGmail(string EmailAddr, string EmailPassword, string Title, string Body, List<string> AttachmentsAddr, List<string> ToMail, List<string> CCMail, string SenderName = "")
        {
            try
            {
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();

                //msg.To.Add("b@b.com");可以發送給多人
                for (int i = 0; i < ToMail.Count; i++)
                    msg.To.Add(ToMail[i]);

                //msg.CC.Add("c@c.com");可以抄送副本給多人 
                for (int i = 0; i < CCMail.Count; i++)
                    msg.CC.Add(CCMail[i]);

                if (SenderName == string.Empty)
                    SenderName = EmailAddr;

                //這裡可以隨便填，不是很重要
                msg.From = new MailAddress(EmailAddr, SenderName, System.Text.Encoding.UTF8);
                /* 上面3個參數分別是發件人地址（可以隨便寫），發件人姓名，編碼*/
                msg.Subject = Title;//郵件標題
                msg.SubjectEncoding = System.Text.Encoding.UTF8;//郵件標題編碼
                msg.Body = Body; //郵件內容
                msg.BodyEncoding = System.Text.Encoding.UTF8;//郵件內容編碼 

                for (int i = 0; i < AttachmentsAddr.Count; i++)
                    msg.Attachments.Add(new Attachment(@AttachmentsAddr[i]));  //附件
                //    msg.Attachments.Add(new Attachment(@"D:\test2.docx"));  //附件

                msg.IsBodyHtml = true;//是否是HTML郵件 
                                      //msg.Priority = MailPriority.High;//郵件優先級 

                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential(EmailAddr, EmailPassword); //這裡要填正確的帳號跟密碼
                client.Host = "smtp.gmail.com"; //設定smtp Server
                client.Port = 25; //設定Port
                client.EnableSsl = true; //gmail預設開啟驗證
                client.Send(msg); //寄出信件
                client.Dispose();
                msg.Dispose();
                //MessageBox.Show("郵件寄送成功！");

                return true;
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message);
                return false;
            }

            return false;
        }

        public static UInt64 NStep(int n)
        {
            UInt64 n_64 = (UInt64)n;
            switch (n)
            {
                case 0:
                    return 1;
                case 1:
                    return 1;
                default:
                    return n_64 * NStep(n - 1);
            }

        }

        public static Image RotateImage(Image img, float rotationAngle)
        {
            //create an empty Bitmap image
            Bitmap bmp = new Bitmap(img.Width, img.Height);

            //turn the Bitmap into a Graphics object
            Graphics gfx = Graphics.FromImage(bmp);

            //now we set the rotation point to the center of our image
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

            //now rotate the image
            gfx.RotateTransform(rotationAngle);

            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

            //set the InterpolationMode to HighQualityBicubic so to ensure a high
            //quality image once it is transformed to the specified size
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //now draw our new image onto the graphics object
            gfx.DrawImage(img, new Point(0, 0));

            //dispose of our Graphics object
            gfx.Dispose();

            //return the image
            return bmp;
        }

        public static void WindowsPlusD()
        {
            Type shellType = Type.GetTypeFromProgID("Shell.Application");
            object shellObject = System.Activator.CreateInstance(shellType);
            shellType.InvokeMember("ToggleDesktop", System.Reflection.BindingFlags.InvokeMethod, null, shellObject, null);
        }

        public static string GetLatestFileName(string dir, string ext)
        {
            List<FileTimeInfo> list = new List<FileTimeInfo>();
            DirectoryInfo d = new DirectoryInfo(dir);
            foreach (FileInfo file in d.GetFiles())
            {
                if (file.Extension.ToUpper() == ext.ToUpper())
                {
                    list.Add(new FileTimeInfo()
                    {
                        FileName = file.FullName,
                        FileCreateTime = file.CreationTime
                    });
                }
            }
            var f = from x in list
                    orderby x.FileCreateTime
                    select x;
            return f.LastOrDefault().FileName;
        }

        public static string PrintListString(List<string> ListStr, string Separate = ",")
        {
            string _result = string.Empty;

            if (ListStr == null)
                return "NULL";

            for(int i = 0;i< ListStr.Count;i++)
            {
                _result += ListStr[i];

                if (i != ListStr.Count - 1)
                    _result += Separate;
            }

            return _result;
        }

        public static string PrintListString(List<int> ListInt, string Separate = ",")
        {
            string _result = string.Empty;

            if (ListInt == null)
                return "NULL";

            for (int i = 0; i < ListInt.Count; i++)
            {
                _result += ListInt[i].ToString();

                if (i != ListInt.Count - 1)
                    _result += Separate;
            }

            return _result;
        }


        class FileTimeInfo
        {
            public string FileName;  //檔名
            public DateTime FileCreateTime; //建立時間
        }

        [DllImport("kernel32.dll")]
        private extern static void GetSystemTime(ref SYSTEMTIME lpSystemTime);
        [DllImport("kernel32.dll")]
        private extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        public static void SetSystemTime(SYSTEMTIME systime)
        {
            SetSystemTime(ref systime);
        }

        public static SYSTEMTIME GetSystemTime()
        {
            SYSTEMTIME systime = new SYSTEMTIME();
            GetSystemTime(ref systime);
            return systime;
        }

        public static bool ByPing(string IPv4Address)
        {
            IPAddress tIP = IPAddress.Parse(IPv4Address);
            Ping tPingControl = new Ping();
            PingReply tReply;

            try
            {
                tReply = tPingControl.Send(tIP);
                tPingControl.Dispose();
            }
            catch
            {
                tPingControl.Dispose();
                return false;
            }


            if (tReply.Status != IPStatus.Success)
                return false;
            else
                return true;
        }

        public static void CallPrograme(string FileName,string Arguments = "")
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();             //设定程序名 
            p.StartInfo.FileName = FileName;
            p.StartInfo.Arguments = Arguments;
            p.Start();
        }

        public static Bitmap ScaleImage(Bitmap pBmp, int pWidth, int pHeight)
        {
            try
            {
                Bitmap tmpBmp = new Bitmap(pWidth, pHeight);
                Graphics tmpG = Graphics.FromImage(tmpBmp);

                //tmpG.InterpolationMode = InterpolationMode.HighQualityBicubic;

                tmpG.DrawImage(pBmp,
                                           new Rectangle(0, 0, pWidth, pHeight),
                                           new Rectangle(0, 0, pBmp.Width, pBmp.Height),
                                           GraphicsUnit.Pixel);
                tmpG.Dispose();
                return tmpBmp;
            }
            catch
            {
                return null;
            }
        }

        public static int Convert_PosInt_or_Reply_Neg1(string str)
        {
            int _result = -1;
            try
            {
                _result = Convert.ToInt32(str);
            }
            catch
            {
                _result = -1;
            }

            return _result;
        }

        public static bool DGV_2_CSV(DataGridView DGV,string Filename="")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(*.csv)|*.csv";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.CreatePrompt = true;
            saveFileDialog.Title = "Export csv File";
            saveFileDialog.FileName = Filename;

           
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName == "")
                return false;
            

            Stream myStream;
            myStream = saveFileDialog.OpenFile();
            StreamWriter sw = new StreamWriter(myStream, System.Text.Encoding.GetEncoding(-0));
            string str = "";
            //try
            {
                for (int i = 0; i < DGV.ColumnCount; i++)
                {
                    if (i > 0)
                    {
                        str += "\t";
                    }
                    str += DGV.Columns[i].HeaderText;
                }
                sw.WriteLine(str);
                for (int j = 0; j < DGV.Rows.Count; j++)
                {
                    string tempStr = "";
                    for (int k = 0; k < DGV.Columns.Count; k++)
                    {
                        if (k > 0)
                        {
                            tempStr += "\t";
                        }

                        if (DGV.Rows[j].Cells[k].Value != null)
                            tempStr += DGV.Rows[j].Cells[k].Value.ToString();
                    }
                    sw.WriteLine(tempStr);
                }
                sw.Close();
                myStream.Close();

                return true;
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    return false;
            //}
            //finally
            //{
            //    sw.Close();
            //    myStream.Close();
               
            //}

            return true;
        }
        //dd



        public static List<string> CheckLastFileInFolder(string FolderPath, string ExtetionFileName = "*")
        {
            List<string> _result = new List<string>();

            return _result;
        }

        public static DateTime FileLastWriteTime(string path)
        {
            FileInfo fi = new FileInfo(@path);

            if (fi == null)
                return DateTime.MinValue;
            else
                return fi.LastWriteTime;
        }

        public static DateTime FileCreationTime(string path)
        {
            FileInfo fi = new FileInfo(@path);

            if (fi == null)
                return DateTime.MinValue;
            else
                return fi.CreationTime;        
        }

       

        public static bool CSV_Writer(string FilePath, List<List<string>> raw_data , string seperate = ",")
        {
            try
            {
                StreamWriter sr = new StreamWriter(FilePath);
                
                string txt = "";
                for (int i = 0; i < raw_data.Count; i++)
                {
                    for (int j = 0; j < raw_data[i].Count; j++)
                    {
                        txt += raw_data[i][j];

                        if (j < raw_data[i].Count - 1)
                            txt += seperate;
                    }
                    txt += Environment.NewLine;
                }

                sr.WriteLine(txt);
                sr.Close();
                
                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("CSV_Writer:" + ex.ToString());
                return false;
            }
        }

        public static class EnumHelper
        {
            public static List<T> ToList<T>()
            {
                return Enum.GetValues(typeof(T)).Cast<T>().ToList<T>();
            }

            public static IEnumerable<T> ToEnumerable<T>()
            {
                return Enum.GetValues(typeof(T)).Cast<T>();
            }
        }

    }

   
}
