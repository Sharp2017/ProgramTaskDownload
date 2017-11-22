using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace XConvert2InCsharp
{
    public partial class ClassXConvert2InCsharp : Form
    {

        [DllImport("XConvert2.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr XC2_ExtractEnergyFile(string lpszAudioFile, string lpszEnergyFileName, IntPtr hWnd, UInt32 nMsg);

        [DllImport("XConvert2.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr XC2_EncFile(string lpszAudioFile, string lpszEncFileName, IntPtr hWnd, UInt32 nMsg);

        [DllImport("XConvert2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int XC2_GetEventNotify(IntPtr xcHandle);
        [DllImport("XConvert2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string XC2_GetLastError();

        [DllImport("XConvert2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int XC2_GetProgress(IntPtr xcHandle);
        [DllImport("XConvert2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void XC2_Cancel(IntPtr xcHandle);
        [DllImport("XConvert2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int XC2_Start(IntPtr xcHandle);


        [DllImport("XConvert2.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int XC2_SetParam(IntPtr xcHandle, string szParam, string szValue);

        [DllImport("XConvert2.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int XC2_GetFileInfo(string lpszAudioFile, StringBuilder xml, ref int Len);

        public IntPtr XConvertHandle = IntPtr.Zero;
        public const int USER = 0x8000;
        public const int WM_NOTIFY = USER + 1;



        bool isDone = false;

        public bool IsDone
        {
            get { return isDone; }
            set { isDone = value; }
        }

        bool isError = false;
        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError
        {
            get
            {
                return isError;
            }

            set
            {
                isError = value;
            }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg
        {
            get
            {
                return errorMsg;
            }

            set
            {
                errorMsg = value;
            }
        }

        string errorMsg = null;


        /// <summary>
        /// 获取音频信息
        /// </summary>
        /// <param name="sourceAudioFile">源文件路径</param>
        /// <returns>音频格式</returns>
        public AudioFormat GetAudioFormat(string sourceAudioFile)
        {
            AudioFormat format = new AudioFormat();
            int nlen = 2048;
            StringBuilder xml = new StringBuilder();
            xml.Capacity = nlen;
            int nRes = XC2_GetFileInfo(sourceAudioFile, xml, ref nlen);
            if (nRes != 0)
            {
                format.IsAudioFile = false;
            }
            else
            {
                format.IsAudioFile = true;
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.LoadXml(xml.ToString());
                    XmlNode rootNode = xmlDoc.SelectSingleNode("FileInfo");
                    XmlNode Bitrate = rootNode.SelectSingleNode("Bitrate");
                    format.Bitrate = int.Parse(Bitrate.InnerText.Trim());
                    XmlNode Duration = rootNode.SelectSingleNode("Duration");
                    format.Duration = long.Parse(Duration.InnerText.Trim());
                    XmlNode ChannelNum = rootNode.SelectSingleNode("ChannelNum");
                    format.ChannelNum = int.Parse(ChannelNum.InnerText.Trim());
                    XmlNode Samplerate = rootNode.SelectSingleNode("Samplerate");
                    format.Samplerate = int.Parse(Samplerate.InnerText.Trim());

                }
                catch
                {
                    string xmlinfo = "<Audio>" + xml.ToString() + "</Audio>";
                    xmlDoc.LoadXml(xmlinfo.ToString());
                    XmlNode rootNode = xmlDoc.SelectSingleNode("Audio");
                    XmlNode Bitrate = rootNode.SelectSingleNode("Bitrate");
                    format.Bitrate = int.Parse(Bitrate.InnerText.Trim());
                    XmlNode Duration = rootNode.SelectSingleNode("Duration");
                    format.Duration = long.Parse(Duration.InnerText.Trim());
                    XmlNode ChannelNum = rootNode.SelectSingleNode("ChannelNum");
                    format.ChannelNum = int.Parse(ChannelNum.InnerText.Trim());
                    XmlNode Samplerate = rootNode.SelectSingleNode("Samplerate");
                    format.Samplerate = int.Parse(Samplerate.InnerText.Trim());
                }
                //string xmlinfo = "<Audio>" + xml.ToString() + "</Audio>";

                //if (rootNode == null)
                //{

                //}
                //else
                //{

                //}

            }
            return format;
        }


        public bool GetAudioEnergyFile(string sourceAudioFile)
        {
            string energyfilename = sourceAudioFile.Trim() + ".wfm";
            XConvertHandle = XC2_ExtractEnergyFile(sourceAudioFile, energyfilename, Handle, WM_NOTIFY);
            if (XConvertHandle == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                XC2_Start(XConvertHandle);
                return true;
            }
        }

        public bool GetAudioEnergyFileSync(string sourceAudioFile)
        {
            int waitCount = 0;
            this.isDone = false;
            string energyfilename = sourceAudioFile.Trim() + ".wfm";
            XConvertHandle = XC2_ExtractEnergyFile(sourceAudioFile, energyfilename, Handle, WM_NOTIFY);
            if (XConvertHandle == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                XC2_Start(XConvertHandle);
                while (!this.isDone && waitCount < 1000)
                {
                    waitCount++;
                    Application.DoEvents();
                    Thread.Sleep(200);
                }
                return this.isDone;
            }
        }



        public void DoCancel()
        {
            XC2_Cancel(XConvertHandle);
            XConvertHandle = IntPtr.Zero;
        }

        /// <summary>
        /// 音频转换
        /// </summary>
        /// <param name="sourceAudioFile">源文件路径</param>
        /// <param name="targetAudioFile">目标文件路径</param>
        /// <param name="targetFileAudioFormat">目标文件格式</param>
        /// <returns>是否可以转换</returns>
        public bool DoXConvert(string sourceAudioFile, string targetAudioFile, AudioFormat targetFileAudioFormat)
        {
            XConvertHandle = XC2_EncFile(sourceAudioFile, targetAudioFile, Handle, WM_NOTIFY);
            this.isDone = false;
            if (XConvertHandle == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.S48)
                {
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "s48");
                }
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.MP3)
                {
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "mp3");
                }
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WAV)
                {
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "wav");
                }
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WMA)
                {
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "wma");
                }
                XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                XC2_Start(XConvertHandle);
                return true;
            }
        }

        /// <summary>
        /// 音频转换
        /// </summary>
        /// <param name="sourceAudioFile">源文件路径</param>
        /// <param name="targetAudioFile">目标文件路径</param>
        /// <param name="targetFileAudioFormat">目标文件格式</param>
        /// <param name="isNeedENERGYFILE">是否提取波形</param>
        /// <returns>转换是否成功</returns>
        public bool DoXConvert(string sourceAudioFile, string targetAudioFile, AudioFormat targetFileAudioFormat, bool isNeedENERGYFILE)
        {
            XConvertHandle = XC2_EncFile(sourceAudioFile, targetAudioFile, Handle, WM_NOTIFY);
            this.isDone = false;
            if (XConvertHandle == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.S48)
                {
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "s48");
                }
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.MP3)
                {
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "mp3");
                }
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WAV)
                {
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "wav");
                }
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WMA)
                {
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "wma");
                }
                XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                if (isNeedENERGYFILE)
                {
                    XC2_SetParam(XConvertHandle, "ENERGYFILE", targetAudioFile + ".wfm");
                }
                XC2_Start(XConvertHandle);
                return true;
            }
        }

        /// <summary>
        /// 音频转换(同步)
        /// </summary>
        /// <param name="sourceAudioFile">源文件路径</param>
        /// <param name="targetAudioFile">目标文件路径</param>
        /// <param name="targetFileAudioFormat">目标文件格式</param>
        /// <param name="isNeedENERGYFILE">是否提取波形</param>
        /// <returns>转换是否成功</returns>
        public bool DoXConvertSync(string sourceAudioFile, string targetAudioFile, AudioFormat targetFileAudioFormat, bool isNeedENERGYFILE)
        {
            try
            {
                XConvertHandle = XC2_EncFile(sourceAudioFile, targetAudioFile, Handle, WM_NOTIFY);
                int waitCount = 0;
                this.isDone = false;
                if (XConvertHandle == IntPtr.Zero)
                {
                    return false;
                }
                else
                {
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.S48)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "s48");
                    }
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.MP3)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "mp3");
                    }
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WAV)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "wav");
                    }
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WMA)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "wma");
                    }

                    if (isNeedENERGYFILE)
                    {
                        XC2_SetParam(XConvertHandle, "ENERGYFILE", targetAudioFile + ".wfm");
                    }
                    XC2_Start(XConvertHandle);
                    while (!this.isDone && waitCount < 10000)
                    {
                        waitCount++;
                        Application.DoEvents();
                        //Thread.Sleep(200);
                    }
                    XC2_Cancel(XConvertHandle);
                    return this.isDone;
                }
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 音频转换(同步)
        /// </summary>
        /// <param name="sourceAudioFile">源文件路径</param>
        /// <param name="targetAudioFile">目标文件路径</param>
        /// <param name="targetFileAudioFormat">目标文件格式</param>
        /// <param name="isNeedENERGYFILE">是否提取波形</param>
        /// <returns>转换是否成功</returns>
        public bool DoXConvertSync_New(string sourceAudioFile, string targetAudioFile, AudioFormat targetFileAudioFormat, bool isNeedENERGYFILE)
        {
            XConvertHandle = XC2_EncFile(sourceAudioFile, targetAudioFile, Handle, WM_NOTIFY);
            int waitCount = 0;
            this.isDone = false;
            if (XConvertHandle == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.S48)
                {
                    XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                    XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "s48");
                }
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.MP3)
                {
                    XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                    XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "mp3");
                }
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WAV)
                {
                    XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "wav");
                }
                if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WMA)
                {
                    XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                    XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                    XC2_SetParam(XConvertHandle, "ENCTYPE", "wma");
                }

                if (isNeedENERGYFILE)
                {
                    XC2_SetParam(XConvertHandle, "ENERGYFILE", targetAudioFile + ".wfm");
                }
                XC2_Start(XConvertHandle);
                while (!this.isDone && waitCount < 10000)
                {
                    waitCount++;
                    Application.DoEvents();
                    Thread.Sleep(20);
                }
                XC2_Cancel(XConvertHandle);
                return this.isDone;
            }
        }

        /// <summary>
        /// 音频转换(同步)
        /// </summary>
        /// <param name="sourceAudioFile">源文件路径</param>
        /// <param name="targetAudioFile">目标文件路径</param>
        /// <param name="targetFileAudioFormat">目标文件格式</param>
        /// <param name="isNeedENERGYFILE">是否提取波形</param>
        /// /// <param name="pError">转码错误信息</param>
        /// <returns>转换是否成功</returns>
        public bool DoXConvertSync_New(string sourceAudioFile, string targetAudioFile, AudioFormat targetFileAudioFormat, bool isNeedENERGYFILE, out string pError)
        {
            try
            {

                XConvertHandle = XC2_EncFile(sourceAudioFile, targetAudioFile, Handle, WM_NOTIFY);
                int waitCount = 0;
                this.isDone = false;
                if (XConvertHandle == IntPtr.Zero)
                {
                    pError = XConvertHandle.ToString();
                    return false;
                }
                else
                {
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.S48)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "s48");
                    }
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.MP3)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "mp3");
                    }
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WAV)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "wav");
                    }
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WMA)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "wma");
                    }

                    if (isNeedENERGYFILE)
                    {
                        XC2_SetParam(XConvertHandle, "ENERGYFILE", targetAudioFile + ".wfm");
                    }
                    XC2_Start(XConvertHandle);
                    while (!this.isDone && waitCount < 10000)
                    {
                        waitCount++;
                        Application.DoEvents();
                        Thread.Sleep(20);
                    }
                    
                    XC2_Cancel(XConvertHandle);
                    pError = null;
                    return this.isDone;
                }

            }
            catch (Exception ex)
            {
                pError = ex.Message;
                return false;


            }
        }
        public bool DoXConvertSync_NewEx(string sourceAudioFile, string targetAudioFile, AudioFormat targetFileAudioFormat, bool isNeedENERGYFILE, out string pError)
        {
            try
            {

                XConvertHandle = XC2_EncFile(sourceAudioFile, targetAudioFile, Handle, WM_NOTIFY);
                int waitCount = 0;
                this.isDone = false;
                if (XConvertHandle == IntPtr.Zero)
                {
                    pError = XConvertHandle.ToString();
                    return false;
                }
                else
                {
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.S48)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "s48");
                    }
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.MP3)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "mp3");
                    }
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WAV)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "wav");
                    }
                    if (targetFileAudioFormat.MyAudioENCTYPE == AudioENCTYPE.WMA)
                    {
                        XC2_SetParam(XConvertHandle, "SAMPLERATE", targetFileAudioFormat.Samplerate.ToString());
                        XC2_SetParam(XConvertHandle, "BITRATE", targetFileAudioFormat.Bitrate.ToString());
                        XC2_SetParam(XConvertHandle, "ENCTYPE", "wma");
                    }

                    if (isNeedENERGYFILE)
                    {
                        XC2_SetParam(XConvertHandle, "ENERGYFILE", targetAudioFile + ".wfm");
                    }
                    XC2_Start(XConvertHandle);
                    while (!this.isDone && !this.isError&& waitCount < 10000)
                    {
                        waitCount++;
                        Application.DoEvents();
                        Thread.Sleep(200);
                    }
                    XC2_Cancel(XConvertHandle);
                    pError = null;

                    if (this.isError)
                    {
                        pError = this.errorMsg;
                        return false;
                    } 

                    return this.isDone;
                }

            }
            catch (Exception ex)
            {
                pError = ex.Message;
                return false;


            }
        }



        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case WM_NOTIFY:   //处理消息
                    {
                        try
                        {
                            if (!isDone)
                            {
                                int n = XC2_GetEventNotify(XConvertHandle);
                                if (n == 0)
                                {
                                    this.isDone = true;
                                    XC2_Cancel(XConvertHandle);
                                    XConvertHandle = IntPtr.Zero;
                                }
                                else if (n == -1)
                                {
                                    this.IsError = true;
                                    this.errorMsg = XC2_GetLastError();
                                    XC2_Cancel(XConvertHandle);
                                    XConvertHandle = IntPtr.Zero;

                                }

                            }
                        }
                        catch
                        {

                        }


                    }

                    break;

                default:
                    base.DefWndProc(ref m);   //调用基类函数处理非自定义消息。
                    break;
            }
        }

        public int GetProgress()
        {
            if (!isDone)
            {
                return XC2_GetProgress(XConvertHandle);
            }
            else
            {
                return 100;
            }

        }

        public ClassXConvert2InCsharp()
        {
            InitializeComponent();
        }


    }

    public enum AudioENCTYPE
    {
        S48, //"s48"
        MP3, //"mp3",
        WAV,  //"wav",
        WMA
    }

    public class AudioFormat
    {
        int bitrate = 0;

        public int Bitrate
        {
            get { return bitrate; }
            set { bitrate = value; }
        }

        long duration = 0;

        public long Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        int channelNum = 0;

        public int ChannelNum
        {
            get { return channelNum; }
            set { channelNum = value; }
        }

        bool isAudioFile = true;

        public bool IsAudioFile
        {
            get { return isAudioFile; }
            set { isAudioFile = value; }
        }

        int samplerate = 0;

        public int Samplerate
        {
            get { return samplerate; }
            set { samplerate = value; }
        }



        AudioENCTYPE myAudioENCTYPE = AudioENCTYPE.S48;

        public AudioENCTYPE MyAudioENCTYPE
        {
            get { return myAudioENCTYPE; }
            set { myAudioENCTYPE = value; }
        }


    }
}
