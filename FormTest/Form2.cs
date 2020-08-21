using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormTest
{
    public partial class Form2 : Form
    {
        const int CLOSE_SIZE = 12;
        public Form1 form1;
        public Form2(Form1 f)
        {
            InitializeComponent();
            form1 = f;
            this.tabControl1.TabPages.Clear();
            this.tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.tabControl1.Padding = new Point(CLOSE_SIZE, CLOSE_SIZE / 2);
            this.tabControl1.DrawItem += new DrawItemEventHandler(this.tabControl1_DrawClose);
            this.tabControl1.MouseDown += new MouseEventHandler(this.tabControl1_MouseClink);
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            form1.Close();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            bool status = true;
            FormDataCollection formDataCollection = new FormDataCollection();
            formDataCollection.TopLevel = false;
            formDataCollection.Dock = DockStyle.Fill;
            formDataCollection.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                if (tabPage.Text == formDataCollection.Text)
                {
                    status = false;
                    this.tabControl1.SelectedTab = tabPage;
                    return;
                    //formDataCollection.Show();
                }
            }
            if (status)
            {
                TabPage page = new TabPage(formDataCollection.Text);
                page.Controls.Add(formDataCollection);
                tabControl1.TabPages.Add(page);
                this.tabControl1.SelectedTab = page;
                formDataCollection.Show();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            bool status = true;
            FormPLCSetting formPLCSetting = new FormPLCSetting();
            formPLCSetting.TopLevel = false;
            formPLCSetting.Dock = DockStyle.Fill;
            formPLCSetting.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            //tabControl1.TabPages.Add(formDataCollection);

            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                if (tabPage.Text == formPLCSetting.Text)
                {
                    status = false;
                    this.tabControl1.SelectedTab = tabPage;
                    return;
                    //formPLCSetting.Show();
                }
            }
            if (status)
            {
                TabPage page = new TabPage(formPLCSetting.Text);
                page.Controls.Add(formPLCSetting);
                tabControl1.TabPages.Add(page);
                this.tabControl1.SelectedTab = page;
                formPLCSetting.Show();
            }
        }

        private void tabControl1_DrawClose(object sender, DrawItemEventArgs e)
        {
            try
            {
                Rectangle rectangle = this.tabControl1.GetTabRect(e.Index);
                e.Graphics.DrawString(this.tabControl1.TabPages[e.Index].Text, this.Font, SystemBrushes.ControlText, rectangle.X + 2, rectangle.Y + 2);

                using (Pen p = new Pen(Color.White))
                {
                    rectangle.Offset(rectangle.Width - (CLOSE_SIZE + 3), 2);
                    rectangle.Width = CLOSE_SIZE;
                    rectangle.Height = CLOSE_SIZE;
                    e.Graphics.DrawRectangle(p, rectangle);
                }

                Color color = e.State == DrawItemState.Selected ? Color.White : Color.White;
                using (Brush b = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(b, rectangle);
                }

                using (Pen p = new Pen(Color.Black))
                {
                    Point p1 = new Point(rectangle.X + 3, rectangle.Y + 3);
                    Point p2 = new Point(rectangle.X + rectangle.Width - 3, rectangle.Y + rectangle.Height - 3);
                    e.Graphics.DrawLine(p, p1, p2);

                    Point p3 = new Point(rectangle.X + 3, rectangle.Y + rectangle.Height - 3);
                    Point p4 = new Point(rectangle.X + rectangle.Width - 3, rectangle.Y + 3);
                    e.Graphics.DrawLine(p, p3, p4);
                }
                e.Graphics.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void tabControl1_MouseClink(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = e.X, y = e.Y;
                Rectangle rectangle = this.tabControl1.GetTabRect(this.tabControl1.SelectedIndex);

                rectangle.Offset(rectangle.Width - (CLOSE_SIZE + 3), 2);
                rectangle.Width = CLOSE_SIZE;
                rectangle.Height = CLOSE_SIZE;

                bool status = x > rectangle.X && x < rectangle.Right
                    && y > rectangle.Y && y < rectangle.Bottom;

                if (status)
                {
                    this.tabControl1.TabPages.Remove(this.tabControl1.SelectedTab);
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            UploadMES a = new UploadMES();
            a.Show();
        }
    }
}
