using ProgramTaskCommonService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using WinTaskProgramMonitor.Classes;

namespace WinTaskProgramMonitor
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
               

                if (Common.IsAppRunning("WinTaskProgramMonitor"))
                {
                    MessageBox.Show("程序正在运行，请先退出！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Globals.ProgramTaskDownloadDevice.ProgramPath = ConfigurationManager.AppSettings["ProgramTaskDownloadDevice"];
                Globals.ProgramTaskDownloadDevice.ProcessName = "WinProgramTaskDownloadDevice";
                Globals.ProgramTaskDownloadDevice.ProgramMainWindowName = "节目任务下载器";
                Globals.ProgramTaskDownloadDevice.ProgramClassName= "FrmMain";

                Globals.ProgramTaskActuatorDevice.ProgramPath = ConfigurationManager.AppSettings["ProgramTaskActuatorDevice"];
                Globals.ProgramTaskActuatorDevice.ProcessName = "WinProgramTaskActuatorDevice";
                Globals.ProgramTaskActuatorDevice.ProgramMainWindowName = "节目入库执行器";
                Globals.ProgramTaskDownloadDevice.ProgramClassName = "FrmMain";

                Globals.GateWayIP= ConfigurationManager.AppSettings["GateWayIP"];

                Application.Run(new FrmMain());
            }
            catch (Exception ex)
            {

                LogService.WriteErr(ex.Message);
            }
           
        }
    }
}
