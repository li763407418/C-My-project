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
    public partial class FormDataCollection : Form
    {
        public FormDataCollection()
        {
            InitializeComponent();
            this.dateTimePicker_Begin.Value = System.DateTime.Now.AddDays(-1);
            this.dateTimePicker_Begin.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker_End.CustomFormat = "yyyy-MM-dd HH:mm:ss";

        }

        private void button_Select_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.tbox_Code.Text != "")
                    this.dataGridView1.DataSource = DataInquireBLL.GetDataByCode(this.tbox_Code.Text);
                else
                    this.dataGridView1.DataSource = DataInquireBLL.GetDataByTime(dateTimePicker_Begin.Value.ToString(), dateTimePicker_End.Value.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
