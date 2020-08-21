using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using IIRP.Com;
namespace IIRP.Scanner
{
    /// <summary>
    /// 扫码枪设备的基类
    /// </summary>
    public class Scanner : IirDeviceClient
    {
        #region Scanner

        public bool sendOK = false;
        public string BarCode = "";
        public bool Recevied_Flat = false;
        public ValueBase EndChar = new ValueBase("", "结束符", "", "读取串口数据的结束符号，一般为空，扫码枪常用 \r");
        public ValueBase DateReceived_ON = new ValueBase("", "DateReceived", "0", "开启串口的自动接收事件 1-开启 0-关闭");
        public ValueBase ReadSN = new ValueBase("", "读取条码指令", "LON", "控制 扫码枪扫码时，向扫码枪发送的控制指令");
        public ValueBase StopReadSN = new ValueBase("", "停止读取条码指令", "LOFF", "控制 扫码枪停止扫码时，向扫码枪发送的控制指令");
        public enum EventType { none, readSN, StopRead, Test };
        public EventType eventStep = EventType.none;

        public Scanner(string name) : base(name)
        {
            ObjName = name;
            Values.Add(EndChar);
            Values.Add(DateReceived_ON);
            Values.Add(ReadSN);
            Values.Add(StopReadSN);
            Init();
            DateReceived_ON.ValueChangedEvent += new ValueBase.ValueChangedHandler(DateReceived_ON_Ischanged);
        }

        public void ScanerDateIni()
        {
            try
            {
                if (Comm != null && Comm is IirPSerialPort && DateReceived_ON.S == "1")
                {
                    IirPSerialPort ISP = (IirPSerialPort)Comm;
                    ISP.serial.DataReceived += new SerialDataReceivedEventHandler(DateReceived);
                    Log.log("开启扫码枪DataReceived事件");
                }
                else if (Comm != null && Comm is IirPSerialPort && DateReceived_ON.S == "0")
                {
                    IirPSerialPort ISP = (IirPSerialPort)Comm;
                    ISP.serial.DataReceived -= new SerialDataReceivedEventHandler(DateReceived);
                    Log.log("关闭扫码枪DataReceived事件");
                }
            }
            catch (Exception ex)
            {
                ex.log("ScanerDateIni函数异常");
            }

        }

        public void DateReceived_ON_Ischanged(object sender, EventArgs e)
        {
            if (DateReceived_ON.ischanged)
            {
                ScanerDateIni();
            }
        }

        private void DateReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (Comm != null && Comm is IirPSerialPort)
                {
                    IirPSerialPort ISP = (IirPSerialPort)Comm;
                    int Count = ISP.serial.BytesToRead;
                    if (Count > 0)
                    {
                        BarCode = "";
                        if (EndChar.S != "")
                        {
                            BarCode = ISP.serial.ReadTo(EndChar.S.Trim());
                            Recevied_Flat = true;
                            Log.log("接收到串口反馈的条码:" + BarCode);

                        }
                        else
                        {
                            BarCode = ISP.serial.ReadExisting();
                            Log.log("接收到串口反馈的条码:" + BarCode);
                        }

                    }
                    //ISP.serial.DiscardInBuffer();
                }
            }
            catch (Exception ex)
            {
                Recevied_Flat = false;
                Log.log("扫码枪DateReceiverd事件异常" + ex.Message);
            }
        }

        protected override void Work()
        {
            if (IsConnected)
            {
                if (CmdList.Count > 0)
                {
                    CMDRETPLC CmdRet = CmdList[CmdList.Count - 1];
                    if (CmdRet != null)
                        _getRet(CmdRet);
                    lock (Lockcmd)
                    {
                        CmdList.Remove(CmdRet);
                    }
                }
            }
            else
            {
                scanStep = ActStep.Start;
            }
        }

        protected override bool DoAfterConnect(CommBase hc)
        {
            if (hc is IirPSerialPort && DateReceived_ON.S == "1")
            {
                ConnType = ConnectedType.RemoteOK;

            }
            return true;
        }
        #endregion
    }
}
