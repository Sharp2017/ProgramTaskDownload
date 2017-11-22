using Infomedia.XStudio.XstudioDataInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using WinProgramTaskActuatorDevice.XStudioWebService;

namespace WinProgramTaskActuatorDevice.Classes
{
    public class Class2XStudioWebService
    {
        Service service = null;

        public Class2XStudioWebService()
        {
            service = new Service();

        }

        ArrayList arr = new ArrayList();


        private void checkUrl()
        {

            Random ran = new Random();
            int tmpRan = ran.Next();

            this.service.Timeout = 3000;

            System.Collections.IEnumerator ie = this.arr.GetEnumerator();
            int count = 0;
            while (ie.MoveNext())
            {
                if (ie.Current.ToString() == Globals.XStudioWebServiceURL)
                {
                    count++;
                }
            }

            if (count >= 3)
            {
                this.service.Url = Globals.XStudioWebServiceURL2;
                this.service.Timeout = 50000;
                return;
            }

            ie = this.arr.GetEnumerator();
            count = 0;
            while (ie.MoveNext())
            {
                if (ie.Current.ToString() == Globals.XStudioWebServiceURL2)
                {
                    count++;
                }
            }

            if (count >= 3)
            {
                this.service.Url = Globals.XStudioWebServiceURL;
                this.service.Timeout = 50000;
                return;
            }
            if (tmpRan % 2 == 0)
            {
                this.service.Url = Globals.XStudioWebServiceURL;
                try
                {

                    if (this.pingServer(this.GetUrlHost(this.service.Url)))
                    {
                        this.service.HelloWorld();

                    }
                    else
                    {
                        this.service.Url = Globals.XStudioWebServiceURL2;
                    }
                }
                catch (System.Exception ex)
                {
                    arr.Add(Globals.XStudioWebServiceURL);
                    this.service.Url = Globals.XStudioWebServiceURL2;
                }
            }
            else
            {
                this.service.Url = Globals.XStudioWebServiceURL2;
                try
                {
                    if (this.pingServer(this.GetUrlHost(this.service.Url)))
                    {
                        this.service.HelloWorld();
                    }
                    else
                    {
                        this.service.Url = Globals.XStudioWebServiceURL;
                    }
                }
                catch (System.Exception ex)
                {
                    arr.Add(Globals.XStudioWebServiceURL2);
                    this.service.Url = Globals.XStudioWebServiceURL;
                }
            }



            this.service.Timeout = 50000;


        }

        private bool pingServer(string server)
        {
            if (Globals.XStudioWebServiceBadURL == server)
            {
                return false;
            }
            Ping p = new Ping();
            return true;

        }

        private string GetUrlHost(string url)
        {
            string tmpurl = url;
            string[] tmpStrings = tmpurl.Split('/');
            if (tmpStrings.Length > 0)
            {
                string[] hosts = tmpStrings[2].Split(':');
                return hosts[0];
            }
            else
            {
                return "";
            }
        }

        public string GetUserInfoByUserLoginName(string loginName, out string fullName)
        {
            try
            {
                this.checkUrl();
                return this.service.GetUserInfoByUserLoginName(loginName, out fullName);
            }
            catch
            {
                try
                {
                    this.checkUrl();
                    return this.service.GetUserInfoByUserLoginName(loginName, out fullName);
                }
                catch (System.Exception ex)
                {
                    fullName = null;
                    return null;
                }
            }

        }

        public UserDataInfoList GetUserInfoByUserID(string userid)
        {
            try
            {
                this.checkUrl();
                UserDataInfoList list = new UserDataInfoList(this.service.GetUserInfoByUserID(userid));
                return list;
            }
            catch
            {
                try
                {
                    this.checkUrl();
                    UserDataInfoList list = new UserDataInfoList(this.service.GetUserInfoByUserID(userid));
                    return list;
                }
                catch (System.Exception ex)
                {
                 
                    return null;
                }
            }

        }

        /// <summary>
        /// 获取programlib对应的Storage
        /// </summary>
        /// <param name="programLibID"></param>
        /// <returns></returns>
        public StorageDataInfo GetStorageInfoByProgramLibID(string programLibID)
        { 
            try
            {
                this.checkUrl();
                string storageXML = this.service.GetStorageInfoByProgramLibID(programLibID, Globals.Zone);
                StorageDataInfo info = new StorageDataInfo(storageXML);
                return info;
            }
            catch
            {
                try
                {
                    this.checkUrl();
                    string storageXML = this.service.GetStorageInfoByProgramLibID(programLibID, Globals.Zone);
                    StorageDataInfo info = new StorageDataInfo(storageXML);
                    return info;
                }
                catch (System.Exception ex)
                {   
                    return null;
                }
            }


        }

        /// <summary>
        /// 新增节目
        /// </summary>
        /// <param name="programXML"></param>
        /// <param name="programlibID"></param>
        /// <returns></returns>
        public int InsertProgram(string programXML, string programlibID)
        {
            try
            {
                this.checkUrl();
                return this.service.InsertToProgram(programXML, programlibID);
            }
            catch
            {
                try
                {
                    this.checkUrl();
                    return this.service.InsertToProgram(programXML, programlibID);
                }
                catch (System.Exception ex)
                {  
                    return 0;
                }
            }

        }

        /// <summary>
        /// 获取programlib
        /// </summary>
        /// <returns></returns>
        public ProgramLibDataInfoList GetProgramLibInfoByUserID(string userID)
        {
            try
            {
                this.checkUrl();
                string programLibXML = this.service.GetProgramLibInfoByUserID(userID);
                ProgramLibDataInfoList list = new ProgramLibDataInfoList(programLibXML);
                return list;
            }
            catch
            {
                try
                {
                    this.checkUrl();
                    string programLibXML = this.service.GetProgramLibInfoByUserID(userID);
                    ProgramLibDataInfoList list = new ProgramLibDataInfoList(programLibXML);
                    return list;
                }
                catch (System.Exception ex)
                {

                    return null;
                }
            }

        }


        public DataTable GetFolderTableByProgramLibIDAndUserIDAndAccessType(string programLibID, string userID, int accessType, bool isCanMake)
        {
            try
            {
                this.checkUrl();
                return this.service.GetFolderTableByProgramLibIDAndUserIDAndAccessType(programLibID, userID, accessType, isCanMake);
            }
            catch
            {
                try
                {
                    this.checkUrl();
                    return this.service.GetFolderTableByProgramLibIDAndUserIDAndAccessType(programLibID, userID, accessType, isCanMake);
                }
                catch (System.Exception ex)
                { 

                    return null;
                }
            }

        }

        public UserFolderRightInfo GetUserFolderRightInfo(string folderID,string pUserID)
        {
            try
            {
                this.checkUrl();
                UserFolderRightInfo info = new UserFolderRightInfo(this.service.GetUserFolderRightInfo(pUserID, folderID));
                return info;
            }
            catch
            {
                try
                {
                    this.checkUrl();
                    UserFolderRightInfo info = new UserFolderRightInfo(this.service.GetUserFolderRightInfo(pUserID, folderID));
                    return info;
                }
                catch (System.Exception ex)
                {
                  
                    return null;
                }
            }
        }

        public int GetFolderFreeSpace(string folderID, string programLibID, bool isprj,string pUserID)
        {
            try
            {
                this.checkUrl();
                return this.service.GetFolderFreeSpaceByType(folderID, programLibID, pUserID, isprj);
            }
            catch
            {
                try
                {
                    this.checkUrl();
                    return this.service.GetFolderFreeSpaceByType(folderID, programLibID, pUserID, isprj);
                }
                catch (System.Exception ex)
                {  
                    return 0;
                }
            }

        }
    }
}
