using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskDataInfo
{
    public class ProgramTaskData
    {
        /// <summary>
        /// 错误码，0表示操作成功，否则表示失败
        /// </summary>
        public int errno { get; set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        public string error { get; set; } 
        public AudioInfo data { get; set; }
          
        /// <summary>
        /// 接口耗费时间
        /// </summary>
        public string page_cost_time { get; set; }

        /// <summary>
        /// 服务器时间戳(秒级)
        /// </summary>
        public string server_time { get; set; }

        /// <summary>
        /// 服务器标识（可以为空）
        /// </summary>
        public string server { get; set; }

       
    }
}
