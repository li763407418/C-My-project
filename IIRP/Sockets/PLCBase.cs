using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IIRP.Com;
using System.Reflection;
namespace IIRP.Sockets
{
    /// <summary>
    /// 解析数据转换的模式
    /// </summary>
    public enum VaryType
    {
        /// <summary>
        /// 顺序
        /// </summary>
        Order,

        /// <summary>
        /// 单字反转
        /// </summary>
        Single,

        /// <summary>
        /// 双字反转
        /// </summary>
        Double,

        /// <summary>
        /// 倒序
        /// </summary>
        reverse

    }

    public enum DateType
    {
        /// <summary>
        /// 布尔类型
        /// </summary>
        Bool,

        /// <summary>
        /// Int16类型
        /// </summary>
        Short,

        /// <summary>
        /// Uint16类型
        /// </summary>
        Ushort,

        /// Int32类型
        /// </summary>
        Int,

        /// <summary>
        /// Uint32类型
        /// </summary>
        Uint,

        /// <summary>
        /// 单精度浮点数类型
        /// </summary>
        Float,

        /// <summary>
        /// 双精度浮点数类型
        /// </summary>
        Double,

        /// <summary>
        /// 字符串
        /// </summary>
        String
    }

    public abstract class PLCBase : IirDeviceClient
    {

        protected VaryType vary = VaryType.reverse;

        /// <summary>
        /// 读PLC返回指令的数据区域起止索引
        /// </summary>
        protected ushort CmdRetIndex { get; set; } = 25;

        /// <summary>
        /// 单个数据字节的长度，西门子为2 ，三菱，欧姆龙，modbusTcp就为1，CIP协议无效
        /// </summary>
        protected ushort WordLenght { get; set; } = 1;


        #region 写PLC的方法

        protected bool Write(string address, byte[] data, int lenght)
        {
            CMDRETPLC cmd = new CMDRETPLC(address, CMDType.WW, lenght);
            cmd.DataBuff = data;
            GetRet(cmd);
            if (cmd.State == CmdState.ReadOK)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 向PLC写入int数组的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">int数组</param>
        /// <returns></returns>     
        public bool Write(string address, int[] data)
        {
            return Write(address, Vary.ArrayToBytes(data, vary), data.Length);
        }

        /// <summary>
        /// 向PLC写入int数据的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">int数据</param>
        /// <returns></returns>     
        public bool Write(string address, int data)
        {
            return Write(address, new int[] { data });
        }

        /// <summary>
        /// 向PLC写入uint数组的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">uint数组</param>
        /// <returns></returns>     
        public bool Write(string address, uint[] data)
        {
            return Write(address, Vary.ArrayToBytes(data, vary), data.Length);
        }

        /// <summary>
        /// 向PLC写入uint数据的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">uint数据</param>
        /// <returns></returns>     
        public bool Write(string address, uint data)
        {
            return Write(address, new uint[] { data });
        }

        /// <summary>
        /// 向PLC写入Float数组的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">Float数组</param>
        /// <returns></returns>     
        public bool Write(string address, float[] data)
        {
            return Write(address, Vary.ArrayToBytes(data, vary), data.Length);
        }

        /// <summary>
        /// 向PLC写入Float数据的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">float数据</param>
        /// <returns></returns>     
        public bool Write(string address, float data)
        {
            return Write(address, new float[] { data });
        }

        /// <summary>
        /// 向PLC写入uShort数组的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">ushort数组</param>
        /// <returns></returns>     
        public bool Write(string address, ushort[] data)
        {
            return Write(address, Vary.ArrayToBytes(data), data.Length);
        }

        /// <summary>
        /// 向PLC写入ushort数据的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">ushort数据</param>
        /// <returns></returns>     
        public bool Write(string address, ushort data)
        {
            return Write(address, new ushort[] { data });
        }

        /// <summary>
        /// 向PLC写入Short数组的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">short数组</param>
        /// <returns></returns>     
        public bool Write(string address, short[] data)
        {
            return Write(address, Vary.ArrayToBytes(data), data.Length);
        }

        /// <summary>
        /// 向PLC写入short数据的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">short数据</param>
        /// <returns></returns>     
        public bool Write(string address, short data)
        {
            return Write(address, new short[] { data });
        }

        /// <summary>
        /// 向PLC写入Double数组的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">Double数组</param>
        /// <returns></returns>     
        public bool Write(string address, double[] data)
        {
            return Write(address, Vary.ArrayToBytes(data, vary), data.Length);
        }

        /// <summary>
        /// 向PLC写入Double数据的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100，D2.100 2-DB块编号 100为地址</param>
        /// <param name="data">int数据</param>
        /// <returns></returns>     
        public bool Write(string address, double data)
        {
            return Write(address, new double[] { data });
        }

        /// <summary>
        /// 向PLC写入一个位的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100.7，D2.100.6 2-DB块编号 100为地址，如果只写了M100默认为M100.0</param>
        /// <param name="data">bool数据</param>
        /// <returns></returns>     
        public virtual bool Write(string address, bool data)
        {
            return Write(address, new bool[1] { data});
        }

        /// <summary>
        /// 向PLC写入多个位的方法
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool Write(string address,bool[]data)
        {
            CMDRETPLC cmd = new CMDRETPLC(address, CMDType.WB);
            cmd.DataBuff = Vary.ArrayToBytes(data);
            GetRet(cmd);
            if (cmd.State == CmdState.ReadOK)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 向PLC写入一个字符串的方法
        /// </summary>
        /// <param name="address">PLC地址 如M100.7，D2.100.6 2-DB块编号 100为地址，如果只写了M100默认为M100.0</param>
        /// <param name="data">字符串</param>
        /// <returns></returns>     
        public virtual bool Write(string address, string data)
        {
            byte[] buff = Vary.StringToBytes(data);
            return Write(address,buff, data.Length);
        }

        #endregion

        #region 读PLC的方法

        /// <summary>
        /// 读单个数据的方法
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="data">需要读取的数据类型,默认为布尔类型</param>
        /// <returns>结果为字符串</returns>
        public virtual string Read(string address,DateType data=DateType.Bool)
        {
            return Read(address, data, 1);
        }

        /// <summary>
        /// 读PLC的方法(数组或者字符串)
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="Data">初始化需要读取的数据类型 </param>
        /// <param name="Lenght">数组和字符串需要指定长度</param>
        /// <returns></returns>
        public virtual string Read(string address, DateType Data, int Lenght)
        {
            if (!CheckAddress(address)) return "";
            bool FlatFirst = false;
            List<CMDRETPLC> CMD = AutoList;
            int count = CMD.Count;
            string str = "";
            CMDRETPLC cmd = new CMDRETPLC(address, CMDType.RB,Lenght);
            GetLenght(cmd,Data);
            if (count == 0)
            {
                CMD.Add(cmd);
                FlatFirst = true;
            }
            else
            {
                var IsExcit = CMD.Where(q => q.Address == address).Select(q=>q).ToList();
                if (IsExcit.Count>0)
                {
                    CMDRETPLC _cmd = IsExcit.First();
                    if (cmd.DateType != _cmd.DateType||Lenght!=_cmd.Lenght)
                    {
                        lock(Lockcmd)
                        {
                            CMD.Remove(_cmd);
                            CMD.Add(cmd);
                            FlatFirst = true;
                        }                      
                    }
                }
                else
                {
                    GetRet(cmd, true);
                    FlatFirst = true;
                }
            }
            if (FlatFirst)
            {
                TimerBase t = new TimerBase();
                while (cmd.State != CmdState.TranOK && t.D < Timeout)
                {
                    IIRSet.Dly(1);
                }
            }
            Cache.GetValue(address,out str);
            return str;
        }

        protected virtual CMDRETPLC GetLenght(CMDRETPLC cmd,DateType _type)
        {
            cmd._CMDType = CMDType.RW;
            int Lenght = cmd.Lenght;
            switch(_type)
            {
                case DateType.Bool:
                    cmd._CMDType = CMDType.RB;
                    if (Lenght==1)
                    {
                        cmd.DateType = "Boolean";
                        cmd.Result = false;
                        cmd.Count = WordLenght;
                    }
                    else
                    {
                        cmd.DateType = "Boolean[]";
                        cmd.Result = new bool[Lenght];
                        cmd.Count = GetBoolLenght((bool[])cmd.Result);
                    }
                    
                    break;
                case DateType.Short:
                    if(Lenght==1)
                    {
                        cmd.DateType = "Int16";
                        cmd.Result = (short)0;
                        cmd.Count = WordLenght;
                    }
                    else
                    {
                        cmd.DateType = "Int16[]";
                        cmd.Result = new short[Lenght];
                        cmd.Count = WordLenght * Lenght;
                    }
                    break;
                case DateType.Ushort:
                    if(Lenght==1)
                    {
                        cmd.DateType = "UInt16";
                        cmd.Result = (ushort)0;
                        cmd.Count = WordLenght;
                    }
                    else
                    {
                        cmd.DateType = "UInt16[]";
                        cmd.Result = new ushort[Lenght];
                        cmd.Count = WordLenght * Lenght;
                    }

                    break;
                case DateType.Int:
                    if(Lenght==1)
                    {
                        cmd.DateType = "Int32";
                        cmd.Result = 0;
                        cmd.Count = WordLenght * 2;
                    }
                    else
                    {
                        cmd.DateType = "Int32[]";
                        cmd.Result = new int[Lenght];
                        cmd.Count = WordLenght * 2 * Lenght;
                    }
                    break;
                case DateType.Uint:
                    if(Lenght==1)
                    {
                        cmd.DateType = "UInt32";
                        cmd.Result = (uint)0;
                        cmd.Count = WordLenght * 2;
                    }
                    else
                    {
                        cmd.DateType = "UInt32[]";
                        cmd.Result = new uint[Lenght];
                        cmd.Count = WordLenght * 2 * Lenght;
                    }
                    break;
                case DateType.Float:
                    if(Lenght==1)
                    {
                        cmd.DateType = "Single";
                        cmd.Result = (float)0;
                        cmd.Count = WordLenght * 2;
                    }
                    else
                    {
                        cmd.DateType = "Single[]";
                        cmd.Result = new float[Lenght];
                        cmd.Count = WordLenght * 2 * Lenght;
                    }
                    break;
                case DateType.Double:
                    if(Lenght==1)
                    {
                        cmd.DateType = "Double";
                        cmd.Result = (double)0;
                        cmd.Count = WordLenght * 4;
                    }
                    else
                    {
                        cmd.DateType = "Double[]";
                        cmd.Result = new double[Lenght];
                        cmd.Count = WordLenght * 4 * Lenght;
                    }
                    break;
                case DateType.String:
                    cmd.DateType = "String";
                    cmd.Result = "";
                    cmd.Count = cmd.Lenght;
                    break;
            }
            return cmd;
        }

        protected virtual int GetBoolLenght(bool[] data)
        {
            return data.Length;
        }

        protected virtual bool CheckAddress(string addres)
        {
            return true;
        }

        /// <summary>
        /// 读取指定地址的一个Ascii字符串
        /// </summary>
        /// <param name="address">读取的地址</param>
        /// <returns>string</returns>
        public string ReadString(string address, int lenght)
        {
            CMDRETPLC cmd = new CMDRETPLC(address, CMDType.RW, lenght) { Result = "" };
            GetRet(cmd);
            byte[] buff = new byte[cmd.Ret.Length - CmdRetIndex];
            if (cmd.State == CmdState.ReadOK)
            {
                cmd.IsSuccess = true;
                Array.Copy(cmd.Ret, CmdRetIndex, buff, 0, cmd.Ret.Length - CmdRetIndex);
                cmd.Result = Encoding.ASCII.GetString(buff);
            }
            else
            {
                cmd.IsSuccess = false;
            }
            return cmd.Result.ToString();
        }

        public CMDRETPLC<T> RCustomDataFromPLC<T>(string address) where T : Custom, new()
        {
            T Custom = new T();
            CMDRETPLC<T> cmd = new CMDRETPLC<T>(address, CMDType.RW, Custom.Readcount) { Result = Custom };
            GetRet(cmd);
            byte[] buff = new byte[cmd.Ret.Length - CmdRetIndex];
            if (cmd.State == CmdState.ReadOK)
            {
                Array.Copy(cmd.Ret, CmdRetIndex, buff, 0, cmd.Ret.Length - CmdRetIndex);
                Custom.ByteToDate(cmd, buff, vary);
                cmd.Result = Custom;
                cmd.IsSuccess = true;
            }
            else
                cmd.IsSuccess = false;
            return cmd;
        }
        #endregion

        #region 重载方法

        protected virtual string[] Area(string Address)
        {
            return null;
        }

        protected override CMDRETPLC TranData(CMDRETPLC cmd)
        {
            if (!AutoList.Contains(cmd)) return cmd;
            int Lenght = cmd.Lenght;
            cmd.IsSuccess = true;
            switch (cmd.DateType)
            {
                case "Boolean":
                    GetBool(cmd);
                    break;
                case "Boolean[]":
                    GetBool(cmd);
                    break;
                case "Int16":
                    GetShort(cmd);
                    break;
                case "Int16[]":
                    GetShort(cmd);
                    break;
                case "UInt16":
                    GetUShort(cmd);
                    break;
                case "UInt16[]":
                    GetUShort(cmd);
                    break;
                case "Int32":
                    GetInt(cmd);
                    break;
                case "Int32[]":
                    GetInt(cmd);
                    break;
                case "UInt32":
                    GetUInt(cmd);
                    break;
                case "UInt32[]":
                    GetUInt(cmd);
                    break;
                case "Single":
                    GetFloat(cmd);
                    break;
                case "Single[]":
                    GetFloat(cmd);
                    break;
                case "Double":
                    GetDouble(cmd);
                    break;
                case "Double[]":
                    GetDouble(cmd);
                    break;
                case "String":
                    GetString(cmd);
                    break;
            }
            Cache.Add(cmd);
            cmd.State = CmdState.TranOK;
            return cmd;
        }

        protected virtual CMDRETPLC GetString(CMDRETPLC cmd)
        {
            byte[] buff = new byte[cmd.Lenght * 2];
            Array.Copy(cmd.Ret, CmdRetIndex, buff, 0, cmd.Ret.Length - CmdRetIndex);
            cmd.Result = Encoding.ASCII.GetString(buff);
            return cmd;
        }

        protected virtual CMDRETPLC GetBool(CMDRETPLC cmd)
        {
            int Count = cmd.Lenght;
            bool[] Bool = new bool[Count];
            for (int i = 0; i < Count; i++)
            {
                if (cmd.Ret.Length >= CmdRetIndex + i)
                {
                    Bool[i] = cmd.Ret[CmdRetIndex + i] > 0 ? true : false;
                }
            }
            if (Count == 1)
                cmd.Result = Bool[0];
            else
                cmd.Result = Bool;
            return cmd;
        }

        protected virtual CMDRETPLC GetShort(CMDRETPLC cmd)
        {
            byte[] buff = new byte[2];
            int Count = cmd.Lenght;
            short[] Short = new short[Count];
            for (int i = 0; i < Count; i++)
            {
                if (cmd.Ret.Length >= CmdRetIndex + i * 2)
                {
                    Array.Copy(cmd.Ret, CmdRetIndex + i * 2, buff, 0, 2);
                    Array.Reverse(buff);
                    Short[i] = BitConverter.ToInt16(buff, 0);
                }
            }
            if (Count == 1)
                cmd.Result = Short[0];
            else
                cmd.Result = Short;
            return cmd;
        }

        protected virtual CMDRETPLC GetUShort(CMDRETPLC cmd)
        {
            byte[] buff = new byte[2];
            int Count = cmd.Lenght;
            ushort[] UShort = new ushort[Count];
            for (int i = 0; i < Count; i++)
            {
                if (cmd.Ret.Length >= CmdRetIndex + i * 2)
                {
                    Array.Copy(cmd.Ret, CmdRetIndex + i * 2, buff, 0, 2);
                    Array.Reverse(buff);
                    UShort[i] = BitConverter.ToUInt16(buff, 0);
                }
            }
            if (Count == 1)
                cmd.Result = UShort[0];
            else
                cmd.Result = UShort;
            return cmd;
        }

        protected virtual CMDRETPLC GetInt(CMDRETPLC cmd)
        {
            byte[] buff = new byte[4];
            int Count = cmd.Lenght;
            int[] Int = new int[Count];
            for (int i = 0; i < Count; i++)
            {
                if (cmd.Ret.Length >= CmdRetIndex + i * 4)
                {
                    Array.Copy(cmd.Ret, CmdRetIndex + i * 4, buff, 0, 4);
                    Int[i] = BitConverter.ToInt32(Vary.Vary4Byte(buff, vary), 0);
                }
            }
            if (Count == 1)
                cmd.Result = Int[0];
            else
                cmd.Result = Int;
            return cmd;
        }

        protected virtual CMDRETPLC GetUInt(CMDRETPLC cmd)
        {
            byte[] buff = new byte[4];
            int Count = cmd.Lenght;
            uint[] UInt = new uint[Count];
            for (int i = 0; i < Count; i++)
            {
                if (cmd.Ret.Length >= CmdRetIndex + i * 4)
                {
                    Array.Copy(cmd.Ret, CmdRetIndex + i * 4, buff, 0, 4);
                    UInt[i] = BitConverter.ToUInt32(Vary.Vary4Byte(buff, vary), 0);
                }
            }
            if (Count == 1)
                cmd.Result = UInt[0];
            else
                cmd.Result = UInt;
            return cmd;
        }

        protected virtual CMDRETPLC GetFloat(CMDRETPLC cmd)
        {
            byte[] buff = new byte[4];
            int Count = cmd.Lenght;
            float[] Float = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                if (cmd.Ret.Length >= CmdRetIndex + i * 4)
                {
                    Array.Copy(cmd.Ret, CmdRetIndex + i * 4, buff, 0, 4);
                    Float[i] = BitConverter.ToSingle(Vary.Vary4Byte(buff, vary), 0);
                }
            }
            if (Count == 1)
                cmd.Result = Float[0];
            else
                cmd.Result = Float;
            return cmd;
        }

        protected virtual CMDRETPLC GetDouble(CMDRETPLC cmd)
        {
            byte[] buff = new byte[8];
            int Count = cmd.Lenght;
            double[] UInt = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                if (cmd.Ret.Length >= CmdRetIndex + i * 8)
                {
                    Array.Copy(cmd.Ret, CmdRetIndex + i * 8, buff, 0, 8);
                    UInt[i] = BitConverter.ToDouble(Vary.Vary8Byte(buff, vary), 0);
                }
            }
            if (Count == 1)
                cmd.Result = UInt[0];
            else
                cmd.Result = UInt;
            return cmd;
        }

        #endregion

        public PLCBase(string name) : base(name)
        {

        }

    }
}