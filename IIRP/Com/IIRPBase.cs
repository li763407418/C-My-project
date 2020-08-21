using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
using IIRP.Com;
namespace IIRP
{
    /// <summary>
    /// 枚举动作执行步骤
    /// </summary>
    public enum ActStep { Start, Act1, Act2, Act3, Act4, Act5, Act6, Act7, Act8, Act9, End };
    
    public delegate void Threadscan();

    public static class IIRSet
    {
        public static string ConfigFileName = System.Environment.CurrentDirectory + "\\config.ini";

        public static int mainThreadId = -1;

        public static string dirDevicelog = Log.Dirpath.S + "/DeviceLog";
        public static string dirLog = Log.Dirpath.S + "/Log";
        public static string dirExLog = Log.Dirpath.S + "/ExcePitionLog";
        public static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;

        /// <summary>
        /// 主线程处理消息队列或者延迟线程执行
        /// </summary>
        /// <param name="i"></param>
        public static void Dly(int i)
        {
            if (IsMainThread) Application.DoEvents();
            else Thread.Sleep(i);
        }

        /// <summary>
        /// 启动所有设备、启动线程扫描、启动日志
        /// </summary>
        public static void IniSystem()
        {
            Log.log("程序开始初始化！", "MainApplication");
            CreatDirFile();
            IirUiEventTimer.timer(100).addEventHandler(new EventHandler(Log.UpdateLog));
            IirUiEventTimer.startAll();
            IirScan.StartScanAll();
        }


        public static void log(this string content, string name = "")
        {
            Log.log(content, name);
        }
        public static void log(this Exception e, string name = "")
        {
            Log.log(e, name);
        }

        /// <summary>
        /// 创建日志文件夹，可以指定创建某个位置的文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void CreatDirFile(string path="")
        {
            if (!Directory.Exists(Log.Dirpath.S)) Directory.CreateDirectory(Log.Dirpath.S);
            if (!Directory.Exists(dirLog)) Directory.CreateDirectory(dirLog);
            if (!Directory.Exists(dirDevicelog)) Directory.CreateDirectory(dirDevicelog);
            if (!Directory.Exists(dirExLog)) Directory.CreateDirectory(dirExLog);
            if (path!="")
            {
                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        /// <summary>
        /// 删除文件夹下的子文件
        /// </summary>
        /// <param name="fileDirect">文件夹路径</param>
        /// <param name="SaveDay">保存天数</param>
        private static void DeleteFile(string fileDirect, int saveDay)
        {
            DateTime nowTime = DateTime.Now;
            DirectoryInfo root = new DirectoryInfo(fileDirect);
            DirectoryInfo[] dics = root.GetDirectories();//获取文件夹

            FileAttributes attr = File.GetAttributes(fileDirect);
            if (attr == FileAttributes.Directory)//判断是不是文件夹
            {
                foreach (DirectoryInfo file in dics)//遍历文件夹
                {
                    TimeSpan t = nowTime - file.CreationTime;  //当前时间  减去 文件创建时间
                    int day = t.Days;
                    if (day > saveDay)   //保存的时间 ；  单位：天
                    {
                        Directory.Delete(file.FullName, true);  //删除超过时间的文件夹
                    }
                }
            }
        }

    }

    #region 自定义存储参数和参数列表
    /// <summary>
    /// 基础的存储参数类型
    /// </summary>
    public class ValueBase : Object
    {
        public delegate void ValueChangedHandler(object sender,EventArgs e);
        public event ValueChangedHandler ValueChangedEvent = null;//值变化引发的事件

        private string _Value = "";
        private string _KeyName = "";
        private string _Setion = "";
        private string _Remark = "";
        private string _OleValue = "";
        public bool ischanged = false;

        /// <summary>
        /// 参数字符串形式
        /// </summary>
        public string S
        {
            get
            {
                return _Value;
            }
            set
            {

                this._OleValue = this._Value;
                _Value = value;
                CheckChanged();
            }
        }

        /// <summary>
        /// 参数的旧值
        /// </summary>
        public string OleValue
        {
            get
            {
                return _OleValue;
            }

        }

        /// <summary>
        /// 参数的Double值
        /// </summary>
        public double D
        {
            get
            {
                return Convert.ToDouble(_Value);
            }
            set
            {
                this._OleValue = this._Value;
                _Value = value.ToString();
                CheckChanged();
            }
        }
        /// <summary>
        /// 参数的int值
        /// </summary>
        public int I
        {
            get
            {
                return Convert.ToInt32(_Value);
            }
            set
            {
                this._OleValue = this._Value;
                _Value = value.ToString();
                CheckChanged();
            }
        }
        /// <summary>
        ///参数的布尔值
        /// </summary>
        public bool B
        {
            get
            {
                string v1 = S.Trim().ToLower();
                return (v1 == "0" || v1 == "f" || v1 == "false") ? false : true;
            }
            set
            {
                this._OleValue = this._Value;
                _Value = value ? "1" : "0";
                CheckChanged();
            }
        }
        public string KeyName
        {
            get
            {
                return _KeyName;
            }
            set
            {
                _KeyName = value;
            }
        }
        public string Setion
        {
            get
            {
                return _Setion;
            }
            set
            {
                _Setion = value;
            }
        }
        public string Remark
        {
            get
            {
                return _Remark;
            }
            set
            {
                _Remark = value;
            }
        }
        /// <summary>
        /// 参数列表
        /// </summary>
        public static List<ValueBase> Valuelist = new List<ValueBase>();

        public ValueBase(string value)
        {
            this.S = value;
        }

        /// <summary>
        /// 初始化参数
        /// </summary>
        /// <param name="setion">节点</param>
        /// <param name="key">键名称</param>
        /// <param name="value">值</param>
        /// <param name="remark">备注</param>
        public ValueBase(string setion, string key, string value, string remark = null)
        {
            this.Setion = setion;
            this.KeyName = key;
            this.S = value;
            this.Remark = remark;
            Read();
            Valuelist.Add(this);
            this.ValueChangedEvent += new ValueChangedHandler(ValuechangeEvent);
        }

        /// <summary>
        /// 从配置文件加载单个参数的方法
        /// </summary>
        public void Read()
        {
            if (this.KeyName == "" || this.Setion == "") return;
            try
            {
                this.S = INI.INIGetStringValue(IIRSet.ConfigFileName, Setion, KeyName, _Value);
            }
            catch (Exception ex)
            {
                Log.log(ex.Message);
            }
        }

        /// <summary>
        /// 保存单个参数到配置文件的方法
        /// </summary>
        public void Save()
        {
            if (KeyName == "" || Setion == "") return;
            try
            {
                INI.INIWriteValue(IIRSet.ConfigFileName, Setion, KeyName, _Value);
            }
            catch (Exception ex)
            {
                Log.log(ex.Message);
            }
        }

        /// <summary>
        /// 值发生改变发生的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ValuechangeEvent(object sender, EventArgs e)
        {
            Save();
        }
        private void CheckChanged()
        {
            if (_Value != _OleValue)
            {
                ischanged = true;
                ValueChangedEvent?.Invoke(this, new EventArgs());
            }
            else
            {
                ischanged = false;
            }
        }
        public static void INIConfig()
        {
            foreach (var a in Valuelist)
            {
                a.Read();
            }
        }
        public static void SaveConfig()
        {
            foreach (var a in Valuelist)
            {
                a.Save();
            }
        }
        /// <summary>
        /// 详细参数设置界面
        /// </summary>
        /// <param name="width">窗体宽度</param>
        /// <param name="height">窗体高度</param>
        /// <param name="exec">是否以对话框的形式生成界面</param>
        public static void ShowSetting(int width = 800, int height = 600, bool exec = false)
        {
            Form f = new Form();
            f.Text = "详细参数配置";
            f.Width = width;
            f.Height = height;
            ShowSetting set = new ShowSetting();
            f.Controls.Add(set);
            set.Dock = DockStyle.Fill;
            if (exec)
            {
                f.ShowDialog();
            }
            else
            {
                f.Show();
            }

        }

    }

    /// <summary>
    /// 可保存参数的集合
    /// </summary>
    public class ValueList
    {
        public List<ValueBase> listValue = new List<ValueBase>();
        public void Add(ValueBase v)
        {
            listValue.Add(v);
        }
        public void Remove(ValueBase v)
        {
            listValue.Remove(v);
        }

        public void Dispose()
        {
            listValue.Clear();
        }

        public void SetSetionName(string setion)
        {
            foreach (ValueBase v in listValue)
            {
                v.Setion = setion;
                v.Read();
            }
        }
    }

    #endregion

    /// <summary>
    /// 扫描线程列表,启动所有动作线程
    /// </summary>
    public class IirScan
    {
        public ActStep step = ActStep.Start;
        public static bool ThreadQuit = false;
        /// <summary>
        /// 确定是否需要1ms的延时等待 默认是有1ms扫描间隔的
        /// </summary>
        public bool sleep1msFlag = true;

        public static List<IirScan> ThreadList = new List<IirScan>();

        public static void StartScanAll()
        {
            foreach (IirScan t in IirScan.ThreadList)
            {
                t.StartScan();
            }
        }
        public static void CloseAll()
        {
            ThreadQuit = true;
        }

        public Threadscan MyRun = null;


        public IirScan()
        {
            ThreadList.Add(this);
            MyRun += Work;
        }
        public IirScan(Threadscan f)
        {
            ThreadList.Add(this);
            MyRun += Work;
            MyRun += f;
        }

        public static IirScan CreateScan(Threadscan f)
        {
            IirScan b = new IirScan(f);
            return b;
        }
        public static IirScan CreateScan(Threadscan[] f)
        {
            IirScan b = new IirScan();
            foreach (Threadscan t in f)
            {
                b.MyRun += t;
            }
            return b;
        }
        public IirScan addscan(Threadscan f)
        {
            MyRun += f;
            return this;
        }

        public virtual void Run()
        {
            IsStarted = true;
            Log.log("Iirscan 线程开始");
            while (!IirScan.ThreadQuit)
            {
                try
                {
                    MyRun();
                }
                catch (Exception e)
                {
                    Log.log(e.ToString());
                }
                Thread.Sleep(1);
            }
            Log.log("Iirscan 线程结束");
            IsStarted = false;
        }

        public virtual void Work()
        {

        }

        public bool IsStarted = false;
        public void StartScan()
        {
            if (IsStarted) return;
            ThreadQuit = false;
            Thread th = new Thread(Run)
            {
                IsBackground = true
            };
            th.Start();
            IsStarted = true;
        }
    }
}