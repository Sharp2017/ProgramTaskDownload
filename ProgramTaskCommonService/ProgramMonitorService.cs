using Dart.PowerTCP.Ftp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace ProgramTaskCommonService
{
    /// <summary>
    /// 程序监控服务
    /// </summary>
  public  class ProgramMonitorService
    {
        #region //程序启动和消息 
      
        public static bool ISProgramRunning(string ProcessName)
        {
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();

            int count = 0;
            foreach (System.Diagnostics.Process myProcess in myProcesses)
            {
                if ((myProcess.ProcessName.ToLower().Contains(ProcessName.ToLower())))
                {
                    count++;
                    break;
                }
            }
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据路径启动程序
        /// </summary>
        /// <param name="pAppPath">程序路径</param>
        /// <param name="pArgs">启动参数</param>
        /// <param name="pError">错误信息</param>
        /// <returns></returns>
        public static bool StartProgram(string pAppPath, string pArgs, out string pError)
        {
            pError = "";
            try
            {
                if (pAppPath != null && File.Exists(pAppPath))
                {
                    Process myprocess = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo(pAppPath, pArgs);
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                    myprocess.StartInfo = startInfo;
                    myprocess.StartInfo.UseShellExecute = false;
                    myprocess.Start();
                    return true;
                }
                else
                {
                    pError = "程序路径错";
                    return false;
                }
            }
            catch (Exception ex)
            {
                pError = ex.Message;
                return false;
            }
           
            
        }

        public static bool SendMessage(string ProcessName, int msg, string WindowName,string ClassName, out string Error)
        {
            Error = "";
            try
            {
                if (ISProgramRunning(ProcessName))
                {
                    IntPtr iHandle = WndProcMsgAPI.FindWindow(ClassName, WindowName);
                    WndProcMsgAPI.PostMessage(iHandle, msg, 1, 1);
                    return true;
                }
                else
                {
                    Error = "程序未启动";
                    return false;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                throw;
            }
           
           
        }

        /// <summary>
        /// 加载外部应用程序
        /// </summary>
        /// <param name="pProcessName">程序进程名称</param>
        /// <param name="pMainWindowName">主窗体名称</param>
        /// <param name="pAppPath">应用程序路径</param>
        /// <param name="pArgs">参数（多个用空格隔开）</param>
        public static bool LoadApplication(string pProcessName, string pAppPath, string pArgs, out string pError)
        {
            pError = "";
            try
            {
                System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
                bool isRunning = false;
                IntPtr iHandle = new IntPtr();
                foreach (System.Diagnostics.Process myProcess in myProcesses)
                {
                    if ((myProcess.ProcessName == pProcessName))
                    {
                        isRunning = true;
                        iHandle = myProcess.MainWindowHandle;
                        break;
                    }
                }
                if (isRunning)
                {
                    
                    WndProcMsgAPI.PostMessage(iHandle, 1157, 1, 1);
                    return true;
                }
                else
                {

                    if (pAppPath != null && File.Exists(pAppPath))
                    {
                        Process myprocess = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo(pAppPath, pArgs);
                        startInfo.WindowStyle = ProcessWindowStyle.Normal;
                        myprocess.StartInfo = startInfo;
                        myprocess.StartInfo.UseShellExecute = false;
                        myprocess.Start();
                        return true;
                    }
                    else
                    {
                        pError = "程序路径错";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                pError = ex.Message;
                return false;
            }
        }
        #endregion

        #region //网络监控

        [DllImport("wininet.dll", EntryPoint = "InternetGetConnectedState")]  
        //判断网络状况的方法,返回值true为连接，false为未连接  
        private extern static bool InternetGetConnectedState(out int conState, int reder);
        /// <summary>
        /// 判断是否可以联网
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static bool InternetGetConnectedState(out int n)
        {
            try
            {
               return InternetGetConnectedState(out n, 0);
            }
            catch (Exception ex)
            {

                throw ex;
            }
           

        }

        #region 方法二
        /// <summary>
        /// 用于检查IP地址或域名是否可以使用TCP/IP协议访问(使用Ping命令),true表示Ping成功,false表示Ping失败 
        /// </summary>
        /// <param name="strIpOrDName">输入参数,表示IP地址或域名</param>
        /// <returns></returns>
        public static bool PingIpOrDomainName(string strIpOrDName)
        {
            try
            {
                Ping objPingSender = new Ping();
                PingOptions objPinOptions = new PingOptions();
                objPinOptions.DontFragment = true;
                string data = "";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int intTimeout = 3000;
                PingReply objPinReply = objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);
                string strInfo = objPinReply.Status.ToString();
                if (strInfo == "Success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool PingIpOrDomainName(string strIpOrDName,out string Error)
        {
            Error = "";
            try
            {
                Ping objPingSender = new Ping();
                PingOptions objPinOptions = new PingOptions();
                objPinOptions.DontFragment = true;
                string data = "";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int intTimeout = 3000;
                PingReply objPinReply;//= objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);
                string strInfo="";//= objPinReply.Status.ToString();
                int count = 0;
                while(strInfo != "Success"&& count<4)
                {
                    objPinReply = objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);
                    strInfo = objPinReply.Status.ToString();
                    count++;
                }
                if (strInfo == "Success")
                {
                    return true;
                }
                else
                {
                    Error = strInfo;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return false;
            }
        }
        #endregion

        #endregion

        #region //FTP监测

        public static bool CheckFtp(string Server, string port,   string Username, string Password)
        {
            //Ftp ftp = new Ftp();
            //try
            //{
            //    ftp.Server = Server;
            //    ftp.ServerPort = Convert.ToInt16(port);
            //    ftp.Username = Username;
            //    ftp.Password = Password;
            //    ftp.Passive = true;
            //    ftp.DoEvents = true;
            //    string dir = ftp.GetDirectory();
            //    return true;
            //}
            //catch
            //{
            //    return false;
            //}

            return true;
        }
        #endregion
    }

    public static class WndProcMsgAPI
    {
        // Get a handle to an application window.
        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName,
        string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetBackgroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hwnd, int msg, int wparam, int lparam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, UInt32 uFlags);

    }

}
