using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IIRP.Modbus;
using IIRP.Com;
using System.Windows.Forms;
namespace IIRP.Sockets
{
    /// <summary>
    /// 汇川AM系列基于ModBusTcp协议的通信驱动程序
    /// 软元件类型:Q-线圈     M-保持寄存器
    /// 功能描述:1.支持读写Q(如Q0.0-Q8192.7) M(0-65535) 
    ///          2.读方法 Read(string Address,DataType data,int lenght=1) 
    ///          参数:
    ///          起始地址:address
    ///          数据类型枚举 DataType
    ///          连续读取的长度 lenght(数组和字符串需要指定长度)
    ///          3.写方法 Write(string address,object data) 
    ///          参数:
    ///          起始地址:address
    ///          写入的数据 :支持 Bool、int16,uint16、int32、uint32、float、double,以及数组、字符串等
    /// </summary>
    /// <remarks></remarks>
    public class VanceNet_AM : ModBusTcp
    {
        protected override byte GetReadFuntionCode(string address, string Area, out string Newaddress)
        {
            byte FuntionCode = 0x01;
            Newaddress = address;
            switch (Area.ToUpper())
            {
                case "Q":
                    if (address.IndexOf('.') > 0)
                    {
                        string[] Adr = address.Split('.');
                        Newaddress = (int.Parse(Adr[0]) * 8 + int.Parse(Adr[1])).ToString();
                    }
                    else
                    {
                        Newaddress = (int.Parse(address) * 8).ToString();
                    }                  
                    break;
                case "M":
                    FuntionCode = 0x03;
                    break;
            }
            return FuntionCode;
        }
        protected override byte GetWriteFuntionCode(string address,string Area, int lenght,out string Newaddress)
        {
            byte FuntionCode = 0x05;
            Newaddress = address;
            switch (Area.ToUpper())
            {
                case "Q":
                    if (address.IndexOf('.') > 0)
                    {
                        string[] Adr = address.Split('.');
                        Newaddress = (int.Parse(Adr[0]) * 8 + int.Parse(Adr[1])).ToString();
                    }
                    else
                    {
                        Newaddress = (int.Parse(address) * 8).ToString();
                    }
                    if (lenght == 1) FuntionCode = 0x05; else FuntionCode = 0x0F;
                    break;
                case "M":
                    if (lenght == 1) FuntionCode = 0x06; else FuntionCode = 0x10;
                    break;
            }
            return FuntionCode;
        }

        protected override bool CheckAddress(string add)
        {
            bool Flat = false;
            string[] adr = Area(add);
            if (adr[0] == "Q" || adr[0] == "M")
            {
                float address = float.Parse(adr[1]);
                if (address >= 0 && (address <= 8192 && adr[0] == "Q" || address <= 65535 && adr[0] == "M"))
                {
                    Flat = true;
                }
                else
                {
                    if (adr[0] == "Q")
                        MessageBox.Show("地址不合法,范围0-8192.7");
                    else
                        MessageBox.Show("地址不合法,范围0-65535");
                }
            }
            else
            {
                MessageBox.Show("通道不合法,支持Q-线圈 M-保持寄存器");
            }
            return Flat;
        }

        public VanceNet_AM(string Ip = "192.168.0.1", string port = "502", string name = "VanceNet_AM") : base(name)
        {
            Station = 0;
            Timeout = 500;
            vary = VaryType.Single;
            CmdRetIndex = 9;
            Comm = new TCP(Ip, int.Parse(port), name);
            Init();
        }
    }
}
