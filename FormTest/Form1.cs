using BLL;
using System;
using System.IO;
using System.Windows.Forms;

namespace FormTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2(this);
            f.Show();
            this.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataMove a = new DataMove();
            a.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                //string localPath = "D:\\焊接能量数据\\阳极";
                string PlocalPath = "D:\\焊接能量数据\\阴极";
                string savePath = "D:\\焊接能量数据\\测试";
                CopyFile(PlocalPath, savePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 文件复制
        /// </summary>
        /// <param name="localPath">源文件路径</param>
        /// <param name="savePath">目标文件路径</param>
        public void CopyFile(string localPath, string savePath)
        {
            try
            {
                if (savePath[savePath.Length - 1] != Path.AltDirectorySeparatorChar)//检查目标目录是否以分隔符结束，如果不是则添加
                {
                    savePath += Path.AltDirectorySeparatorChar;
                }
                if (!System.IO.Directory.Exists(savePath))//判断文件夹是否存在
                {
                    Directory.CreateDirectory(savePath);
                }
                string[] fileList = Directory.GetFileSystemEntries(localPath);//获取源文件
                foreach (string file in fileList)
                {
                    if (Directory.Exists(file))
                    {
                        CopyFile(file, savePath + Path.GetFileName(file));//如果是文件夹，递归执行
                    }
                    else
                    {
                        File.Copy(file, savePath + Path.GetFileName(file), true);//文件则进行复制，并且覆盖复制
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FormListView f = new FormListView();
            f.Show();
        }
    }
}
