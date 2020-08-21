using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IIRP.Modbus;
using IIRP.Com;
namespace IIRP.Sockets
{

    /// <summary>
    /// 汇川H系列 基于ModBusTcp通讯
    /// </summary>
    /// <remarks>支持功能码01、03、05、06、15、16</remarks>
    public class VanceNet_H : ModBusTcp
    {
        /// <summary>
        /// 起始地址偏移
        /// </summary>
        public enum Aears
        {
            M = 0,
            M1=0x1F40,
            S = 0xE000,
            T = 0xF000,
            C = 0xF400,
            C1 = 0xF700,
            X = 0xF800,
            Y = 0xFC00,
            D = 0,
        }

        protected byte[] ReadCommand(CMDRETPLC cmd)
        {
            byte FuntionCode = 0x01;
            string[] address = Area(cmd.Address);
            ushort add = Convert.ToUInt16(address[1]);
            if (cmd._CMDType == CMDType.RW)
            {
                FuntionCode = 0x03;
                switch (address[0].ToUpper())
                {
                    case "T":
                        add += (ushort)Aears.T;
                        break;
                    case "C":
                        if (add >= 0 && add < 200)
                            add += (ushort)Aears.C;
                        else
                        {
                            add += (ushort)Aears.C1;
                            cmd.Lenght = cmd.Lenght * 2;
                        }                           
                        break;
                }
            }
            else
            {
                switch (address[0].ToUpper())
                {
                    case "S":
                        add += (ushort)Aears.S;
                        break;
                    case "T":
                        add += (ushort)Aears.T;
                        break;
                    case "C":
                        add += (ushort)Aears.C;
                        break;
                    case "X":
                        add += (ushort)Aears.X;
                        break;
                    case "Y":
                        add += (ushort)Aears.Y;
                        break;
                }
            }
            List<byte> Buff = new List<byte>();
            Buff.AddRange(BitConverter.GetBytes(MessageIndex).Reverse());//消息号
            Buff.AddRange(new byte[4] { 0, 0, 0, 6 });// Modbus-标志号 0x0000,后面指令的长度最后赋值
            Buff.Add(Station);//站号
            Buff.Add(FuntionCode); //功能码
            Buff.AddRange(BitConverter.GetBytes(add).Reverse());//地址
            Buff.AddRange(BitConverter.GetBytes(cmd.Lenght).Reverse());
            return Buff.ToArray();
        }

        protected byte[] WriteCommand(CMDRETPLC cmd)
        {
            byte FuntionCode = 0x05;
            string[] address = Area(cmd.Address);
            ushort add = Convert.ToUInt16(address[1]);
            if (cmd._CMDType == CMDType.WW)
            {
                
                FuntionCode = 0x06;
                switch (address[0].ToUpper())
                {
                    case "T":
                        add += (ushort)Aears.T;
                        break;
                    case "C":
                        if (add >= 0 && add < 200)
                            add += (ushort)Aears.C;
                        else
                        {
                            add += (ushort)Aears.C1;
                            cmd.Lenght = cmd.Lenght * 2;
                        }
                        break;
                }
            }
            else
            {
                switch (address[0].ToUpper())
                {
                    case "S":
                        add += (ushort)Aears.S;
                        break;
                    case "T":
                        add += (ushort)Aears.T;
                        break;
                    case "C":
                        add += (ushort)Aears.C;
                        break;
                    case "X":
                        add += (ushort)Aears.X;
                        break;
                    case "Y":
                        add += (ushort)Aears.Y;
                        break;
                }
            }
            List<byte> Buff = new List<byte>();
            Buff.AddRange(BitConverter.GetBytes(MessageIndex).Reverse());//消息号
            Buff.AddRange(new byte[4] { 0, 0, 0, 6 });// Modbus-标志号 0x0000,后面指令的长度最后赋值
            Buff.Add(Station);//站号
            Buff.Add(FuntionCode); //功能码
            Buff.AddRange(BitConverter.GetBytes(add).Reverse());//地址
            Buff.AddRange(BitConverter.GetBytes(cmd.Lenght).Reverse());
            return Buff.ToArray();
        }

        

        public VanceNet_H(string Ip = "192.168.0.1", string port = "502", byte station = 1, string name = "VanceNet") : base(name)
        {
            Station = station;
            Timeout = 500;
            vary = VaryType.Single;
            CmdRetIndex = 9;
            Comm = new TCP(Ip, int.Parse(port), name);
            Init();
        }
    }
}
