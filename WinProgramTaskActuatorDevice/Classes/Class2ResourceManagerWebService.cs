using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinProgramTaskActuatorDevice.ResourceManagerWS;

namespace WinProgramTaskActuatorDevice.Classes
{
   public class Class2ResourceManagerWebService

    {
        ResourceManager service = null;
        public Class2ResourceManagerWebService()
        {
            service = new ResourceManager();
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
                if (ie.Current.ToString() == Globals.ResourceManagerURL)
                {
                    count++;
                }
            }

            if (count >= 3)
            {
                this.service.Url = Globals.ResourceManagerURL2;
                this.service.Timeout = 50000;
                return;
            }

            ie = this.arr.GetEnumerator();
            count = 0;
            while (ie.MoveNext())
            {
                if (ie.Current.ToString() == Globals.ResourceManagerURL2)
                {
                    count++;
                }
            }

            if (count >= 3)
            {
                this.service.Url = Globals.ResourceManagerURL;
                this.service.Timeout = 50000;
                return;
            }
            if (tmpRan % 2 == 0)
            {

                try
                {
                    service.Url = Globals.ResourceManagerURL;
                    string result = service.HelloWord();

                }
                catch (System.Exception ex)
                {
                    arr.Add(Globals.ResourceManagerURL);
                    service.Url = Globals.ResourceManagerURL2;
                }
            }
            else
            {

                try
                {
                    service.Url = Globals.ResourceManagerURL2;
                    string result = service.HelloWord();

                }
                catch (System.Exception ex)
                {
                    arr.Add(Globals.ResourceManagerURL2);
                    service.Url = Globals.ResourceManagerURL;
                }
            }

            this.service.Timeout = 50000;
        }
        public XStudioDS.ProgramLibDataTable GetProgramLibsByUserID(Guid userID, out int nextProgramLibNumber)
        {
            checkUrl();
            return service.GetProgramLibsByUserID(userID, out nextProgramLibNumber);
        }
        public XStudioDS.ProgramLibDataTable GetProgramLibsByUserIDProgramLibType(Guid userID, int programLibType)
        {
            checkUrl();
            return service.GetProgramLibsByUserIDProgramLibType(userID, programLibType);
        }
        public bool CreateFolder(Guid folderID, string folderName, string shortName, Guid programLibID, short folderType,
                               Guid parentID, Guid rootID, Guid creatorID, string creatorName, int maxSpaceMB,
                               int freeSpaceMB, int maxSpaceHour, int freeSpaceHour, bool shared, short superviseLevel,
                               bool materialAccess, int materialMB, int materialHour, bool projectAccess, int projectMB,
                               int projectHour, bool telephoneAccess, int telephoneMB, int telephoneHour,
                               string telephoneNumber, bool isNeedExport, int keepDays, Guid systemID,
                               int resourceCategory1, int resourceCategory2, string resourceCategoryName2,
                               Guid createUserID, string createUserName)
        {
            checkUrl();
            return service.CreateFolder(folderID, folderName, shortName, programLibID, (short)folderType,
                                                  parentID, rootID, creatorID, creatorName, maxSpaceMB, freeSpaceMB,
                                                  maxSpaceHour, freeSpaceHour, shared, (short)superviseLevel,
                                                  materialAccess, materialMB, materialHour, projectAccess, projectMB,
                                                  projectHour, telephoneAccess, telephoneMB, telephoneHour,
                                                  telephoneNumber, isNeedExport, keepDays, systemID, resourceCategory1,
                                                  resourceCategory2, resourceCategoryName2, createUserID, createUserName);
        }
    }
}
