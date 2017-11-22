using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinProgramTaskActuatorDevice.RightManagerWS;

namespace WinProgramTaskActuatorDevice.Classes
{
    public class Class2RightManagerWebService
    {
        RightManager service = null;
        public Class2RightManagerWebService()
        {
            service = new RightManager();
        }
        ArrayList arr = new ArrayList(); 
        private void checkUrl()
        {
            Random ran = new Random();
            int tmpRan = ran.Next();
            System.Collections.IEnumerator ie = this.arr.GetEnumerator();
            int count = 0;
            while (ie.MoveNext())
            {
                if (ie.Current.ToString() == Globals.RightManagerURL)
                {
                    count++;
                }
            }

            if (count >= 3)
            {
                this.service.Url = Globals.RightManagerURL2;
                this.service.Timeout = 50000;
                return;
            }

            ie = this.arr.GetEnumerator();
            count = 0;
            while (ie.MoveNext())
            {
                if (ie.Current.ToString() == Globals.RightManagerURL2)
                {
                    count++;
                }
            }

            if (count >= 3)
            {
                this.service.Url = Globals.RightManagerURL;
                this.service.Timeout = 50000;
                return;
            }
            if (tmpRan % 2 == 0)
            {

                try
                {
                    service.Url = Globals.RightManagerURL;
                    string result = service.HelloWord();

                }
                catch (System.Exception ex)
                {
                    arr.Add(Globals.RightManagerURL);
                    service.Url = Globals.RightManagerURL2;
                }
            }
            else
            {

                try
                {
                    service.Url = Globals.RightManagerURL2;
                    string result = service.HelloWord();

                }
                catch (System.Exception ex)
                {
                    arr.Add(Globals.RightManagerURL2);
                    service.Url = Globals.RightManagerURL;
                }
            }

            this.service.Timeout = 50000;
        }

        public RightManagerDS.SystemsDataTable GetSystems()
        {
            checkUrl();
            return service.GetSystems(Globals.SystemKey);
        }

        public RightManagerDS.ResourcesDataTable GetResourcesBySystemIDCat1(Guid systemID, int cat1, string systemKey)
        {
            checkUrl();
            return service.GetResourcesBySystemIDCat1(systemID, cat1, systemKey);
        }
        public List<ViewAllRightModel> GetViewAllRightByUserID(Guid userID, Guid systemID, string systemKey)
        {
            checkUrl();
            ViewAllRightModel[] varArr = service.GetViewAllRightByUserID(userID, systemID, systemKey);
            if (varArr == null) return null;
            return new List<ViewAllRightModel>(varArr);
        }
        public RightManagerDS.ResourceCategory2RightCategoryDataTable GetResourceCategory2RightCategoryBySystemIDResCat
                                                              (Guid systemID, int resourceCategory, string systemKey)
        {
            checkUrl();
            return service.GetResourceCategory2RightCategoryBySystemIDResCat(systemID, resourceCategory, systemKey);
        }

        public RightManagerDS.RightsDataTable GetRightsByRigthCategory(Guid systemID, int category, string systemKey)
        {
            checkUrl();
            return service.GetRightsByRigthCategory(systemID, category, systemKey);
        }

        /// <summary>
        ///授权
        /// </summary>
        /// <param name="resourceIDs">资源ID</param>
        /// <param name="rightIDs">权限ID</param>
        /// <param name="userID">用户ID</param>
        /// <param name="systemKey">systemKey</param>
        /// <returns></returns>
        public bool ApplyByResourceIDRightIDUserID(List<Guid> resourceIDs, List<Guid> rightIDs, Guid userID,
                                                  string systemKey)
        {
            checkUrl();
            return service.ApplyByResourceIDRightIDUserID(resourceIDs.ToArray(), rightIDs.ToArray(),
                                                                 userID, Guid.Empty, Globals.ProgramTaskActuatorDeviceID, systemKey);
        }
    }
}
