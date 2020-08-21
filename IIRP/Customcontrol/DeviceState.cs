using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using static IIRP.Customcontrol.InfoState;

namespace IIRP.Customcontrol
{
    /**************************************************************
     * 
     * 表示单个设备通信状态的控件
     **************************************************************/
    public partial class DeviceState : UserControl
    {
        public string objectname = "";
        public enum State { Red, Green, Gray, AliceBlue, Yellow }
        public DeviceState(string name)
        {
            InitializeComponent();
            label1.Text = name;
            objectname = name;
        }

        public void Ini(LEDState ls)
        {
            toolTip1.SetToolTip(this, ls.tips.S);
            toolTip1.SetToolTip(label1, ls.tips.S);
        }
        public void ChangeState(State s)
        {
            switch (s)
            {
                case State.Red:
                    if (this.BackColor != Color.Red)
                    {
                        this.BackColor = Color.Red;
                    }
                    label1.Text = objectname+"(连接失败)";
                    break;
                case State.Gray:
                    if (this.BackColor != Color.Gray)
                    {
                        this.BackColor = Color.Gray;
                    }
                    label1.Text =objectname+ "(未连接)";
                    break;
                case State.AliceBlue:
                    if (this.BackColor != Color.AliceBlue)
                    {
                        this.BackColor = Color.AliceBlue;
                    }
                    label1.Text = objectname+"(握手失败)";
                    break;
                case State.Yellow:
                    if (this.BackColor != Color.Yellow)
                    {
                        this.BackColor = Color.Yellow;
                    }

                    label1.Text = objectname+"(通讯NG)";
                    break;
                case State.Green:
                    if (this.BackColor != Color.SpringGreen)
                    {
                        this.BackColor = Color.SpringGreen;
                    }
                    label1.Text =objectname+ "(通讯OK)";
                    break;
            }
        }
    }
}