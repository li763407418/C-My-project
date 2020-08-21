using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IIRP
{
    public partial class ShowSetting : UserControl
    {
        List<TabPage> listPage = new List<TabPage>();
        List<DataGridView> listDgv = new List<DataGridView>();
        List<string> listGrName = new List<string>();
        public ShowSetting()
        {
            InitializeComponent();
        }
        void initConfig()
        {
            try
            {
                foreach (DataGridView v in listDgv)
                {
                    v.Rows.Clear();
                }

                for (int i = 0; i < ValueBase.Valuelist.Count; i++)
                {
                    ValueBase v = ValueBase.Valuelist[i];

                    string GpName = v.Setion.Trim().Length <= 0 ? "未初始化的参数" : v.Setion;
                    int index = listGrName.IndexOf(GpName);
                    DataGridView dgv = null;
                    if (index == -1)
                    {
                        TabPage page = new TabPage();
                        dgv = new DataGridView();
                        dgv.ColumnCount = 3;
                        dgv.Columns[0].HeaderText = "名称";
                        dgv.Columns[1].HeaderText = "值";
                        dgv.Columns[2].HeaderText = "备注";
                        dgv.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                        dgv.Dock = DockStyle.Fill;
                        listPage.Add(page);
                        listGrName.Add(GpName);
                        listDgv.Add(dgv);
                        page.Controls.Add(dgv);
                        page.Text = GpName;
                        tabControl1.Controls.Add(page);
                        page.ResumeLayout(false);
                    }
                    else
                    {
                        dgv = listDgv[index];
                    }
                    dgv.RowCount = dgv.RowCount + 1;
                    int num = dgv.RowCount - 2;

                    dgv.Rows[num].Cells[0].Value = v.KeyName;
                    dgv.Rows[num].Cells[1].Value = v.S;
                    dgv.Rows[num].Cells[2].Value = v.Remark;

                    Application.DoEvents();
                }
                foreach (DataGridView dgv in listDgv)
                {
                    dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
                    dgv.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
                    dgv.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                    dgv.Columns[0].ReadOnly = true;
                    dgv.Columns[2].ReadOnly = true;

                    dgv.Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
                    dgv.Columns[2].DefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
                    dgv.Columns[1].DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.SkyBlue;
                    dgv.Columns[0].Width = 200;
                    dgv.Columns[1].Width = 150;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        void saveConfig()
        {
            try
            {

                for (int i = 0; i < listPage.Count; i++)
                {
                    DataGridView dgv = listDgv[i];
                    for (int k = 0; k < dgv.RowCount; k++)
                    {
                        foreach (ValueBase v in ValueBase.Valuelist)
                        {
                            if (dgv.Rows[k].Cells[0].Value == null) continue;
                            if (v.Setion == listPage[i].Text
                                && v.KeyName == dgv.Rows[k].Cells[0].Value.ToString())
                            {
                                v.S = (dgv.Rows[k].Cells[1].Value == null) ? "" : dgv.Rows[k].Cells[1].Value.ToString();
                            }
                        }
                    }
                }

                ValueBase.SaveConfig();
                MessageBox.Show("保存参数成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void ShowSetting_Load(object sender, EventArgs e)
        {
            initConfig();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveConfig();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ValueBase.INIConfig();
            initConfig();
        }
    }
}
