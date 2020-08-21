using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using BLL;

namespace FormTest
{
    public partial class DataMove : Form
    {
        public DataMove()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread move = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        bool status = true;
                        if (DateTime.Now.Hour == 10 || DateTime.Now.Hour == 18)
                        {
                            if (DateTime.Now.Minute == 18 && DateTime.Now.Second == 0 && status)
                            {
                                status = false;
                                if (MoveDataBLL.Get() > 0)
                                {
                                    MoveDataBLL.Move();
                                    Thread.Sleep(5000);
                                    MoveDataBLL.Delete();
                                }
                                status = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            });
            move.IsBackground = true;
            move.Start();
        }
    }
}
