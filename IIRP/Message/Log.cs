using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace IIRP
{
    /// <summary>
    /// 此类用于读写日志
    /// </summary>
    public class Log
    {

        public static ValueBase Dirpath = new ValueBase("System", "日志存放位置", "D:/HistoryData","存放历史日志的根目录");
        public static ValueBase ClearDay = new ValueBase("System", "清理*天前的日志", "30", "单位(天),例如清理30天前的日志");
        public static ValueBase MaxLog = new ValueBase("System", "日志缓冲区大小", "1000");
        public static List<Log> loglist = new List<Log>();
        public static List<List<Log>> LogUi = new List<List<Log>>();
        public static ListBox listbox = null;
        public DateTime time = DateTime.Now;

        public string Name = "";
        public string Content = "";
        public int threadID = -1;
        public static object Locklog = new object();
        public static IirScan scan = IirScan.CreateScan(ScanSaveLog);

        /// <summary>
        /// 初始化方法
        /// </summary>
        /// <param name="name">打印日志的位置</param>
        /// <param name="content">打印日志的内容</param>
        public Log(string content, string name)
        {
            threadID = Thread.CurrentThread.ManagedThreadId;
            time = DateTime.Now;
            Content = content;
            Name = name;
            loglist.Add(this);
            if (loglist.Count > MaxLog.I)
            {
                loglist.Clear();
            }
        }
        public static void log(string content, string name = "")
        {
            TimerBase t = new TimerBase();
            try
            {
                if (Locklog != null)
                {
                    lock (Locklog)
                    {
                        Log l = new Log(content, name);
                    }
                }
                else
                {
                    Log l = new Log(content, name);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void log(Exception e, string name = "")
        {
            TimerBase t = new TimerBase();
            try
            {
                if (Locklog != null)
                {
                    lock (Locklog)
                    {
                        Log l = new Log(e.Message, name);
                    }
                }
                else
                {
                    Log l = new Log(e.Message, name);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 日志字符串格式化
        /// </summary>
        /// <returns></returns>
        public string LogFormat()
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss.fff ->") + Name + ":" + Content;
        }

        /// <summary>
        /// 更新日志到界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void UpdateLog(object sender, EventArgs e)
        {
            try
            {
                if (LogUi.Count == 0) return;

                while (LogUi.Count > 0)
                {
                    List<Log> list = LogUi.First();
                    lock (Locklog)
                    {
                        LogUi.Remove(list);
                    }
                    if (listbox != null)
                    {
                        foreach (Log l in list)
                        {
                            listbox.Items.Add(l.LogFormat());
                            if (listbox.Items.Count > 1000) listbox.Items.RemoveAt(0);
                        }
                        if (listbox.Items.Count > 2)
                            listbox.SelectedIndex = listbox.Items.Count - 1;
                    }
                }
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        /// <summary>
        /// 绑定listbox
        /// </summary>
        /// <param name="listBox1"></param>
        public static void bind(ListBox listBox1)
        {
            listbox = listBox1;
            listbox.MouseDoubleClick += new MouseEventHandler(logDoubleClick);

        }

        public static TimerBase t = new TimerBase();

        /// <summary>
        /// 定期清除文件夹数据
        /// </summary>
        /// <param name="path">文件夹路径</param>
        public static void DeleteFile(string path,int clrearday)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    DirectoryInfo info = new DirectoryInfo(path);
                    foreach (var file in info.GetFiles())
                    {
                        DateTime Time = file.CreationTime;
                        if ((DateTime.Now-Time).TotalDays>clrearday)
                        {
                            file.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.log("定时清除文件异常" + ex.Message);
            }
        }

        /// <summary>
        /// 定期清除文件夹
        /// </summary>
        /// <param name="path">文件夹路径</param>
        public static void DeleteFileOlder(string path, int clrearday)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    DirectoryInfo info = new DirectoryInfo(path);
                    foreach (var file in info.GetDirectories())
                    {
                        DateTime Time = file.CreationTime;                       
                        if ((DateTime.Now - Time).TotalDays > clrearday)
                        {
                            foreach(var fi in file.GetFiles())
                            {
                                fi.Delete();
                            }
                            file.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.log("定时清除文件异常" + ex.Message);
            }
        }
        /// <summary>
        /// 日志保存函数
        /// </summary>
        public static void ScanSaveLog()
        {
            if (t.D > 100 || loglist.Count > 1000)
            {
                if (loglist.Count > 0)
                {
                    if (t.D < 10 && loglist.Count > 900) return;
                    List<Log> list = new List<Log>();
                    t.D = 0;
                    lock (Locklog)
                    {
                        list = loglist;
                        loglist = new List<Log>();
                    }

                    string fn = IIRSet.dirLog + "/log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    try
                    {
                        FileStream fs = new FileStream(fn, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

                        if (!File.Exists(fn))
                        {
                            sw.WriteLine(fn, "================header=============");
                        }
                        foreach (Log d in list)
                        {
                            sw.WriteLine(d.time.ToString("yyyy-MM-dd HH:mm:ss.fff") + "--->(" + d.threadID.ToString() + ")" + d.Name + ":" + d.Content);
                        }

                        sw.Close();
                        fs.Close();

                    }
                    catch (Exception ex)
                    {
                        log(ex.ToString());
                    }
                    LogUi.Add(list);
                    
                }
            }
        }

        public static void logDoubleClick(object sender, EventArgs e)
        {
            if (listbox.SelectedItem != null)
                MessageBox.Show(listbox.SelectedItem.ToString());
        }
    }

    /// <summary>
    /// 毫秒计数器 
    /// </summary>

    public class TimerBase
    {
        static Dictionary<string, TimerBase> iTimer = new Dictionary<string, TimerBase>();
        DateTime t = DateTime.Now;
        public List<DateTime> timeList = new List<DateTime>();
        public List<double> StepList = new List<double>();
        public List<double> TotalList = new List<double>();

        public TimerBase()
        {
            Clear();
        }

        public void Clear()
        {
            StepList.Clear();
            TotalList.Clear();
            timeList.Clear();

            StepList.Add(0);
            TotalList.Add(0);
            t = DateTime.Now;
            timeList.Add(t);
        }

        public TimerBase(string timerName) : this()
        {
            if (!iTimer.ContainsKey(timerName))
                iTimer.Add(timerName, this);
        }
        public void Count()
        {
            DateTime t1 = DateTime.Now;
            double dt = new TimeSpan(t1.Ticks - t.Ticks).TotalMilliseconds;
            double dt1 = new TimeSpan(t1.Ticks - timeList[timeList.Count - 1].Ticks).TotalMilliseconds;
            StepList.Add(dt1);
            TotalList.Add(dt);
            timeList.Add(DateTime.Now);
        }
        public double D
        {
            get
            {
                return new TimeSpan(DateTime.Now.Ticks - t.Ticks).TotalMilliseconds;
            }
            set
            {
                t = DateTime.Now.AddMilliseconds(value);
            }
        }
        public string S
        {
            get { return D.ToString("F2"); }
        }

        public void reset()
        {
            t = DateTime.Now;
        }

        public static TimerBase Reset(string timerName, double time = 0)
        {
            if (string.IsNullOrEmpty(timerName)) throw new Exception("timer名称不允许为空！！！！");
            if (!iTimer.ContainsKey(timerName))
                iTimer.Add(timerName, new TimerBase());

            iTimer[timerName].D = time;
            return iTimer[timerName];
        }

        public static TimerBase Timer(string timerName)
        {
            if (string.IsNullOrEmpty(timerName)) throw new Exception("定时器名称不允许为空！！！！");
            if (!iTimer.ContainsKey(timerName)) throw new Exception("找不到timer名称=" + timerName + "的定时器，请先新建定时器");
            return iTimer[timerName];
        }
    }

    /// <summary>
    /// System.Windows.Forms.Timer 事件定时器
    /// </summary>
    public class IirUiEventTimer
    {

        static Dictionary<int, IirUiEventTimer> iTimer = new Dictionary<int, IirUiEventTimer>();
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        public IirUiEventTimer(int timerInterval)
        {
            t.Interval = timerInterval;
            if (timerInterval <= 0) throw new Exception("定时器时间间隔必须大于0ms");
            if (!iTimer.ContainsKey(timerInterval))
                iTimer.Add(timerInterval, this);
        }

        public void addEventHandler(EventHandler e)
        {
            t.Tick += e;
        }

        public void removeEventHandler(EventHandler e)
        {
            t.Tick -= e;
        }

        public void start()
        {
            t.Enabled = true;
        }
        public void stop()
        {
            t.Enabled = false;
        }
        public static void startAll()
        {
            foreach (KeyValuePair<int, IirUiEventTimer> item in iTimer)
            {
                item.Value.start();
            }
        }
        public static void stopAll()
        {
            foreach (KeyValuePair<int, IirUiEventTimer> item in iTimer)
            {
                item.Value.stop();
            }
        }

        public static IirUiEventTimer timer(int timerInterval)
        {
            if (timerInterval <= 0) throw new Exception("定时器时间间隔必须大于0ms");
            if (!iTimer.ContainsKey(timerInterval))
            {
                IirUiEventTimer t1 = new IirUiEventTimer(timerInterval);
            }
            return iTimer[timerInterval];
        }
    }


}
