using ProgramTaskCommonService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TaskDataInfo;
using WinTaskProgramMonitor.Classes;

namespace WinTaskProgramMonitor
{
    public partial class FrmMain : Form
    {
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
                    this.btnStart.Image = global::WinTaskProgramMonitor.Properties.Resources.stop;

                    this.btnStart.Text = "停止";
                }
                else
                {
                    this.btnStart.Image = global::WinTaskProgramMonitor.Properties.Resources.Start;

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

        #region Function

        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="pContent"></param>
        private void AppendMessageLine(string pContent)
        {
            this.txtInfo.AppendText(DateTime.Now + ":  " + pContent + "\n");
        }

        #region 获取任务 
        /// <summary>
        /// 获取任务，添加任务
        /// </summary>
        private void TaskProgramMonitor()
        {
            try
            {
                string error = "";
                if (txtInfo.Text.Length > txtInfo.MaxLength)
                    txtInfo.Text = "";
                Application.DoEvents();
                if (!isStart)
                {
                    AppendMessageLine("停止监控！");
                    return;
                }

                AppendMessageLine("开始监控！");

                #region //下载器监控
                if (ProgramMonitorService.ISProgramRunning(Globals.ProgramTaskDownloadDevice.ProcessName))
                {
                    //LogService.Write(Globals.ProgramTaskDownloadDevice.ProgramMainWindowName + "运行正常");
                    AppendMessageLine(Globals.ProgramTaskDownloadDevice.ProgramMainWindowName + "运行正常");
                }
                else
                {

                    if (ProgramMonitorService.StartProgram(Globals.ProgramTaskDownloadDevice.ProgramPath, null, out error))
                    {
                        LogService.Write("启动" + Globals.ProgramTaskDownloadDevice.ProgramMainWindowName + "成功！");
                        AppendMessageLine("启动" + Globals.ProgramTaskDownloadDevice.ProgramMainWindowName + "成功！");
                    }
                    else
                    {
                        LogService.Write("启动" + Globals.ProgramTaskDownloadDevice.ProgramMainWindowName + "失败，原因：" + error);
                        AppendMessageLine("启动" + Globals.ProgramTaskDownloadDevice.ProgramMainWindowName + "失败，原因：" + error);
                    }

                }
                #endregion

                #region //执行器监控 

                if (ProgramMonitorService.ISProgramRunning(Globals.ProgramTaskActuatorDevice.ProcessName))
                {
                    LogService.Write(Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "运行正常");
                    AppendMessageLine(Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "运行正常");
                }
                else
                {

                    if (ProgramMonitorService.StartProgram(Globals.ProgramTaskActuatorDevice.ProgramPath, null, out error))
                    {
                        LogService.Write("启动" + Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "成功！");
                        AppendMessageLine("启动" + Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "成功！");
                    }
                    else
                    {
                        LogService.Write("启动" + Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "失败，原因：" + error);
                        AppendMessageLine("启动" + Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "失败，原因：" + error);
                    }

                }

                #endregion

                #region //联网监控 

                //int n = 0;
                //if (!ProgramMonitorService.InternetGetConnectedState(out n))
                //{
                //    LogService.Write("设备联网正常");
                //    AppendMessageLine("设备联网正常"); 
                    
                //}
                //else
                //{
                //    LogService.Write("设备不可联网");
                //    AppendMessageLine("设备不可联网");

                //    if (ProgramMonitorService.SendMessage(Globals.ProgramTaskDownloadDevice.ProcessName, WndProcMsgModel.WM_MSG_INTERNET_ERROR, Globals.ProgramTaskDownloadDevice.ProgramMainWindowName, null, out error))
                //    {
                //        LogService.Write("发送网络异常消息到"+ Globals.ProgramTaskDownloadDevice.ProgramMainWindowName+"成功");
                //        AppendMessageLine("发送网络异常消息到" + Globals.ProgramTaskDownloadDevice.ProgramMainWindowName + "成功"); 
                //    }
                //    else
                //    {
                //        LogService.Write("发送网络异常消息到" + Globals.ProgramTaskDownloadDevice.ProgramMainWindowName + "失败："+error);
                //        AppendMessageLine("发送网络异常消息到" + Globals.ProgramTaskDownloadDevice.ProgramMainWindowName + "失败：" + error);
                //    }
                   

                //}
                #endregion

                #region //网闸监控 
                //if (ProgramMonitorService.PingIpOrDomainName(Globals.GateWayIP))
                //{
                //    LogService.Write("网闸访问正常");
                //    AppendMessageLine("网闸访问正常");
                //}
                //else
                //{
                //    LogService.Write("网闸不可访问");
                //    AppendMessageLine("网闸不可访问");

                //    if (ProgramMonitorService.SendMessage(Globals.ProgramTaskActuatorDevice.ProcessName, WndProcMsgModel.WM_MSG_GATEWAYE_ERROR, Globals.ProgramTaskActuatorDevice.ProgramMainWindowName, null, out error))
                //    {
                //        LogService.Write("发送网闸异常消息到" + Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "成功");
                //        AppendMessageLine("发送网闸异常消息到" + Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "成功");
                //    }
                //    else
                //    {
                //        LogService.Write("发送网闸异常消息到" + Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "失败：" + error);
                //        AppendMessageLine("发送网闸异常消息到" + Globals.ProgramTaskActuatorDevice.ProgramMainWindowName + "失败：" + error);
                //    }
                //}
                #endregion

          

            }
            catch (Exception ex)
            {

                LogService.WriteErr("程序错误，方法：TaskProgramMonitor 错误信息：" + ex.Message);
            }
            finally
            {
                Globals.ExecuteStopDate = DateTime.Now;
            }
        }

        private void TaskProgramMonitorFunc()
        {
            try
            {
                this.Invoke(new TaskProgramMonitors(this.TaskProgramMonitor));
            }
            catch { }
        }

        private delegate void TaskProgramMonitors();

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

                                if (span.Seconds >= Globals.MonitorInterval)
                                {
                                    ExcThread = new Thread(new ThreadStart(this.TaskProgramMonitorFunc));

                                    ExcThread.IsBackground = true;
                                    ExcThread.Start();
                                }


                            }

                        }
                        else
                        {
                            ExcThread = new Thread(new ThreadStart(this.TaskProgramMonitorFunc));
                            ExcThread.IsBackground = true;
                            ExcThread.Start();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
               LogService.WriteErr("程序错误，方法：ThreadManager 错误信息：" + ex.Message);
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

       
    }
 
}
