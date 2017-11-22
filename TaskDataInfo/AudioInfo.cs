using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskDataInfo
{
    public class AudioInfo
    { 
        /// <summary>
        /// 任务ID
        /// </summary>
        public string task_id { get; set; }

        /// <summary>
        /// 音频名称
        /// </summary>
        public string audio_name { get; set; }

        /// <summary>
        /// 音频地址
        /// </summary>
        public string audio_url { get; set; }

        /// <summary>
        /// 文件md5
        /// </summary>
        public string audio_md5 { get; set; }

        /// <summary>
        /// 用户登录名
        /// </summary>
        public string user_name { get; set; }

        /// <summary>
        /// 用户密码 
        /// </summary>
        public string user_password { get; set; }

        /// <summary>
        /// 回调状态 1:调用接口成功，0:失败
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 文件大小 (单位：K)
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// 错误详情
        /// </summary>
        public string detail_report { get; set; }


    }
}
