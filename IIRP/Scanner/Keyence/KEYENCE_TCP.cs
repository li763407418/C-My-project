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
    public class KEYENCE_TCP : Scanner
    {
        public KEYENCE_TCP(string Ip,string port="9600",string _name = "基恩士扫码枪") : base(_name)
        {
            Comm = new TCP(Ip ,int.Parse(port),_name);
            ObjName = _name;
            Init();
        }

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
