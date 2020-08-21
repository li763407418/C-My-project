using IIRP.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;

namespace FormTest
{
    public partial class FormPLCSetting : Form
    {
        public static OmronCip Cip;
        //public static OmronCip Cip = new OmronCip("192.168.5.9",21,"PLC连接");
        public FormPLCSetting()
        {
            InitializeComponent();
            //Cip.Open();

            this.button1.BackColor = Color.Red;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            { 
                for(int i = 44818; i <= 44818; i++)
                {
                    Cip = new OmronCip("192.168.0.9", i, "PLC连接");
                    Cip.Open();
                    if (Cip.IsConnected)
                    {
                        this.button1.BackColor = Color.Green;
                        MessageBox.Show(i.ToString());
                    }
                    else
                    {
                        Cip.Close();
                    }
                    if (i == 44818)
                        MessageBox.Show("完事了");
                }

            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FormPLCSetting_Load(object sender, EventArgs e)
        {
            DataTable dataTable = PLCSettingBLL.GetPLCSetting();
            dataGridView1.DataSource = dataTable;
        }
    }
}
