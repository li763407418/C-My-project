using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using IIRP.Com;
using System.Threading;
using System.Windows.Forms;

namespace IIRP.Sockets
{
    /// <summary>
    /// 西门子PLC
    /// </summary>
    public class S7Net : PLCBase
    {

        #region 私有方法和成员

        /// <summary>
        /// 指令格式
        /// </summary>        
        byte[] Cmd = new byte[31]
        {
          //Header Start Length=17 =============================================
          0x03,0x00,//报文头
          0x00,0x1F,//指令长度 高位在前，低位在后
          0x02,0xF0,0x80,//固定
          0x32,// TCP协议标识
          0x01,//命令:发
          0x00,0x00,//冗余识别:0x0000
          0x00,0x01,//协议数据单元引用，由请求事件增加
          0x00,0x0E,//参数指令长度，高位在前低位在后
          0x00,0x00,//读取内部数据时为00，写入数据时为data数据长度
          //Header END=========================================================
          //参数指令 start Length=14===========================================
          0x04,//04-读 05-写
          0x01,//读取数据块的个数
          0x12,//指定有效值类型
          0x0A,//本次地址访问长度
          0x10,//语法标记，ANY
          0x02,//02-按字为单位 01-按位为单位
          0x00,0x01,//访问字或位的个数 高位在前，低位在后
          0x00,0x00,//DB块编号，不是全为0
          0x83,//访问数据类型：I区：0x81 Q:0x82 M:0x83 D、DB:0x84 T:0x1D C:0x1c
          0x00,0x00,0x00,//偏移位置
        };

        /// <summary>
        /// 生成按字节读PLC地址区域的指令
        /// </summary>
        /// <param name="address">要读的地址，如1000</param>
        /// <param name="length">连续读取的字节长度</param>
        /// <param name="sa">PLC区域如I、M、Q、DB、T、C</param>
        /// <returns>字节数组</returns>
        private byte[] BuidReadByteCommand(string address, string area, int length = 1)
        {

            byte[] Rcmd = Cmd;
            int dbAddress = 0;//db块编号
            int startAddress = 0;
            byte type = 0;
            switch (area)
            {
                case "I":
                    type = 0x81;
                    startAddress = GetStartAddress(address);
                    break;
                case "Q":
                    type = 0x82;
                    startAddress = GetStartAddress(address);
                    break;
                case "M":
                    type = 0x83;
                    startAddress = GetStartAddress(address);
                    break;
                case "DB":
                    type = 0x84;
                    // 传入参数为1.100代表 1号DB块第100地址  1.100.7表示1号DB块第100地址的第7位
                    string[] adds = address.Split('.');
                    dbAddress = Convert.ToInt32(adds[0]);
                    startAddress = GetStartAddress(address.Substring(address.IndexOf('.') + 1));
                    break;
                case "T":
                    type = 0x1D;
                    startAddress = GetStartAddress(address);
                    break;
                case "C":
                    type = 0x1C;
                    startAddress = GetStartAddress(address);
                    break;
            }
            Rcmd[22] = 0x02;
            Rcmd[23] = (byte)(length / 256);
            Rcmd[24] = (byte)(length % 256);
            Rcmd[25] = (byte)(dbAddress / 256);
            Rcmd[26] = (byte)(dbAddress % 256);
            Rcmd[27] = type;
            Rcmd[28] = (byte)(startAddress / 256 / 256 % 256);
            Rcmd[29] = (byte)(startAddress / 256 % 256);
            Rcmd[30] = (byte)(startAddress % 256);
            return Rcmd;
        }

        /// <summary>
        /// 生成按位读取PLC地址区域的指令
        /// </summary>
        /// <param name="address">要读的地址，如1000.7</param>
        /// <param name="count">需要读取多少个字</param>
        /// <param name="sa">PLC区域如I、M、Q、DB、T、C</param>
        /// <returns></returns>
        private byte[] BuidReadBitCommand(string address, string area)
        {

            byte[] Rcmd = Cmd;
            int dbAddress = 0;//db块编号
            int startAddress = 0;
            byte type = 0;
            switch (area)
            {
                case "I":
                    type = 0x81;
                    startAddress = GetStartAddress(address);
                    break;
                case "Q":
                    type = 0x82;
                    startAddress = GetStartAddress(address);
                    break;
                case "M":
                    type = 0x83;
                    startAddress = GetStartAddress(address);
                    break;
                case "DB":
                    type = 0x84;
                    ///DB1.DBX1000.01 传入参数为1.100代表 1号DB块第100个字  1.100.7表示1号DB块第100个字的第7位
                    string[] adds = address.Split('.');
                    dbAddress = Convert.ToInt32(adds[0]);
                    startAddress = GetStartAddress(address.Substring(address.IndexOf('.') + 1));
                    break;
                case "T":
                    type = 0x1D;
                    startAddress = GetStartAddress(address);
                    break;
                case "C":
                    type = 0x1C;
                    startAddress = GetStartAddress(address);
                    break;
            }
            Rcmd[22] = 0x01;
            Rcmd[23] = 0x00;
            Rcmd[24] = 0x01;
            Rcmd[25] = (byte)(dbAddress / 256);
            Rcmd[26] = (byte)(dbAddress % 256);
            Rcmd[27] = type;
            Rcmd[28] = (byte)(startAddress / 256 / 256 % 256);
            Rcmd[29] = (byte)(startAddress / 256 % 256);
            Rcmd[30] = (byte)(startAddress % 256);
            return Rcmd;
        }

        /// <summary>
        /// 生成按字节写PLC地址的指令
        /// </summary>
        /// <param name="address">写入的地址</param>
        /// <param name="data">写入的数据</param>
        /// <param name="sa">写入的地址区域</param>
        /// <returns></returns>
        private byte[] BuidWriteByteCommand(string address, byte[] data, string area)
        {
            if (data == null) data = new byte[0];
            byte[] Wcmd = new byte[35 + data.Length];
            Cmd.CopyTo(Wcmd, 0);
            int dbAddress = 0;//db块编号
            int startAddress = 0;
            byte type = 0;
            switch (area)
            {
                case "I":
                    type = 0x81;
                    startAddress = GetStartAddress(address);
                    break;
                case "Q":
                    type = 0x82;
                    startAddress = GetStartAddress(address);
                    break;
                case "M":
                    type = 0x83;
                    startAddress = GetStartAddress(address);
                    break;
                case "DB":
                    type = 0x84;
                    ///DB1.DBX1000.01 传入参数为1.100代表 1号DB块第100个字  1.100.7表示1号DB块第100个字的第7位
                    string[] adds = address.Split('.');
                    dbAddress = Convert.ToInt32(adds[0]);
                    startAddress = GetStartAddress(address.Substring(address.IndexOf('.') + 1));
                    break;
                case "T":
                    type = 0x1D;
                    startAddress = GetStartAddress(address);
                    break;
                case "C":
                    type = 0x1C;
                    startAddress = GetStartAddress(address);
                    break;
            }
            Wcmd[2] = (byte)((Wcmd.Length) / 256);
            Wcmd[3] = (byte)((Wcmd.Length) % 256);
            Wcmd[15] = (byte)((4 + data.Length) / 256);
            Wcmd[16] = (byte)((4 + data.Length) % 256);
            Wcmd[17] = 0x05;
            Wcmd[22] = 0x02;
            Wcmd[23] = (byte)(data.Length / 256);
            Wcmd[24] = (byte)(data.Length % 256);
            Wcmd[25] = (byte)(dbAddress / 256);
            Wcmd[26] = (byte)(dbAddress % 256);
            Wcmd[27] = type;
            Wcmd[28] = (byte)(startAddress / 256 / 256 % 256);
            Wcmd[29] = (byte)(startAddress / 256 % 256);
            Wcmd[30] = (byte)(startAddress % 256);
            Wcmd[31] = 0x00;
            Wcmd[32] = 0x04;//按字写入
            Wcmd[33] = (byte)(data.Length * 8 / 256);
            Wcmd[34] = (byte)(data.Length * 8 % 256);//按位计算的长度
            data.CopyTo(Wcmd, 35);
            return Wcmd;

        }

        /// <summary>
        /// 生成按位写PLC地址的指令
        /// </summary>
        /// <param name="address">写入的地址</param>
        /// <param name="data">写入的数据</param>
        /// <param name="sa">写入的地址区域</param>
        /// <returns></returns>
        private byte[] BuidWriteBitCommand(string address, byte[] data, string area)
        {
            byte[] buffer = new byte[1];
            buffer[0] = data[0];
            byte[] Wcmd = new byte[35 + buffer.Length];
            Cmd.CopyTo(Wcmd, 0);
            int dbAddress = 0;//db块编号
            int startAddress = 0;
            byte type = 0;
            switch (area)
            {
                case "I":
                    type = 0x81;
                    startAddress = GetStartAddress(address);
                    break;
                case "Q":
                    type = 0x82;
                    startAddress = GetStartAddress(address);
                    break;
                case "M":
                    type = 0x83;
                    startAddress = GetStartAddress(address);
                    break;
                case "DB":
                    type = 0x84;
                    ///DB1.DBX1000.01 传入参数为1.100代表 1号DB块第100个字  1.100.7表示1号DB块第100个字的第7位
                    string[] adds = address.Split('.');
                    dbAddress = Convert.ToInt32(adds[0]);
                    startAddress = GetStartAddress(address.Substring(address.IndexOf('.') + 1));
                    break;
                case "T":
                    type = 0x1D;
                    startAddress = GetStartAddress(address);
                    break;
                case "C":
                    type = 0x1C;
                    startAddress = GetStartAddress(address);
                    break;
            }
            Wcmd[2] = (byte)((Wcmd.Length) / 256);
            Wcmd[3] = (byte)((Wcmd.Length) % 256);
            Wcmd[15] = (byte)((4 + buffer.Length) / 256);
            Wcmd[16] = (byte)((4 + buffer.Length) % 256);
            Wcmd[17] = 0x05;
            Wcmd[22] = 0x01;
            Wcmd[23] = (byte)(buffer.Length / 256);
            Wcmd[24] = (byte)(buffer.Length % 256);
            Wcmd[25] = (byte)(dbAddress / 256);
            Wcmd[26] = (byte)(dbAddress % 256);
            Wcmd[27] = type;
            Wcmd[28] = (byte)(startAddress / 256 / 256 % 256);
            Wcmd[29] = (byte)(startAddress / 256 % 256);
            Wcmd[30] = (byte)(startAddress % 256);

            Wcmd[31] = 0x00;
            Wcmd[32] = 0x03;//按位写入
            Wcmd[33] = (byte)(buffer.Length / 256);
            Wcmd[34] = (byte)(buffer.Length % 256);
            buffer.CopyTo(Wcmd, 35);
            return Wcmd;

        }

        /// <summary>
        /// 获得偏移地址
        /// </summary>
        /// <param name="address">传入的地址</param>
        /// <param name="RW_way">返回的读写方式</param>
        /// <returns></returns>
        private int GetStartAddress(string address)
        {
            if (address.IndexOf('.') < 0)
            {
                return Convert.ToInt32(address) * 8;
            }
            else
            {
                string[] temp = address.Split('.');
                return Convert.ToInt32(temp[0]) * 8 + Convert.ToInt32(temp[1]);
            }
        }
        #endregion

        #region 重写函数

        /// <summary>
        /// COTP握手指令
        /// </summary>
        private byte[] PLCHead1 = new byte[22]
        {
                0x03,  // 01 RFC1006 ISO-8075 Header 
                0x00,  // 02 RS默认00
                0x00,  // 03 THI：请求码通常为00
                0x16,  // 04 数据总长度 PLCHead1.lenght
                0x11,  // 05 头长度连接类型0x11:tcp  0x12 ISO-on-TCP
                0xE0,  // 06 主动建立连接 OXD0-连接确认 0X80断开请求 0XDC断开确认
                0x00,  // 07 
                0x00,  // 08 远程地址 0x0000
                0x00,  // 09 
                0x2E,  // 10 本地地址0x0001
                0x00,  // 11 级别
                0xC1,  // 12 本地TSAP代号
                0x02,  // 13 本地TSAP长度
                0x01,  // 14 本地TSAP访问资源
                0x00,  // 15 本地TSAP访问点
                0xC2,  // 16 远程TSAP代号
                0x02,  // 17 远程TSAP长度
                0x03,  // 18 远程TSAP访问资源 01-PG 02-OP 03-S7服务器
                0x00,  // 19 远程TSAP访问点
                0xC0,  // 20 TPDU数据长度 C0
                0x01,  // 21 代码长度 0x01
                0x09   // 22 TPDU TPDU缓存大小(代表1024字节)
        };
        /// <summary>
        /// S7握手指令
        /// </summary>
        private byte[] PLCHead2 = new byte[25]
        {

                0x03,//1 S7-ISO8075版本
                0x00,//2 备用
                0x00,//3
                0x19,//4 数据总长度
                0x02,//5 头长度
                0xF0,//6 PDU类型
                0x80,//7 00-无数据 80有数据
                0x32,//8 协议标识
                0x01,//9 用户ID
                0x00,//10 备用
                0x00,//11 备用
                0xFF,//12
                0xFF,//13 帧标识
                0x00,//14参数长度高位
                0x08,//15参数长度低位
                0x00,//16
                0x00,//17 数据长度
                0xF0,//18 功能
                0x00,//19备用
                0x00,//20
                0x03,//21 PAR任务1
                0x00,//22
                0x03,//23PAR任务2
                0x01,//PDU数据长度
                0x00
        };

        protected override bool CheckAddress(string addres)
        {
            bool Flat = false;
            string[] adr = Area(addres);
            if (adr[0] == "Q" || adr[0] == "DB"||adr[0]=="I" || adr[0] == "M" || adr[0] == "T" || adr[0] == "C")
            {
                Flat = true;
            }
            else
            {
                MessageBox.Show("通道不合法,支持I、Q、M、DB、T、C");
            }
            return Flat;
        }

        protected override string[] Area(string Address)
        {
            string[] ADDr = new string[2];
            if (Address.ToUpper().Contains("DB"))
            {
                ADDr[0] = "DB";
                ADDr[1] = Address.Substring(2);
            }
            else
            {
                ADDr[0] = Address.Substring(0, 1);
                ADDr[1] = Address.Substring(1);
            }
            return ADDr;
        }

        /// <summary>
        /// 用于西门子PLC握手校验的函数
        /// </summary>
        /// <param name="cb"></param>
        /// <returns></returns>
        protected override bool DoAfterConnect(CommBase cb)
        {
            CMDRETPLC hand1 = new CMDRETPLC(PLCHead1);
            GetRet(hand1);
            CMDRETPLC hand2 = new CMDRETPLC(PLCHead2);
            GetRet(hand2);
            if (hand1.Ret == null || hand2.Ret == null) return false;
            bool flat = (hand1.State == CmdState.ReadOK) && (hand2.State == CmdState.ReadOK);
            return flat;
        }

        /// <summary>
        /// 指令转换函数
        /// </summary>
        /// <param name="cmdRet">指令</param>
        /// <returns>字节数组</returns>
        protected override byte[] TranCmd(CMDRETPLC cmdRet)
        {
            string address = cmdRet.Address;
            string[] ADDr = new string[2] { "DB", "0.0" };
            if (cmdRet._CMDType != CMDType.WH_Hand)
                ADDr = Area(address);
            int len = cmdRet.Count;
            switch (cmdRet._CMDType)
            {
                case CMDType.RW: return BuidReadByteCommand(ADDr[1], ADDr[0], len);
                case CMDType.RB: return BuidReadBitCommand(ADDr[1], ADDr[0]);
                case CMDType.WW: return BuidWriteByteCommand(ADDr[1], cmdRet.DataBuff, ADDr[0]);
                case CMDType.WB: return BuidWriteBitCommand(ADDr[1], cmdRet.DataBuff, ADDr[0]);
                default: break;
            }

            return cmdRet.Cmd;
        }

        protected override CmdState CheckRet(CMDRETPLC cmdRet)
        {

            byte[] data = cmdRet.Ret;
            if (data != null && data.Length >= 20)
            {
                switch (cmdRet._CMDType)
                {
                    case CMDType.WH_Hand:
                        if (data.Length == 22 || data.Length == 27) cmdRet.State = CmdState.ReadOK;
                        break;
                    case CMDType.RW:
                        if (cmdRet.Ret[24] == cmdRet.Cmd[24] * 8 || cmdRet.Ret.Length == CmdRetIndex + cmdRet.Cmd[24]) cmdRet.State = CmdState.ReadOK;
                        break;
                    case CMDType.RB:
                        if (cmdRet.Ret[24] == 1) cmdRet.State = CmdState.ReadOK;
                        break;
                    case CMDType.WW:
                        if (data[data.Length - 1] == 0xFF) cmdRet.State = CmdState.ReadOK;
                        break;
                    case CMDType.WB:
                        if (data[data.Length - 1] == 0xFF) cmdRet.State = CmdState.ReadOK;
                        break;
                }

            }
            return cmdRet.State;
        }

        #endregion

        public void ReadDB(string Address,object a)
        {
            System.Type type = a.GetType();
            int count = 0;            
            foreach (PropertyInfo p in type.GetProperties())
            {
                object value = p.GetValue(a,null);
                if (p.Name != "Readcount")
                {
                    if (value.GetType() == typeof(long) || value.GetType() == typeof(ulong) || value.GetType() == typeof(double))
                    {
                        count += 8;
                    }
                    else if (value.GetType() == typeof(int) || value.GetType() == typeof(uint) || value.GetType() == typeof(float))
                    {
                        count += 4;
                    }
                    else if (value.GetType() == typeof(short) || value.GetType() == typeof(ushort)||value.GetType()== typeof(bool))
                    {
                        count += 2;
                    }
                    else if(value.GetType()==typeof(string))
                    {
                        count += 256;
                    }
                }

            }
            CMDRETPLC cmd = new CMDRETPLC(Address,CMDType.RW, count);
            GetRet(cmd, true);
            while(cmd.State!=CmdState.ReadOK)
            {
                Thread.Sleep(1);
            }
            if(cmd.State == CmdState.ReadOK)
            {
                byte[] buff = new byte[cmd.Ret.Length - CmdRetIndex];
                Array.Copy(cmd.Ret, CmdRetIndex, buff, 0, cmd.Ret.Length - CmdRetIndex);
                List<byte> Buff = buff.ToList();
                foreach (PropertyInfo p in type.GetProperties())
                {
                    object value = p.GetValue(this, null);
                    if (p.Name != "Readcount")
                    {
                        if (value.GetType() == typeof(double))
                        {
                            byte[] Temp = new byte[8];
                            Array.Copy(Buff.ToArray(), 0, Temp, 0, 8);
                            value = BitConverter.ToDouble(Vary.Vary8Byte(Temp, vary), 0);
                            Buff.RemoveRange(0, 8);
                        }
                        else if (value.GetType() == typeof(int))
                        {
                            byte[] Temp = new byte[4];
                            Array.Copy(Buff.ToArray(), 0, Temp, 0, 4);
                            value = BitConverter.ToInt32(Vary.Vary4Byte(Temp, vary), 0);
                            Buff.RemoveRange(0, 4);
                        }
                        else if (value.GetType() == typeof(uint))
                        {
                            byte[] Temp = new byte[4];
                            Array.Copy(Buff.ToArray(), 0, Temp, 0, 4);
                            value = BitConverter.ToUInt32(Vary.Vary4Byte(Temp, vary), 0);
                            Buff.RemoveRange(0, 4);
                        }
                        else if (value.GetType() == typeof(float))
                        {
                            byte[] Temp = new byte[4];
                            Array.Copy(Buff.ToArray(), 0, Temp, 0, 4);
                            value = BitConverter.ToSingle(Vary.Vary4Byte(Temp, vary), 0);
                            Buff.RemoveRange(0, 4);
                        }
                        else if (value.GetType() == typeof(short))
                        {
                            value = BitConverter.ToInt16(Buff.ToArray(), 0);
                            Buff.RemoveRange(0, 2);
                        }
                        else if (value.GetType() == typeof(ushort))
                        {
                            value = BitConverter.ToUInt16(Buff.ToArray(), 0);
                            Buff.RemoveRange(0, 2);
                        }
                        else if (value.GetType() == typeof(bool))
                        {
                            value = BitConverter.ToUInt16(Buff.ToArray(), 0) > 1 ? true : false;
                            Buff.RemoveRange(0, 2);
                        }
                    }
                }
            }

        }

        #region Siemens_PLC()
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="PLC_Type">PLC型号,默认S1200 支持S200、S300、S400、S1500</param>
        /// <param name="ip">PLC ip地址</param>
        /// <param name="name">PLC名称</param>
        public S7Net(string PLC_Type = "S1200", string ip = "", string name = "S7PLC") : base(name)
        {
            WordLenght = 2;
            CmdRetIndex = 25;
            Timeout = 500;
            vary = VaryType.reverse;
            Comm = new TCP(ip, 102);
            switch (PLC_Type)
            {
                case "S200":
                    PLCHead1[13] = 0x10;
                    PLCHead1[17] = 0x10;
                    break;
                case "S300":
                    PLCHead1[21] = 2;
                    break;
                case "S400":
                    PLCHead1[21] = 2; break;
                case "S1200": break;

                case "S1500":
                    PLCHead1[13] = 0x10;
                    PLCHead1[17] = 0x03;
                    PLCHead1[21] = 0; break;
            }
            Init();
        }

        #endregion
    }
}