using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TaskDataInfo;

namespace ProgramTaskDBService
{
    public static class ProgramTaskDBManager
    {
        private static string getTmpAppConnectionString()
        {
            string connStr = "";
            try
            {
                connStr = ConfigurationManager.AppSettings.Get("primaryConnection").ToString();
                if (connStr == null)
                {
                    return "";
                }
                else
                {
                    return connStr;
                }
            }
            catch
            {
                return connStr;
            }

        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <returns></returns>
        public static bool InsertIntoTask(TaskInfo pTaskInfo)
        {

            using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.Transaction = conn.BeginTransaction();
                        cmd.CommandText = "insert into ProgramTaskData (TaskID,AudioName,AudioUrl,AudioMd5, UserName,UserPassWord,ServerTime,TaskDownloadDeviceID,TaskDownloadIP,TaskDownloadComputerName,TaskDownloadDateTime,TaskState,IsLocked,AudioSize,NeedReDo) " +
                                          "values (@TaskID,@AudioName,@AudioUrl,@AudioMd5,@UserName,@UserPassWord,@ServerTime,@TaskDownloadDeviceID,@TaskDownloadIP,@TaskDownloadComputerName,@TaskDownloadDateTime,@TaskState,@IsLocked,@AudioSize,@NeedReDo)";
                        cmd.Parameters.AddWithValue("@TaskID", pTaskInfo.data.task_id);
                        cmd.Parameters.AddWithValue("@AudioName", pTaskInfo.data.audio_name);
                        cmd.Parameters.AddWithValue("@AudioUrl", pTaskInfo.data.audio_url);
                        cmd.Parameters.AddWithValue("@AudioMd5", pTaskInfo.data.audio_md5);
                        cmd.Parameters.AddWithValue("@UserName", pTaskInfo.data.user_name);
                        cmd.Parameters.AddWithValue("@UserPassWord", pTaskInfo.data.user_password);
                        cmd.Parameters.AddWithValue("@AudioSize", pTaskInfo.data.size);
                        cmd.Parameters.AddWithValue("@ServerTime", pTaskInfo.server_time);
                        cmd.Parameters.AddWithValue("@TaskDownloadDeviceID", pTaskInfo.TaskDownloadDeviceID);
                        cmd.Parameters.AddWithValue("@TaskDownloadIP", pTaskInfo.TaskDownloadIP);
                        cmd.Parameters.AddWithValue("@TaskDownloadComputerName", pTaskInfo.TaskDownloadComputerName);
                        cmd.Parameters.AddWithValue("@TaskDownloadDateTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@TaskState", 0);
                        cmd.Parameters.AddWithValue("@IsLocked", 0);
                        cmd.Parameters.AddWithValue("@NeedReDo", 0);
                        cmd.ExecuteNonQuery();

                        cmd.Transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (cmd.Transaction != null)
                        {
                            cmd.Transaction.Rollback();
                        }
                        throw ex;
                    }
                }
            }
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <returns></returns>
        public static bool InsertIntoTaskDetail(TaskDetail pTaskDetail)
        {

            using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.Transaction = conn.BeginTransaction();
                        cmd.CommandText = "insert into TaskDetail (ID,TaskID,TaskStepName,TaskActuatorIP, TaskActuatorComputerName,ActuatorDateTime,TaskStepReport,TaskSetpState) " +
                                          "values (@ID,@TaskID,@TaskStepName,@TaskActuatorIP,@TaskActuatorComputerName,@ActuatorDateTime,@TaskStepReport,@TaskSetpState)";
                        cmd.Parameters.AddWithValue("@ID", pTaskDetail.ID);
                        cmd.Parameters.AddWithValue("@TaskID", pTaskDetail.TaskID);
                        cmd.Parameters.AddWithValue("@TaskStepName", pTaskDetail.TaskStepName);
                        cmd.Parameters.AddWithValue("@TaskActuatorIP", pTaskDetail.TaskActuatorIP);
                        cmd.Parameters.AddWithValue("@TaskActuatorComputerName", pTaskDetail.TaskActuatorComputerName);
                        cmd.Parameters.AddWithValue("@ActuatorDateTime", pTaskDetail.ActuatorDateTime);
                        cmd.Parameters.AddWithValue("@TaskStepReport", pTaskDetail.TaskStepReport);
                        cmd.Parameters.AddWithValue("@TaskSetpState", pTaskDetail.TaskSetpState); 
                        cmd.ExecuteNonQuery();

                        cmd.Transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    { 
                        if (cmd.Transaction != null)
                        {
                            cmd.Transaction.Rollback();
                        }
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// <returns></returns>
        public static bool UpdateTask(TaskInfo pTaskInfo)
        {

            using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
            {
                SqlCommand cmd = new SqlCommand();
                try
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.Transaction = conn.BeginTransaction();
                    cmd.CommandText = "Update ProgramTaskData Set  TaskState=@TaskState, TaskActuatorID=@TaskActuatorID, TaskActuatorIP=@TaskActuatorIP, TaskActuatorComputerName=@TaskActuatorComputerName, TaskCompletionDatetime=@TaskCompletionDatetime, AudioAudioCheckDatetime=@AudioAudioCheckDatetime, AudioConvertDatetime = @AudioConvertDatetime , NeedReDo = @NeedReDo  where TaskID=@TaskID";
                     
                    cmd.Parameters.AddWithValue("@TaskID", pTaskInfo.data.task_id);
                    cmd.Parameters.AddWithValue("@TaskState", pTaskInfo.TaskState);
                    cmd.Parameters.AddWithValue("@TaskActuatorID", pTaskInfo.TaskActuatorDeviceID);
                    cmd.Parameters.AddWithValue("@TaskActuatorIP", pTaskInfo.TaskActuatorDeviceIP);
                    cmd.Parameters.AddWithValue("@TaskActuatorComputerName", pTaskInfo.TaskActuatorDeviceComputerName);
                    cmd.Parameters.AddWithValue("@TaskCompletionDatetime", pTaskInfo.TaskCompletionDatetime);
                    cmd.Parameters.AddWithValue("@AudioAudioCheckDatetime", pTaskInfo.AudioAudioCheckDatetime);
                    cmd.Parameters.AddWithValue("@AudioConvertDatetime", pTaskInfo.AudioConvertDatetime);
                    cmd.Parameters.AddWithValue("@NeedReDo", pTaskInfo.NeedReDo);

                    cmd.ExecuteNonQuery();
                    cmd.Transaction.Commit();
                    return true;
                }
                catch (System.Exception ex)
                {
                    cmd.Transaction.Rollback();
                    return false;
                }

            }
        }

        /// <summary>
        /// 任务是否存在
        /// </summary>
        /// <param name="pTaskID"></param>
        /// <returns></returns>
        public static bool IsExistTask(string pTaskID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
                {

                    string sql = "select * from ProgramTaskData where TaskID='" + pTaskID + "' ";
                    SqlDataAdapter sd = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    sd.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 是否有未执行的任务
        /// </summary>
        /// <returns></returns>
        public static bool IsExistUnDoTask()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
                {

                    string sql = "select * from ProgramTaskData where TaskState=0 and IsLocked=0 ";
                    SqlDataAdapter sd = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    sd.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        /// <summary>
        /// 根据状态查询任务
        /// </summary>
        /// <param name="pTaskState"></param>
        /// <returns></returns>
        public static List<TaskInfo> GetTaskByState(string pTaskState)
        {
            try
            {
                List<TaskInfo> TaskInfoList = new List<TaskInfo>();
                using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
                {

                    string sql = "select * from ProgramTaskData ";

                    if (pTaskState != null && pTaskState.Length > 0)
                    {
                        sql += " where TaskStatus= " + pTaskState + "";
                    }
                    sql += "order by TaskDownloadDateTime Desc";
                    SqlDataAdapter sd = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    sd.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            try
                            {
                                TaskInfo _TaskInfo = new TaskInfo();
                                _TaskInfo = GetDataRow(dt.Rows[0]);
                                TaskInfoList.Add(_TaskInfo);
                            }
                            catch (Exception)
                            {

                                continue;
                            }

                        }

                    }
                    else
                    {
                        return null;
                    }
                    return TaskInfoList;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 更具锁定状体获取任务
        /// </summary>
        /// <param name="pTaskState"></param>
        /// <returns></returns>
        public static List<TaskInfo> GetTaskByLocked(string pIsLocked)
        {
            try
            {
                List<TaskInfo> TaskInfoList = new List<TaskInfo>();
                using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
                {

                    string sql = "select * from ProgramTaskData ";

                    if (pIsLocked != null && pIsLocked.Length > 0)
                    {
                        sql += " where IsLocked= " + pIsLocked + "";
                    }
                    sql += "order by TaskDownloadDateTime Desc";
                    SqlDataAdapter sd = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    sd.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            try
                            {
                                TaskInfo _TaskInfo = new TaskInfo();
                                _TaskInfo = GetDataRow(dt.Rows[0]);
                                TaskInfoList.Add(_TaskInfo);
                            }
                            catch (Exception)
                            {

                                continue;
                            }

                        }

                    }
                    else
                    {
                        return null;
                    }
                    return TaskInfoList;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 根据状态查询任务
        /// </summary>
        /// <param name="pTaskState"></param>
        /// <returns></returns>
        public static TaskInfo GetTaskByTaskID(string pTaskID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
                {

                    string sql = "select * from ProgramTaskData where TaskID='" + pTaskID + "' ";
                    SqlDataAdapter sd = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    sd.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {

                        return GetDataRow(dt.Rows[0]);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
         
        /// <summary>
        /// 根据任务加锁
        /// </summary>
        /// <param name="pTaskID"></param>
        /// <param name="islock">是否锁定</param>
        /// <returns></returns>
        public static bool LockTaskByID(string pTaskID, bool islock)
        {
            using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
            {
                SqlCommand cmd = new SqlCommand();
                try
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.Transaction = conn.BeginTransaction(); 
                    cmd.CommandText = "Update ProgramTaskData Set IsLocked=" + (islock == true ? '1' : '0') + " where TaskID='" + pTaskID + "' "; 
                    cmd.ExecuteNonQuery();
                    cmd.Transaction.Commit();
                    return true;
                }
                catch (System.Exception ex)
                {
                    cmd.Transaction.Rollback();
                    return false;
                }

            }
        }

       /// <summary>
       /// 根据任务ID删除任务
       /// </summary>
       /// <param name="pTaskID"></param>
       /// <returns></returns>
        public static bool DelTaskByID(string pTaskID)
        {
            using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
            {
                SqlCommand cmd = new SqlCommand();
                try
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.Transaction = conn.BeginTransaction(); 

                    cmd.CommandText = "delete TaskDetail  where  TaskID='" + pTaskID + "' ";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete ProgramTaskData  where  TaskID='" + pTaskID + "' ";
                    cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                    return true;
                }
                catch (System.Exception ex)
                {
                    cmd.Transaction.Rollback();
                    return false;
                }

            }
        }

        /// <summary>
        /// 获取一条需要入库的任务
        /// </summary>
        /// <param name="pTaskState">任务状态</param>
        /// <param name="isLocked">是否锁定的</param>
        /// <returns></returns>
        public static TaskInfo GetOneUnLockedTask(int pTaskState=0,bool isLocked=false)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
                {

                    string sql = "select * from ProgramTaskData where TaskState=" + pTaskState + " and IsLocked = " + (isLocked == true ? '1' : '0')+ " order by TaskDownloadDateTime  ";
                    SqlDataAdapter sd = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    sd.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    { 
                        return GetDataRow(dt.Rows[0]);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="pTaskActuatorComputerName">当前执行任务的计算机名称</param>
        /// <returns></returns>
        public static TaskInfo GetOneUnLockedTask(string pTaskActuatorComputerName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getTmpAppConnectionString()))
                {
                    //string sql =  "select * from ProgramTaskData where   IsLocked=0 and (TaskState=0 or TaskID in (select a.TaskID from ProgramTaskData a ,TaskDetail b where a.TaskID=b.TaskID and a.TaskState=-1 and b.TaskActuatorComputerName<>'" + pTaskActuatorComputerName + "' ) and (select count(distinct TaskActuatorComputerName) from TaskDetail  where  TaskID in ( select TaskID from ProgramTaskData where TaskState=-1 and TaskActuatorComputerName<>'"+ pTaskActuatorComputerName + "' ) )<" + count + " ) order by TaskDownloadDateTime  ";
                   
                    string sql = "select * from ProgramTaskData where  IsLocked = 0 and((TaskState = 0 and  NeedReDo=0) or(TaskState = -1 and NeedReDo=1 and TaskID in(SELECT distinct TaskID  FROM TaskDetail where TaskActuatorComputerName <> '" + pTaskActuatorComputerName + "') and TaskID not in (select distinct a.TaskID as TaskID from(SELECT TaskID  FROM TaskDetail where TaskActuatorComputerName <> '"+ pTaskActuatorComputerName + "') a, (SELECT TaskID  FROM TaskDetail where TaskActuatorComputerName = '"+ pTaskActuatorComputerName + "') b where a.TaskID = b.TaskID and a.TaskID in (select TaskID from ProgramTaskData where TaskState = -1 ))))order by TaskDownloadDateTime";
                  
                   SqlDataAdapter sd = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    sd.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        return GetDataRow(dt.Rows[0]);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

       

        /// <summary>
        /// 根据行构造对象
        /// </summary>
        /// <param name="pDataRow"></param>
        /// <returns></returns>
        private static TaskInfo GetDataRow(DataRow pDataRow)
        {
            try
            {
                if (pDataRow == null)
                    return null;
                TaskInfo _TaskInfo = new TaskInfo();
                AudioInfo _AudioData = new AudioInfo();
                _AudioData.task_id = pDataRow["TaskID"].ToString();
                _AudioData.audio_name = pDataRow["AudioName"].ToString();
                _AudioData.audio_url = pDataRow["AudioUrl"].ToString();
                _AudioData.audio_md5 = pDataRow["AudioMd5"].ToString();
                _AudioData.user_name = pDataRow["UserName"].ToString();
                _AudioData.user_password = pDataRow["UserPassWord"].ToString();
                _AudioData.size = Convert.ToInt32(pDataRow["AudioSize"].ToString());
                _TaskInfo.data = _AudioData;
                _TaskInfo.TaskState = Convert.ToInt32(pDataRow["TaskState"].ToString());
                _TaskInfo.TaskDownloadDeviceID = pDataRow["TaskDownloadDeviceID"].ToString();
                _TaskInfo.TaskDownloadIP = pDataRow["TaskDownloadIP"].ToString();
                _TaskInfo.TaskDownloadComputerName = pDataRow["TaskDownloadComputerName"].ToString();
                _TaskInfo.TaskDownloadDateTime = Convert.ToDateTime(pDataRow["TaskDownloadDateTime"]);
                _TaskInfo.TaskActuatorDeviceID = pDataRow["TaskActuatorID"].ToString();
                _TaskInfo.TaskActuatorDeviceIP = pDataRow["TaskActuatorIP"].ToString();
                _TaskInfo.TaskActuatorDeviceComputerName = pDataRow["TaskActuatorComputerName"].ToString();
               
                //try
                //{
                //_TaskInfo.TaskCompletionDatetime = Convert.ToDateTime(pDataRow["TaskCompletionDatetime"]);
                //}
                //catch (Exception)
                //{

                //    _TaskInfo.TaskCompletionDatetime = null;
                //}

                _TaskInfo.server_time = pDataRow["ServerTime"].ToString();
                _TaskInfo.Islocked= Convert.ToInt32(pDataRow["IsLocked"]);
                _TaskInfo.NeedReDo = Convert.ToInt32(pDataRow["NeedReDo"].ToString());

                return _TaskInfo;
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
