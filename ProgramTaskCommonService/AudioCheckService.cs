using AudioTechReviewDAL;
using Infomedia.XStudio.XstudioDataInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProgramTaskCommonService
{
    /// <summary>
    /// 音频质检服务
    /// </summary>
    public class AudioCheckService
    {

        public static bool AudioCheck(AudioCheckSettingsInfo pAinfo, string fileName, string title, out string report)
        {
            try
            {  
                ClassAudioTechReview.StartAudioCheck(pAinfo, fileName);

                while (ClassAudioTechReview.GetProgress() != 100)
                {
                    Application.DoEvents();
                }
                report = "";
                if (ClassAudioTechReview.GetAudioCheckStatus())
                {
                    if (File.Exists(fileName + ".txt"))
                    {
                        report = File.ReadAllText(fileName + ".txt", Encoding.GetEncoding("gb2312"));

                    }

                    try
                    {
                        if (File.Exists(fileName + ".txt"))
                        {
                            File.Delete(fileName + ".txt");
                        }
                    }
                    catch { }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                report = ex.Message;
                return false;
            }
            finally
            {
                ClassAudioTechReview.DestroyInstance();
            }



        }
    }
}
