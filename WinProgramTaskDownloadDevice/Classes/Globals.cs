using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace WinProgramTaskDownloadDevice.Classes
{
    public class Globals
    { 
        /// <summary>
        /// 检查停止时间
        /// </summary>
        public static DateTime ExecuteStopDate; 

        /// <summary>
        /// 标记手动启动按钮
        /// </summary>
        public static bool IsClicked = false; 
         
        /// <summary>
        /// 节目任务下载器的ID
        /// </summary>
        public static string ProgramTaskDownloadDeviceID;

        /// <summary>
        /// 任务请求URL
        /// </summary>
        public static string TaskDataRequestURL=string.Empty;
       
        /// <summary>
        /// 计算机IP
        /// </summary>
        public static string ComputerIP;

        /// <summary>
        /// 计算机名称
        /// </summary>
        public static string ComputerName;

        /// <summary>
        /// 任务请求 间隔时间单位：秒
        /// </summary>
        public static int TaskRequestInterval=1;

        public const string  TingTingFM= "oJI9t7GP#fXK";

        public static TingTingAPIService TingTingAPIService = new TingTingAPIService();

        public static bool IsFree = false;

        /// <summary>
        /// 
        /// </summary>
        public static int TaskTimeOut = 60;

        /// <summary>
        /// 
        /// </summary>
        public static int TaskExecuteOutThreshold = 1;

        public static int TaskExecuteTime = 60;

    }
}
