using ProgramTaskCommonService;
using ProgramTaskDBService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TaskDataInfo;
using WinProgramTaskDownloadDevice.Classes;

namespace WinProgramTaskDownloadDevice
{
    public partial class FrmMain : Form
    {
        /// <summary>
        /// 网络是否有问题
        /// </summary>
        bool IsHaveINTERNETError = false;

        Thread ManagerThread;
        Thread ExcThread;
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
                    this.btnStart.Image = global::WinProgramTaskDownloadDevice.Properties.Resources.stop;

                    this.btnStart.Text = "停止";
                }
                else
                {
                    this.btnStart.Image = global::WinProgramTaskDownloadDevice.Properties.Resources.Start;

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
            IsStart = true;
        }

        #region Function

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
        /// 是否锁定
        /// </summary>
        /// <param name="task_id"></param>
        /// <returns></returns>
        private bool IsLocked(string task_id)
        {
            try
            {
                TaskInfo _TaskInfo = ProgramTaskDBManager.GetTaskByTaskID(task_id);

                if (_TaskInfo.Islocked == 1)
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

        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="pContent"></param>
        private void AppendMessageLine(string pContent)
        {
            this.txtInfo.AppendText(DateTime.Now + ":  " + pContent + "\n");
        }

        /// <summary>
        /// 解锁超时任务
        /// </summary>
        private void UnLockTimeOutTask()
        {
            try
            {
                //获取锁定的任务

                List<TaskInfo> _TaskInfoList = ProgramTaskDBManager.GetTaskByLocked("1");

                if (_TaskInfoList != null && _TaskInfoList.Count > 0)
                {
                    foreach (TaskInfo _TaskInfo in _TaskInfoList)
                    { 
                        double size = _TaskInfo.data.size;
                        try
                        {
                            size = (size / 1024.0);
                        }
                        catch
                        {
                            size = 0;
                        }
                        double timesize = Globals.TaskExecuteTime * size * Globals.TaskExecuteOutThreshold;

                        if (DateTime.Compare(_TaskInfo.TaskDownloadDateTime.AddSeconds(timesize), DateTime.Now) < 0)
                        {
                            AppendMessageLine("任务超时!");
                            LogService.Write("任务超时!");

                            #region //解锁锁定的任务 

                            if (ProgramTaskDBManager.LockTaskByID(_TaskInfo.data.task_id, false))
                            {
                                AppendMessageLine("解除锁定超时任务成功!");
                                LogService.Write("解除锁定超时任务成功!");
                                //ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(_TaskInfo.data.task_id, "解除锁定", 1, "解除锁定任务成功", DateTime.Now));

                            }
                            else
                            {
                                AppendMessageLine("解除锁定超时任务失败， 任务ID：" + _TaskInfo.data.task_id);
                                LogService.Write("解除锁定超时任务失败， 任务ID：" + _TaskInfo.data.task_id);

                            }
                            #endregion 

                        }

                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #region 获取任务 
        /// <summary>
        /// 获取任务，添加任务
        /// </summary>
        private void InsertTask()
        {
            try
            {
                if (txtInfo.Text.Length > txtInfo.MaxLength)
                    txtInfo.Text = "";
                Application.DoEvents();
                if (!isStart)
                {
                    AppendMessageLine("任务停止！");
                    return;
                }
                //解锁超时任务
                UnLockTimeOutTask();
                if (ProgramTaskDBManager.IsExistUnDoTask())
                {
                    LogService.Write("有未完成任务...");
                    AppendMessageLine("有未完成任务...");
                    return;

                }

                AppendMessageLine("开始获取任务");
                TaskInfo _TaskInfo = Globals.TingTingAPIService.GetTask();
                if (_TaskInfo.TaskState == -1)
                {
                    LogService.Write("获取任务失败,错误信息：" + _TaskInfo.ErrorMsg);
                    AppendMessageLine("获取任务失败,错误信息：" + _TaskInfo.ErrorMsg);
                    return;
                }
                else
                {
                    if (_TaskInfo.data == null || _TaskInfo.data.task_id == null || _TaskInfo.data.task_id.Length <= 0)
                    {
                        if (Globals.IsFree)
                        {
                            AppendMessageLine("未获取到新任务！");
                        }
                        Globals.IsFree = true;
                        Application.DoEvents();
                        return;
                    }
                    Globals.IsFree = false;
                    LogService.Write("获取任务成功，音频名称：" + _TaskInfo.data.audio_name);
                    AppendMessageLine("获取任务成功，音频名称：" + _TaskInfo.data.audio_name);

                    if (ProgramTaskDBManager.IsExistTask(_TaskInfo.data.task_id))
                    {
                        LogService.Write("任务已存在，任务ID：" + _TaskInfo.data.task_id);
                        AppendMessageLine("任务已存在，任务ID：" + _TaskInfo.data.task_id);

                        TaskInfo _TemTaskInfo=ProgramTaskDBManager.GetTaskByTaskID(_TaskInfo.data.task_id);

                        if (_TemTaskInfo!=null&& _TemTaskInfo.TaskState!=0)
                        {
                            #region //反馈回调
                            string error = "";
                            if (_TemTaskInfo.data.detail_report==null)
                            {
                                _TemTaskInfo.data.detail_report = "";
                            }
                            if (_TemTaskInfo.error == null)
                            {
                                _TemTaskInfo.error = "";
                            }
                           
                            if (Globals.TingTingAPIService.TaskDealCallback(_TemTaskInfo, out error))
                            {
                                AppendMessageLine("请求回调成功!");
                                LogService.Write("请求回调成功!");
                                ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(_TemTaskInfo.data.task_id, "回调", 1, "请求回调成功", DateTime.Now));
                            }
                            else
                            {
                                AppendMessageLine("请求回调失败，错误信息：" + error);
                                LogService.Write("请求回调失败，错误信息：" + error);
                                ProgramTaskDBManager.InsertIntoTaskDetail(CreateTaskDetail(_TemTaskInfo.data.task_id, "回调", -1, "请求回调失败，错误信息：" + error, DateTime.Now));
                            }
                            #endregion
                        } 

                    }
                    else
                    {
                        if (ProgramTaskDBManager.InsertIntoTask(_TaskInfo))
                        {
                            LogService.Write("任务添加成功，任务ID：" + _TaskInfo.data.task_id);
                            AppendMessageLine("任务添加成功，任务ID：" + _TaskInfo.data.task_id);
                        }
                        else
                        {
                            LogService.Write("任务添加失败，任务ID：" + _TaskInfo.data.task_id);
                            AppendMessageLine("任务添加失败，任务ID：" + _TaskInfo.data.task_id);
                        }
                    }
                } 
            }
            catch (Exception ex)
            {
                AppendMessageLine("程序错误，方法：InsertTask 错误信息：" + ex.Message);
                LogService.WriteErr("程序错误，方法：InsertTask 错误信息：" + ex.Message);
            }
            finally
            {
                Globals.ExecuteStopDate = DateTime.Now;
                ProgramTaskCommonService.Common.FlushMemory();
            }
        }

        private void InsertTaskFunc()
        {
            try
            {
                this.Invoke(new InsertTasks(this.InsertTask));
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

                                if (span.Seconds >= Globals.TaskRequestInterval)
                                {
                                    ProgramTaskCommonService.Common.FlushMemory();
                                    ExcThread = new Thread(new ThreadStart(this.InsertTaskFunc));

                                    ExcThread.IsBackground = true;
                                    ExcThread.Start();
                                }


                            }

                        }
                        else
                        {
                            ExcThread = new Thread(new ThreadStart(this.InsertTaskFunc));
                            ExcThread.IsBackground = true;
                            ExcThread.Start();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //Globals.WriteLocalLog("程序错误，方法：ThreadManager 错误信息：" + ex.Message);
            }
        }
        #endregion

        #region //窗体事件
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsStart)
            {
                MessageBox.Show("监测运行中，请先停止！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }

            if (ManagerThread != null && ManagerThread.IsAlive)
            {
                ManagerThread.Abort();
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
                MessageBox.Show("监测运行中，无法设置！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void btnCloseForm_Click(object sender, EventArgs e)
        {
            if (IsStart)
            {
                notifyIcon_MouseDoubleClick(sender, null);
                MessageBox.Show("监测运行中，请先停止！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Close();
            }
        }
        #endregion

        #region //WinMessage

        //protected override void WndProc(ref System.Windows.Forms.Message msg)
        //{

        //    switch (msg.Msg)
        //    {
        //        case WndProcMsgModel.WM_MSG_INTERNET_ERROR: //处理消息 
        //            {
        //                IsHaveINTERNETError = true;
        //                if (this.IsStart)
        //                {
        //                    this.IsStart = false;
        //                }
        //            }



        //            break;

        //        default:

        //            base.WndProc(ref msg);//调用基类函数处理非自定义消息。

        //            break;

        //    }

        //  

        //}
        #endregion
    }
}
