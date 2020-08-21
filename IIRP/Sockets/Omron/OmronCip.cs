using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IIRP.Com;
using System.Windows.Forms;
using System.Threading;
namespace IIRP.Sockets
{

    public class OmronCip : IirDeviceClient
    {

        #region 公有成员

        /// <summary>
        /// 获取或设置当前PLC的槽号信息，应该在连接之间设定
        /// </summary>
        public byte Slot { get; set; } = 0;

        /// <summary>
        /// 移除指令互锁
        /// </summary>
        bool LockRemove = false;

        #region 读PLC的方法

        /// <summary>
        /// 读取plc返回的对象
        /// </summary>
        /// <param name="TagName">标签数组</param>
        /// <returns>object[]数据</returns>
        public string Read(string TagName)
        {
            string str = "";
            try
            {
                List<CMDRETPLC> CMD = AutoList;
                List<CMDRETPLC> Result = new List<CMDRETPLC>();
                bool FlatFirst = false;
                Result.Add(new CMDRETPLC() { Address = TagName, Result = new object() });
                if (CMD.Count == 0)
                {
                    CMD.Add(new CMDRETPLC(Result));
                    FlatFirst = true;
                }
                else
                {
                    if (!Cache.GetValue(TagName, out str))
                    {
                        lock (Lockcmd)
                        {
                            List<CMDRETPLC> listcmd = CMD[CMD.Count - 1].ListCMDRET;
                            if(listcmd.Count>0 && listcmd[0].Only)
                            {
                                CMD.Add(new CMDRETPLC(Result));
                            }
                            else
                            {
                                listcmd.AddRange(Result);
                            }
                            _getRet(CMD[CMD.Count - 1]);
                            FlatFirst = true;
                        }
                    }

                }
                if (FlatFirst)
                {
                    TimerBase t = new TimerBase();
                    while (!Cache.GetValue(TagName, out str) && t.D < Timeout)
                    {
                        Thread.Sleep(1);
                    }
                    if (t.D > Timeout)
                    {
                        LockRemove = true;
                    }
                    if (CMD[CMD.Count - 1].ErrCode == 0x1B)
                    {
                        List<CMDRETPLC> New = new List<CMDRETPLC>
                           {
                                new CMDRETPLC() { Address = TagName, Result = new object() }
                            };
                        lock (Lockcmd)
                        {
                            if (!LockRemove)
                            {
                                int index = CMD[CMD.Count - 1].ListCMDRET.Count - Result.Count;
                                CMD[CMD.Count - 1].ListCMDRET.RemoveRange(index, Result.Count);
                            }
                            CMD.Add(new CMDRETPLC(New));
                            _getRet(CMD[CMD.Count - 1]);
                            t.reset();
                            while (!Cache.GetValue(TagName, out str) && t.D < Timeout)
                            {
                                Thread.Sleep(1);
                            }
                        }
                    }
                }
                Cache.GetValue(TagName, out str);
            }
            catch (Exception ex)
            {
                ex.log("Read");
            }
            return str;
        }

        /// <summary>
        /// 读取plc返回的对象
        /// </summary>
        /// <param name="TagName">标签数组</param>
        /// <returns>object[]数据</returns>
        public string Read(string TagName, bool Only)
        {
            string str = "";
            try
            {
                List<CMDRETPLC> CMD = AutoList;
                List<CMDRETPLC> Result = new List<CMDRETPLC>();
                bool FlatFirst = false;
                Result.Add(new CMDRETPLC() { Address = TagName, Result = new object(), Only = Only });
                if (!Cache.GetValue(TagName, out str))
                {
                    lock (Lockcmd)
                    {
                        CMD.Add(new CMDRETPLC(Result));
                        FlatFirst = true;
                    }
                }
                if (FlatFirst)
                {
                    TimerBase t = new TimerBase();
                    while (!Cache.GetValue(TagName, out str) && t.D < Timeout)
                    {
                        Thread.Sleep(1);
                    }
                    if (t.D > Timeout)
                    {
                        LockRemove = true;
                    }
                }
                Cache.GetValue(TagName, out str);
            }
            catch (Exception ex)
            {
                ex.log("Read");
            }
            return str;
        }


        /// <summary>
        /// 解析读PLC返回的数据
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="isRead"></param>
        /// <returns></returns>
        private List<CMDRETPLC> ParsingData(CMDRETPLC cmd, bool isRead = true)
        {
            List<CMDRETPLC> list = cmd.ListCMDRET;
            try
            {
                int offect = 38;//数据的偏移地址
                if (cmd.Ret[40] != 0x8A)
                {
                    list[0].Cmd = cmd.Cmd;
                    list[0].ErrCode = cmd.Ret[42];
                    list[0].State = cmd.State;
                    list[0].ReadBuffer = cmd.ReadBuffer;
                    SwichErrCode(list[0]);
                    if (list[0].ErrCode == 0)
                    {
                        if (isRead)
                        {
                            list[0].Lenght = (cmd.ReadBuffer.Count - 46) / GetLenght(list[0], cmd.Ret[44]);
                            if (cmd.Ret[44] == 0xD0)
                            {
                                list[0].Count = BitConverter.ToUInt16(new byte[2] { cmd.Ret[46], cmd.Ret[47] }, 0);
                                byte[] Data1 = new byte[list[0].Count];
                                list[0].Lenght = 1;
                                Array.Copy(cmd.Ret, 48, Data1, 0, Data1.Length);
                                VaryDataType(list[0], Data1);
                            }
                            else
                            {
                                GetResult(list[0], 46);
                            }
                            Cache.Add(list[0]);

                        }
                    }
                    else
                    {
                        list[0].Result = "ERROR";
                        Cache.Add(list[0]);
                        IsConnected = false;
                    }
                }
                else
                {
                    offect = 44;
                    for (int i = 0; i < list.Count; i++)
                    {
                        int offectStart = BitConverter.ToUInt16(cmd.Ret, offect + 2 + i * 2) + offect;
                        list[i].ErrCode = cmd.Ret[offectStart + 2];
                        list[i].ReadBuffer = cmd.ReadBuffer;
                        list[i].State = cmd.State;
                        SwichErrCode(list[i]);
                        if (list[i].ErrCode == 0)
                        {
                            if (isRead)
                            {
                                GetLenght(list[i], cmd.Ret[offectStart + 4]);
                                if (cmd.Ret[offectStart + 4] == 0xD0)
                                {
                                    list[0].Lenght = 1;
                                    list[i].Count = BitConverter.ToUInt16(new byte[2] { cmd.Ret[offectStart + 6], cmd.Ret[offectStart + 7] }, 0);
                                    byte[] Data1 = new byte[list[i].Count];
                                    Array.Copy(cmd.Ret, offectStart + 8, Data1, 0, Data1.Length);
                                    VaryDataType(list[i], Data1);
                                }
                                else
                                {
                                    if (i == list.Count - 1)
                                    {
                                        list[i].Lenght = (cmd.Ret.Length - offectStart - 6) / list[i].Count;
                                    }
                                    else
                                    {
                                        list[i].Lenght = (BitConverter.ToUInt16(cmd.Ret, offect + 4 + i * 2) - BitConverter.ToUInt16(cmd.Ret, offect + 2 + i * 2) - 6) / list[i].Count;
                                    }
                                    GetResult(list[i], offectStart + 6);

                                }
                                Cache.Add(list[i]);
                            }
                        }
                        else
                        {
                            list[i].Result = "ERROR";
                            Cache.Add(list[i]);
                            IsConnected = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.IsConnected = false;
            }

            return list;
        }

        /// <summary>
        /// 解析单个元素或者数组的方法
        /// </summary>
        /// <param name="cmd">指令</param>
        /// <param name="count">需要解析数据的个数</param>
        /// <param name="offect">偏移量</param>
        /// <returns></returns>
        private CMDRETPLC GetResult(CMDRETPLC cmd, int offect)
        {
            try
            {
                byte[] Data = new byte[cmd.Lenght * cmd.Count];
                Array.Copy(cmd.Ret, offect, Data, 0, Data.Length);
                VaryDataType(cmd, Data);
            }
            catch (Exception ex)
            {
                ex.log("解析异常");
                IsConnected = false;
                cmd.State = CmdState.TranNG;
            }
            return cmd;
        }

        private static void SwichErrCode(CMDRETPLC Cip)
        {
            switch (Cip.ErrCode)
            {
                case 0x00:
                    Cip.ErrMessage = "成功";
                    Cip.IsSuccess = true;
                    break;
                case 0x04:
                    Cip.ErrMessage = "它没有正确生成或匹配标记不存在。";
                    break;
                case 0x05:
                    Cip.ErrMessage = "引用的特定项（通常是实例）无法找到";
                    break;
                case 0x06:
                    Cip.ErrMessage = "请求的数据量不适合响应缓冲区。 发生了部分数据传输";
                    break;
                case 0x0A:
                    Cip.ErrMessage = "尝试处理其中一个属性时发生错误";
                    break;
                case 0x13:
                    Cip.ErrMessage = "命令中没有提供足够的命令数据/参数来执行所请求的服务";
                    break;
                case 0x1C:
                    Cip.ErrMessage = "与属性计数相比，提供的属性数量不足";
                    break;
                case 0x1E:
                    Cip.ErrMessage = "此服务中的服务请求出错";
                    break;
                case 0x20:
                    Cip.ErrMessage = "命令中参数的数据类型与实际参数的数据类型不一致";
                    break;
                case 0x26:
                    Cip.ErrMessage = "IOI字长与处理的IOI数量不匹配";
                    break;
                default:
                    Cip.ErrMessage = "未知错误";
                    break;
            }
        }
        #endregion

        #region 写PLC的方法

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签数组</param>
        /// <param name="Data">bool数组</param>
        /// <returns>结果</returns>
        public bool Write(string[] TagName, bool[] Data)
        {
            if (TagName.Length != Data.Length) throw new Exception("标签数量和写入的参数数量不一致，请检查");
            List<CMDRETPLC> list = new List<CMDRETPLC>();
            for (int i = 0; i < TagName.Length; i++)
            {
                list.Add(new CMDRETPLC() { Address = TagName[i], Result = Data[i], DateType = typeof(bool).Name });
            }
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签</param>
        /// <param name="Data">bool数组</param>
        /// <returns>结果</returns>
        public bool Write(string TagName, bool[] Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>();
            list.Add(new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(bool[]).Name });
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签数组</param>
        /// <param name="Data">short数组</param>
        /// <returns>结果</returns>
        public bool Write(string[] TagName, short[] Data)
        {
            if (TagName.Length != Data.Length) throw new Exception("标签数量和写入的参数数量不一致，请检查");
            List<CMDRETPLC> list = new List<CMDRETPLC>();
            for (int i = 0; i < TagName.Length; i++)
            {
                list.Add(new CMDRETPLC() { Address = TagName[i], Result = Data[i], DateType = typeof(short).Name });
            }
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签</param>
        /// <param name="Data">short数组</param>
        /// <returns>结果</returns>
        public bool Write(string TagName, short[] Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(short[]).Name }
            };
            return Write(list);
        }
        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签</param>
        /// <param name="Data">ushort数组</param>
        /// <returns>结果</returns>
        public bool Write(string[] TagName, ushort[] Data)
        {
            if (TagName.Length != Data.Length) throw new Exception("标签数量和写入的参数数量不一致，请检查");
            List<CMDRETPLC> list = new List<CMDRETPLC>();
            for (int i = 0; i < TagName.Length; i++)
            {
                list.Add(new CMDRETPLC() { Address = TagName[i], Result = Data, DateType = typeof(ushort).Name });
            }
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签</param>
        /// <param name="Data">ushort数组</param>
        /// <returns>结果</returns>
        public bool Write(string TagName, ushort[] Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(ushort[]).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签数组</param>
        /// <param name="Data">int数组</param>
        /// <returns>结果</returns>
        public bool Write(string[] TagName, int[] Data)
        {
            if (TagName.Length != Data.Length) throw new Exception("标签数量和写入的参数数量不一致，请检查");
            List<CMDRETPLC> list = new List<CMDRETPLC>();
            for (int i = 0; i < TagName.Length; i++)
            {
                list.Add(new CMDRETPLC() { Address = TagName[i], Result = Data[i], DateType = typeof(int).Name });
            }
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签</param>
        /// <param name="Data">int数组</param>
        /// <returns>结果</returns>
        public bool Write(string TagName, int[] Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(int[]).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签数组</param>
        /// <param name="Data">uint数组</param>
        /// <returns>结果</returns>
        public bool Write(string[] TagName, uint[] Data)
        {
            if (TagName.Length != Data.Length) throw new Exception("标签数量和写入的参数数量不一致，请检查");
            List<CMDRETPLC> list = new List<CMDRETPLC>();
            for (int i = 0; i < TagName.Length; i++)
            {
                list.Add(new CMDRETPLC() { Address = TagName[i], Result = Data[i], DateType = typeof(uint).Name });
            }
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签</param>
        /// <param name="Data">uint数组</param>
        /// <returns>结果</returns>
        public bool Write(string TagName, uint[] Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(uint[]).Name }
            };
            return Write(list);
        }
        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签数组</param>
        /// <param name="Data">float数组</param>
        /// <returns>写入的结果</returns>
        public bool Write(string[] TagName, float[] Data)
        {
            if (TagName.Length != Data.Length) throw new Exception("标签数量和写入的参数数量不一致，请检查");
            List<CMDRETPLC> list = new List<CMDRETPLC>();
            for (int i = 0; i < TagName.Length; i++)
            {
                list.Add(new CMDRETPLC() { Address = TagName[i], Result = Data[i], DateType = typeof(float).Name });
            }
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签</param>
        /// <param name="Data">float数组</param>
        /// <returns>写入的结果</returns>
        public bool Write(string TagName, float[] Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(float[]).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签数组</param>
        /// <param name="Data">double数组</param>
        /// <returns>写入的结果</returns>
        public bool Write(string[] TagName, double[] Data)
        {
            if (TagName.Length != Data.Length) throw new Exception("标签数量和写入的参数数量不一致，请检查");
            List<CMDRETPLC> list = new List<CMDRETPLC>();
            for (int i = 0; i < TagName.Length; i++)
            {
                list.Add(new CMDRETPLC() { Address = TagName[i], Result = Data[i], DateType = typeof(double).Name });
            }
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签</param>
        /// <param name="Data">double数组</param>
        /// <returns>写入的结果</returns>
        public bool Write(string TagName, double[] Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(double[]).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写PLC的方法
        /// </summary>
        /// <param name="TagName">标签</param>
        /// <param name="Data">bool数据/param>
        /// <returns>结果</returns>
        public bool Write(string TagName, bool Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(bool).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写plc的方法
        /// </summary>
        /// <param name="TagName">标签名</param>
        /// <param name="Data">int16数据</param>
        /// <returns>写入结果bool</returns>
        public bool Write(string TagName, short Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(short).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写plc的方法
        /// </summary>
        /// <param name="TagName">标签名</param>
        /// <param name="Data">uint16数据</param>
        /// <returns>写入结果bool</returns>
        public bool Write(string TagName, ushort Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(ushort).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写plc的方法
        /// </summary>
        /// <param name="TagName">标签名</param>
        /// <param name="Data">int32数据</param>
        /// <returns>写入结果bool</returns>
        public bool Write(string TagName, int Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(int).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写plc的方法
        /// </summary>
        /// <param name="TagName">标签名</param>
        /// <param name="Data">uint32数据</param>
        /// <returns>写入结果bool</returns>
        public bool Write(string TagName, uint Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(uint).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写plc的方法
        /// </summary>
        /// <param name="TagName">标签名</param>
        /// <param name="Data">单精度浮点数据</param>
        /// <returns>写入结果bool</returns>
        public bool Write(string TagName, float Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(float).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写plc的方法
        /// </summary>
        /// <param name="TagName">标签名</param>
        /// <param name="Data">int32数据</param>
        /// <returns>写入结果bool</returns>
        public bool Write(string TagName, double Data)
        {
            List<CMDRETPLC> list = new List<CMDRETPLC>
            {
                new CMDRETPLC() { Address = TagName, Result = Data, DateType = typeof(double).Name }
            };
            return Write(list);
        }

        /// <summary>
        /// 写plc的方法
        /// </summary>
        /// <param name="TagName">标签名</param>
        /// <param name="Data">字符串数组</param>
        /// <returns>写入结果bool</returns>
        public bool Write(string[] TagName, string[] Data)
        {
            if (TagName.Length != Data.Length) throw new Exception("标签数量和写入的参数数量不一致，请检查");
            List<CMDRETPLC> list = new List<CMDRETPLC>();
            for (int i = 0; i < TagName.Length; i++)
            {
                list.Add(new CMDRETPLC() { Address = TagName[i], Result = Data[i], DateType = typeof(string).Name });
            }
            return Write(list);
        }

        /// <summary>
        /// 写plc的方法
        /// </summary>
        /// <param name="TagName">标签名</param>
        /// <param name="Data">字符串数组</param>
        /// <returns>写入结果bool</returns>
        public bool Write(string TagName, string Data)
        {
            return Write(new string[1] { TagName }, new string[1] { Data });
        }

        /// <summary>
        /// 写入PLC的方法
        /// </summary>
        /// <param name="item">Cip数据列表</param>
        /// <returns></returns>
        public bool Write(List<CMDRETPLC> item)
        {
            List<CMDRETPLC> list = item;
            CMDRETPLC cmd = new CMDRETPLC(list, CMDType.WW);
            GetRet(cmd);
            if (cmd.State == CmdState.ReadOK)
            {
                item = ParsingData(cmd, false);
            }
            return true;

        }
        #endregion
        #endregion

        #region 私有成员
        /// <summary>
        /// 读写数据服务的命令
        /// </summary>
        private byte[] RWCommandCode = new byte[2] { 0x6F, 0 };

        private ushort DataID { get; set; } = 1;

        /// <summary>
        /// 握手指令-注册会话
        /// </summary>
        private byte[] Registercmd = new byte[28];

        /// <summary>
        /// 会话句柄
        /// </summary>
        private byte[] _SessionHandle = new byte[4];

        /// <summary>
        /// 会话句柄
        /// </summary>
        public byte[] SessionHandle
        {
            get
            {
                return _SessionHandle;
            }
            set
            {
                _SessionHandle = value;
            }
        }

        /// <summary>
        /// 连接标识ID
        /// </summary>

        private byte[] _SerialNumber = new byte[4];
        /// <summary>
        /// 连接标识ID
        /// </summary>
        public byte[] SerialNumber
        {
            get
            {
                return _SerialNumber;
            }
            set
            {
                _SerialNumber = value;
            }
        }

        private byte[] Header = new byte[24]
        {
            0x6F,0x00,//命令 2byte
            0x40,0x00,//长度 2byte
            0x00,0x00,0x00,0x00,//会话句柄 4byte
            0x00,0x00,0x00,0x00,//状态默认0 4byte
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,//发送方描述默认0 8byte
            0x00,0x00,0x00,0x00,//选项默认0 4byte
        };

        private byte[] CommandSpecificData = new byte[16]
        {
            0x00,0x00,0x00,0x00,//接口句柄 CIP默认为0x00000000 4byte
            0x01,0x00,//超时默认0x0001 4byte
            0x02,0x00,//项数默认0x0002 4byte
            0x00,0x00,//空地址项默认0x0000 2byte
            0x00,0x00,//长度默认0x0000 2byte
            0xb2,0x00,//未连接数据项默认为 0x00b2
            0x30,0x00,//后面数据包的长度 48个字节
        };
        private byte[] CipMessage = new byte[48]
        {
            0x54,//服务
            0x02,//请求路径大小固定为0x02
            0x20,0x06,0x24,0x01,//固定为0x01240620(有可能改变)
            0x0A,0x05,//超时
            0x00,0x00,0x00,0x00,//O-T-network connetion ID
            0x00,0x00,0x00,0x00,//T-O-network connetion ID
            0x00,0x00,//ConnetionSerial Number
            0x01,0x01,//Verder ID
            0x00,0x00,0x00,0x00,//Orginator serial number
            0x01,//连接超时倍数
            0x00,0x00,0x00,//保留数据
            0x40,0x4B,0x4C,0x00,//T-O RPI
            0xF8,0x43,//T-O网络连接参数
            0x40,0x4B,0x4C,0x00,//T-O RPI
            0xF8,0x43,//T-O网络连接参数
            0xA3,//传输类型
            0x03,//连接路径大小
            0x01,0x00,0x20,0x02,0x24,0x01//连接路径
        };

        #region  指令相关

        public int GetLenght(CMDRETPLC cmd, byte type)
        {
            switch (type)
            {
                case 0xc1:
                    cmd.Count = 2;
                    cmd.DateType = typeof(bool).Name;
                    break;
                case 0xc2:
                    cmd.Count = 1;
                    cmd.DateType = typeof(sbyte).Name;
                    break;
                case 0xc3:
                    cmd.Count = 2;
                    cmd.DateType = typeof(short).Name;
                    break;
                case 0xc4:
                    cmd.Count = 4;
                    cmd.DateType = typeof(int).Name;
                    break;
                case 0xc7:
                    cmd.Count = 2;
                    cmd.DateType = typeof(ushort).Name;
                    break;
                case 0xc8:
                    cmd.Count = 4;
                    cmd.DateType = typeof(uint).Name;
                    break;
                case 0xca:
                    cmd.Count = 4;
                    cmd.DateType = typeof(float).Name;
                    break;
                case 0xcb:
                    cmd.Count = 8;
                    cmd.DateType = typeof(double).Name;
                    break;
                case 0xD0:
                    cmd.DateType = typeof(string).Name;
                    cmd.Count = 1;
                    break;
            }
            return cmd.Count;
        }
        public object VaryDataType(CMDRETPLC cmd, byte[] Data)
        {
            object result = cmd.Result;
            string type = cmd.DateType;
            int count = cmd.Lenght;
            if (cmd.Lenght > 1)
            {
                type += "[]";
                cmd.DateType = type;
            }
            switch (type)
            {
                case "Boolean":
                    cmd.Result = BitConverter.ToUInt16(Data, 0) > 0 ? true : false;
                    break;
                case "Boolean[]":
                    bool[] Bool = new bool[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (Data.Length > (i * 2))
                            Bool[i] = BitConverter.ToUInt16(Data, i * 2) > 0 ? true : false;
                        else
                            cmd.ErrMessage = "数组解析长度不足";
                    }
                    cmd.Result = Bool;
                    break;
                case "SByte":
                    cmd.Result = BitConverter.ToInt16(new byte[2] { Data[0], 0 }, 0);
                    break;
                case "Int16":
                    cmd.Result = BitConverter.ToInt16(Data, 0);
                    break;
                case "Int16[]":
                    short[] Short = new short[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (Data.Length > (i * 2))
                            Short[i] = BitConverter.ToInt16(Data, i * 2);
                        else
                            cmd.ErrMessage = "数组解析长度不足";
                    }
                    cmd.Result = Short;
                    break;
                case "UInt16":
                    cmd.Result = BitConverter.ToUInt16(Data, 0);
                    break;
                case "UInt16[]":
                    ushort[] uShort = new ushort[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (Data.Length > (i * 2))
                            uShort[i] = BitConverter.ToUInt16(Data, i * 2);
                        else
                            cmd.ErrMessage = "数组解析长度不足";
                    }
                    cmd.Result = uShort;
                    break;
                case "Int32":
                    cmd.Result = BitConverter.ToInt32(Data, 0);
                    break;
                case "Int32[]":
                    int[] Int = new int[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (Data.Length > (i * 4))
                            Int[i] = BitConverter.ToInt32(Data, i * 4);
                        else
                            cmd.ErrMessage = "数组解析长度不足";
                    }
                    cmd.Result = Int;
                    break;
                case "UInt32":
                    cmd.Result = BitConverter.ToUInt32(Data, 0);
                    break;
                case "UInt32[]":
                    uint[] uInt = new uint[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (Data.Length > (i * 4))
                            uInt[i] = BitConverter.ToUInt32(Data, i * 4);
                        else
                            cmd.ErrMessage = "数组解析长度不足";
                    }
                    cmd.Result = uInt;
                    break;
                case "Single":
                    cmd.Result = BitConverter.ToSingle(Data, 0);
                    break;
                case "Single[]":
                    float[] Float = new float[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (Data.Length > (i * 4))
                            Float[i] = BitConverter.ToSingle(Data, i * 4);
                        else
                            cmd.ErrMessage = "解析长度不足";
                    }
                    cmd.Result = Float;
                    break;
                case "Double":
                    cmd.Result = BitConverter.ToDouble(Data, 0);
                    break;
                case "Double[]":
                    double[] Double = new double[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (Data.Length > (i * 8))
                            Double[i] = BitConverter.ToDouble(Data, i * 8);
                        else
                            cmd.ErrMessage = "解析长度不足";
                    }
                    cmd.Result = Double;
                    break;
                case "String":
                    cmd.Result = Encoding.ASCII.GetString(Data);
                    break;
            }
            return cmd.Result;
        }

        public byte[] RefDataType(CMDRETPLC cmd)
        {
            List<byte> list = new List<byte>();
            switch (cmd.DateType)
            {
                case "Boolean":
                    list.AddRange(new byte[4] { 0xc1, 0, 1, 0 });
                    list.AddRange(new byte[2] { (byte)((bool)cmd.Result ? 1 : 0), 0 });
                    break;
                case "Boolean[]":
                    list.AddRange(new byte[4] { 0xc1, 0, 1, 0 });
                    bool[] Bdata = (bool[])cmd.Result;
                    for (int i = 0; i < Bdata.Length; i++)
                    {
                        list.AddRange(new byte[2] { (byte)(Bdata[i] ? 1 : 0), 0 });
                    }
                    break;
                case "SByte":
                    list.AddRange(new byte[5] { 0xc2, 0, 1, 0, Encoding.ASCII.GetBytes(cmd.Result as string)[0] });
                    break;
                case "Int16":
                    list.AddRange(new byte[4] { 0xc3, 0, 1, 0 });
                    list.AddRange(BitConverter.GetBytes((short)cmd.Result));
                    break;
                case "Int16[]":
                    list.AddRange(new byte[4] { 0xc3, 0, 1, 0 });
                    short[] SHdata = (short[])cmd.Result;
                    list.AddRange(Vary.ArrayToBytes(SHdata, false));
                    break;
                case "UInt16":
                    list.AddRange(new byte[4] { 0xc7, 0, 1, 0 });
                    list.AddRange(BitConverter.GetBytes((ushort)cmd.Result));
                    break;
                case "UInt16[]":
                    list.AddRange(new byte[4] { 0xc7, 0, 1, 0 });
                    ushort[] USdata = (ushort[])cmd.Result;
                    list.AddRange(Vary.ArrayToBytes(USdata, false));
                    break;
                case "Int32":
                    list.AddRange(new byte[4] { 0xc4, 0, 1, 0 });
                    list.AddRange(BitConverter.GetBytes((int)cmd.Result));
                    break;
                case "Int32[]":
                    list.AddRange(new byte[4] { 0xc4, 0, 1, 0 });
                    int[] Idata = (int[])cmd.Result;
                    list.AddRange(Vary.ArrayToBytes(Idata, VaryType.Order));
                    break;
                case "UInt32":
                    list.AddRange(new byte[4] { 0xc8, 0, 1, 0 });
                    list.AddRange(BitConverter.GetBytes((uint)cmd.Result));
                    break;
                case "UInt32[]":
                    list.AddRange(new byte[4] { 0xc8, 0, 1, 0 });
                    uint[] UIdata = (uint[])cmd.Result;
                    list.AddRange(Vary.ArrayToBytes(UIdata, VaryType.Order));
                    break;
                case "Single":
                    list.AddRange(new byte[4] { 0xca, 0, 1, 0 });
                    list.AddRange(BitConverter.GetBytes((float)cmd.Result));
                    break;
                case "Single[]":
                    list.AddRange(new byte[4] { 0xca, 0, 1, 0 });
                    float[] Fdata = (float[])cmd.Result;
                    list.AddRange(Vary.ArrayToBytes(Fdata, VaryType.Order));
                    break;
                case "Double":
                    list.AddRange(new byte[4] { 0xcb, 0, 1, 0 });
                    list.AddRange(BitConverter.GetBytes((double)cmd.Result));
                    break;
                case "Double[]":
                    list.AddRange(new byte[4] { 0xcb, 0, 1, 0 });
                    double[] Ddata = (double[])cmd.Result;
                    list.AddRange(Vary.ArrayToBytes(Ddata, VaryType.Order));
                    break;
                case "String":
                    list.AddRange(new byte[4] { 0xd0, 0, 1, 0 });
                    byte[] buff = Encoding.ASCII.GetBytes(cmd.Result.ToString());
                    int lenght = buff.Length;
                    if (buff.Length % 2 == 1)
                        lenght += 1;
                    list.AddRange(BitConverter.GetBytes((ushort)(lenght)));
                    list.AddRange(buff);
                    if (buff.Length % 2 == 1)
                        list.Add(0);
                    break;
            }
            return list.ToArray();
        }


        /// <summary>
        /// 生成写数据服务的完整命令
        /// </summary>
        /// <param name="items">标签集合</param>
        /// <returns></returns>
        private byte[] ReadCommand(CMDRETPLC items)
        {
            List<byte> list = new List<byte>();
            Byte[] buff = Header;
            Array.Copy(RWCommandCode, 0, buff, 0, 2);
            byte[] buff1 = CommandSpecificData;
            Byte[] buff2 = GetReadCipMessega(items);
            buff1[14] = BitConverter.GetBytes((ushort)buff2.Length)[0];
            buff1[15] = BitConverter.GetBytes((ushort)buff2.Length)[1];
            Array.Copy(BitConverter.GetBytes((ushort)(buff1.Length + buff2.Length)), 0, buff, 2, 2);
            list.AddRange(buff);
            list.AddRange(buff1);
            list.AddRange(buff2);
            return list.ToArray();
        }



        /// <summary>
        /// 生成读服务的Cip消息
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private byte[] GetReadCipMessega(CMDRETPLC CMD)
        {
            List<byte> list = new List<byte>();
            ushort Lenght = 0;//CIP消息的长度，包含了请求帧的两个字节
            try
            {
                //--------------------CipMessge-----------------------------

                list.AddRange(new byte[6] { 0x52, 0x2, 0x20, 0x6, 0x24, 0x1 });// 服务 1byte  请求路径大小 1byte  请求路径4byte
                list.AddRange(new byte[2] { 0x0a, 0xF0 });//超时
                list.AddRange(new byte[2] { 0, 0 });//CIP指令长度
                if (CMD.ListCMDRET.Count == 1)
                {
                    list.Add(0x4C);
                    byte[] buff = PerfectConvert(CMD.ListCMDRET[0].Address);
                    list.Add((byte)((buff.Length) / 2));//节点长度
                    list.AddRange(buff);//节点数据
                    list.AddRange(new byte[2] { 1, 0 });
                    Lenght += (ushort)(4 + buff.Length);

                }
                else
                {
                    list.AddRange(new byte[6] { 0x0A, 0x02, 0x20, 0x02, 0x24, 0x01 });//服务 1byte  固定0x0A 请求路径大小 固定  0x02 请求路径0x01240220
                    list.AddRange(BitConverter.GetBytes((ushort)CMD.ListCMDRET.Count));//项数
                    ushort offect = (ushort)(0x02 + 2 * CMD.ListCMDRET.Count);//偏移量
                    Lenght += 8;
                    List<byte> temp = new List<byte>();
                    for (int i = 0; i < CMD.ListCMDRET.Count; i++)
                    {
                        list.AddRange(BitConverter.GetBytes(offect));//写入偏移量
                        Lenght += 2;
                        temp.Add(0x4c);
                        byte[] buff = PerfectConvert(CMD.ListCMDRET[i].Address);
                        temp.Add((byte)(buff.Length / 2));//读取的数据长度
                        temp.AddRange(buff); //添加读写的数据格式             
                        temp.AddRange(new byte[2] { 1, 0 });//服务命令指定数据
                        offect += (ushort)(4 + buff.Length);//统计偏移量
                        Lenght += (ushort)(4 + buff.Length);//统计指令长度;

                    }
                    list.AddRange(temp);
                }

            }
            catch (Exception ex)
            {
                CMD.ErrMessage = ex.Message;
                ex.log("GetReadCipMessega");
            }
            list.AddRange(new byte[4] { 1, 0, 1, Slot });
            byte[] Refbuff = list.ToArray();
            byte[] RefLenght = new byte[2] { (byte)((Lenght) % 256), (byte)((Lenght) / 256) };
            Array.Copy(RefLenght, 0, Refbuff, 8, 2);
            return Refbuff;
        }

        /// <summary>
        /// 生成写数据服务的完整命令
        /// </summary>
        /// <param name="items">标签集合</param>
        /// <returns></returns>
        private byte[] WriteCommand(CMDRETPLC CMD)
        {
            List<byte> list = new List<byte>();
            Byte[] buff = Header;
            Array.Copy(RWCommandCode, 0, buff, 0, 2);
            byte[] buff1 = CommandSpecificData;
            Byte[] buff2 = GetWriteCipMessega(CMD);
            buff1[14] = BitConverter.GetBytes((ushort)buff2.Length)[0];
            buff1[15] = BitConverter.GetBytes((ushort)buff2.Length)[1];
            Array.Copy(BitConverter.GetBytes((ushort)(buff1.Length + buff2.Length)), 0, buff, 2, 2);
            list.AddRange(buff);
            list.AddRange(buff1);
            list.AddRange(buff2);
            return list.ToArray();
        }

        /// <summary>
        /// 生成写数据服务的Cip消息
        /// </summary>
        /// <param name="items">cip标签列表</param>
        /// <returns></returns>
        private byte[] GetWriteCipMessega(CMDRETPLC CMD)
        {
            List<byte> list = new List<byte>();
            ushort Lenght = 2;//CIP消息的长度，包含了请求帧的两个字节
            try
            {
                //--------------------CipMessge-----------------------------

                list.AddRange(new byte[6] { 0x52, 0x2, 0x20, 0x6, 0x24, 0x1 });// 服务 1byte  请求路径大小 1byte  请求路径4byte
                list.AddRange(new byte[2] { 0x0a, 0xF0 });//超时
                list.AddRange(new byte[2] { 0, 0 });//CIP指令长度
                if (CMD.ListCMDRET.Count == 1)
                {
                    list.Add(0x4D);
                    byte[] buff = PerfectConvert(CMD.ListCMDRET[0].Address);
                    list.Add((byte)((buff.Length) / 2));//节点长度                    
                    list.AddRange(buff);//标签名数据
                    byte[] buff1 = RefDataType(CMD.ListCMDRET[0]);
                    list.AddRange(buff1);
                    Lenght += (ushort)(buff.Length + buff1.Length);

                }
                else
                {
                    list.AddRange(new byte[6] { 0x0A, 0x02, 0x20, 0x02, 0x24, 0x01 });//服务 1byte  固定0x0A 请求路径大小 固定  0x02 请求路径0x01240220
                    list.AddRange(BitConverter.GetBytes((ushort)CMD.ListCMDRET.Count));//项数
                    ushort offect = (ushort)(0x02 + 2 * CMD.ListCMDRET.Count);//偏移量
                    Lenght += 8;
                    List<byte> temp = new List<byte>();
                    for (int i = 0; i < CMD.ListCMDRET.Count; i++)
                    {
                        list.AddRange(BitConverter.GetBytes(offect));//写入偏移量
                        Lenght += 2;
                        temp.Add(0x4D);
                        byte[] buff = PerfectConvert(CMD.ListCMDRET[i].Address);
                        temp.Add((byte)(buff.Length / 2));//写入的数据长度
                        temp.AddRange(buff); //添加写入的数据格式
                        byte[] buff1 = RefDataType(CMD.ListCMDRET[i]);
                        list.AddRange(buff1);
                        offect += (ushort)(4 + buff.Length + buff1.Length);//统计偏移量
                        Lenght += (ushort)(buff.Length + buff1.Length);//统计指令长度;
                    }
                    list.AddRange(temp);
                }
            }
            catch (Exception ex)
            {
                CMD.ErrMessage = ex.Message;
                ex.log("GetWriteCipMessega");
            }
            list.AddRange(new byte[4] { 1, 0, 1, 0 });
            byte[] Refbuff = list.ToArray();
            byte[] RefLenght = new byte[2] { (byte)((Lenght) % 256), (byte)((Lenght) / 256) };
            Array.Copy(RefLenght, 0, Refbuff, 8, 2);
            return Refbuff;
        }

        /// <summary>
        /// 把标签转换成字节数组
        /// </summary>
        /// <param name="TagName">标签名</param>
        /// <returns>字节数组</returns>
        private byte[] PerfectConvert(string TagName)
        {
            List<byte> list = new List<byte>();
            string[] TagNames = TagName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < TagNames.Length; i++)
            {
                if (!TagNames[i].Contains<char>('['))
                {
                    list.Add(0x91);
                    byte[] tag = Encoding.ASCII.GetBytes(TagNames[i]);
                    list.Add((byte)(tag.Length));
                    list.AddRange(tag);
                    if ((tag.Length % 2) == 1)
                    {
                        list.Add(0);
                    }

                }
                else
                {
                    string[] strArray2 = TagNames[i].Split(new char[] { '[' });
                    list.Add(0x91);
                    byte[] tag = Encoding.ASCII.GetBytes(strArray2[0]);
                    list.Add((byte)tag.Length);
                    list.AddRange(tag);
                    if ((tag.Length % 2) == 1)
                    {
                        list.Add(0);
                    }
                    string[] strArray3 = strArray2[1].Split(new char[] { ',' });
                    for (int j = 0; j < strArray3.Length; j++)
                    {
                        string s = Regex.Replace(strArray3[j], "[^.0-9]", "");
                        if (int.Parse(s) < 0x100)
                        {
                            list.Add(0x28);
                            byte item = byte.Parse(s, System.Globalization.NumberStyles.Integer);
                            list.Add(item);
                        }
                        else
                        {
                            list.AddRange(new byte[2] { 0x29, 0 });
                            byte[] bytes = BitConverter.GetBytes(int.Parse(s));
                            list.AddRange(new byte[] { bytes[0], bytes[1] });
                        }
                    }
                }
            }
            return list.ToArray();
        }

        #endregion

        #endregion

        #region 重载方法

        /// <summary>
        /// 循环处理指令发送接收的工作线程
        /// </summary>
        protected override void Work()
        {
            try
            {
                if (CmdList.Count > 0)
                {

                    CMDRETPLC cmd = CmdList[0];
                    if (cmd != null)
                        _getRet(cmd);
                    lock (Lockcmd)
                    {
                        CmdList.Remove(cmd);
                    }
                }
                if (AutoList.Count > 0)
                {
                    CMDRETPLC CMDRETPLC = null;
                    lock (Lockcmd)
                    {
                        CMDRETPLC = AutoList[(CmdIndex++) % AutoList.Count];
                        if (CMDRETPLC != null)
                        {
                            _getRet(CMDRETPLC);
                        }
                        if (CMDRETPLC.ErrCode == 0x1B && LockRemove)
                        {
                            LockRemove = false;
                            Cache.Remove(CMDRETPLC.ListCMDRET[CMDRETPLC.ListCMDRET.Count - 1].Address);
                            CMDRETPLC.ListCMDRET.RemoveRange(CMDRETPLC.ListCMDRET.Count - 1, 1);                          
                        }
                    }
                }
                Thread.Sleep(CmdTime.I);
            }
            catch (Exception ex)
            {
                log("Work" + ex.Message);

            }

        }

        /// <summary>
        /// 对读写指令进行转换
        /// </summary>
        /// <param name="cmdRet"></param>
        /// <returns></returns>
        protected override byte[] TranCmd(CMDRETPLC cmdRet)
        {

            int len = cmdRet.Lenght;
            switch (cmdRet._CMDType)
            {
                case CMDType.RW: return ReadCommand(cmdRet);
                case CMDType.RB: break;
                case CMDType.WW: return WriteCommand(cmdRet);
                case CMDType.WB: break;
                default: break;
            }

            return cmdRet.Cmd;
        }

        protected override CMDRETPLC TranData(CMDRETPLC cmd)
        {
            if (!AutoList.Contains(cmd)) return cmd;
            if (cmd.State == CmdState.ReadOK)
            {
                cmd.ListCMDRET = ParsingData(cmd);
            }
            return cmd;
        }

        /// <summary>
        /// 注册一个会话消息
        /// </summary>
        /// <param name="hc"></param>
        /// <returns></returns>
        protected override bool DoAfterConnect(CommBase hc)
        {
            Registercmd[0] = 0x65;//命令-注册请求
            Registercmd[2] = 0x04;//命令长度
            Registercmd[24] = 1;//协议版本
            CMDRETPLC cmd = new CMDRETPLC(Registercmd);
            GetRet(cmd);
            if (cmd.Ret == null) return false;
            bool Flat = false;
            if (cmd.State == CmdState.ReadOK)
            {
                Array.Copy(cmd.Ret, 4, SessionHandle, 0, 4);//获取会话ID

                Array.Copy(SessionHandle, 0, Header, 4, 4);
                Flat = true;

            }
            return Flat ? true : false;
        }

        protected override CmdState CheckRet(CMDRETPLC cmdRet)
        {
            byte[] data = cmdRet.Ret;
            if (data.Length > 9)
            {
                if (data[8] >= 4 && data[8] <= 99) data[8] = 0x04;
                switch (data[8])
                {
                    case 0:
                        cmdRet.ErrMessage = "成功";
                        switch (cmdRet._CMDType)
                        {
                            case CMDType.WH_Hand:
                                cmdRet.State = CmdState.ReadOK;
                                break;
                            case CMDType.RW:
                                cmdRet.State = CmdState.ReadOK;
                                break;
                            case CMDType.WW:
                                cmdRet.State = CmdState.ReadOK;
                                break;
                        }
                        break;
                    case 0x01:
                        cmdRet.ErrMessage = "发出了无效或不受支持的封装命令";
                        break;
                    case 0x02:
                        cmdRet.ErrMessage = "接收器中的内存资源不足，无法处理命令";
                        break;
                    case 0x03:
                        cmdRet.ErrMessage = "封装消息的数据部分中的数据形成不良或不正确";
                        break;
                    case 0x04:
                        cmdRet.ErrMessage = "Reserved for legacy(RA)";
                        break;
                    case 0x64:
                        cmdRet.ErrMessage = "向目标发送封装消息时，始发者使用了无效的会话句柄。";
                        break;
                    case 0x65:
                        cmdRet.ErrMessage = "目标收到一个无效长度的信息";
                        break;
                    case 0x69:
                        cmdRet.ErrMessage = "不支持的封装协议修订";
                        break;
                    default:
                        cmdRet.ErrMessage = "未知错误";
                        break;

                }
                if (data.Length == 44 && data[42] == 0x1B)
                {
                    cmdRet.State = CmdState.ReadNG;
                    cmdRet.ErrCode = 0x1B;
                    cmdRet.ErrMessage = "标签名超限";
                }
            }
            return cmdRet.State;
        }

        public override void Close()
        {
            if (IsConnected)
            {
                byte[] URegistercmd = new byte[24];
                URegistercmd[0] = 0x66;
                URegistercmd[2] = 0x00;
                Array.Copy(SessionHandle, 0, Registercmd, 4, 4);
                CMDRETPLC cmd = new CMDRETPLC(Registercmd);
                GetRet(cmd);
                if (cmd.State == CmdState.ReadOK)
                {
                    log("卸载注册请求成功");
                }
                else
                {
                    log("卸载注册请求失败;" + cmd.ErrMessage);
                }
            }

            base.Close();
        }

        #endregion

        /// <summary>
        /// 实例化一个OmronCip协议的通讯对象
        /// </summary>
        /// <param name="RemoteIP">目标主机IP地址</param>
        /// <param name="Port">端口号</param>
        /// <param name="name">实例名称</param>
        /// <param name="_parent">父类</param>
        public OmronCip(string RemoteIP = "", int Port = 44818, string name = "OmronCip") : base(name)
        {
            ObjName = name;
            MaxByte = 450;
            Comm = new TCP(RemoteIP, Port);
            Init();

        }
    }
}
