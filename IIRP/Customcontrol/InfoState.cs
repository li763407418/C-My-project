using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IIRP.Com;

namespace IIRP.Customcontrol
{
    /**************************************************************
     * 
     * 监控所有设备通信状态的控件
     **************************************************************/
    public partial class InfoState : UserControl
    {
        public InfoState()
        {
            InitializeComponent();
            this.AddDeviceStateEvent += new AddDeviceStateHandler(AddDeviceState);
        }
        public bool IsLoad = false;//设备是否加载
        public  delegate void AddDeviceStateHandler(object sender, EventArgs e);
        public event AddDeviceStateHandler AddDeviceStateEvent =null;

        /// <summary>
        /// 初始化通信状态
        /// </summary>
        public void INI()
        {
            AddDeviceStateEvent?.Invoke(this, new EventArgs());
        }
        public void AddDeviceState(object sender,EventArgs e)
        {
            foreach (var a in IirPDevice.listDevice)
            {
                DeviceState d = new DeviceState(a.ObjName);
                a.led = new LEDState(a,d);
                d.Ini(a.led);
                flowLayoutPanel1.Controls.Add(d);
                IsLoad = true;
            }
        }
        public class LEDState
        {
            public static List<LEDState> lslist = new List<LEDState>();
            IirPDevice Device = null;
            public  ValueBase tips = new ValueBase("未连接");
            public bool IsDevice = false;
            DeviceState Ds = null;
            public LEDState(IirPDevice d,DeviceState ds)
            {
                d.led = this;
                Device = d;
                tips.S = d.tips.S;
                IsDevice = true;
                Ds = ds;
                lslist.Add(this);
            }
            public static void UpdateLedState()
            {
                foreach(LEDState ls in lslist)
                {
                    ls.UpdateUi();
                }
            }
            public void UpdateUi()
            {
                try
                {
                    if (IsDevice)
                    {
                        
                        switch (Device.ConnType)
                        {
                            case ConnectedType.None: Ds.ChangeState(DeviceState.State.Gray); break;
                            case ConnectedType.LocalNG: Ds.ChangeState(DeviceState.State.Red); break;
                            case ConnectedType.LocalOK: Ds.ChangeState(DeviceState.State.AliceBlue); break;
                            case ConnectedType.RemoteNG: Ds.ChangeState(DeviceState.State.Yellow); break;
                            case ConnectedType.RemoteOK: Ds.ChangeState(DeviceState.State.Green); break;
                        }
                    }
                }
                catch (Exception ex)
                {

                    ex.log();
                }
            }
        }

        public void timer1_Tick(object sender, EventArgs e)
        {
            if(!IsLoad)
            {
                INI();
            }
            LEDState.UpdateLedState();
        }
    }
}
