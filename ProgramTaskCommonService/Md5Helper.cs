using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ProgramTaskCommonService
{
    public static class Md5Helper
    {
        public static string Md5Encoding(string pwd)
        {
            byte[] result = Encoding.Default.GetBytes(pwd.Trim());
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "");

        }


        /// <summary>
        /// 对给定文件路径的文件加上标签
        /// </summary>
        /// <param name="path">要加密的文件的路径</param>
        /// <returns>标签的值</returns>
        public static string MD5pdf(string path, string key)
        {

            try
            {
                FileStream get_file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] pdfFile = new byte[get_file.Length];
                get_file.Read(pdfFile, 0, (int)get_file.Length);//将文件流读取到Buffer中
                get_file.Close();

                string result = MD5Buffer(pdfFile, 0, pdfFile.Length);//对Buffer中的字节内容算MD5
                result = MD5String(result + key);//这儿点的key相当于一个密钥，这样一般人就是知道使用MD5算法，但是若不知道这个字符串还是无法计算出正确的MD5

                byte[] md5 = System.Text.Encoding.ASCII.GetBytes(result);//将字符串转换成字节数组以便写人到文件中

                FileStream fsWrite = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                fsWrite.Write(pdfFile, 0, pdfFile.Length);//将pdf文件，MD5值 重新写入到文件中。
                fsWrite.Write(md5, 0, md5.Length);
                //fsWrite.Write(pdfFile, 10, pdfFile.Length - 10);
                fsWrite.Close();

                return result;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        /// <summary>
        /// 对给定路径的文件进行验证
        /// </summary>
        /// <param name="path"></param>
        /// <returns>是否加了标签或是否标签值与内容值一致</returns>
        public static bool Check(string path, string key)
        {
            try
            {
                FileStream get_file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);


                byte[] pdfFile = new byte[get_file.Length];
                get_file.Read(pdfFile, 0, (int)get_file.Length);
                get_file.Close();
                string result = MD5Buffer(pdfFile, 0, pdfFile.Length - 32);//对pdf文件除最后32位以外的字节计算MD5，这个32是因为标签位为32位。
                result = MD5String(result + key);

                string md5 = System.Text.Encoding.ASCII.GetString(pdfFile, pdfFile.Length - 32, 32);//读取pdf文件最后32位，其中保存的就是MD5值
                return result == md5;
            }
            catch
            {

                return false;

            }
        }
        private static string MD5Buffer(byte[] pdfFile, int index, int count)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider get_md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash_byte = get_md5.ComputeHash(pdfFile, index, count);

            string result = System.BitConverter.ToString(hash_byte);
            result = result.Replace("-", "");
            return result;
        }
        private static string MD5String(string str)
        {
            byte[] MD5Source = System.Text.Encoding.ASCII.GetBytes(str);
            return MD5Buffer(MD5Source, 0, MD5Source.Length);

        }

        public static string getMD5Hash(string pathName)
        {
            string strResult = "";

            string strHashData = "";
            

            byte[] arrbytHashValue;

            System.IO.FileStream oFileStream = null;

            System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher =new System.Security.Cryptography.MD5CryptoServiceProvider();

            try

            {

                oFileStream = new System.IO.FileStream(pathName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);

                arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);//计算指定Stream 对象的哈希值

                oFileStream.Close();

                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”

                strHashData = System.BitConverter.ToString(arrbytHashValue);

                //替换-

                strHashData = strHashData.Replace("-", "");

                strResult = strHashData;

            }

            catch (System.Exception ex)

            {
                strResult = "";
                //MessageBox.Show(ex.Message);

            } 


            return strResult;

        }



    }
}
