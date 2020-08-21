using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IIRP.Com;
namespace IIRP.Sockets.Omron
{
    public  class OmronHostLink:PLCBase
    {
        #region OmronHostLink
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Com">串口号、COM1、COM2等</param>
        /// <param name="port">波特率 4800、9600、19200、38400、,57600,115200</param>
        /// <param name="databit">数据位 5、6、7、8</param>
        /// <param name="Parity">校验位 N-无校验 E:偶校验 O:奇校验 </param>
        /// <param name="name">设备名称</param>
        public OmronHostLink(string Com, int port , int databit,string Parity,int stopbit, string name = "OmronHostLinkPLC") : base(name)
        {
            CmdRetIndex = 30;
            WordLenght = 1;
            vary = VaryType.Double;
            ObjName = name;
            Comm = new IirPSerialPort(Com, port,databit,Parity,stopbit,name);
            Init();
        }
        #endregion
    }
}
