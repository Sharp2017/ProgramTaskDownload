using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WinProgramTaskActuatorDevice.Classes
{
    public class MepgAudio
    {
        private static Boolean IsMepg(Byte Header0, Byte Header1, Byte Header2, Byte Header3)
        {
            if ((Header0 == 0xFF) && (Header1 == 0xFD) && (Header2 == 0xC4))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static Int16 GetLayer(Byte Header1)
        {
            switch (Header1 & 0x06)
            {
                case 0x06:
                    return 1;
                case 0x04:
                    return 2;
                case 0x03:
                    return 3;
                default:
                    return 0;
            }
        }

        private static int GetBitRate(Int16 layer, Byte Header2)
        {
            if (layer == 2)
            {
                switch (Header2 & 0xF0)
                {
                    case 0xE0:
                        return 384;

                    case 0xD0:
                        return 320;

                    case 0xC0:
                        return 256;

                    case 0xB0:
                        return 224;

                    case 0xA0:
                        return 192;

                    case 0x90:
                        return 160;

                    case 0x80:
                        return 128;

                    case 0x70:
                        return 112;

                    case 0x60:
                        return 96;

                    case 0x50:
                        return 80;

                    case 0x40:
                        return 64;

                    case 0x30:
                        return 56;

                    case 0x20:
                        return 48;

                    case 0x10:
                        return 32;

                    default:
                        return 0;

                }
            }
            else
            {
                return 0;
            }

        }

        private static int GetSampleFreq(Byte Header1, Byte Header2)
        {
            Boolean isHighFreq = ((Header1 & 0x08) == 0x08);
            if (isHighFreq)
            {
                switch (Header2 & 0x0c)
                {
                    case 0x08:
                        return 32000;
                    case 0x04:
                        return 48000;
                    case 0x0:
                        return 44100;
                    default:
                        return 0;
                }
            }
            else
            {
                return 0;
            }
        }



        private static int GetFrameSize(Int16 layer, int bitRate, int sampleFreq)
        {

            if (layer == 2)
            {
                switch (sampleFreq)
                {
                    case 48000:
                        switch (bitRate)
                        {
                            case 384:
                                return 1152;
                            case 320:
                                return 960;
                            case 256:
                                return 768;
                            case 224:
                                return 672;
                            case 192:
                                return 576;
                            case 160:
                                return 480;
                            case 128:
                                return 384;
                            case 112:
                                return 336;
                            case 96:
                                return 288;
                            case 80:
                                return 240;
                            case 64:
                                return 192;
                            case 56:
                                return 168;
                            case 48:
                                return 144;
                            case 32:
                                return 96;
                            default:
                                return 0;
                        }
                    case 44100:
                        switch (bitRate)
                        {
                            case 384:
                                return 1253;
                            case 320:
                                return 1044;
                            case 256:
                                return 835;
                            case 224:
                                return 731;
                            case 192:
                                return 626;
                            case 160:
                                return 522;
                            case 128:
                                return 417;
                            case 112:
                                return 365;
                            case 96:
                                return 313;
                            case 80:
                                return 261;
                            case 64:
                                return 208;
                            case 56:
                                return 182;
                            case 48:
                                return 156;
                            case 32:
                                return 104;
                            default:
                                return 0;
                        }
                    case 32000:
                        switch (bitRate)
                        {

                            case 384:
                                return 1728;
                            case 320:
                                return 1440;
                            case 256:
                                return 1152;
                            case 224:
                                return 1008;
                            case 192:
                                return 864;
                            case 160:
                                return 720;
                            case 128:
                                return 576;
                            case 112:
                                return 504;
                            case 96:
                                return 432;
                            case 80:
                                return 360;
                            case 64:
                                return 288;
                            case 56:
                                return 252;
                            case 48:
                                return 216;
                            case 32:
                                return 144;
                            default:
                                return 0;
                        }
                    default: return 0;


                }
            }
            else
                return 0;
        }

        private static int GetDuration(long fileSize, int frameSize, int sampleFreq)
        {
            Int64 int64Temp1, int64Temp2;
            int64Temp1 = fileSize;
            int64Temp1 = int64Temp1 * 1152 * 10;
            int64Temp2 = frameSize;
            int64Temp2 = int64Temp2 * (sampleFreq / 100);
            int duration = (int)(int64Temp1 / int64Temp2);
            if (duration < 0)
                duration = 0;
            return duration;
        }

        public static Boolean GetMepgInfo(String audioFileName, ref MepgInfo mepgInfo)
        {
            if (File.Exists(audioFileName))
            {
                FileStream mepgFileStream = null;

                try
                {
                    mepgFileStream = File.Open(audioFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch
                {
                    return false;
                }

                try
                {
                    mepgInfo.FileSize = mepgFileStream.Length;
                    Byte[] headerBuffer = new Byte[32];
                    int byteRead = mepgFileStream.Read(headerBuffer, 0, 4);
                    if (byteRead == 4)
                    {
                        mepgInfo.FileIsMPEG = IsMepg(headerBuffer[0], headerBuffer[1], headerBuffer[2], headerBuffer[3]);
                        if (!mepgInfo.FileIsMPEG)
                        {
                            return false;
                        }
                        mepgInfo.Layer = GetLayer(headerBuffer[1]);
                        mepgInfo.BitRate = GetBitRate(mepgInfo.Layer, headerBuffer[2]);
                        mepgInfo.SamplingFreq = GetSampleFreq(headerBuffer[1], headerBuffer[2]);
                        mepgInfo.FrameSize = GetFrameSize(mepgInfo.Layer, mepgInfo.BitRate, mepgInfo.SamplingFreq);
                        mepgInfo.Duration = GetDuration(mepgInfo.FileSize, mepgInfo.FrameSize, mepgInfo.SamplingFreq);
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }
                finally
                {
                    mepgFileStream.Close();
                }
            }
            else
            {

                return false;
            }
        }





        public static int GetAudioDuration(String audioFileName)
        {
            MepgInfo mepgInfo = new MepgInfo();
            GetMepgInfo(audioFileName, ref mepgInfo);
            return mepgInfo.Duration;
        }

        public static String DurationToStr(int duration)
        {
            int hour, min, second, minsecond;
            hour = duration / (1000 * 3600);
            string _hour = "";
            if (hour < 10)
            {
                _hour = "0" + hour;
            }
            else
            {
                _hour = hour.ToString();
            }
            duration = duration % (1000 * 3600);
            min = duration / (1000 * 60);
            string _min = "";
            if (min < 10)
            {
                _min = "0" + min;
            }
            else
            {
                _min = min.ToString();
            }
            duration = duration % (1000 * 60);
            second = duration / 1000;
            string _second = "";
            if (second < 10)
            {
                _second = "0" + second;
            }
            else
            {
                _second = second.ToString();
            }

            minsecond = duration % 1000;

            string _minsecond = "000";
            if (minsecond >= 1000)
            {
                _minsecond = minsecond.ToString().Substring(0, 3);
            }
            else
            {
                if (minsecond >= 100)
                {
                    _minsecond = minsecond.ToString();
                }
                else
                {
                    _minsecond = "0" + _minsecond;
                }
            }
            return String.Format("{0}:{1}:{2}:{3}", _hour, _min, _second, _minsecond.ToString().Substring(0, 3));
        }

        public static String DurationToStr1(int duration)
        {
            int hour, min, second, minsecond;
            hour = duration / (1000 * 3600);
            string _hour = "";
            if (hour < 10)
            {
                _hour = "0" + hour;
            }
            else
            {
                _hour = hour.ToString();
            }
            duration = duration % (1000 * 3600);
            min = duration / (1000 * 60);
            string _min = "";
            if (min < 10)
            {
                _min = "0" + min;
            }
            else
            {
                _min = min.ToString();
            }
            duration = duration % (1000 * 60);
            second = duration / 1000;
            string _second = "";
            if (second < 10)
            {
                _second = "0" + second;
            }
            else
            {
                _second = second.ToString();
            }

            minsecond = duration % 1000;

            string _minsecond = "00";
            if (minsecond >= 100)
            {
                _minsecond = minsecond.ToString().Substring(0, 2);
            }
            else
            {
                if (minsecond >= 10)
                {
                    _minsecond = minsecond.ToString();
                }
                else
                {
                    _minsecond = "0" + _minsecond;
                }
            }
            return String.Format("{0}:{1}:{2}:{3}", _hour, _min, _second, _minsecond.ToString().Substring(0, 2));
        }

        public static String DurationToStr(Int64 duration)
        {
            Int64 hour, min, second;
            hour = duration / (1000 * 3600);
            string _hour = "";
            if (hour < 10)
            {
                _hour = "0" + hour;
            }
            else
            {
                _hour = hour.ToString();
            }
            duration = duration % (1000 * 3600);
            min = duration / (1000 * 60);
            string _min = "";
            if (min < 10)
            {
                _min = "0" + min;
            }
            else
            {
                _min = min.ToString();
            }
            duration = duration % (1000 * 60);
            second = duration / 1000;
            string _second = "";
            if (second < 10)
            {
                _second = "0" + second;
            }
            else
            {
                _second = second.ToString();
            }
            return String.Format("{0}:{1}:{2}", _hour, _min, _second);
        }


    }

    public struct MepgInfo
    {
        public Boolean FileIsMPEG;
        public Int16 Layer;             //1,2,3
        public int BitRate;
        public int SamplingFreq;
        public int FrameSize;
        public int Duration;
        public long FileSize;

    }
}
