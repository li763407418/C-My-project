using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IIRP.Com;
namespace IIRP.Sockets
{
    public class OmronFins : PLCBase
    {
        #region Fins通讯协议

        #region 私有成员
        /// <summary>
        /// 本机IP地址
        /// </summary>
        ValueBase SortPort = new ValueBase("", "本机IP地址", "192.168.1.1", "与PLC连接的网卡IP地址");

        /// <summary>
        /// PLC单元号
        /// </summary>
        ValueBase RemoteCPU = new ValueBase("", "PLC单元号", "0", "PLC的CPU单元号");

        /// <summary>
        /// 本机节点号
        /// </summary>
        public byte _SortPort
        {
            get
            {
                byte s = 0;
                if (SortPort.S.IndexOf('.') > 0)
                {
                    string[] str = SortPort.S.Split('.');
                    s = (byte)Convert.ToInt32(str[str.Length - 1]);
                }
                return s;
            }
        }

        /// <summary>
        /// PLC的节点号
        /// </summary>
        public byte _RemotePort
        {
            get
            {
                byte s = 0;
                TCP t = (TCP)Comm;
                if (t.IpAddress.S.IndexOf('.') > 0)
                {
                    string[] str = t.IpAddress.S.Split('.');
                    s = (byte)Convert.ToInt32(str[str.Length - 1]);
                }
                return s;
            }
        }

        /// <summary>
        ///  PLC CPU单元号
        /// </summary>
        public byte _RemoteCpu
        {
            get
            {
                return (byte)RemoteCPU.I;
            }
        }

        /// <summary>
        /// Fins握手指令
        /// </summary>
        public byte[] PLChand = new byte[20]
        {
            0x46,0x49,0x4E,0x53,// ASCII码:FINS
            0x00,0x00,0x00,0x0C,//长度12字节
            0x00,0x00,0x00,0x00,//指令
            0x00,0x00,0x00,0x00,//错误代码
            0x00,0x00,0x00,0x01,//最后一个是本机节点 00-FE
        };

        private byte[] Head = new byte[16]
        {
            0x46,0x49,0x4E,0x53,//F I N S
            0x00,0x00,0x00,0x00,//发送的字节数
            0x00,0x00,0x00,0x02,//指令
            0x00,0x00,0x00,0x00//错误代码
        };

        private byte[] FinsCmd = new byte[10]
        {
            0x80,//ICF
            0x00,//RSV
            0x02,//GCT
            0x00,//DNA目标网络号
            0x00,//DA1节点号 4
            0x00,//DA2单元号
            0x00,//SNA源网络号
            0x00,//SA1源节点号 7
            0x00,//SA2源单元号
            0x00,//SID 设置00
        };
        #endregion

        protected override bool CheckAddress(string addres)
        {

            bool Flat = false;
            try
            {
                string[] adr = Area(addres);
                if (adr[0] == "D" || adr[0] == "A"|| adr[0] == "CIO" || adr[0] == "H" || adr[0] == "W")
                {
                    float address = float.Parse(adr[1]);
                    if (address >= 0)
                    {
                        Flat = true;
                    }
                    else
                    {
                        MessageBox.Show("地址不合法");
                    }
                }
                else
                {
                    MessageBox.Show("通道不合法,支持 W、A、D、CIO、H");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("地址校验出错，请确认地址是否输入正确");
            }
            return Flat;
        }

        /// <summary>
        /// 读PLC的指令
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <param name="area">PLC区域，如CIO、DM、AR、HR等</param>
        /// <param name="lenght">读取长度</param>
        /// <param name="isByte">按字节或者按位读取-True:按字节 Flase:按位</param>
        /// <returns></returns>
        private byte[] ReadCommand(string address, string area, int lenght = 1, bool isByte = true)
        {
            byte[] Buff = new byte[34];
            try
            {
                ushort addr = 0;
                Array.Copy(Head, 0, Buff, 0, 16);
                Array.Copy(FinsCmd, 0, Buff, 16, 10);
                byte[] tmp = BitConverter.GetBytes(Buff.Length - 8);
                Array.Reverse(tmp);
                tmp.CopyTo(Buff, 4);
                Buff[20] = _RemotePort;//PLC节点号
                Buff[21] = _RemoteCpu;//PLC单元号
                Buff[23] = _SortPort; //本机节点号
                Buff[26] = 0x01;
                Buff[27] = 0x01; //0101读取存储区域
                switch (area)
                {
                    case "D":
                        Buff[28] = (byte)(isByte ? 0x82 : 0x02);
                        break;
                    case "CIO":
                        Buff[28] = (byte)(isByte ? 0xB0 : 0x30);
                        break;
                    case "W":
                        Buff[28] = (byte)(isByte ? 0xB1 : 0x31);
                        break;
                    case "H":
                        Buff[28] = (byte)(isByte ? 0xB2 : 0x32);
                        break;
                    case "A":
                        Buff[28] = (byte)(isByte ? 0xB3 : 0x33);
                        break;
                }
                if (isByte)
                {
                    addr = ushort.Parse(address);
                    Buff[29] = BitConverter.GetBytes(addr)[1];
                    Buff[30] = BitConverter.GetBytes(addr)[0];
                    Buff[31] = 0;
                }
                else
                {
                    string[] splits = address.Split('.');
                    addr = ushort.Parse(splits[0]);
                    Buff[29] = BitConverter.GetBytes(addr)[1];
                    Buff[30] = BitConverter.GetBytes(addr)[0];
                    if (splits.Length > 1)
                    {
                        Buff[31] = byte.Parse(splits[1]);
                    }
                }
                Buff[32] = (byte)(lenght / 256);
                Buff[33] = (byte)(lenght % 256);
            }
            catch (Exception ex)
            {
                ex.Message.log("Read");
            }
            return Buff;

        }

        /// <summary>
        /// 写入PLC的指令
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="area">PLC区域</param>
        /// <param name="Data">写入的数据</param>
        /// <param name="isByte">按字节或者按位写入-True:按字节 Flase:按位</param>
        /// <returns>字节数组</returns>
        private byte[] WriteCommand(string address, string area, byte[] Data, bool isByte = true)
        {
            byte[] Buff = new byte[34 + Data.Length];
            ReadCommand(address, area, 1, isByte).CopyTo(Buff, 0);
            byte[] tmp = BitConverter.GetBytes(Buff.Length - 8);
            Array.Reverse(tmp);
            tmp.CopyTo(Buff, 4);
            Buff[27] = 0x02;
            if (isByte)
            {
                Buff[32] = (byte)(Data.Length / 2 / 256);
                Buff[33] = (byte)(Data.Length / 2 % 256);
            }
            else
            {
                Buff[32] = (byte)(Data.Length / 256);
                Buff[33] = (byte)(Data.Length % 256);
            }
            Data.CopyTo(Buff, 34);
            return Buff;
        }

        #region 重写函数
        public override bool Write(string address, string data)
        {
            List<byte> buff = Vary.StringToBytes(data).ToList();
            if (buff.Count % 2 == 1)
            {
                buff.Add(0);
            }
            for (int i = 0; i < buff.Count; i++)
            {
                if (i % 2 == 0)
                {
                    byte temp = buff[i];
                    buff[i] = buff[i + 1];
                    buff[i + 1] = temp;
                }
            }
            return Write(address, buff.ToArray(), data.Length);
        }

        protected override CMDRETPLC GetString(CMDRETPLC cmd)
        {
            byte[] buff = new byte[cmd.Lenght * 2];
            Array.Copy(cmd.Ret, CmdRetIndex, buff, 0, cmd.Ret.Length - CmdRetIndex);
            for (int i = 0; i < buff.Length; i++)
            {
                if (i % 2 == 0 && buff.Length >= i + 2)
                {
                    byte temp = buff[i];
                    buff[i] = buff[i + 1];
                    buff[i + 1] = temp;
                }
            }
            cmd.Result = Encoding.ASCII.GetString(buff);
            return cmd;
        }

        protected override bool DoAfterConnect(CommBase hc)
        {
            PLChand[19] = _SortPort;
            CMDRETPLC cmd = new CMDRETPLC(PLChand);
            GetRet(cmd);
            if (cmd.Ret == null) return false;
            return cmd.State == CmdState.ReadOK ? true : false;
        }

        /// <summary>
        /// 检查指令读取的状态
        /// </summary>
        /// <param name="cmdRet">指令</param>
        /// <returns>指令读取状态</returns>
        protected override CmdState CheckRet(CMDRETPLC cmdRet)
        {
            byte[] data = cmdRet.Ret;
            CMDRETPLC scmd = (CMDRETPLC)cmdRet;
            if (data != null && data.Length >= 20)
            {
                if (data[15] == 0x00)
                {
                    switch (scmd._CMDType)
                    {
                        case CMDType.WH_Hand:
                            if (data[11] == 0x01) scmd.State = CmdState.ReadOK; else scmd.State = CmdState.ReadNG;
                            break;
                        default:
                            if (data[11] == 0x02) scmd.State = CmdState.ReadOK; else scmd.State = CmdState.ReadNG;
                            break;
                    }
                }
                else
                {
                    switch (data[15])
                    {
                        case 0x01:
                            scmd.ErrMessage = "数据头不是FINS或ASCII格式";
                            break;
                        case 0x02:
                            scmd.ErrMessage = "数据长度过长";
                            break;
                        case 0x03:
                            scmd.ErrMessage = "命令错误";
                            break;
                        case 0x20:
                            scmd.ErrMessage = "连接通信被占用";
                            break;
                        case 0x21:
                            scmd.ErrMessage = "指定的节点处于连接中";
                            break;
                        default:
                            scmd.ErrMessage = "未知异常";
                            break;
                    }
                    scmd.State = CmdState.ReadNG;
                }

            }

            return scmd.State;
        }

        protected override byte[] TranCmd(CMDRETPLC cmdRet)
        {
            string[] address = Area(cmdRet.Address);
            int len = cmdRet.Count;
            switch (cmdRet._CMDType)
            {
                case CMDType.RW: return ReadCommand(address[1], address[0], len);
                case CMDType.RB: return ReadCommand(address[1], address[0], len, false);
                case CMDType.WW: return WriteCommand(address[1], address[0], cmdRet.DataBuff);
                case CMDType.WB: return WriteCommand(address[1], address[0], cmdRet.DataBuff, false);
                default: break;
            }
            return cmdRet.Cmd;
        }

        protected override string[] Area(string Address)
        {
            string[] ADDr = new string[2];
            if (Address.ToUpper().Contains("CIO"))
            {
                ADDr[0] = "CIO";
                ADDr[1] = Address.Substring(3);
            }
            else
            {
                ADDr[0] = Address.Substring(0, 1);
                ADDr[1] = Address.Substring(1);
            }
            return ADDr;
        }
        #endregion

        #endregion

        #region OmronPLC_Fins
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="RemoteIP">目标IP地址</param>
        /// <param name="SortIP">本机IP地址</param>
        /// <param name="Port">端口号</param>
        /// <param name="name">设备名称</param>
        /// <param name="_parent"></param>
        public OmronFins(string RemoteIP = "", int port = 9600, string SourIP = "", string name = "OmronPLC") : base(name)
        {
            Values.Add(SortPort);
            Values.Add(RemoteCPU);
            CmdRetIndex = 30;
            WordLenght = 1;
            vary = VaryType.Single;
            ObjName = name;
            Comm = new TCP(RemoteIP, port);
            Init();
        }
        #endregion
    }
}
