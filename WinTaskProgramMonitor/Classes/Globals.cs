using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskDataInfo;

namespace WinTaskProgramMonitor.Classes
{
    public class Globals
    {
        /// <summary>
        /// 检查停止时间
        /// </summary>
        public static DateTime ExecuteStopDate;

        /// <summary>
        /// 监控执行 间隔时间单位：秒
        /// </summary>
        public static int MonitorInterval;

        /// <summary>
        /// 任务下载器信息
        /// </summary>
        public static ProgramInfo ProgramTaskDownloadDevice =new ProgramInfo();

        /// <summary>
        /// 任务执行器信息
        /// </summary>
        public static ProgramInfo ProgramTaskActuatorDevice = new ProgramInfo();

        /// <summary>
        /// 网闸IP
        /// </summary>
        public static string GateWayIP = "";

    }
}
