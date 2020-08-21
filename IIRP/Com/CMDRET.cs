using IIRP.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IIRP.Com;
using System.Collections.Concurrent;

namespace IIRP
{
    /**************************** 此类用于存储发送或接收的指令**********************
     * 
     **********************************************************************************/

    /// <summary>
    ///指令的状态
    /// </summary>
    public enum CmdState
    {
        /// <summary>
        /// 空的指令
        /// </summary>
        None,
        /// <summary>
        /// 转换错误
        /// </summary>
        TranError,
        /// <summary>
        /// 发送成功
        /// </summary>
        SendOK,
        /// <summary>
        /// 发送失败
        /// </summary>
        SendNG,
        /// <summary>
        /// 接收成功
        /// </summary>
        ReadOK,
        /// <summary>
        /// 接收失败
        /// </summary>
        ReadNG,
        /// <summary>
        /// 解析成功
        /// </summary>
        TranOK,
        /// <summary>
        /// 解析失败
        /// </summary>
        TranNG
    };

    /// <summary>
    /// 读写模式
    /// </summary>
    public enum CMDType
    {

        RW,
        /// <summary>
        /// 按位读指令
        /// </summary>
        RB,
        /// <summary>
        /// 按字节写指令
        /// </summary>
        WW,

        /// <summary>
        /// 按位写指令
        /// </summary>
        WB,
        /// <summary>
        /// 写握手指令
        /// </summary>
        WH_Hand,
    };

    public class CMDRET
    {
        /// <summary>
        /// 执行成功的标志
        /// </summary>
        public bool IsSuccess = false;
        /// <summary>
        /// 要发送的命令
        /// </summary>
        public byte[] Cmd = null;

        public CmdState State = CmdState.None;
        /// <summary>
        /// 接收到的结果
        /// </summary>
        public byte[] Ret { get { return ReadBuffer?.ToArray(); } }

        /// <summary>
        /// 发送的时间
        /// </summary>
        public DateTime TimeSend = DateTime.Now;

        /// <summary>
        /// 接收的时间
        /// </summary>
        public DateTime TimeRead = DateTime.Now;


        /// <summary>
        /// 接收到的字节
        /// </summary>
        public List<byte> ReadBuffer = new List<byte>();

        /// <summary>
        /// 存放数据
        /// </summary>
        public byte[] DataBuff = null;

        /// <summary>
        /// 总的接收字节数量
        /// </summary>
        public int ReadMaxBytes { get { return ReadBuffer.Count; } }

        /// <summary>
        ///   byte型 cmd 命令 转换为 16进制字符串
        /// </summary>
        public virtual string CmdHexStr { get { return Vary.ByteToHexStr(Cmd); } }

        /// <summary>
        ///  byte型 ret 命令 转换为 16进制字符串
        /// </summary>
        public virtual string RetHexStr { get { return Vary.ByteToHexStr(Ret); } }

        /// <summary>
        ///  byte型 ret 命令 转换为 16进制字符串
        /// </summary>
        public virtual string RetStr { get { return Vary.ByteToStr(Ret); } }

        public CMDRET()
        {

        }
        public CMDRET(string cmdbyte)
        {
            Cmd = Encoding.ASCII.GetBytes(cmdbyte);
        }

        public CMDRET(byte[] Arraybyte)
        {
            Cmd = Arraybyte;
        }

        /// <summary>
        /// 清除所有（cmd不清）
        /// </summary>
        public virtual void Clear()
        {
            State = CmdState.None;
            TimeSend = DateTime.Now;
            TimeRead = DateTime.Now;
            ReadBuffer = new List<byte>();
        }
    }

    public class CMDRETPLC : CMDRET
    {
        /// <summary>
        /// PLC地址或标签名称
        /// </summary>
        public string Address = "";

        /// <summary>
        ///读取的数据长度
        /// </summary>
        public int Lenght = 1;

        /// <summary>
        /// 读取的PLC基本单位长度(西门子以字节为单位、欧姆龙、三菱等以字为单位)
        /// </summary>
        public int Count { get; set; } = 1;
        /// <summary>
        /// 数据类型
        /// </summary>
        public string DateType = typeof(bool).Name;

        /// <summary>
        /// 读写模式
        /// </summary>
        public CMDType _CMDType = CMDType.RW;

        public List<CMDRETPLC> ListCMDRET = new List<CMDRETPLC>();
    
        public object Result=null;

        /// <summary>
        /// 错误代码
        /// </summary>
        public byte ErrCode { get; set; } = 0;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrMessage = "";

        public bool Only = false;

        public CMDRETPLC(byte[] handle)
        {
            Cmd = handle;
            _CMDType = CMDType.WH_Hand;
        }

        public CMDRETPLC(string address, CMDType type, int lenght = 1)
        {
            Address = address;
            Lenght = lenght;
            _CMDType = type;
        }
        public CMDRETPLC(string address, string dateType, CMDType type, int lenght = 1)
        {
            Address = address;
            Lenght = lenght;
            DateType = dateType;
            _CMDType = type;
        }
        public CMDRETPLC(List<CMDRETPLC> cmd, CMDType type = CMDType.RW)
        {

            Address = cmd[0].Address;
            Lenght = cmd[0].Lenght;
            DateType = cmd[0].DateType;
            ListCMDRET = cmd;
            _CMDType = type;
        }

        public CMDRETPLC()
        {
        }
    }

    public class CMDRETPLC<T> : CMDRETPLC
    {
        public new T Result;
        public CMDRETPLC(string address, CMDType type, int lenght = 1)
        {
            Address = address;
            Lenght = lenght;
            _CMDType = type;
        }
    }

}