using Infomedia.XStudio.XstudioDataInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinProgramTaskActuatorDevice.RightManagerWS;

namespace WinProgramTaskActuatorDevice.Classes
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
        /// 节目任务执行器的ID
        /// </summary>
        public static string ProgramTaskActuatorDeviceID;

        /// <summary>
        /// 任务请求URL
        /// </summary>
        public static string TaskDataRequestURL = string.Empty;

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
        public static int TaskActuatorInterval=2;

        public const string TingTingFM = "oJI9t7GP#fXK";

        public static TingTingAPIService TingTingAPIService = new TingTingAPIService();

        /// <summary>
        /// 音频质检参数
        /// </summary>
        public static AudioCheckSettingsInfo AudioCheckInfo=new AudioCheckSettingsInfo();

        /// <summary>
        ///授权系统ID Xstudio
        /// </summary>
        public static Guid CurrentSystemID = Guid.Empty;
        public static RightManagerDS.SystemsRow sysInfo;
        /// <summary>
        /// 系统Key
        /// </summary>
        public static string SystemKey ="";
        public static string RightManagerURL = "";
        public static string RightManagerURL2 = "";

        public static Class2RightManagerWebService RightManagerWebService = new Class2RightManagerWebService();

        public static string ResourceManagerURL = "";
        public static string ResourceManagerURL2 = "";
        public static Class2ResourceManagerWebService ResourceManager = new Class2ResourceManagerWebService();

        public static string ProgramLibName = "";

        public static string ProgramLibID;
        /// <summary>
        /// 类别的最大空间（小时）
        /// </summary>
        public static int MaxSpaceHour = 20;

        public static readonly int HourS48InStorageMB = 32 * 3600 / 1024;

        public static string XStudioWebServiceURL = "";
        public static string XStudioWebServiceURL2 = "";
        public static string XStudioWebServiceBadURL = "";
        public static Class2XStudioWebService XStudioWebService = new Class2XStudioWebService();

        public const string FolderRightName = "素材类别权【适用于制作系统、资源管理器】$$访问类别权|素材类别权【适用于制作系统、资源管理器】$$制作节目权|素材类别权【适用于制作系统、资源管理器】$$删除节目权";
        public const string ProgramLibRightName = "素材库权【适用于制作系统、资源管理器】$$访问权";//"素材库权【适用于制作系统、资源管理器】$$制作节目权";
        public static string FolderRightKey = "XStudio.004Folder.001Access";
        public static string FolderRightID = "";
        public static string ProgramLibRightID = "";
        public static string ProgramLibRightKey = "";
        public static ProgramLibDataInfo MyProgramLibDataInfo = null;

        public static string Zone = "1";

        public static StorageDataInfo StorageInfo = null;

     

        //public static int UserSpaceAlarmValue = 10;

        public static bool IsFree = false;

        /// <summary>
        /// 
        /// </summary>
        public static int TaskTimeOut = 60;

        /// <summary>
        /// 
        /// </summary>
        //public static int TaskExecuteOutThreshold = 60;

        public static string GateWayIP = "";
        public static string FTPIP = "";
        public static string FTPPort = "";
        /// <summary>
        /// FTP1 ,2,3,4
        /// </summary>
        public static string StorageInfoID = "";
        /// <summary>
        /// 文件临时下载目录
        /// </summary>
        public static string TempFileDownLoadPath = @"D:";
    }
}
