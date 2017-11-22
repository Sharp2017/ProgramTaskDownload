using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskDataInfo
{
   public class WndProcMsgModel
    {
        public const int WM_USER = 0x0400;
        public const int WM_MSG_BASE = WM_USER + 200;

       
        /// <summary>
        /// 网闸错误
        /// </summary>
        public const int WM_MSG_GATEWAYE_ERROR = WM_MSG_BASE + 1;
        /// <summary>
        /// 外网错误
        /// </summary>
        public const int WM_MSG_INTERNET_ERROR = WM_MSG_BASE + 2;
    }
}
