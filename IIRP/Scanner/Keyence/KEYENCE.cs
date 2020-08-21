using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IIRP.Com;
namespace IIRP.Scanner
{
    /// <summary>
    /// 基恩士扫码枪
    /// </summary>
    public class KEYENCE : Scanner
    {
        /// <summary>
        /// 基于TCP的网口客户端实例
        /// </summary>
        /// <param name="Ip">Ip地址</param>
        /// <param name="port">端口号</param>
        /// <param name="_name">设备名称</param>
        public KEYENCE(string Ip,string port="9600",string _name = "基恩士扫码枪(网口)") : base(_name)
        {
            Comm = new TCP(Ip ,int.Parse(port),_name);
            ObjName = _name;
            Init();
        }
        /// <summary>
        /// 基于串口的客户端实例
        /// </summary>
        /// <param name="ComPort">串口号</param>
        /// <param name="Baudrate">波特率</param>
        /// <param name="Databit">数据位</param>
        /// <param name="Parity">校验位</param>
        /// <param name="StopBit">停止位</param>
        /// <param name="name">设备名称</param>
        public KEYENCE(string ComPort,int Baudrate,int Databit=8,string Parity="N",int StopBit=1,string name="基恩士扫码枪(串口)"):base(name)
        {
            Comm = new IirPSerialPort(ComPort, Baudrate, Databit, Parity, StopBit);
            ObjName = name;
            Init();
        }
        /// <summary>
        /// 校验函数
        /// </summary>
        /// <param name="CmdRet"></param>
        /// <returns></returns>
        protected override CmdState CheckRet(CMDRETPLC CmdRet)
        {
            if (CmdRet.RetStr.Contains("\r") && CmdRet.Ret != null && CmdRet.Ret.Length > 2)
            {
                BarCode = CmdRet.RetStr.Replace("\r", "").Trim();
                return CmdState.ReadOK;
            }
            else if (DateReceived_ON.S == "1")
            {
                return CmdState.ReadOK;
            }
            return CmdState.None;
        }

    }
}
