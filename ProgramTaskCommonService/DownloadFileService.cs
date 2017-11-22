using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ProgramTaskCommonService
{
    /// <summary>
    /// 下载服务
    /// </summary>
    public class DownloadFileService
    {
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="pURL">下载文件地址</param>
        /// <param name="Filename">下载后另存为（全路径）</param>

        public static bool HttpDownloadFile(string pURL, string pFilename, out string pError)
        {

            try
            {
                pError = null;
                if (pURL == null || pURL.Length <= 0)
                {
                    pError = "下载地址为空！";
                    return false;
                }
                else
                {
                    //if (!File.Exists(pFilename))
                    //{
                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                        System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(pURL);
                        System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                        System.IO.Stream st = myrp.GetResponseStream();
                        System.IO.Stream so = new System.IO.FileStream(pFilename, System.IO.FileMode.Create);
                        byte[] by = new byte[1024];
                        int osize = st.Read(by, 0, (int)by.Length);
                        while (osize > 0)
                        {
                            so.Write(by, 0, osize);
                            osize = st.Read(by, 0, (int)by.Length);
                        }
                        so.Close();
                        st.Close();
                        myrp.Close();
                        Myrq.Abort();
                        return true;
                    //}
                    //else
                    //{
                    //    pError = "下载保存的路径不存在！";
                    //    return true;
                    //}

                }
            }
            catch (System.Exception ex)
            {
                pError = "[pFilename:"+ pFilename + " pURL:" + pURL+"]:"+ex.Message;
                return false;
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
    }
}
