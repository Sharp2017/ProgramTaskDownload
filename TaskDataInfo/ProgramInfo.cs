using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskDataInfo
{
  public  class ProgramInfo
    {
        /// <summary>
        /// 进程的名称
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// 程序的启动路径
        /// </summary>
        public string ProgramPath { get; set; }

        /// <summary>
        /// 程序主窗体名称
        /// </summary>
        public string ProgramMainWindowName { get; set; }
        /// <summary>
        /// 程序主窗体类名 
        /// </summary>
        public string ProgramClassName { get; set; }
    }
}
