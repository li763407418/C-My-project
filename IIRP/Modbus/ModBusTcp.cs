using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IIRP.Sockets;
using IIRP.Com;
using System.Threading;
using System.Windows.Forms;
namespace IIRP.Modbus
{

    /// <summary>
    /// ModbusTcp 客户端
    /// </summary>
    /// <remarks>寄存器字母指代 C-线圈:00001-09999 T-离散量:10001-19999 (只读) D-保持寄存器-40001-49999 I-输入寄存器(只读):30001-39999</remarks>
    public class ModBusTcp : PLCBase
    {
        #region 私有成员、方法
        /// <summary>
        /// 消息号自增
        /// </summary>
        protected ushort MessageIndex { get; set; } = 0;
        /// <summary>
        /// 站号
        /// </summary>
        protected byte Station { get; set; } = 0;

        /// <summary>
        /// 获取bool数组的长度
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override int GetBoolLenght(bool[] data)
        {
            return (data.Length + 7) / 8;
        }

        /// <summary>
        /// 解析布尔类型的数据
        /// </summary>
        /// <param name="cmd">指令</param>
        /// <returns>指令</returns>
        protected override CMDRETPLC GetBool(CMDRETPLC cmd)
        {
            int count = cmd.Lenght;
            bool[] data = new bool[count];
            byte[] buff = new byte[cmd.Count];
            if (count == 1)
            {
                cmd.Result = cmd.Ret[CmdRetIndex] > 0 ? true : false;
            }
            else
            {
                Array.Copy(cmd.Ret, CmdRetIndex, data, 0, cmd.Count);
                for (int i = 0; i < count; i++)
                {
                    int index = i / 8; //此位在字节数组中的索引
                    int offect = i % 8; //此位在字节中的索引
                    byte temp = 0;
                    switch (offect)
                    {
                        case 0: temp = 0x01; break;//最低位
                        case 1: temp = 0x02; break;
                        case 2: temp = 0x04; break;
                        case 3: temp = 0x08; break;
                        case 4: temp = 0x10; break;
                        case 5: temp = 0x20; break;
                        case 6: temp = 0x40; break;
                        case 7: temp = 0x80; break;
                        default: break;
                    }
                    if ((buff[index] & temp) == temp)//
                    {
                        data[i] = true;
                    }
                }
                cmd.Result = data;
            }
            return cmd;
        }

        /// <summary>
        /// 布尔数组转换成字节数组的方法
        /// </summary>
        /// <param name="data">布尔数组</param>
        /// <returns>字节数组</returns>
        protected virtual byte[] ArrayToBytes(bool[] data)
        {
            int count = (data.Length + 7) / 8;
            byte[] Buff = new byte[count];
            for (int i = 0; i < count; i++)
            {
                int index = i / 8; //此位在字节数组中的索引
                int offect = i % 8; //此位在字节中的索引
                byte temp = 0;
                switch (offect)
                {
                    case 0: temp = 0x01; break;//最低位
                    case 1: temp = 0x02; break;
                    case 2: temp = 0x04; break;
                    case 3: temp = 0x08; break;
                    case 4: temp = 0x10; break;
                    case 5: temp = 0x20; break;
                    case 6: temp = 0x40; break;
                    case 7: temp = 0x80; break;
                    default: break;
                }
                Buff[index] += (byte)(data[i] ? temp : 0);
            }
            return Buff;
        }

        /// <summary>
        /// 写单个位变量的方法 
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="data">高位在前,低位在后</param>
        /// <returns></returns>
        public override bool Write(string address, bool data)
        {
            return Write(address, new byte[2] { 0x00, (byte)(data ? 0x01 : 0x00) }, 1);
        }

        /// <summary>
        /// 写多个位变量的方法
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="data">布尔数组</param>
        /// <returns>写入结果 成功:true 失败:false</returns>
        public override bool Write(string address, bool[] data)
        {
            return Write(address, ArrayToBytes(data), data.Length);
        }

        /// <summary>
        /// 校验读写地址是否合法
        /// </summary>
        /// <param name="add">地址</param>
        /// <returns>bool</returns>
        protected override bool CheckAddress(string add)
        {
            bool Flat = false;
            string[] adr = Area(add);
            if (adr[0] == "T" || adr[0] == "D" || adr[0] == "C" || adr[0] == "I")
            {
                Flat = true;
            }
            else
            {
                MessageBox.Show("通道不合法,T:离散量 D:保持寄存器 I:输入寄存器: C:线圈");
            }
            return Flat;

        }

        /// <summary>
        /// 读指令
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="FuntionCode">功能号</param>
        /// <param name="lenght">长度</param>
        /// <returns></returns>
        protected byte[] ReadCommand(string address, string Area, ushort lenght = 1)
        {
            string NewAddress = address;
            byte FuntionCode = GetReadFuntionCode(address,Area, out NewAddress);
            ushort add = Convert.ToUInt16(NewAddress);
            List<byte> Buff = new List<byte>();
            Buff.AddRange(BitConverter.GetBytes(MessageIndex).Reverse());//消息号
            Buff.AddRange(new byte[4] { 0, 0, 0, 6 });// Modbus-标志号 0x0000,后面指令的长度最后赋值
            Buff.Add(Station);//站号
            Buff.Add(FuntionCode); //功能码
            Buff.AddRange(BitConverter.GetBytes(add).Reverse());//地址
            Buff.AddRange(BitConverter.GetBytes(lenght).Reverse());
            return Buff.ToArray();
        }

        /// <summary>
        ///写指令
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="FuntionCode">功能号</param>
        /// <param name="lenght">长度</param>
        /// <returns></returns>
        protected byte[] WriteCommand(string address, string Area, byte[] Data, int lenght = 1)
        {
            string NewAddress = address;
            byte FuntionCode = GetWriteFuntionCode(address,Area, lenght,out NewAddress);
            ushort add = Convert.ToUInt16(NewAddress);
            List<byte> Buff = new List<byte>();
            Buff.AddRange(BitConverter.GetBytes(MessageIndex).Reverse());//消息号
            Buff.AddRange(new byte[4] { 0, 0, 0, 0 });// Modbus-标志号 0x0000,后面指令的长度最后赋值
            Buff.Add(Station);//站号
            Buff.Add(FuntionCode); //功能码
            Buff.AddRange(BitConverter.GetBytes(add).Reverse());//地址
            switch (FuntionCode)
            {
                case 0x0F:
                    Buff.Add((byte)(Data.Length / 256));
                    Buff.Add((byte)(Data.Length % 256));
                    Buff.Add((byte)(Data.Length));
                    break;
                case 0x10:
                    Buff.Add((byte)(Data.Length / 2 / 256));
                    Buff.Add((byte)(Data.Length / 2 % 256));
                    Buff.Add((byte)(Data.Length));
                    break;
            }
            Buff.AddRange(Data);
            byte[] buff = Buff.ToArray();
            ushort Lenght = (ushort)(buff.Length - 6);
            Array.Copy(BitConverter.GetBytes(Lenght).Reverse().ToArray(), 0, buff, 4, 2);
            return buff;
        }

        /// <summary>
        /// 返回读功能码
        /// </summary>
        /// <param name="Area">通道 C:线圈-0x01 D:保持寄存器-0x03 T:离散量-0x02 I:输入寄存器 0x04</param>
        /// <returns></returns>
        protected virtual byte GetReadFuntionCode(string address, string Area, out string Newaddress)
        {
            byte FuntionCode = 0x01;
            Newaddress = address;
            switch (Area.ToUpper())
            {
                case "C":
                    FuntionCode = 0x01;
                    break;
                case "T":
                    FuntionCode = 0x02;
                    break;
                case "D":
                    FuntionCode = 0x03;
                    break;
                case "I":
                    FuntionCode = 0x04;
                    break;
            }
            return FuntionCode;
        }

        /// <summary>
        /// 返回写功能码
        /// </summary>
        /// <param name="Area">通道 C:线圈 功能码 05、15  D:寄存器 功能码06/16</param>
        /// <param name="lenght">写的数据个数</param>
        /// <returns></returns>
        protected virtual byte GetWriteFuntionCode(string address, string Area, int lenght, out string Newaddress)
        {
            byte FuntionCode = 0x05;
            Newaddress = address;
            switch (Area.ToUpper())
            {
                case "C":
                    if (lenght == 1) FuntionCode = 0x05; else FuntionCode = 0x0F;
                    break;
                case "D":
                    if (lenght == 1) FuntionCode = 0x06; else FuntionCode = 0x10;
                    break;
            }
            return FuntionCode;
        }

        #endregion
        /// <summary>
        /// 校验读取的数据
        /// </summary>
        /// <param name="scmd"></param>
        /// <returns></returns>
        protected override CmdState CheckRet(CMDRETPLC scmd)
        {
            byte[] data = scmd.Ret;
            MessageIndex += 1;
            if (MessageIndex < 0)
                MessageIndex = 0;
            if ((scmd.Cmd[7] + 0x80) == scmd.Ret[7])
            {
                scmd.IsSuccess = false;
                scmd.ErrCode = scmd.Ret[8];
                switch (scmd.Ret[8])
                {
                    case 0x01:
                        scmd.ErrMessage = "不支持的功能码";
                        break;
                    case 0x02:
                        scmd.ErrMessage = "地址越界";
                        break;
                    case 0x03:
                        scmd.ErrMessage = "读取长度超过最大值";
                        break;
                    case 0x04:
                        scmd.ErrMessage = "读写异常";
                        break;
                }
                scmd.State = CmdState.ReadNG;
            }
            else
            {
                scmd.State = CmdState.ReadOK;
            }
            return scmd.State;
        }

        /// <summary>
        /// 重写指令转换函数
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected override byte[] TranCmd(CMDRETPLC cmd)
        {
            string[] address = Area(cmd.Address);
            ushort len = (ushort)cmd.Lenght;
            switch (cmd._CMDType)
            {
                case CMDType.RW:
                    return ReadCommand(address[1], address[0],(ushort)cmd.Count);
                case CMDType.RB:
                    return ReadCommand(address[1], address[0],(ushort)cmd.Count);
                case CMDType.WB:
                    return WriteCommand(address[1], address[0], cmd.DataBuff,cmd.Count/2);
                case CMDType.WW:
                    return WriteCommand(address[1], address[0], cmd.DataBuff,cmd.Count/2);
            }
            return cmd.Cmd;
        }

        protected override string[] Area(string Address)
        {
            string[] ADDr = new string[2];
            ADDr[0] = Address.Substring(0, 1);
            ADDr[1] = Address.Substring(1);
            return ADDr;
        }

        public ModBusTcp(string name):base(name)
        {

        }
        public ModBusTcp(string ip, string port = "502", byte station = 1, string name = "ModBusTCP") : base(name)
        {
            Station = station;
            MessageIndex = 0;
            CmdRetIndex = 9;
            Timeout = 500;
            vary = VaryType.Single;
            Comm = new TCP(ip, Convert.ToInt32(port), name);
            Init();
        }
    }
}
