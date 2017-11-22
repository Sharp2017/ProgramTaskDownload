using ProgramTaskCommonService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using TaskDataInfo;

namespace WinProgramTaskDownloadDevice.Classes
{
    public class TingTingAPIService
    {
        /// <summary>
        /// 获取任务信息
        /// </summary>
        /// <returns></returns>
        public TaskInfo GetTask()
        {
            TaskInfo _TaskInfo = new TaskInfo();
            try
            {
                string _Url = "/infomedia/get_audio_task";
                if (Globals.TaskDataRequestURL == string.Empty)
                {
                    _TaskInfo.TaskState = -1;
                    _TaskInfo.ErrorMsg = "请求地址为空！";
                }
                _Url = Globals.TaskDataRequestURL + _Url;
                string result = string.Empty;

                #region //组织参数  
                string api_hash = "";
                string _time = Common.DateTimeToStamp(DateTime.Now).ToString();
                SortedList _SortedList = new SortedList();
                _SortedList.Add("client", Globals.ComputerName);
                _SortedList.Add("time", _time);

                foreach (DictionaryEntry item in _SortedList)
                {
                    string tem = item.Key.ToString() + "=" + HttpUtility.UrlEncode(item.Value.ToString());
                    api_hash += tem + "&";
                }
                api_hash = api_hash.Substring(0, api_hash.Length - 1);
                api_hash += "_" + Globals.TingTingFM;
                api_hash = Md5Helper.Md5Encoding(api_hash).ToLower();

                Encoding encoding = Encoding.GetEncoding("gb2312");
                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("client", Globals.ComputerName);
                parameters.Add("time", _time);
                parameters.Add("api_hash", api_hash);

                #endregion

                HttpWebResponse response = HttpWebResponseUtility.CreatePostHttpResponse(_Url, parameters, null, null, encoding, null);

                #region //解析数据 

                if (response.StatusCode == HttpStatusCode.OK)
                {

                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        result = sr.ReadToEnd();
                    }

                    System.Web.Script.Serialization.JavaScriptSerializer a = new
                System.Web.Script.Serialization.JavaScriptSerializer();
                    _TaskInfo = a.Deserialize<TaskInfo>(result);
                    if (_TaskInfo.errno != 0)
                    {
                        _TaskInfo.TaskState = -1;
                        _TaskInfo.ErrorMsg = "任务请求失败 :" + _TaskInfo.error + "讯听时间：" + _TaskInfo.server_time + "参数时间：" + _time;
                    }
                    else
                    {
                        _TaskInfo.TaskDownloadComputerName = Globals.ComputerName;
                        _TaskInfo.TaskDownloadDeviceID = Globals.ProgramTaskDownloadDeviceID;
                        _TaskInfo.TaskDownloadIP = Globals.ComputerIP;
                        _TaskInfo.TaskState = 0;
                        _TaskInfo.TaskDownloadDateTime = DateTime.Now;
                    }

                }
                else
                {
                    _TaskInfo.TaskState = -1;
                    _TaskInfo.ErrorMsg = "任务请求失败 :" + response.StatusDescription;
                }
                #endregion

            }
            catch (Exception ex)
            {
                _TaskInfo.TaskState = -1;
                _TaskInfo.ErrorMsg = "任务请求失败 :" + ex.Message;
            }

            return _TaskInfo;
        }

        // URL特殊符号及对应的十六进制值编码：   

        //+  URL 中+号表示空格 %2B   
        //空格 URL中的空格可以用+号或者编码 %20   
        ///  分隔目录和子目录 %2F    
        //?  分隔实际的 URL 和参数 %3F    
        //% 指定特殊字符 %25    
        //# 表示书签 %23    
        //& URL 中指定的参数间的分隔符 %26    
        //= URL 中指定参数的值 %3D  
        /// <summary>
        /// 任务回调
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <param name="pError"></param>
        /// <returns></returns>
        public bool TaskDealCallback(TaskInfo pTaskInfo, out string pError)
        {
            pError = "";

            try
            {
                string _Url = "/infomedia/deal_callback";
                if (Globals.TaskDataRequestURL == string.Empty)
                {
                    pError = "请求地址为空!";
                }
                _Url = Globals.TaskDataRequestURL + _Url;
                string result = string.Empty;

                #region //组织参数  
                Encoding encoding;
                IDictionary<string, string> parameters;
                CreatParameter(pTaskInfo, out encoding, out parameters);

                //LogService.Write("client：" + Globals.ComputerName);
                //LogService.Write("time：" + _time);
                //LogService.Write("task_id：" + task_id);
                //LogService.Write("deal_result：" + deal_result);
                //LogService.Write("agent_deal_time：" + agent_deal_time);
                //LogService.Write("transfer_to_pnet_time：" + transfer_to_pnet_time);
                //LogService.Write("errno：" + errno);
                //LogService.Write("error：" + error);
                //LogService.Write("detail_report:" + detail_report);
                //LogService.Write("api_hash：" + api_hash);
                #endregion


                HttpWebResponse response = null;
                bool IsTimeOut = true;
                // HttpWebResponse response = HttpWebResponseUtility.CreatePostHttpResponse(_Url, parameters, null, null, encoding, null);

                DateTime TryTime = DateTime.Now;
                try
                {
                    response = HttpWebResponseUtility.CreatePostHttpResponse(_Url, parameters, null, null, encoding, null);
                }
                catch (Exception)
                {
                    #region //服务请求失败启动30分钟重试机制  
                    while (DateTime.Compare(TryTime.AddMinutes(30), DateTime.Now) > 0)
                    {
                        try
                        {
                            CreatParameter(pTaskInfo, out encoding, out parameters);
                            response = HttpWebResponseUtility.CreatePostHttpResponse(_Url, parameters, null, null, encoding, null);
                            IsTimeOut = false;
                            break;

                        }
                        catch  
                        {

                        }
                        //休眠一分钟
                        Thread.Sleep(60000);
                    }
                    if (IsTimeOut)
                    {
                        CreatParameter(pTaskInfo, out encoding, out parameters);
                        response = HttpWebResponseUtility.CreatePostHttpResponse(_Url, parameters, null, null, encoding, null);
                    }


                    #endregion
                }

                #region //解析数据 
                TryTime = DateTime.Now;
                if (response.StatusCode == HttpStatusCode.OK)
                {

                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        result = sr.ReadToEnd();
                    }

                    System.Web.Script.Serialization.JavaScriptSerializer a = new
              System.Web.Script.Serialization.JavaScriptSerializer();
                    TaskInfo _TaskInfo = a.Deserialize<TaskInfo>(result);
                    if (_TaskInfo.data.status == "1")
                    {
                        pError = "回调成功!";
                        return true;
                    }
                    else
                    {
                        pError = _TaskInfo.error;
                        return false;
                    }

                }
                else
                {
                    #region //服务请求失败启动30分钟重试机制 

                    IsTimeOut = true;
                    while (DateTime.Compare(TryTime.AddMinutes(30), DateTime.Now) > 0)
                    {
                        CreatParameter(pTaskInfo, out encoding, out parameters);

                        response = HttpWebResponseUtility.CreatePostHttpResponse(_Url, parameters, null, null, encoding, null);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            IsTimeOut = false;
                            break;
                        }


                        //休眠一分钟
                        Thread.Sleep(60000);

                    }
                    if (!IsTimeOut)
                    {
                        using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            result = sr.ReadToEnd();
                        }

                        System.Web.Script.Serialization.JavaScriptSerializer a = new
                  System.Web.Script.Serialization.JavaScriptSerializer();
                        TaskInfo _TaskInfo = a.Deserialize<TaskInfo>(result);
                        if (_TaskInfo.data.status == "1")
                        {
                            pError = "回调成功!";
                            return true;
                        }
                        else
                        {
                            pError = _TaskInfo.error;
                            return false;
                        }
                    }
                    else
                    {
                        pError = "任务请求失败 :" + response.StatusDescription;
                        return false;
                    }
                    #endregion

                }
                #endregion

            }
            catch (Exception ex)
            {

                pError = "任务请求失败 :" + ex.Message;
                return false;
            }

        }

        private static void CreatParameter(TaskInfo pTaskInfo, out Encoding encoding, out IDictionary<string, string> parameters)
        {
            //公共参数构造
            string api_hash = "";
            string _time = Common.DateTimeToStamp(DateTime.Now).ToString();

            SortedList _SortedList = new SortedList();
            _SortedList.Add("client", Globals.ComputerName);
            _SortedList.Add("time", _time);

            //函数参数构造
            //任务ID
            string task_id = pTaskInfo.data.task_id;
            //代理服务器完成转码技审时间(时间戳 秒级)
            string deal_result = pTaskInfo.TaskState == 2 ? "1" : "0";
            //代理服务器完成转码技审时间(时间戳 秒级)
            string agent_deal_time = Common.DateTimeToStamp(pTaskInfo.AudioAudioCheckDatetime).ToString();
            //传输制作网完成时间(时间戳 秒级)
            string transfer_to_pnet_time = Common.DateTimeToStamp(pTaskInfo.TaskCompletionDatetime).ToString();
            //错误标识
            string errno = pTaskInfo.TaskState == 2 ? "1" : "0";
            //错误描述
            string error = pTaskInfo.error;
            if (error != null && error.Length > 0)
            {
                byte[] b = System.Text.Encoding.UTF8.GetBytes(error);
                error = Convert.ToBase64String(b);
            }

            string detail_report = pTaskInfo.data.detail_report;
            if (detail_report != null && detail_report.Length > 0)
            {
                byte[] d = System.Text.Encoding.UTF8.GetBytes(detail_report);
                detail_report = Convert.ToBase64String(d);//.Replace("+", "%20").Replace("/", "%2F");
            }


            // error = "error";
            _SortedList.Add("task_id", task_id);
            _SortedList.Add("deal_result", deal_result);
            _SortedList.Add("agent_deal_time", agent_deal_time);
            _SortedList.Add("transfer_to_pnet_time", transfer_to_pnet_time);
            _SortedList.Add("errno", errno);
            _SortedList.Add("error", error);
            _SortedList.Add("detail_report", detail_report);

            foreach (DictionaryEntry item in _SortedList)
            {
                string value = HttpUtility.UrlEncode(item.Value.ToString(), Encoding.UTF8);

                if (item.Key.ToString() == "error" || item.Key.ToString() == "detail_report")
                {
                    value = item.Value.ToString().Replace("+", "%20").Replace("/", "%2F").Replace("=", "%3D");
                }
                string tem = item.Key.ToString() + "=" + value.Replace("%7E", "~");
                api_hash += tem + "&";
            }

            api_hash = api_hash.Substring(0, api_hash.Length - 1);
            api_hash += "_" + Globals.TingTingFM;

            //LogService.Write("api_hash：" + api_hash);
            api_hash = Md5Helper.Md5Encoding(api_hash).ToLower();
            //LogService.Write("api_hash Md5" + api_hash);

            encoding = Encoding.UTF8;
            parameters = new Dictionary<string, string>();
            parameters.Add("client", Globals.ComputerName);
            parameters.Add("time", _time);
            parameters.Add("task_id", task_id);
            parameters.Add("deal_result", deal_result);
            parameters.Add("agent_deal_time", agent_deal_time);
            parameters.Add("transfer_to_pnet_time", transfer_to_pnet_time);
            parameters.Add("errno", errno);
            parameters.Add("error", error);
            parameters.Add("detail_report", detail_report);
            parameters.Add("api_hash", api_hash);
        }

    }
}
