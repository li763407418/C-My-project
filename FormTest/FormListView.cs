using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;

namespace FormTest
{
    public partial class FormListView : Form
    {
        private Color SetColor = Color.AliceBlue;
        const int CLOSE_SIZE = 12;
        public FormListView()
        {
            InitializeComponent();
        }

        private void FormListView_Load(object sender, EventArgs e)
        {

            DataTable dt = ViewLoadBLL.GetViewButton();                      //获取数据库中配置的菜单栏信息
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                Button bt = new Button();                                    //添加一级菜单栏button按钮
                this.panel_View.Controls.Add(bt);
                bt.Text = dt.Rows[i]["ViewName"].ToString();                //添加一级菜单栏名称
                //bt.Tag = dt.Rows[i]["ViewPage"].ToString();
                bt.MouseClick += new MouseEventHandler(button_MouseClick);  //添加菜单栏button点击事情
                bt.Size = new System.Drawing.Size(120, 40);
                bt.UseVisualStyleBackColor = true;
                bt.BringToFront();
                bt.Dock = DockStyle.Top;

                DataTable dtPage = ViewLoadBLL.GetViewPage(dt.Rows[i]["ParentView"].ToString());    //获取数据库中配置的二级菜单栏信息
                ListBox lb = new ListBox();
                this.panel_View.Controls.Add(lb);
                for (int j = 0; j < dtPage.Rows.Count; j++)
                {

                    lb.Items.AddRange(new object[]
                    {
                        dtPage.Rows[j]["ViewName"].ToString()           //向listbox中逐个添加item
                    });
                    lb.ItemHeight = 30;                                 //设置item的高度
                }

                lb.Size = new Size(201, dtPage.Rows.Count * 30 + 10);   //动态设置listbox的高度
                lb.BringToFront();
                lb.Dock = DockStyle.Top;
                lb.DrawMode = DrawMode.OwnerDrawVariable;
                lb.DrawItem += _listBox_DrawItem;                       //设置listbox中item名称的样式
                lb.MeasureItem += _listBox_MeasureItem;                 //无用处
                lb.SelectedIndexChanged += SelectedIndexChanged;        //当所选item发生变化时控制页面变化的方法
                lb.Visible = false;
            }

            this.tabControl1.TabPages.Clear();
            this.tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.tabControl1.Padding = new Point(CLOSE_SIZE, CLOSE_SIZE / 2);
            this.tabControl1.DrawItem += new DrawItemEventHandler(this.tabControl1_DrawClose);  //绘制关闭按钮
            this.tabControl1.MouseDown += new MouseEventHandler(this.tabControl1_MouseClink);   //关闭按钮事件

        }

        /// <summary>
        /// 点击一级菜单按钮时控制listbox显示或隐藏方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_MouseClick(object sender, MouseEventArgs e)
        {
            //因为创建的时候是一级菜单button之后紧跟着创建其二级菜单listbox，故用控件的tabIndex属性以找到一级菜单button对应的二级菜单listbox，改变其visible属性以控制其显示或隐藏
            Button button = (Button)sender;
            if (!button.Parent.GetNextControl(button, true).Visible)
                button.Parent.GetNextControl(button, true).Visible = true;
            else
                button.Parent.GetNextControl(button, true).Visible = false;
        }

        /// <summary>
        /// listbox所选的项发生变化时，控制tabControl1上页面跟随变化的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                bool status = true;             //页面是否存在标识

                ListBox listBox = (ListBox)sender;
                string pagePath = ViewLoadBLL.GetPagePath(listBox.SelectedItem.ToString()).Rows[0]["ViewPage"].ToString();  //获取对应页面的文件名称
                Assembly assembly = Assembly.LoadFile(Application.StartupPath + "\\FormTest.exe");              //加载FormTest.exe中的文件内容（用以获取其上的窗体对象）
                Type type = assembly.GetType(pagePath, false);          //获取对应的窗体对象
                Form form = (Form)Activator.CreateInstance(type);       //根据获取的窗体对象进行实例化
                //form.Show();
                form.TopLevel = false;
                form.Dock = DockStyle.Fill;
                form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                form.BackColor = Color.White;
                foreach (TabPage tabPage in tabControl1.TabPages)   //遍历tabControl1中是否已存在对应窗体，如果存在则让其选择显示
                {
                    if (tabPage.Text == form.Text)
                    {
                        status = false;
                        this.tabControl1.SelectedTab = tabPage;
                        return;
                        //formDataCollection.Show();
                    }
                }
                if (status)         //如果不存在，则将窗体添加进tabControl1中并显示
                {
                    TabPage page = new TabPage(form.Text);
                    page.Controls.Add(form);
                    tabControl1.TabPages.Add(page);
                    this.tabControl1.SelectedTab = page;
                    form.Show();
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }




        /// <summary>
        /// listbox中item的绘制方法，让其上文字居中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                ListBox lb = (ListBox)sender;
                e.DrawBackground();
                e.DrawFocusRectangle();

                System.Drawing.StringFormat strFmt = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoClip);
                strFmt.Alignment = System.Drawing.StringAlignment.Center;//水平居中
                strFmt.LineAlignment = StringAlignment.Center;//垂直居中

                RectangleF rf = new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

                e.Graphics.DrawString(lb.Items[e.Index].ToString(), e.Font, new System.Drawing.SolidBrush(e.ForeColor), rf, strFmt);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void _listBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 30;
        }


        /// <summary>
        /// 绘制tabControl上各页的关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_DrawClose(object sender, DrawItemEventArgs e)
        {
            try
            {
                Rectangle rectangle = this.tabControl1.GetTabRect(e.Index);
                e.Graphics.DrawString(this.tabControl1.TabPages[e.Index].Text, this.Font, SystemBrushes.ControlText, rectangle.X + 2, rectangle.Y + 2);

                using (Pen p = new Pen(Color.White))            //先画一个矩形
                {
                    rectangle.Offset(rectangle.Width - (CLOSE_SIZE + 3), 2);
                    rectangle.Width = CLOSE_SIZE;
                    rectangle.Height = CLOSE_SIZE;
                    e.Graphics.DrawRectangle(p, rectangle);
                }

                Color color = e.State == DrawItemState.Selected ? Color.White : Color.White;    
                using (Brush b = new SolidBrush(color))//给矩形填充颜色，我这边全是用的白色填充 
                {
                    e.Graphics.FillRectangle(b, rectangle);
                }

                using (Pen p = new Pen(Color.Black))    //在先前绘制的矩形内画对角线，绘制一个 x 图案出来
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

        /// <summary>
        /// 点击关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_MouseClink(object sender, MouseEventArgs e)
        {
            //判断鼠标点击时是否在所绘制的关闭按钮内，是则关闭对应页面，否则不做处理
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

        public void ShowMessage(string message)
        {
            textBox1.Text += message + "\r\n";
            textBox1.ScrollToCaret();
        }
    }
}
