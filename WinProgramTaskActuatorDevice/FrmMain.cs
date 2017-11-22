
using Infomedia.XStudio.XstudioDataInfo;
using ProgramTaskCommonService;
using ProgramTaskDBService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using TaskDataInfo;
using WinProgramTaskActuatorDevice.Classes;
using WinProgramTaskActuatorDevice.RightManagerWS;

namespace WinProgramTaskActuatorDevice
{
    public partial class FrmMain : Form
    {
        [DllImport("WavePickerDll.dll", EntryPoint = "ExtractWave")]
        public static extern bool ExtractWave(string lpszFileName, int Handle, int msgID, long id);

        Thread ManagerThread;
        Thread SysInitThread;
        Thread ExcThread;
        Dart.PowerTCP.Ftp.Ftp ftp1;
        //string DownloadFilePath = "";
        bool isStart;
        public bool IsStart
        {
            get
            {
                return isStart;
            }

            set
            {
                isStart = value;
                if (isStart)
                {
                    this.btnStart.Image = global::WinProgramTaskActuatorDevice.Properties.Resources.stop;

                    this.btnStart.Text = "停止";
                }
                else
                {
                    this.btnStart.Image = global::WinProgramTaskActuatorDevice.Properties.Resources.Start;

                    this.btnStart.Text = "启动";
                }
            }
        }
        public FrmMain()
        {
            InitializeComponent();
            Application.DoEvents();
            ManagerThread = new Thread(new ThreadStart(this.ThreadManager));
            ManagerThread.IsBackground = true;
            ManagerThread.Start();
        }

        private void InitStorageInfo()
        {
            try
            {
                switch (Globals.StorageInfoID)
                {
                    case "1":
                        Globals.StorageInfo.FTPServer1 = Globals.FTPIP;
                        Globals.StorageInfo.FTPPort1 = Globals.FTPPort;
                        break;
                    case "2":
                        Globals.StorageInfo.FTPServer2 = Globals.FTPIP;
                        Globals.StorageInfo.FTPPort2 = Globals.FTPPort;
                        break;
                    case "3":
                        Globals.StorageInfo.FTPServer3 = Globals.FTPIP;
                        Globals.StorageInfo.FTPPort3 = Globals.FTPPort;
                        break;
                    case "4":
                        Globals.StorageInfo.FTPServer4 = Globals.FTPIP;
                        Globals.StorageInfo.FTPPort4 = Globals.FTPPort;
                        break;
                    default:
                        Globals.StorageInfo.FTPServer1 = Globals.FTPIP;
                        Globals.StorageInfo.FTPPort1 = Globals.FTPPort;
                        break;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region 获取任务 
        /// <summary>
        /// 获取任务，添加任务
        /// </summary>
        private void InsertProgarmTask()
        { 
            string ConvertFileS48Path = "";
            string task_id = null;
            string UserID = null;
            string UserFullName = null;
            string userName = "";
            string FolderID = null;
            string FolderName = null;
            string ftpPath = null;
            //质检报告
            string report = "";
            //回调的错误信息
            string ErrorBack = "";
            bool isTaskOk = false;
            TaskInfo _TaskInfo = null;
            //是否需要回调 
            bool isNeedCoback = true;
            //是否需要重做 0 不重做 1 重做
            int NeedReDo = 0;
            string tempDownloadFilePath = "";
            Application.DoEvents();

            if (txtInfo.Text.Length > txtInfo.MaxLength)
                txtInfo.Text = "";

            if (!isStart)
            {
                AppendMessageLine("任务停止!");
                return;
            }
            try
            {


                #region //网络检查
                int n = 0;
                if (!ProgramMonitorService.InternetGetConnectedState(out n))
                {
                    LogService.Write("设备不可联网");
                    AppendMessageLine("设备不可联网");
                    return;

                }
                string error = "";
                if (!ProgramMonitorService.PingIpOrDomainName(Globals.GateWayIP, out error))
                {
                    LogService.Write("网闸不可访问：" + error);
                    AppendMessageLine("网闸不可访问：" + error);
                    return;

                }

                Globals.StorageInfo = Globals.XStudioWebService.GetStorageInfoByProgramLibID(Globals.ProgramLibID);
                if (Globals.StorageInfo != null)
                {
                    AppendMessageLine("获取存储信息成功!");
                    InitStorageInfo();

                }
                else
                {
                    AppendMessageLine("获取存储信息失败!");
                    return;
                }


                if (!ProgramMonitorService.CheckFtp(Globals.StorageInfo.FTPServer1, Globals.StorageInfo.FTPPort1, Globals.StorageInfo.FTPUser1, Globals.StorageInfo.FTPPassword1))
                {
                    LogService.Write("FTP服务器不可访问");
                    AppendMessageLine("FTP服务器不可访问");
                    return;
                }

                #endregion



                //任务状态(-1:失败任务，0:未开始，2:成功完成)
                #region //任务执行 

                try
                {
                    // _TaskInfo = ProgramTaskDBManager.GetOneUnLockedTask();
                    _TaskInfo = ProgramTaskDBManager.GetOneUnLockedTask(Globals.ComputerName);
                    if (_TaskInfo == null)
                    {
                        if (Globals.IsFree)
                        {
                            AppendMessageLine("未发现新任务!");
                        }
                        Globals.IsFree = true;
                        Application.DoEvents();
                        return;
                    }
                    else
                    {

                        Globals.IsFree = false;
 

                        AppendMessageLine("获取任务成功，\n\t音频名称：" + _TaskInfo.data.audio_name + ",\n\t任务ID：" + _TaskInfo.data.task_id);
                        LogService.Write("获取任务成功，\n\t音频名称：" + _TaskInfo.data.audio_name + ",\n\t任务ID：" + _TaskInfo.data.task_id);

                        task_id = _TaskInfo.data.task_id;
                        _TaskInfo.data.detail_report = "";
                        _TaskInfo.error = "";

                        if (ProgramTaskDBManager.LockTaskByID(task_id, true))
                        {
                            AppendMessageLine("锁定任务成功!");
                            LogService.Write("锁定任务成功!");
                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "锁定任务", 1, "锁定任务成功", DateTime.Now));

                            _TaskInfo.AudioConvertDatetime = DateTime.Now;
                            _TaskInfo.AudioAudioCheckDatetime = DateTime.Now;

                            #region //任务OK 
                            userName = _TaskInfo.data.user_name;
                            UserID = Globals.XStudioWebService.GetUserInfoByUserLoginName(userName, out UserFullName);
                            //00000000 - 0000 - 0000 - 0000 - 000000000000
                            if (UserID != null && UserID != Guid.Empty.ToString() && UserID.Length > 0)
                            {
                                UserDataInfoList _UserDataInfoList = Globals.XStudioWebService.GetUserInfoByUserID(UserID);
                                if (_UserDataInfoList != null && _UserDataInfoList.UserDataInfoArray != null && _UserDataInfoList.UserDataInfoArray.Length > 0)
                                {
                                    ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "获取用户信息", 1, "获取用户信息成功", DateTime.Now));

                                    UserDataInfo _UserDataInfo = _UserDataInfoList.UserDataInfoArray[0];
                                    if (Md5Helper.Md5Encoding(_UserDataInfo.Password).ToLower() == _TaskInfo.data.user_password.ToLower())
                                    {
                                        AppendMessageLine("用户认证成功!Name：" + UserFullName);
                                        LogService.Write("用户认证成功!Name：" + UserFullName);

                                        ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "用户认证", 1, "用户认证成功", DateTime.Now));

                                        #region //素材库访问权验证 

                                        if (this.IsHaveProgramLibRight(Globals.ProgramLibID, Globals.ProgramLibRightName, UserID))
                                        {
                                            AppendMessageLine("用户素材库访问权验证成功!");
                                            LogService.Write("用户素材库访问权验证成功!");
                                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "素材库访问权", 1, "用户" + userName + "素材库访问权验证成功", DateTime.Now));

                                            #region //素材库空间验证 
                                            //this.GetMyProgramLibInfo(UserID);
                                            //if (Globals.MyProgramLibDataInfo != null)
                                            //{
                                            //    AppendMessageLine("获取素材库详情成功!");
                                            //    LogService.Write("获取素材库详情成功!");
                                            //    ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "获取素材库详情", 1, "用户" + userName + "获取素材库详情成功", DateTime.Now));
                                            //    if (Globals.MyProgramLibDataInfo.FreeSpace > 0)
                                            //    {

                                            //    }
                                            //    else
                                            //    {
                                            //        ErrorBack = "素材库" + Globals.MyProgramLibDataInfo.ProgramLibName + "空间不足，大小：" + Globals.MyProgramLibDataInfo.FreeSpace;

                                            //        AppendMessageLine("素材库" + Globals.MyProgramLibDataInfo.ProgramLibName + "空间不足，大小：" + Globals.MyProgramLibDataInfo.FreeSpace);
                                            //        LogService.Write("素材库" + Globals.MyProgramLibDataInfo.ProgramLibName + "空间不足，大小：" + Globals.MyProgramLibDataInfo.FreeSpace);
                                            //        ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "获取素材库详情", -1, "素材库" + Globals.MyProgramLibDataInfo.ProgramLibName + "空间不足，大小：" + Globals.MyProgramLibDataInfo.FreeSpace, DateTime.Now));

                                            //    }
                                            //}
                                            //else
                                            //{

                                            //    AppendMessageLine("获取素材库详情失败!");
                                            //    LogService.Write("获取素材库详情失败!");
                                            //    ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "获取素材库详情", -1, "用户" + userName + "获取素材库详情失败", DateTime.Now));

                                            //    NeedReDo = 1;
                                            //}
                                            #endregion

                                            #region //用户认证OK   

                                            FolderName = userName + "云素材";
                                            FolderID = this.GetCategoryIDByName(2, FolderName);
                                            bool isFolderOK = false;
                                            #region //类别判断 

                                            if (FolderID != null && FolderID.Length > 0)
                                            {
                                                isFolderOK = true;
                                                AppendMessageLine("获取类别信息成功!类别名称：" + FolderName);
                                                LogService.Write("获取类别信息成功!类别名称：" + FolderName);
                                                ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "获取类别信息", 1, "获取类别信息成功", DateTime.Now));

                                            }
                                            else
                                            {
                                                FolderID = Guid.NewGuid().ToString();
                                                string CreatFolderError = "";
                                                if (CreatFolder(FolderName, new Guid(FolderID), out CreatFolderError))
                                                {
                                                    isFolderOK = true;

                                                    AppendMessageLine("类别创建成功!");
                                                    LogService.Write("类别创建成功!");
                                                    ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "类别创建", 1, "类别创建成功", DateTime.Now));
                                                }
                                                else
                                                {
                                                    isFolderOK = false;
                                                    ErrorBack = "类别创建失败";
                                                    AppendMessageLine("类别创建失败!错误信息：" + CreatFolderError);
                                                    LogService.Write("类别创建失败!错误信息：" + CreatFolderError);
                                                    ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "类别创建", -1, "类别创建失败!错误信息：" + CreatFolderError, DateTime.Now));
                                                    NeedReDo = 1;

                                                }
                                            }
                                            #endregion
                                            if (isFolderOK)
                                            {
                                                bool isRightOK = false;

                                                #region //类别OK    

                                                #region //类别权限判断 
                                                if (!this.IsHaveRight(FolderID, Globals.FolderRightName, UserID))
                                                {
                                                    List<Guid> resourceIDs = new List<Guid>();
                                                    resourceIDs.Add(new Guid(FolderID));
                                                    List<Guid> rightIDs = new List<Guid>();
                                                    rightIDs.Add(new Guid(Globals.FolderRightID.Split('|')[0]));
                                                    rightIDs.Add(new Guid(Globals.FolderRightID.Split('|')[1]));
                                                    rightIDs.Add(new Guid(Globals.FolderRightID.Split('|')[2]));
                                                    string ApplyRightError = "";
                                                    if (this.ApplyByResourceIDRightIDUserID(resourceIDs, rightIDs, UserID, out ApplyRightError))
                                                    {
                                                        isRightOK = true;
                                                        AppendMessageLine("类别权限设置成功!");
                                                        LogService.Write("类别权限设置成功!");
                                                        ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "类别权限设置", 1, "类别权限设置成功", DateTime.Now));
                                                    }
                                                    else
                                                    {
                                                        isRightOK = false;
                                                        ErrorBack = "类别权限设置失败";
                                                        AppendMessageLine("类别权限设置失败!");
                                                        LogService.Write("类别权限设置失败!");
                                                        ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "类别权限设置", -1, "类别权限设置失败", DateTime.Now));
                                                        NeedReDo = 1;
                                                    }
                                                }
                                                else
                                                {
                                                    isRightOK = true;
                                                }
                                                #endregion
                                                if (isRightOK)
                                                {
                                                    #region //类别权限OK 

                                                    DataRow dr = GetFolderInfo(UserID, FolderID);
                                                    if (dr != null)
                                                    {
                                                        AppendMessageLine("获取类别存储信息成功!");
                                                        LogService.Write("获取类别存储信息成功!");
                                                        ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "获取类别存储信息", 1, "获取类别存储信息成功", DateTime.Now));
                                                        #region //获取类别存储成功 

                                                        int free = int.Parse(dr["freeSpaceMB"].ToString());

                                                        if (IsHaveSpeac(UserID, FolderID, free))
                                                        {
                                                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "类别空间判断", 1, "类别空间充足", DateTime.Now));
                                                            ftpPath = Globals.ProgramLibName + "\\" + dr["shortname"];

                                                           string   DownloadFilePath = Globals.TempFileDownLoadPath + "\\DownloadTemp\\";
                                                            //如果不存在就创建file文件夹
                                                            try
                                                            {
                                                                if (!Directory.Exists(DownloadFilePath))
                                                                {
                                                                    Directory.CreateDirectory(DownloadFilePath);
                                                                }
                                                                tempDownloadFilePath = DownloadFilePath + _TaskInfo.data.audio_url.Substring(_TaskInfo.data.audio_url.LastIndexOf("/") + 1);
                                                            }
                                                            catch(Exception ex)
                                                            {
                                                                LogService.WriteErr("创建下载路径错误！错误信息：" + ex.Message);
                                                            }  

                                                            string Error = "";
                                                            #region //空间足够 
                                                            if (DownloadFileService.HttpDownloadFile(_TaskInfo.data.audio_url, tempDownloadFilePath, out Error))
                                                            {
                                                                AppendMessageLine("下载文件成功!");
                                                                LogService.Write("下载文件成功!");
                                                                ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "下载文件", 1, "下载文件成功", DateTime.Now));
                                                                if (Md5Helper.getMD5Hash(tempDownloadFilePath).ToLower() == _TaskInfo.data.audio_md5.ToLower())
                                                                {
                                                                    AppendMessageLine("文件验证成功!");
                                                                    LogService.Write("文件验证成功!");
                                                                    ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "文件验证", 1, "文件验证成功", DateTime.Now));

                                                                    #region //下载成功 

                                                                    AppendMessageLine("转码中......");
                                                                    if (ConvertService.DoConvert(tempDownloadFilePath, out Error, out ConvertFileS48Path))
                                                                    {
                                                                        #region //转码成功
                                                                        _TaskInfo.AudioConvertDatetime = DateTime.Now;
                                                                        AppendMessageLine("文件转码成功!");
                                                                        LogService.Write("文件转码成功!");
                                                                        ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "文件转码", 1, "文件转码成功", _TaskInfo.AudioConvertDatetime));
                                                                        AppendMessageLine("质检中......");
                                                                        if (AudioCheckService.AudioCheck(Globals.AudioCheckInfo, ConvertFileS48Path, _TaskInfo.data.audio_name, out report))
                                                                        {
                                                                            _TaskInfo.AudioAudioCheckDatetime = DateTime.Now;
                                                                            AppendMessageLine("文件质检成功!");
                                                                            LogService.Write("文件质检成功!");
                                                                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "文件质检", 1, "文件质检成功", _TaskInfo.AudioAudioCheckDatetime));

                                                                        }
                                                                        else
                                                                        {
                                                                            ErrorBack = "文件质检失败";
                                                                            AppendMessageLine("文件质检失败：" + report);
                                                                            LogService.Write("文件质检失败：" + report);
                                                                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "文件质检", -1, "文件质检失败：" + report, _TaskInfo.AudioConvertDatetime));

                                                                        }

                                                                        #region //质检完成

                                                                        string pImportError = "";
                                                                        if (doImport(_TaskInfo.data.audio_name, ConvertFileS48Path, UserID, UserFullName, FolderID, ftpPath, out pImportError))
                                                                        {
                                                                            #region //入库成功
                                                                            _TaskInfo.TaskCompletionDatetime = DateTime.Now;
                                                                            AppendMessageLine("入库成功!");
                                                                            LogService.Write("入库成功!");
                                                                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "入库", 1, pImportError, DateTime.Now));

                                                                            isTaskOk = true;
                                                                            #endregion
                                                                        }
                                                                        else
                                                                        {
                                                                            ErrorBack = "入库失败";
                                                                            AppendMessageLine("入库失败，原因：" + pImportError);
                                                                            LogService.Write("入库失败，原因：" + pImportError);
                                                                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "入库", -1, "入库失败，原因：" + pImportError, DateTime.Now));
                                                                            NeedReDo = 1;
                                                                        }

                                                                        #endregion

                                                                        #endregion

                                                                    }
                                                                    else
                                                                    {
                                                                        ErrorBack = "文件转码失败";
                                                                        AppendMessageLine(" 文件转码失败：" + Error);
                                                                        LogService.Write(" 文件转码失败：" + Error);
                                                                        ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "文件转码", -1, " 文件转码失败：" + Error, DateTime.Now));

                                                                        NeedReDo = 1;
                                                                    }

                                                                    #endregion
                                                                }
                                                                else
                                                                {
                                                                    ErrorBack = "文件验证失败";
                                                                    AppendMessageLine("文件验证失败");
                                                                    LogService.Write("文件验证失败");
                                                                    ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "文件验证", -1, "文件验证失败", DateTime.Now));
                                                                    NeedReDo = 1;
                                                                }


                                                            }
                                                            else
                                                            {
                                                                ErrorBack = "下载文件失败";
                                                                AppendMessageLine("下载文件失败：" + Error);
                                                                LogService.Write("下载文件失败：" + Error);
                                                                ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "下载文件", -1, "下载文件失败：" + Error, DateTime.Now));
                                                                NeedReDo = 1;

                                                            }

                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            ErrorBack = "类别空间不足";
                                                            AppendMessageLine("类别空间不足!");
                                                            LogService.Write("类别空间不足!");
                                                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "类别空间判断", -1, "类别空间不足", DateTime.Now));
                                                        }

                                                        #endregion

                                                    }

                                                    else
                                                    {
                                                        ErrorBack = "获取类别存储信息失败";
                                                        AppendMessageLine("获取类别存储信息失败!");
                                                        LogService.Write("获取类别存储信息失败!");
                                                        ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "获取类别存储信息", -1, "获取类别存储信息失败", DateTime.Now));
                                                        NeedReDo = 1;

                                                    }

                                                    #endregion

                                                }
                                                #endregion

                                            }

                                            #endregion

                                        }
                                        else
                                        {
                                            ErrorBack = "用户" + userName + "没有素材库访问权";
                                            AppendMessageLine("用户没有素材库访问权!");
                                            LogService.Write("用户没有素材库访问权!");
                                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "素材库访问权验证", -1, "用户" + userName + "没有素材库访问权", DateTime.Now));
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        ErrorBack = "用户密码错误";
                                        AppendMessageLine("用户密码错误!");
                                        LogService.Write("用户密码错误!");
                                        ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "用户认证", -1, "用户密码错误", DateTime.Now));
                                    }
                                }
                                else
                                {
                                    ErrorBack = "获取用户信息失败";
                                    AppendMessageLine("获取用户信息失败!");
                                    LogService.Write("获取用户信息失败!");
                                    ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "获取用户信息", -1, "获取用户信息失败", DateTime.Now));

                                    //return;
                                }

                            }
                            else
                            {
                                ErrorBack = "获取用户信息失败";
                                AppendMessageLine("获取用户信息失败!");
                                LogService.Write("获取用户信息失败!");
                                ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "获取用户信息", -1, "获取用户信息失败", DateTime.Now));
                                //return;
                            }

                            #endregion
                            if (_TaskInfo.TaskState == 0&&!isTaskOk)//新任务执行失败不需要回调
                            {
                                isNeedCoback = false;
                            }

                            _TaskInfo.TaskActuatorDeviceID = Globals.ProgramTaskActuatorDeviceID;
                            _TaskInfo.TaskActuatorDeviceComputerName = Globals.ComputerName;
                            _TaskInfo.TaskActuatorDeviceIP = Globals.ComputerIP;
                            _TaskInfo.TaskCompletionDatetime = DateTime.Now;
                            _TaskInfo.data.detail_report = report;
                            _TaskInfo.NeedReDo = NeedReDo;
                            //错误信息
                            _TaskInfo.error = ErrorBack;//.Replace("!", "");

                            if (isTaskOk)
                            {
                                _TaskInfo.TaskState = 2;
                            }
                            else
                            {
                                _TaskInfo.TaskState = -1;
                            }
                           

                            #region //更新状态成功 
                            if (ProgramTaskDBManager.UpdateTask(_TaskInfo))
                            {
                                AppendMessageLine("更新任务状态成功!");
                                LogService.Write("更新任务状态成功!");
                                ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "更新任务状态", 1, "更新任务状态成功", DateTime.Now));
                            }
                            else
                            {
                                AppendMessageLine("更新状态失败!");
                                LogService.Write("更新状态失败!");
                                ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "更新任务状态", -1, "更新状态失败", DateTime.Now));
                            }

                            #endregion 

                        }

                        else
                        {
                            AppendMessageLine("锁定任务失败!");
                            LogService.Write("锁定任务失败!");
                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "锁定任务", -1, "锁定任务失败", DateTime.Now));

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                #endregion

            }
            catch (Exception ex)
            {
                isNeedCoback = false;
                AppendMessageLine("任务执行中程序错误：" + ex.Message);
                LogService.WriteErr("任务执行中程序错误：" + ex.Message);
            }
            finally
            {
                try
                {
                    #region //反馈回调
                    string error = "";
                    if (isNeedCoback && _TaskInfo != null)
                    {
                        if (Globals.TingTingAPIService.TaskDealCallback(_TaskInfo, out error))
                        {
                            AppendMessageLine("请求回调成功!");
                            LogService.Write("请求回调成功!");
                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "回调", 1, "请求回调成功", DateTime.Now));
                        }
                        else
                        {
                            AppendMessageLine("请求回调失败，错误信息：" + error);
                            LogService.Write("请求回调失败，错误信息：" + error);
                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "回调", -1, "请求回调失败，错误信息：" + error, DateTime.Now));
                        }
                    }

                    #endregion
                }
                catch (Exception ee)
                {

                    AppendMessageLine("任务执行中程序错误：" + ee.Message);
                    LogService.WriteErr("任务执行中程序错误：" + ee.Message);
                }

                #region //清理 

                try
                {
                    if (_TaskInfo != null)
                    {
                        if (ProgramTaskDBManager.LockTaskByID(_TaskInfo.data.task_id, false))
                        {
                            AppendMessageLine("解除锁定任务成功!");
                            LogService.Write("解除锁定任务成功!");
                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "解除锁定", 1, "解除锁定任务成功", DateTime.Now));

                        }
                        else
                        {
                            AppendMessageLine("解除锁定任务失败， 任务ID：" + task_id);
                            LogService.Write("解除锁定任务失败， 任务ID：" + task_id);
                            ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(task_id, "解除锁定", -1, "解除锁定任务失败， 任务ID：" + task_id, DateTime.Now));
                        }
                    }

                }
                catch { }
                try
                {
                    if (File.Exists(tempDownloadFilePath))
                    {
                        File.Delete(tempDownloadFilePath);
                    }
                    if (File.Exists(ConvertFileS48Path))
                    {
                        File.Delete(ConvertFileS48Path);
                    }
                    if (File.Exists(ConvertFileS48Path + ".txt"))
                    {
                        File.Delete(ConvertFileS48Path + ".txt");
                    }
                    if (File.Exists(ConvertFileS48Path+ ".wfm"))
                    {
                        File.Delete(ConvertFileS48Path + ".wfm");
                    }
                }
                catch
                {

                }
                #endregion

                Globals.ExecuteStopDate = DateTime.Now;
                ProgramTaskCommonService.Common.FlushMemory();
            }
        }
        /// <summary>

        private void InsertProgarmTaskFunc()
        {
            try
            {
                this.Invoke(new InsertTasks(this.InsertProgarmTask));
            }
            catch { }
        }

        private delegate void InsertTasks();

        #endregion

        /// <summary>
        /// 管理线程
        /// </summary>
        private void ThreadManager()
        {
            try
            {
                while (true)
                {
                    Application.DoEvents();

                    if (this.isStart)
                    {
                        if (ExcThread != null)
                        {

                            if (!ExcThread.IsAlive)
                            {
                                TimeSpan span = DateTime.Now - Globals.ExecuteStopDate;

                                if (span.Seconds >= Globals.TaskActuatorInterval)
                                {
                                    ExcThread = new Thread(new ThreadStart(this.InsertProgarmTaskFunc));

                                    ExcThread.IsBackground = true;
                                    ExcThread.Start();
                                }


                            }

                        }
                        else
                        {
                            ExcThread = new Thread(new ThreadStart(this.InsertProgarmTaskFunc));
                            ExcThread.IsBackground = true;
                            ExcThread.Start();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //  LogService.WriteErr("程序错误，方法：ThreadManager 错误信息：" + ex.Message);
            }
        }


        #region //窗体事件
        private void FrmMain_Load(object sender, EventArgs e)
        {
            StartSysInitThread();
        }
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsStart)
            {
                MessageBox.Show("监测运行中，请先停止!", "提示!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }

            if (ManagerThread != null && ManagerThread.IsAlive)
            {
                ManagerThread.Abort();
            }
            if (SysInitThread != null && SysInitThread.IsAlive)
            {
                SysInitThread.Abort();
            }

        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //判断是否已经最小化于托盘 
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示 
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点 
                this.Activate();
                //任务栏区显示图标 
                this.ShowInTaskbar = true;
                //托盘区图标隐藏 
                notifyIcon.Visible = false;
            }
        }
        private void FrmMain_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮 
            if (WindowState == FormWindowState.Minimized)
            {
                //托盘显示图标等于托盘图标对象 
                //注意notifyIcon1是控件的名字而不是对象的名字 

                //隐藏任务栏区图标 
                this.ShowInTaskbar = false;
                //图标显示在托盘区 
                notifyIcon.Visible = true;
            }
        }

        private void btnShowForm_Click(object sender, EventArgs e)
        {
            notifyIcon_MouseDoubleClick(sender, null);
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            IsStart = !IsStart;
            if (IsStart)
            {
                Globals.IsClicked = true;
            }
        }

        private void btnSetting_Click(object sender, EventArgs e)
        { 
            if (IsStart)
            {
                MessageBox.Show("监测运行中，无法设置!", "提示!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                StartSysInitThread();
            }

        }
        private void btnCloseForm_Click(object sender, EventArgs e)
        {
            if (IsStart)
            {
                notifyIcon_MouseDoubleClick(sender, null);
                MessageBox.Show("监测运行中，请先停止!", "提示!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Close();
            }
        }

        #endregion

        #region //Function 

        #region //系统初始化 

        /// 系统初始化
        /// </summary>
        private void StartSysInitThread()
        {
            try
            {
                btnStart.Enabled = false;
                this.txtInfo.Text = "";
                AppendMessageLine("开始初始化系统......");

                this.SysInitThread = new Thread(new ThreadStart(this.InitSysFunc));

                SysInitThread.IsBackground = true;
                SysInitThread.Start();
            }
            catch (Exception ex)
            {

                LogService.WriteErr("系统初始化错误：" + ex.Message);
            }

        }
        private void InitSysFunc()
        {
            try
            {

                this.Invoke(new SysInit(this.InitSys));
            }
            catch { }
        }

        private delegate void SysInit();
        private void InitSys()
        {
            Application.DoEvents();


            try
            {
                #region //网络监察
                //int n = 0;
                //if (!ProgramMonitorService.InternetGetConnectedState(out n))
                //{
                //    LogService.Write("设备不可联网");
                //    AppendMessageLine("设备不可联网");
                //    return;

                //}
                //if (!ProgramMonitorService.PingIpOrDomainName(Globals.GateWayIP))
                //{
                //    LogService.Write("网闸不可访问");
                //    AppendMessageLine("网闸不可访问");
                //    return;

                //}

                #endregion

               
                #region //获取系统信息  
                RightManagerDS.SystemsDataTable systemDT = Globals.RightManagerWebService.GetSystems();
                if (systemDT != null || systemDT.Count > 0)
                {
                    foreach (RightManagerDS.SystemsRow sr in systemDT)
                    {
                        if (sr.Description.ToLower().Contains("xstudio"))
                        {
                            Globals.sysInfo = sr;
                            break;
                        }
                    }
                }
                #endregion
                AppendMessageLine("获取系统信息成功!");

                #region //获取素材库ID
                Globals.ProgramLibID = null;
                Globals.ProgramLibID = GetCategoryIDByName(1, Globals.ProgramLibName);
                if (Globals.ProgramLibID != null)
                {
                    AppendMessageLine("获取素材库信息成功!");
                }
                else
                {
                    AppendMessageLine("初始化失败，获取素材库信息失败!");
                    return;
                }
                #endregion

                Globals.ProgramLibRightID = null;
                GetProgramLibRightInfo();

                if (Globals.FolderRightID != null && Globals.ProgramLibRightID.Length > 0)
                {
                    AppendMessageLine("获取素材库权限信息成功!");
                }
                else
                {

                    AppendMessageLine("初始化失败，获取素材库权限信息失败!");
                    return;
                }

                Globals.FolderRightID = null;
                GetFolderRightInfo();

                if (Globals.FolderRightID != null && Globals.FolderRightID.Split('|').Length == 3)
                {
                    AppendMessageLine("获取类别权限信息成功!");
                }
                else
                {

                    AppendMessageLine("初始化失败，获取类别权限信息失败!");
                    return;
                }

                //Globals.StorageInfo = Globals.XStudioWebService.GetStorageInfoByProgramLibID(Globals.ProgramLibID);
                //if (Globals.StorageInfo != null)
                //{
                //    AppendMessageLine("获取存储信息成功!");

                //}
                //else
                //{
                //    AppendMessageLine("初始化失败，获取存储信息失败!");
                //    return;
                //}

                btnStart.Enabled = true;
                this.IsStart = true;
            }
            catch (Exception ex)
            {
                AppendMessageLine("初始化系统错误：" + ex.Message);
            }

        }

        /// <summary>
        /// 获取类别权限信息
        /// </summary>
        private void GetFolderRightInfo()
        {
            try
            {
                string rightName1 = Globals.FolderRightName.Split('|')[0];
                string rightName2 = Globals.FolderRightName.Split('|')[1];
                string rightName3 = Globals.FolderRightName.Split('|')[2];

                RightManagerDS.RightsDataTable result = new RightManagerDS.RightsDataTable();
                RightManagerDS.ResourceCategory2RightCategoryDataTable rc2rcDT =
                    Globals.RightManagerWebService.GetResourceCategory2RightCategoryBySystemIDResCat
                                            (Globals.sysInfo.ID, 2,
                                            Globals.sysInfo.Keys);
                foreach (RightManagerDS.ResourceCategory2RightCategoryRow rc2rcRow in rc2rcDT)
                {
                    RightManagerDS.RightsDataTable _RightsDataTable = Globals.RightManagerWebService.GetRightsByRigthCategory(Globals.sysInfo.ID,
                                    rc2rcRow.RightCategory, Globals.sysInfo.Keys);
                    for (int i = 0; i < _RightsDataTable.Rows.Count; i++)
                    {
                        if (_RightsDataTable.Rows[i]["Name"].ToString() == rightName1)
                        {
                            Globals.FolderRightID = _RightsDataTable.Rows[i]["ID"].ToString();
                            Globals.FolderRightKey = _RightsDataTable.Rows[i]["Keys"].ToString();
                        }
                        if (_RightsDataTable.Rows[i]["Name"].ToString() == rightName2)
                        {
                            Globals.FolderRightID = Globals.FolderRightID + "|" + _RightsDataTable.Rows[i]["ID"].ToString();
                            Globals.FolderRightKey = Globals.FolderRightKey + "|" + _RightsDataTable.Rows[i]["Keys"].ToString();
                        }
                        if (_RightsDataTable.Rows[i]["Name"].ToString() == rightName3)
                        {
                            Globals.FolderRightID = Globals.FolderRightID + "|" + _RightsDataTable.Rows[i]["ID"].ToString();
                            Globals.FolderRightKey = Globals.FolderRightKey + "|" + _RightsDataTable.Rows[i]["Keys"].ToString();
                        }
                        if (_RightsDataTable.Rows[i]["Name"].ToString() == Globals.ProgramLibRightName)
                        {
                            Globals.ProgramLibRightID = _RightsDataTable.Rows[i]["ID"].ToString();
                            Globals.ProgramLibRightKey = _RightsDataTable.Rows[i]["Keys"].ToString();
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 获取素材库权限信息
        /// </summary>
        private void GetProgramLibRightInfo()
        {
            try
            {
                RightManagerDS.RightsDataTable result = new RightManagerDS.RightsDataTable();
                RightManagerDS.ResourceCategory2RightCategoryDataTable rc2rcDT =
                    Globals.RightManagerWebService.GetResourceCategory2RightCategoryBySystemIDResCat
                                            (Globals.sysInfo.ID, 1,
                                            Globals.sysInfo.Keys);
                foreach (RightManagerDS.ResourceCategory2RightCategoryRow rc2rcRow in rc2rcDT)
                {
                    RightManagerDS.RightsDataTable _RightsDataTable = Globals.RightManagerWebService.GetRightsByRigthCategory(Globals.sysInfo.ID,
                                    rc2rcRow.RightCategory, Globals.sysInfo.Keys);
                    for (int i = 0; i < _RightsDataTable.Rows.Count; i++)
                    {
                        if (_RightsDataTable.Rows[i]["Name"].ToString() == Globals.ProgramLibRightName)
                        {
                            Globals.ProgramLibRightID = _RightsDataTable.Rows[i]["ID"].ToString();
                            Globals.ProgramLibRightKey = _RightsDataTable.Rows[i]["Keys"].ToString();
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region //权限和类别管理 

        /// <summary>
        /// 根据名称获取ID
        /// </summary>
        /// <param name="pCategoryName"></param>
        /// <returns></returns>
        private string GetCategoryIDByName(int cat1, string pCategoryName)
        {
            string ResourceID = null;
            try
            {
                RightManagerDS.ResourcesDataTable resAllCategory = new RightManagerDS.ResourcesDataTable();
                resAllCategory = Globals.RightManagerWebService.GetResourcesBySystemIDCat1(Globals.sysInfo.ID, cat1, Globals.sysInfo.Keys);
                DataRow[] drs = resAllCategory.Select("", "Category1,Category2,Category3");
                RightManagerDS.ResourcesDataTable result = new RightManagerDS.ResourcesDataTable();
                foreach (DataRow dr in drs)
                {
                    if (dr["Name"].ToString() == pCategoryName)
                    {
                        ResourceID = dr["ID"].ToString();
                        break;
                    }
                }
                return ResourceID;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// 判断是否有权限
        /// </summary>
        /// <param name="pResourceID">资源ID</param>
        /// <param name="pRightName">权限名称</param>
        /// <param name="pUserID">用户ID</param>
        /// <returns></returns>
        private bool IsHaveRight(string pResourceID, string pRightName, string pUserID)
        {
            bool isHavRight = false;
            string rightName1 = pRightName.Split('|')[0];
            string rightName2 = pRightName.Split('|')[1];
            string rightName3 = pRightName.Split('|')[2];
            int count = 0;
            try
            {
                List<ViewAllRightModel> lstVARModel = Globals.RightManagerWebService.GetViewAllRightByUserID
                                                      (new Guid(pUserID), Globals.sysInfo.ID,
                                                      Globals.sysInfo.Keys);

                foreach (ViewAllRightModel varModel in lstVARModel)
                {
                    if (count == 3)
                    {
                        isHavRight = true;
                        break;

                    }
                    if (varModel.ResourceID.ToString() == pResourceID &&
                        varModel.UserID.ToString() == pUserID && varModel.RightName == rightName1)
                    {
                        count++;
                    }
                    if (varModel.ResourceID.ToString() == pResourceID &&
                       varModel.UserID.ToString() == pUserID && varModel.RightName == rightName2)
                    {
                        count++;
                    }
                    if (varModel.ResourceID.ToString() == pResourceID &&
                       varModel.UserID.ToString() == pUserID && varModel.RightName == rightName3)
                    {
                        count++;
                    }

                }
                return isHavRight;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 判断是否有权限
        /// </summary>
        /// <param name="pResourceID">资源ID</param>
        /// <param name="pRightName">权限名称</param>
        /// <param name="pUserID">用户ID</param>
        /// <returns></returns>
        private bool IsHaveProgramLibRight(string pResourceID, string pRightName, string pUserID)
        {
            bool isHavRight = false;

            try
            {
                List<ViewAllRightModel> lstVARModel = Globals.RightManagerWebService.GetViewAllRightByUserID
                                                      (new Guid(pUserID), Globals.sysInfo.ID,
                                                      Globals.sysInfo.Keys);

                foreach (ViewAllRightModel varModel in lstVARModel)
                {
                    if (varModel.ResourceID.ToString() == pResourceID &&
                        varModel.UserID.ToString() == pUserID && varModel.RightName == pRightName)
                    {
                        isHavRight = true;
                        break;
                    }

                }
                return isHavRight;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 创建类别
        /// </summary>
        /// <param name="pFolderName"></param>
        /// <param name="pError"></param>
        /// <returns></returns>
        private bool CreatFolder(string pFolderName, Guid pFolderID, out string pError)
        {
            pError = "";
            try
            {
                Guid folderID = pFolderID;
                string folderName = pFolderName;
                string shortName = PinYin.GetChineseSpell(folderName + DateTime.Now.ToString("yyyyMMddHHmmss"));
                Guid programLibID = new Guid(Globals.ProgramLibID);
                short folderType = 0;
                Guid parentID = Guid.Empty;
                Guid rootIDRootID = folderID;
                Guid creatorID = Guid.Empty;
                string creatorName = Globals.ProgramTaskActuatorDeviceID;
                int maxSpaceMB = Globals.MaxSpaceHour * Globals.HourS48InStorageMB;
                int freeSpaceMB = maxSpaceMB;
                int maxSpaceHour = Globals.MaxSpaceHour;
                int freeSpaceHour = Globals.MaxSpaceHour;
                bool shared = false;
                short superviseLevel = 0;
                bool materialAccess = true;
                int materialMB = 0;
                int materialHour = 0;
                bool projectAccess = false;
                int projectMB = 0;
                int projectHour = 0;
                bool telephoneAccess = false;
                int telephoneMB = 0;
                int telephoneHour = 0;
                string telephoneNumber = "";
                bool isNeedExport = true;
                int keepDays = 0;
                Guid systemID = Globals.sysInfo.ID;
                int resourceCategory1 = 2;
                int resourceCategory2 = 2;
                string resourceCategoryName2 = Globals.ProgramLibName;
                Guid createUserID = Guid.Empty;
                string createUserName = Globals.ProgramTaskActuatorDeviceID;
                return Globals.ResourceManager.CreateFolder(folderID, folderName, shortName, programLibID, folderType, parentID, rootIDRootID, creatorID, Globals.ProgramTaskActuatorDeviceID, maxSpaceMB, freeSpaceMB, maxSpaceHour, freeSpaceHour, shared, superviseLevel, materialAccess, materialMB, materialHour, projectAccess, projectMB, projectHour, telephoneAccess, telephoneMB, telephoneHour, telephoneNumber, isNeedExport, keepDays, systemID, resourceCategory1, resourceCategory2, resourceCategoryName2, createUserID, createUserName);

            }
            catch (Exception ex)
            {
                pError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 授权
        /// </summary>
        /// <param name="resourceIDs"></param>
        /// <param name="rightIDs"></param>
        /// <param name="userID"></param>
        /// <param name="pError"></param>
        /// <returns></returns>
        private bool ApplyByResourceIDRightIDUserID(List<Guid> resourceIDs, List<Guid> rightIDs, string userID, out string pError
                                                 )
        {
            pError = "";
            try
            {
                return Globals.RightManagerWebService.ApplyByResourceIDRightIDUserID(resourceIDs, rightIDs, new Guid(userID), Globals.sysInfo.Keys);
            }
            catch (Exception ex)
            {
                pError = ex.Message;
                return false;
            }

        }

        #endregion

        #region //入库
        /// <summary>
        /// 获取类别信息
        /// </summary>
        /// <param name="pUserID"></param>
        /// <param name="folderid"></param>
        /// <returns></returns>
        private DataRow GetFolderInfo(string pUserID, string folderid)
        {
            try
            {
                DataTable dt = Globals.XStudioWebService.GetFolderTableByProgramLibIDAndUserIDAndAccessType(Globals.ProgramLibID, pUserID, 2, true);
                DataRow[] rows = dt.Select(" folderid='" + (folderid) + "'");
                return rows[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private bool IsHaveSpeac(string pUserID, string folderid, int free)
        {
            // this.ftpPath = this.libName + "\\" + message["shortname"];
            try
            {

                if (free <= 0)
                {
                    //这个类别的剩余空间已满!
                    return false;
                }
                try
                {
                    UserFolderRightInfo userFolderRightInfo = Globals.XStudioWebService.GetUserFolderRightInfo(folderid, pUserID);

                    if (userFolderRightInfo.IsCanManageFolder)
                    {
                        int free1 = Globals.XStudioWebService.GetFolderFreeSpace(folderid, Globals.ProgramLibID, true, pUserID);

                        if (free1 / 112 <= 1)
                        {

                            //您在这个类别中分配的空间已满，请及时清理!

                            return false;
                        }
                        //else
                        //{
                        //    if (free1 / 112 <= Globals.UserSpaceAlarmValue)
                        //    {
                        //        XtraMessageBox.Show(ClassFunction.LanguageResourceManager.GetString("YouAllocateSpaceInThisCategory") + (free1 / 112) + ClassFunction.LanguageResourceManager.GetString("HoursLeft_PleaseClean"), ClassFunction.LanguageResourceManager.GetString("Tips"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    }

                        //}
                    }
                    else
                    {
                        int free1 = Globals.XStudioWebService.GetFolderFreeSpace(folderid, Globals.ProgramLibID, true, pUserID);


                        if (free1 / 112 <= 1)
                        {
                            //您在这个类别中分配的空间已满，请及时清理!
                            return false;
                        }
                        //else
                        //{
                        //    if (free1 / 112 <= ClassFunction.UserSpaceAlarmValue)
                        //    {
                        //        XtraMessageBox.Show(ClassFunction.LanguageResourceManager.GetString("YouAllocateSpaceInThisCategory") + (free1 / 112) + ClassFunction.LanguageResourceManager.GetString("HoursLeft_PleaseClean"), ClassFunction.LanguageResourceManager.GetString("Tips"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    }
                        //}
                    }
                }
                catch { return false; }
                return true;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private bool doImport(string Title, string localFile, string pUserID, string pFullName, string pFolderID, string ftpPath, out string pImportError)
        { 
            try
            {

                ProgramDataInfo programinfo = new ProgramDataInfo();
                programinfo.ProgramID = Guid.NewGuid().ToString();
                programinfo.Title = Title;// Path.GetFileNameWithoutExtension(localFile);
                programinfo.Duration = MepgAudio.GetAudioDuration(localFile);
                programinfo.ProgramLength = new FileInfo(localFile).Length;
                using (this.ftp1 = new Dart.PowerTCP.Ftp.Ftp())
                {
                    //this.Tag = node;
                    //this.ftp1.Progress += new Dart.PowerTCP.Ftp.FtpProgressEventHandler(ftp1_Progress);
                    this.ftp1.Server = Globals.StorageInfo.FTPServer1;
                    this.ftp1.Username = Globals.StorageInfo.FTPUser1;
                    this.ftp1.ServerPort = int.Parse(Globals.StorageInfo.FTPPort1);
                    this.ftp1.Password = Globals.StorageInfo.FTPPassword1;
                    this.ftp1.BlockSize = 6553600;
                    this.ftp1.ProgressSize = 6553600;
                    try
                    {
                        this.ftp1.Put(localFile, "/" + ftpPath.Replace("\\", "/") + "/" + programinfo.ProgramID + ".s48");
                        ExtractWave(localFile, this.Handle.ToInt32(), 12300, 0);
                        if (File.Exists(localFile + ".wfm"))
                        {
                            try
                            {
                                this.ftp1.Put(localFile + ".wfm", "/" + ftpPath.Replace("\\", "/") + "/" + programinfo.ProgramID + ".s48.wfm");
                            }
                            catch
                            {

                            }

                        }
                    }
                    catch (Exception)
                    {
                        this.ftp1.Server = Globals.StorageInfo.FTPServer1;
                        this.ftp1.Username = Globals.StorageInfo.FTPUser1;
                        this.ftp1.ServerPort = int.Parse(Globals.StorageInfo.FTPPort1);
                        this.ftp1.Password = Globals.StorageInfo.FTPPassword1;
                        this.ftp1.BlockSize = 655360;
                        this.ftp1.ProgressSize = 655360;
                        try
                        {
                            this.ftp1.Put(localFile, "/" + ftpPath.Replace("\\", "/") + "/" + programinfo.ProgramID + ".s48");
                            ExtractWave(localFile, this.Handle.ToInt32(), 12300, 0);
                            if (File.Exists(localFile + ".wfm"))
                            {
                                try
                                {
                                    this.ftp1.Put(localFile + ".wfm", "/" + ftpPath.Replace("\\", "/") + "/" + programinfo.ProgramID + ".s48.wfm");
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            pImportError = ex.Message;
                            LogService.WriteErr(" this.ftp1.Server" + this.ftp1.Server + " Username: " + this.ftp1.Username + " ServerPort " + this.ftp1.ServerPort + " this.ftp1.Password:" + this.ftp1.Password + "  localFile :" + localFile + " Path:" + "/" + ftpPath + "/" + programinfo.ProgramID + ".s48");
                            pImportError = " this.ftp1.Server" + this.ftp1.Server + " Username: " + this.ftp1.Username + " ServerPort " + this.ftp1.ServerPort + " this.ftp1.Password:" + this.ftp1.Password + "  localFile :" + localFile + " Path:" + "/" + ftpPath + "/" + programinfo.ProgramID + ".s48" + "  ex.Message:" + ex.Message; 
                            return false;
                        }
                    }
                }


                programinfo.StorageID = Globals.StorageInfo.StorageID;
                programinfo.SoundFileName = ftpPath + "\\" + programinfo.ProgramID + ".s48";
                programinfo.ProgramType = 10;

                programinfo.IsDeleted = false;
                programinfo.CreatorID = pUserID;// ClassFunction.MyUserInfo.UserID;
                programinfo.CreatorName = pFullName;// ClassFunction.MyUserInfo.FullName;
                programinfo.SoundFileLastEditTime = DateTime.Now;
                programinfo.ProjectLastEditTime = DateTime.Now;
                programinfo.Released = true;
                programinfo.DeletedDateTime = DateTime.Parse("1900-1-1");
                programinfo.ProgramLibID = Globals.ProgramLibID;
                programinfo.FolderID = pFolderID;// folderID;
                programinfo.IsMixDone = 1;
                programinfo.CreateDateTime = DateTime.Now;

                if (Globals.XStudioWebService.InsertProgram(programinfo.ToXmlString(), programinfo.ProgramLibID) > 0)
                {
                    try
                    {
                        if (File.Exists(localFile))
                        {
                            File.Delete(localFile);
                        }

                    }
                    catch
                    {

                    }

                    //XStudioLog.XStudioLog.SendUserLogByUDP(ClassFunction.MyUserInfo.UserID, ClassFunction.MyUserInfo.UserName, ClassFunction.MyUserInfo.FullName, "", "用户" + ClassFunction.MyUserInfo.FullName + "将本地音频《" + localFile + "》上传至素材库成功!", programinfo.ProgramID, LogDatabaseDll.LogDatabaseWS.E_Operation.Send, LogDatabaseDll.LogDatabaseWS.E_System.XStudio);
                    pImportError = "将云素材《" + Title + "》上传到制作网素材库：" + Globals.ProgramLibName + " 成功！素材ID：" + programinfo.ProgramID;

                    return true;
                }
                else
                {

                    pImportError = "写入数据记录错误!";
                    return false;
                }
            }
            catch (Exception ex)
            {

                pImportError = "写入数据记录错误!" + ex.Message; 
                return false;
            }
        }
        #endregion

        private void test()
        {
            try
            {

                string UserFullName = "";
                Globals.ProgramLibID = GetCategoryIDByName(1, Globals.ProgramLibName);
                string UserID = Globals.XStudioWebService.GetUserInfoByUserLoginName("csgly", out UserFullName);
                UserDataInfoList _UserDataInfoList = Globals.XStudioWebService.GetUserInfoByUserID(UserID);
                this.GetMyProgramLibInfo(UserID);
                //TaskInfo _TaskInfo = ProgramTaskDBManager.GetOneUnLockedTask("db-test-1");
                //_TaskInfo.AudioConvertDatetime = DateTime.Now;
                //_TaskInfo.TaskActuatorDeviceID = Globals.ProgramTaskActuatorDeviceID;
                //_TaskInfo.TaskActuatorDeviceComputerName = Globals.ComputerName;
                //_TaskInfo.TaskActuatorDeviceIP = Globals.ComputerIP;
                //_TaskInfo.AudioAudioCheckDatetime = DateTime.Now;
                //_TaskInfo.TaskCompletionDatetime = DateTime.Now;
                //_TaskInfo.data.task_id = "669";
                //_TaskInfo.NeedReDo = 1;
                //_TaskInfo.data.detail_report = "-------------------------------------------------------------------------辅助技审检测报告------------------------------------------------------------------------ -本次技审报告产生时间:2017 / 1 / 18 17:20:05    ------------------------------------------------------------------------ -    本次辅助技审检测参数项及阈值设置    ------------------------------------------------------------------------ -    低电平检测    : 极限时间长度: 4000(毫秒) 极限阈值: -40.00(dBfs) 极限比例阈值: 1.00电平过高检测: 极限时间长度: 1000(毫秒) 极限阈值: -3.00(dBfs) 极限比例阈值: 1.00反相检测: 极限时间长度: 30000(毫秒) 极限阈值: 0.70       极限比例阈值: 1.00------------------------------------------------------------------------ -本次辅助技审普通检测结果------------------------------------------------------------------------ -类型          开始时间 结束时间--------------------------------------------------------------------------------------------------------------------------------------------------本次辅助技审检测异常数: 0------------------------------------------------------------------------ -本次辅助技审极限检测结果------------------------------------------------------------------------ -类型          开始时间 结束时间-------------------------------------------------------------------------左声道电平过大     :	00:08:55.080        00:09:00.984左声道电平过大: 00:09:12.504        00:09:13.896右声道电平过大: 00:09:25.320        00:09:26.736左声道电平过大: 00:09:31.992        00:09:33.720电平过大: 00:09:34.008        00:09:34.992左声道电平过大: 00:09:37.608        00:09:43.848右声道电平过大: 00:09:44.616        00:09:47.256左声道电平过大: 00:09:51.432        00:09:52.536电平过大: 00:09:53.232        00:09:54.576右声道电平过大: 00:09:54.672        00:09:55.824左声道电平过大: 00:09:59.952        00:10:01.704------------------------------------------------------------------------ -本次辅助技审检测异常数: 11";
                //    _TaskInfo.TaskState = 2;
                //    _TaskInfo.error = "";
                //    string error = "";
                //    if (Globals.TingTingAPIService.TaskDealCallback(_TaskInfo, out error))
                //    {
                //        AppendMessageLine("请求回调成功!");
                //        LogService.Write("请求回调成功!");
                //    }
                //    else
                //    {
                //        AppendMessageLine("请求回调失败!" + error);
                //        LogService.Write("请求回调失败!" + error);
                //    }
                //ProgramTaskDBService.ProgramTaskDBManager.InsertIntoTask(_TaskInfo);

                //string UserFullName = "";
                //string UserID = Globals.XStudioWebService.GetUserInfoByUserLoginName("test", out UserFullName);
                //this.IsHaveProgramLibRight(Globals.ProgramLibID, Globals.ProgramLibRightName, UserID);


            }
            catch (Exception ex)
            {

                throw;
            }
        }
        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="pContent"></param>
        private void AppendMessageLine(string pContent)
        {
            this.txtInfo.AppendText(DateTime.Now + ":  " + pContent + "\n");
        }

        private TaskDetail CreateTaskDetail(string pTask_id, string pTaskStepName, int pTaskSetpState, string pTaskStepReport, DateTime pActuatorDateTime)
        {
            TaskDetail _TaskDetail = new TaskDetail();
            _TaskDetail.ID = Guid.NewGuid();
            _TaskDetail.TaskID = pTask_id;
            _TaskDetail.TaskStepName = pTaskStepName;
            _TaskDetail.TaskSetpState = pTaskSetpState;
            _TaskDetail.TaskActuatorComputerName = Globals.ComputerName;
            _TaskDetail.TaskActuatorIP = Globals.ComputerIP;
            _TaskDetail.TaskStepReport = pTaskStepReport;
            _TaskDetail.ActuatorDateTime = pActuatorDateTime;
            return _TaskDetail;
        }

        /// <summary>
        /// 获取素材库基本信息
        /// </summary>
        /// <param name="UserID"></param>
        private void GetMyProgramLibDataInfo(string UserID)
        {
            try
            {
                Globals.MyProgramLibDataInfo = null;
                ProgramLibDataInfoList list = Globals.XStudioWebService.GetProgramLibInfoByUserID(UserID);

                if (list != null && list.ProgrramDataInfoArray != null)
                {
                    foreach (ProgramLibDataInfo item in list.ProgrramDataInfoArray)
                    {
                        if (item.ProgramLibName == Globals.ProgramLibName && item.ProgramLibID == Globals.ProgramLibID)
                        {
                            Globals.MyProgramLibDataInfo = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.MyProgramLibDataInfo = null;
                LogService.WriteErr("获取素材库信息错误：" + ex.Message);
            }
        }
        private void GetMyProgramLibInfo(string UserID)
        {
            try
            {
                Globals.MyProgramLibDataInfo = null;
                ResourceManagerWS.XStudioDS.ProgramLibDataTable list = Globals.ResourceManager.GetProgramLibsByUserIDProgramLibType(new Guid(UserID), 0);

                if (list != null && list.Rows != null)
                {
                    for (int i = 0; i < list.Rows.Count; i++)
                    {
                        if (list.Rows[i]["ProgramLibName"].ToString() == Globals.ProgramLibName && list.Rows[i]["ProgramLibID"].ToString() == Globals.ProgramLibID)
                        {
                            Globals.MyProgramLibDataInfo = new ProgramLibDataInfo();
                            Globals.MyProgramLibDataInfo.ProgramLibID = list.Rows[i]["ProgramLibID"].ToString();
                            Globals.MyProgramLibDataInfo.ProgramLibName = list.Rows[i]["ProgramLibName"].ToString();
                            Globals.MyProgramLibDataInfo.DBName = list.Rows[i]["DBName"].ToString();
                            Globals.MyProgramLibDataInfo.FreeSpace = Convert.ToInt32(list.Rows[i]["FreeSpaceMB"].ToString());

                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Globals.MyProgramLibDataInfo = null;
                LogService.WriteErr("获取素材库信息错误：" + ex.Message);
            }
        }
        #endregion

    }
}
