using ProgramTaskCommonService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using WinProgramTaskDownloadDevice.Classes;

namespace WinProgramTaskDownloadDevice
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {


                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (Common.IsAppRunning("WinProgramTaskDownloadDevice"))
                {
                    MessageBox.Show("程序正在运行，请先退出！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Globals.TaskDataRequestURL = ConfigurationManager.AppSettings["TaskDataRequestURL"];
                Globals.ProgramTaskDownloadDeviceID = ConfigurationManager.AppSettings["ProgramTaskDownloadDeviceID"];
                Globals.ComputerIP = Common.GetComputerIP();
                Globals.ComputerName = System.Net.Dns.GetHostName();

                try
                {
                    Globals.TaskExecuteTime = Convert.ToInt32(ConfigurationManager.AppSettings["TaskExecuteTime"]);
                }
                catch (Exception)
                {

                    Globals.TaskExecuteTime = 60;
                }
                try
                {
                    Globals.TaskExecuteOutThreshold = Convert.ToInt32(ConfigurationManager.AppSettings["TaskExecuteOutThreshold"]);
                }
                catch (Exception)
                {

                    Globals.TaskExecuteOutThreshold = 1;
                }
                
                try
                {
                    Globals.TaskRequestInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TaskRequestInterval"]);
                }
                catch (Exception)
                {

                    Globals.TaskRequestInterval = 1;
                }
                Application.Run(new FrmMain());
            }
            catch (Exception ex)
            {

                LogService.WriteErr(ex.Message);
            }

        }
    }
}
