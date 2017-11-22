using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskDataInfo
{
    /// <summary>
    /// 任务信息
    /// </summary>
  public  class TaskInfo: ProgramTaskData
    {
        #region //任务下载器 
     
        /// <summary>
        /// 任务下载器ID
        /// </summary>
        public string TaskDownloadDeviceID { get; set; }

        /// <summary>
        /// 任务下载器IP
        /// </summary>
        public string TaskDownloadIP { get; set; }
        /// <summary>
        /// 任务下载器计算机名称
        /// </summary>
        public string TaskDownloadComputerName { get; set; }

        /// <summary>
        /// 任务下载时间
        /// </summary>
        public  DateTime TaskDownloadDateTime { get; set; }

        #endregion

        /// <summary>
        /// 任务状态 (-1:失败任务，0:未开始，1:执行中，2:成功完成)
        /// </summary> 
        public int TaskState
        {
            get
            {
                return taskState;
            }

            set
            {
                taskState = value;
            }
        }
        private int taskState = -1;

       

        #region //任务执行器 

        /// <summary>
        /// 任务执行器ID
        /// </summary>
        public string TaskActuatorDeviceID { get; set; }

        /// <summary>
        /// 任务执行器IP
        /// </summary>
        public string TaskActuatorDeviceIP { get; set; }

        /// <summary>
        /// 任务执行器计算机名称
        /// </summary>
        public string TaskActuatorDeviceComputerName { get; set; }
        #endregion

        /// <summary>
        /// 任务完成时间
        /// </summary>
        public DateTime TaskCompletionDatetime { get; set; }
        /// <summary>
        /// 任务错误信息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 任务是否加锁 0：没有 ，1:锁定
        /// </summary>
        public int Islocked { get; set; }

        /// <summary>
        /// 转码完成时间
        /// </summary>
        public DateTime AudioConvertDatetime { get; set; }

        /// <summary>
        /// 质检完成时间
        /// </summary>
        public DateTime AudioAudioCheckDatetime { get; set; }

        private int needReDo = 0;
        /// <summary>
        /// 是否需要重做 (0 不重做 1 重做)
        /// </summary>
        public int NeedReDo
        {
            get
            {
                return needReDo;
            }

            set
            {
                needReDo = value;
            }
        }
    }
}
