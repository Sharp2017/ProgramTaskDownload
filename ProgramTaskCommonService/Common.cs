using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ProgramTaskCommonService
{
   public class Common
    {
        
        // 时间戳转为C#格式时间
        public DateTime StampToDateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);

            return dateTimeStart.Add(toNow);
        }

        // DateTime时间格式转换为Unix时间戳格式
        public static int DateTimeToStamp(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// 获取当前机器的IP（所有）
        /// </summary>
        /// <returns></returns>
        public static string GetComputerIP()
        {
            string _ComputerIP = "";
            IPAddress[] IPAddressList = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            if (IPAddressList != null)
            {
                foreach (IPAddress item in IPAddressList)
                {
                    if (item.AddressFamily == AddressFamily.InterNetwork)
                    {
                        _ComputerIP += item + "|";
                    }
                }
            }
            if (_ComputerIP.Length > 0 && _ComputerIP.LastIndexOf('|') == _ComputerIP.Length - 1)
            {
                _ComputerIP = _ComputerIP.Substring(0, _ComputerIP.Length - 1);
            }

            return _ComputerIP;
        }


        public static void WriteLocalLog(string conent)
        {
            try
            {
                if (!Directory.Exists(Application.StartupPath + "\\log"))
                {
                    Directory.CreateDirectory(Application.StartupPath + "\\log");
                }

                string fileName = DateTime.Now.ToString("yyyyMMdd") + "ErrorLog.txt";

                StreamWriter stream = File.AppendText(Application.StartupPath + "\\log\\" + fileName);
                stream.WriteLine(DateTime.Now + ":  " + conent);
                stream.Flush();
                stream.Close();

            }
            catch (System.Exception ex)
            {

            }

        }

        public static bool IsAppRunning(string appName)
        {
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();

            int count = 0;
            foreach (System.Diagnostics.Process myProcess in myProcesses)
            {
                if (myProcess.ProcessName.ToLower() == (appName.ToLower()))
                {
                    count++;
                }
            }
            if (count > 1)
            {

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 刷新内存
        /// </summary>
        public static void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            System.Diagnostics.Process.GetCurrentProcess().MinWorkingSet = new System.IntPtr(5);
        }


    }
}
