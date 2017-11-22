using ProgramTaskCommonService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using WinProgramTaskActuatorDevice.Classes;

namespace WinProgramTaskActuatorDevice
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
                if (Common.IsAppRunning("WinProgramTaskActuatorDevice"))
                {
                    MessageBox.Show("程序正在运行，请先退出!", "提示!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Globals.TaskDataRequestURL = ConfigurationManager.AppSettings["TaskDataRequestURL"];
                Globals.ProgramTaskActuatorDeviceID = ConfigurationManager.AppSettings["ProgramTaskActuatorDeviceID"];
                Globals.ComputerIP = Common.GetComputerIP();
                Globals.ComputerName = System.Net.Dns.GetHostName();
                Globals.SystemKey = ConfigurationManager.AppSettings["SystemKey4FirstCheck"];
                Globals.RightManagerURL = ConfigurationManager.AppSettings["RightManagerURL"];
                Globals.RightManagerURL2 = ConfigurationManager.AppSettings["RightManagerURL2"];
                Globals.ResourceManagerURL = ConfigurationManager.AppSettings["ResourceManagerURL"];
                Globals.ResourceManagerURL2 = ConfigurationManager.AppSettings["ResourceManagerURL2"];
                Globals.XStudioWebServiceURL = ConfigurationManager.AppSettings["XStudioWebServiceURL"];
                Globals.XStudioWebServiceURL2 = ConfigurationManager.AppSettings["XStudioWebServiceURL2"];
                Globals.ProgramLibName = ConfigurationManager.AppSettings["ProgramLibName"];
                Globals.Zone = ConfigurationManager.AppSettings["Zone"];
                Globals.TempFileDownLoadPath = ConfigurationManager.AppSettings["TempFileDownLoadPath"];
                Globals.GateWayIP = ConfigurationManager.AppSettings["GateWayIP"];
                #region FTP 
                Globals.FTPIP = ConfigurationManager.AppSettings["FTPIP"];
                Globals.FTPPort = ConfigurationManager.AppSettings["FTPPort"];
                Globals.StorageInfoID = ConfigurationManager.AppSettings["StorageInfoID"];
                #endregion


                #region //获取质检参数  
                Globals.AudioCheckInfo.IsCheckMutedbfs = ConfigurationManager.AppSettings["IsCheckMutedbfs"].ToString().Trim() == "0" ? false : true;
                Globals.AudioCheckInfo.Mutedbfs = Convert.ToInt32(ConfigurationManager.AppSettings["Mutedbfs"].ToString().Trim());
                Globals.AudioCheckInfo.MuteDuration = Convert.ToInt32(ConfigurationManager.AppSettings["MuteDuration"].ToString().Trim());
                Globals.AudioCheckInfo.IsCheckReverse = ConfigurationManager.AppSettings["IsCheckReverse"].ToString().Trim() == "0" ? false : true;
                Globals.AudioCheckInfo.Reverse = Convert.ToDouble(ConfigurationManager.AppSettings["Reverse"].ToString().Trim());
                Globals.AudioCheckInfo.ReversDuration = Convert.ToInt32(ConfigurationManager.AppSettings["ReversDuration"].ToString().Trim());
                Globals.AudioCheckInfo.IsCheckOverloaddbfs = ConfigurationManager.AppSettings["IsCheckOverloaddbfs"].ToString().Trim() == "0" ? false : true;
                Globals.AudioCheckInfo.Overloaddbfs = Convert.ToInt32(ConfigurationManager.AppSettings["Overloaddbfs"].ToString().Trim());
                Globals.AudioCheckInfo.IsCheckSLevelThreshold_Limit = ConfigurationManager.AppSettings["IsCheckSLevelThreshold_Limit"].ToString().Trim() == "0" ? false : true;
                Globals.AudioCheckInfo.SLevelThreshold_Limit = Convert.ToInt16(ConfigurationManager.AppSettings["SLevelThreshold_Limit"].ToString().Trim());
                #endregion

                //try
                //{
                //    Globals.UserSpaceAlarmValue = Convert.ToInt32(ConfigurationManager.AppSettings["UserSpaceAlarmValue"]);
                //}
                //catch (Exception)
                //{

                //    Globals.UserSpaceAlarmValue = 10;
                //}

                try
                {
                    Globals.MaxSpaceHour = Convert.ToInt32(ConfigurationManager.AppSettings["MaxSpaceHour"]);
                }
                catch (Exception)
                {

                    Globals.MaxSpaceHour = 20;
                }
                try
                {
                    Globals.TaskActuatorInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TaskActuatorInterval"]);
                }
                catch (Exception)
                {

                    Globals.TaskActuatorInterval = 2;
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
