using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskDataInfo
{
   public  class TaskDetail
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 关联任务表
        /// </summary>
        public string TaskID { get; set; }

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string TaskStepName { get; set; }

        /// <summary>
        /// 执行任务的计算机IP
        /// </summary>
        public string TaskActuatorIP { get; set; }
        /// <summary>
        ///  执行任务的计算机名称
        /// </summary>
        public string TaskActuatorComputerName { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime ActuatorDateTime { get; set; }

        /// <summary>
        /// 详情
        /// </summary>
        public string TaskStepReport { get; set; }

        /// <summary>
        /// 任务状态 1成功 -1失败
        /// </summary>
        public int TaskSetpState { get; set; }
         

    }
}
