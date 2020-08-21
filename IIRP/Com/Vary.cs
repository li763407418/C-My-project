using IIRP.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IIRP.Com
{

    /// <summary>
    /// 用于各种类型转换的静态类
    /// </summary>
    public static class Vary
    {
        /// <summary>
        /// 字符串转16进制字符串
        /// </summary>
        /// <param name="s">要转换测字符串</param>
        /// <param name="encode">编码方式</param>
        /// <returns>返回的结果字符串</returns>
        public static string StringToHexString(this string s, Encoding encode)
        {
            byte[] b = encode.GetBytes(s);//按照指定编码将string编程字节数组
            string result = string.Empty;
            for (int i = 0; i < b.Length; i++)//逐字节变为16进制字符，以%隔开
            {
                result += "%" + Convert.ToString(b[i], 16);
            }
            return result;
        }

        /// <summary>
        /// 字节数组转换成16进制字符串函数
        /// </summary>
        /// <param name="bytes">需要转换的字节数组</param>
        /// <returns>转换的结果字符串</returns>
        public static string ByteToHexStr(this byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        /// <summary>
        /// 字节数组转换成字符串的函数
        /// </summary>
        /// <param name="data">需要转换的字节数组</param>
        /// <returns>转换后的字符串</returns>
        public static string ByteToStr(this byte[] data)
        {
            if (data == null) return "";
            return Encoding.ASCII.GetString(data);
        }

        /// <summary>
        /// day-精确到天 second 精确到秒 millionSecond 精确到毫秒
        /// </summary>
        public enum Format { day, second, millisecond }
        /// <summary>
        /// Datetime转字符串
        /// </summary>
        /// <param name="date">时间</param>
        /// <param name="f">枚举</param>
        /// <returns>字符串</returns>
        public static string DateTimeToStr(Format f)
        {
            string time = "";
            switch (f)
            {
                case Format.day:
                    time = DateTime.Now.ToString("yyyy-MM-dd");
                    break;
                case Format.second:
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                case Format.millisecond:
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    break;
            }
            return time;
        }

        #region 指令转换器

        public static byte[] Vary4Byte(byte[] Data,VaryType type)
        {
            byte[] Buff = new byte[4];
            Data.CopyTo(Buff, 0);
            switch (type)
            {
                case VaryType.Single:
                    Buff[0] = Data[1];
                    Buff[1] = Data[0];
                    Buff[2] = Data[3];
                    Buff[3] = Data[2];
                    break;
                case VaryType.Double:
                    Buff[0] = Data[2];
                    Buff[1] = Data[3];
                    Buff[2] = Data[0];
                    Buff[3] = Data[1];
                    break;
                case VaryType.reverse:
                    Array.Reverse(Buff);
                    break;
                default:
                    break;
            }
            return Buff;
        }

        public static byte[] Vary8Byte(byte[] Data ,VaryType type)
        {
            byte[] Buff = new byte[8];
            Data.CopyTo(Buff, 0);
            switch (type)
            {
                case VaryType.Single:
                    Buff[0] = Data[1];
                    Buff[1] = Data[0];
                    Buff[2] = Data[3];
                    Buff[3] = Data[2];
                    Buff[4] = Data[5];
                    Buff[5] = Data[4];
                    Buff[6] = Data[7];
                    Buff[7] = Data[6];
                    break;
                case VaryType.Double:
                    Buff[0] = Data[6];
                    Buff[1] = Data[7];
                    Buff[2] = Data[4];
                    Buff[3] = Data[5];
                    Buff[4] = Data[2];
                    Buff[5] = Data[3];
                    Buff[6] = Data[0];
                    Buff[7] = Data[1];
                    break;
                case VaryType.reverse:
                    Array.Reverse(Buff);
                    break;
                default:
                    break;
            }
            return Buff;
        }

        /// <summary>
        /// short数组转换成Byte数组
        /// </summary>
        /// <param name="data">short数组</param>
        /// <returns>字节数组</returns>
        public static byte[] ArrayToBytes(short[] data,bool Reverse=true)
        {
            if (data == null) return null;

            byte[] buffer = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(data[i]);
                if(Reverse)
                {
                    Array.Reverse(temp);
                }
                temp.CopyTo(buffer, 2 * i);
            }
            return buffer;
        }

        /// <summary>
        /// ushort数组转换成Byte数组
        /// </summary>
        /// <param name="data">ushort数组</param>
        /// <returns>字节数组</returns>
        public static byte[] ArrayToBytes(ushort[] data,bool Reverse=true)
        {
            if (data == null) return null;

            byte[] buffer = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(data[i]);
                if (Reverse)
                {
                    Array.Reverse(temp);
                }
                temp.CopyTo(buffer, 2 * i);
            }
            return buffer;
        }

        /// <summary>
        /// bool数组转换成字节数组
        /// </summary>
        /// <param name="data">bool数组</param>
        /// <returns>字节数组</returns>
        public static byte[]ArrayToBytes(bool[]data)
        {
            if (data == null) return null;
            byte[] buff = new byte[data.Length];
            for(int i=0;i<buff.Length;i++)
            {
                buff[i] = (byte)(data[i] ? 1 : 0);
            }
            return buff;
        }

        /// <summary>
        /// int数组转换成Byte数组
        /// </summary>
        /// <param name="data">int数组</param>
        /// <returns>字节数组</returns>
        public static byte[] ArrayToBytes(int[] data,VaryType type)
        {
            if (data == null) return null;

            byte[] buffer = new byte[data.Length * 4];
            for (int i = 0; i < data.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(data[i]);
                Vary4Byte(temp,type).CopyTo(buffer, 4 * i);
            }
            return buffer;
        }

        /// <summary>
        /// uint数组转换成Byte数组
        /// </summary>
        /// <param name="data">uint数组</param>
        /// <returns>字节数组</returns>
        public static byte[] ArrayToBytes(uint[] data,VaryType type)
        {
            if (data == null) return null;

            byte[] buffer = new byte[data.Length * 4];
            for (int i = 0; i < data.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(data[i]);
                Vary4Byte(temp,type).CopyTo(buffer, 4 * i);
            }
            return buffer;
        }

        /// <summary>
        /// Float数组转换成Byte数组
        /// </summary>
        /// <param name="data">Float数组</param>
        /// <returns></returns>
        public static byte[] ArrayToBytes(float[] data,VaryType type)
        {
            if (data == null) return null;

            byte[] buffer = new byte[data.Length * 4];
            for (int i = 0; i < data.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(data[i]);
                Vary4Byte(temp,type).CopyTo(buffer, 4 * i);
            }
            return buffer;
        }

        /// <summary>
        /// Double数组转换成Byte数组
        /// </summary>
        /// <param name="data">Double数组</param>
        /// <returns></returns>
        public static byte[]  ArrayToBytes(double[] data,VaryType type)
        {
            if (data == null) return null;

            byte[] buffer = new byte[data.Length * 8];
            for (int i = 0; i < data.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(data[i]);
                Vary8Byte(temp,type).CopyTo(buffer, 8 * i);
            }
            return buffer;
        }
        #endregion
        /// <summary>
        /// 字符串转成字节数组
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>字节数组</returns>
        public static byte[] StringToBytes(string str)
        {
            if (str == null) return null;
            byte[] buffer = Encoding.ASCII.GetBytes(str);
            return buffer;

        }

        /// <summary>
        /// object转换成字符串
        /// </summary>
        /// <param name="Data">object</param>
        /// <returns>字符串</returns>
        public static string ArrayToString(object Data)
        {
            string str = "";
            if (Data == null) return str;
            string type = Data.GetType().Name;
            if(type.Contains("[]"))
            {
                switch(type)
                {
                    case "Boolean[]":
                        str = ArrayToString((bool[])Data);
                        break;
                    case "Int16[]":
                        str = ArrayToString((short[])Data);
                        break;
                    case "UInt16[]":
                        str = ArrayToString((ushort[])Data);
                        break;
                    case "Int32[]":
                        str = ArrayToString((int[])Data);
                        break;
                    case "UInt32[]":
                        str = ArrayToString((uint[])Data);
                        break;
                    case "Single[]":
                        str = ArrayToString((float[])Data);
                        break;
                    case "Double[]":
                        str = ArrayToString((double[])Data);
                        break;
                }
            }
            else
            {
                str = Data.ToString();
            }
            return str;
        }

        /// <summary>
        /// int数组转换成字符串的方法
        /// </summary>
        /// <param name="array">int[]</param>
        /// <returns>字符串</returns>
        public static string ArrayToString(bool[] array)
        {
            string str = "";
            if (array == null) return str;
            str += "[ ";
            foreach (var a in array)
            {
                str += a.ToString() + ",";
            }
            str = str.Remove(str.Length - 1, 1);
            str += "]";
            return str;
        }

        /// <summary>
        /// int数组转换成字符串的方法
        /// </summary>
        /// <param name="array">int[]</param>
        /// <returns>字符串</returns>
        public static string ArrayToString(int[] array)
        {
            string str = "";
            if (array == null) return str;
            str += "[ ";
            foreach (var a in array)
            {
                str += a.ToString() + ",";
            }
            str = str.Remove(str.Length - 1, 1);
            str += "]";
            return str;
        }

        /// <summary>
        /// uint数组转换成字符串的方法
        /// </summary>
        /// <param name="array">uint[]</param>
        /// <returns>字符串</returns>
        public static string ArrayToString(uint[] array)
        {
            string str = "";
            if (array == null) return str;
            str += "[ ";
            foreach (var a in array)
            {
                str += a.ToString() + ",";
            }
            str = str.Remove(str.Length - 1, 1);
            str += "]";
            return str;
        }

        /// <summary>
        /// short数组转换成字符串的方法
        /// </summary>
        /// <param name="array">short[]</param>
        /// <returns>字符串</returns>
        public static string ArrayToString(short[] array)
        {
            string str = "";
            if (array == null) return str;
            str += "[ ";
            foreach (var a in array)
            {
                str += a.ToString() + ",";
            }
            str = str.Remove(str.Length - 1, 1);
            str += "]";
            return str;
        }

        /// <summary>
        /// ushort数组转换成字符串的方法
        /// </summary>
        /// <param name="array">ushort[]</param>
        /// <returns>字符串</returns>
        public static string ArrayToString(ushort[] array)
        {
            string str = "";
            if (array == null) return str;
            str += "[ ";
            foreach (var a in array)
            {
                str += a.ToString() + ",";
            }
            str = str.Remove(str.Length - 1, 1);
            str += "]";
            return str;
        }

        /// <summary>
        /// float数组转换成字符串的方法
        /// </summary>
        /// <param name="array">float[]</param>
        /// <returns>字符串</returns>
        public static string ArrayToString(float[] array)
        {
            string str = "";
            if (array == null) return str;
            str += "[ ";
            foreach (var a in array)
            {
                str += a.ToString("F") + ",";
            }
            str = str.Remove(str.Length - 1, 1);
            str += "]";
            return str;
        }

        /// <summary>
        /// double数组转换成字符串的方法
        /// </summary>
        /// <param name="array">double[]</param>
        /// <returns>字符串</returns>
        public static string ArrayToString(double[] array)
        {
            string str = "";
            if (array == null) return str;
            str += "[ ";
            foreach (var a in array)
            {
                str += a.ToString("F2") + ",";
            }
            str = str.Remove(str.Length - 1, 1);
            str += "]";
            return str;
        }

        /// <summary>
        ///字节数组转成ASCII码
        /// </summary>
        /// <param name="Value">字节数组</param>
        /// <returns>字节数组</returns>
        public static byte[] ValueToASCII(byte[] Value)
        {
            List<byte> list = new List<byte>();
            for(int i=0;i<Value.Length;i++)
            {
               list.AddRange(Encoding.ASCII.GetBytes(Value[i].ToString("X2")));
            }
            return list.ToArray();
        }

        /// <summary>
        ///字节数组转成ASCII码
        /// </summary>
        /// <param name="Value">字节数组</param>
        /// <returns>字节数组</returns>
        public static byte[] ValueToASCII(short[] Value)
        {
            List<byte> list = new List<byte>();
            for (int i = 0; i < Value.Length; i++)
            {
                list.AddRange(Encoding.ASCII.GetBytes(Value[i].ToString("X4")));
            }
            return list.ToArray();
        }

        /// <summary>
        ///字节数组转成ASCII码
        /// </summary>
        /// <param name="Value">字节数组</param>
        /// <returns>字节数组</returns>
        public static byte[] ValueToASCII(ushort[] Value)
        {
            List<byte> list = new List<byte>();
            for (int i = 0; i < Value.Length; i++)
            {
                list.AddRange(Encoding.ASCII.GetBytes(Value[i].ToString("X4")));
            }
            return list.ToArray();
        }


    }
}
