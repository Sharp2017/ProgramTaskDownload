
using System;
using System.IO;
using XConvert2InCsharp;

namespace ProgramTaskCommonService
{
    /// <summary>
    /// 转码服务
    /// </summary>
    public class ConvertService
    {
        /// <summary>
        /// 音频文件转码
        /// </summary>
        /// <param name="pFilePath">源文件地址 </param>
        /// <param name="pError">错误信息</param>
        /// <param name="pS48File">转码后的文件地址</param>
        /// <returns></returns>
        public static bool DoConvert(string pFilePath, out string pError, out string pS48File)
        {
            try
            {  
                string s48File = pFilePath + ".s48";
                if (!File.Exists(pFilePath))
                {
                    pError = "文件不存在";
                    pS48File = null;
                    return false;
                }

                AudioFormat audio = new AudioFormat();
                audio.Bitrate = 256;
                audio.ChannelNum = 2;
                audio.Samplerate = 48000;
                audio.MyAudioENCTYPE = AudioENCTYPE.S48;

                ClassXConvert2InCsharp convert = new ClassXConvert2InCsharp();

                if (convert.DoXConvertSync_NewEx(pFilePath, s48File, audio, false, out pError))
                { 
                    pError = null;
                    pS48File = s48File;
                    if (File.Exists(s48File))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                   
                }
                else
                {
                    pS48File = null;
                    return false;
                }
            }
            catch (Exception ex)
            { 
                pError = ex.Message;
                pS48File = null;
                return false;
            }

        }
    }
}
