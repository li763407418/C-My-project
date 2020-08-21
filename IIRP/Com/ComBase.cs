using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using IIRP.Message;
using System.IO.Ports;
using System.Threading;
using System.IO;
using static IIRP.Customcontrol.InfoState;
using System.Collections.Concurrent;

namespace IIRP.Com
{

    #region 通讯协议的基类

    /// <summary>
    /// 连接类通信状态
    /// </summary>
    public enum ConnectedType { None, LocalNG, LocalOK, RemoteNG, RemoteOK };

    public abstract class IirPobject
    {
        /// <summary>
        /// 类的名称
        /// </summary>
        public string ObjName = "";

        /// <summary>
        /// 获取类型
        /// </summary>
        public string Type { get { return GetType().ToString(); } }

        public IirPobject(string name)
        {
            SetObjName(name);
        }

        protected void SetObjName(string _objName)
        {
            ObjName = _objName;
        }

        #region 一般函数
        /// <summary>
        /// 打印 日志信息
        /// </summary>
        /// <param name="s">要打印的内容</param>
        public void log(string s, string name = "")
        {
            Log.log(s, (name == "") ? ObjName : name);
        }
        public void log(Exception ex, string name = "")
        {
            Log.log(ex.ToString(), (name == "") ? ObjName : name);
        }

        /// <summary>
        /// 用于静态方法内部写日志的方法
        /// </summary>
        /// <param name="ex">日志内容</param>
        public static void Slog(string s, string name = "")
        {
            Log.log(s, name);
        }

        /// <summary>
        /// 用于静态方法内部写异常消息
        /// </summary>
        /// <param name="ex">异常信息</param>
        public static void Slog(Exception ex, string Name = "")
        {
            Log.log(ex, Name);
        }
        #endregion
    }

    #endregion

    #region 通讯协议的基础

    public abstract class CommBase : IirPobject
    {
        #region 公有变量

        /// <summary>
        /// 异常消息列表
        /// </summary>
        public List<ExceptionMessage> ExceptionList = new List<ExceptionMessage>();

        public bool IsConnected { get; set; } = false;

        /// <summary>
        /// 获取已经从网络接收且供读取的数据量
        /// </summary>
        protected int Available { get { return BytesToRead(); } }

        /// <summary>
        /// 通讯关联的设备对象
        /// </summary>
        public IirPDevice ID = null;

        /// <summary>
        /// 配置参数是否已经变化
        /// </summary>
        protected bool SettingIsChanged = false;

        /// <summary>
        /// 可保存参数集合
        /// </summary>
        public ValueList Values = new ValueList();

        /// <summary>
        /// 通讯的连接配置参数
        /// </summary>
        public ValueBase ConnInfo = new ValueBase("");

        protected object Locker = new object();

        #endregion
        public CommBase(string name) : base(name)
        {

        }

        public virtual bool Connect()
        {
            return IsConnected;
        }

        /// <summary>
        /// 检查参数变化引起自动重连的事件
        /// </summary>
        /// <returns></returns>
        protected virtual void CheckIschanged(object sender, EventArgs e)
        {

        }

        public virtual int BytesToRead()
        {
            return 0;
        }
        /// <summary>
        /// 发送指令
        /// </summary>
        /// <param name="_cmd">发送的字节</param>
        /// <returns></returns>
        public virtual object Send(byte[] _cmd)
        {
            return 0;
        }

        /// <summary>
        /// 接收结果的函数
        /// </summary>
        /// <param name="data">接收的字节数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public virtual int Read(ref byte[] data, int offset, int length)
        {
            return 0;
        }

        public virtual bool Close()
        {
            return true;
        }

    }
    #endregion

    /// <summary>
    /// 基本通讯类型TCP、UDP、串口等的基类
    /// </summary>
    public class IirPComm : CommBase
    {
        public IirPComm(string name) : base(name)
        {
            Values.SetSetionName(name);
        }
    }

    public class IirPNet : IirPComm
    {
        #region Net
        private static int BasePort = 51000;
        protected int LocalPort = BasePort++;
        protected EndPoint LocalEP = null;
        protected EndPoint RemoteEP = null;
        protected Socket socket = null;
        public ValueBase IpAddress = new ValueBase("", "IpAddress", "127.0.0.1", "设置设备通信的IP地址");
        public ValueBase Port = new ValueBase("", "Port", "9600", "设置设备通信的端口");
        /// <summary>
        /// 指定套接字的新实例
        /// </summary>
        protected SocketType St = SocketType.Dgram;
        /// <summary>
        /// 指定Socket类支持的协议
        /// </summary>
        protected ProtocolType Pt = ProtocolType.Tcp;

        public IirPNet(string name = "") : base(name)
        {
            Values.Add(IpAddress);
            Values.Add(Port);
            Values.SetSetionName(name);
            IpAddress.ValueChangedEvent += new ValueBase.ValueChangedHandler(CheckIschanged);
            Port.ValueChangedEvent += new ValueBase.ValueChangedHandler(CheckIschanged);
        }

        #endregion

        #region 重写的函数

        protected override void CheckIschanged(object sender, EventArgs e)
        {
            if (IpAddress.ischanged || Port.ischanged)
            {
                SettingIsChanged = true;
                IpAddress.ischanged = false;
                Port.ischanged = false;
            }
        }

        public override bool Connect()
        {
            try
            {
                if (socket != null)
                {
                    if (Pt == ProtocolType.Tcp && socket.Connected)
                    {
                        socket.Disconnect(true);
                    }
                    socket.Dispose();
                }

            }
            catch (Exception ex)
            {
                ExceptionList.Add(new ExceptionMessage(this, ex));
            }

            socket = new Socket(AddressFamily.InterNetwork, St, Pt);
            ConnInfo.S = $"IP地址:{IpAddress.S} 端口号:{Port.S}";
            if (RemoteEP == null || SettingIsChanged)
            {
                SettingIsChanged = false;
                RemoteEP = new IPEndPoint(IPAddress.Parse(IpAddress.S), Port.I);
            }
            try
            {
                switch (Pt)
                {
                    case ProtocolType.IP:
                        break;

                    case ProtocolType.IPv4:
                        break;
                    case ProtocolType.Tcp:
                        socket.Connect(RemoteEP);
                        break;
                    case ProtocolType.Udp:
                        if (LocalEP == null) LocalEP = new IPEndPoint(IPAddress.Any, LocalPort);
                        socket.Bind(LocalEP);
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                        break;
                    default:
                        break;
                }
                socket.ReceiveTimeout = 1000;
                socket.SendTimeout = 300;
            }
            catch (Exception ex)
            {
                ExceptionList.Add(new ExceptionMessage(this, ex));
                log(ex.Message + "Connect");
                return false;
            }
            return true;
        }
        public override object Send(byte[] _cmd)
        {
            if (RemoteEP == null)
                return socket.Send(_cmd);
            else
                return socket.SendTo(_cmd, RemoteEP);
        }

        public override int Read(ref byte[] data, int offset, int length)
        {
            return socket.Receive(data, length, SocketFlags.None);
        }
        public override int BytesToRead()
        {
            return socket.Available;
        }

        public override bool Close()
        {
            try
            {
                socket.Disconnect(false);
                socket.Dispose();
            }
            catch (Exception ex)
            {
                ExceptionList.Add(new ExceptionMessage(this, ex));
                return false;
            }
            return true;
        }
        #endregion
    }

    public class IirPSerialPort : IirPComm
    {
        #region COM
        public SerialPort serial = new SerialPort();

        ValueBase PortName = new ValueBase("", "端口号", "COM1", "设置串口的名称 命名方式为大写 COM + 整数, 如 COM1 、COM2、COM3等 ");
        ValueBase BaudRate = new ValueBase("", "波特率", "9600", "设置串口的波特率,一般备选项是：1200,2400,4800,9600,19200,38400,57600,115200");
        ValueBase DataBits = new ValueBase("", "数据位", "8", "设置串口的数据位一般为：5,6,7,8");
        ValueBase Parity = new ValueBase("", "校验位", "E", "设置串口的校验位 E:偶校验 O:奇校验（是大写字母O，不是数字0），N：无校验");
        ValueBase StopBits = new ValueBase("", "停止位", "1", "设置串口的停止位 一般 1, 1.5, 2");

        /// <summary>
        /// 串口初始化构造函数
        /// </summary>
        /// <param name="name">串口对象名称</param>
        /// <param name="port">串口号</param>
        /// <param name="baudrate">波特率</param>
        /// <param name="databit">数据位</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopbit">停止位</param>
        public IirPSerialPort(string port = "", int baudrate = 9600, int databit = 8, string parity = "E", int stopbit = 1, string name = "") : base(name)
        {
            if (port != "")
            {
                PortName.S = port;
                BaudRate.I = baudrate;
                DataBits.I = databit;
                Parity.S = parity;
                StopBits.I = stopbit;
            }
            serial.ReadTimeout = 1000;
            serial.WriteTimeout = 1000;
            Values.Add(PortName);
            Values.Add(BaudRate);
            Values.Add(DataBits);
            Values.Add(Parity);
            Values.Add(StopBits);
            PortName.ValueChangedEvent += new ValueBase.ValueChangedHandler(CheckIschanged);
            BaudRate.ValueChangedEvent += new ValueBase.ValueChangedHandler(CheckIschanged);
            DataBits.ValueChangedEvent += new ValueBase.ValueChangedHandler(CheckIschanged);
            Parity.ValueChangedEvent += new ValueBase.ValueChangedHandler(CheckIschanged);
            StopBits.ValueChangedEvent += new ValueBase.ValueChangedHandler(CheckIschanged);
            Values.SetSetionName(name);
        }

        protected override void CheckIschanged(object sender, EventArgs e)
        {
            if (PortName.ischanged ||
                BaudRate.ischanged ||
                Parity.ischanged ||
                DataBits.ischanged ||
                StopBits.ischanged)
            {

                SettingIsChanged = true;
                PortName.ischanged = false;
                BaudRate.ischanged = false;
                Parity.ischanged = false;
                DataBits.ischanged = false;
                StopBits.ischanged = false;
            }
        }
        void SetCOM()
        {
            serial.PortName = PortName.S;
            serial.BaudRate = BaudRate.I;
            serial.DataBits = DataBits.I;
            switch (Parity.S)
            {
                case "E": serial.Parity = System.IO.Ports.Parity.Even; break;
                case "N": serial.Parity = System.IO.Ports.Parity.None; break;
                case "O": serial.Parity = System.IO.Ports.Parity.Odd; break;
            }
            switch (StopBits.I)
            {
                case 0: serial.StopBits = System.IO.Ports.StopBits.None; break;
                case 1: serial.StopBits = System.IO.Ports.StopBits.One; break;
                case 2: serial.StopBits = System.IO.Ports.StopBits.Two; break;
            }
            SettingIsChanged = false;

        }

        #endregion

        #region 重写函数

        public override object Send(byte[] _cmd)
        {
            serial.Write(_cmd, 0, _cmd.Length);
            return _cmd.Length;
        }
        public override int Read(ref byte[] data, int offset, int length)
        {
            serial.Read(data, offset, length);

            return 0;
        }
        public override int BytesToRead()
        {
            return serial.BytesToRead;
        }
        public override bool Connect()
        {
            base.Connect();
            if (serial != null && serial.IsOpen == true)
            {
                serial.Close();
            }
            SetCOM();
            serial.Open();
            SettingIsChanged = false;
            string st = "0";
            switch (serial.StopBits)
            {
                case System.IO.Ports.StopBits.None:
                    st = "0";
                    break;
                case System.IO.Ports.StopBits.One:
                    st = "1";
                    break;
                case System.IO.Ports.StopBits.OnePointFive:
                    st = "1.5";
                    break;
                case System.IO.Ports.StopBits.Two:
                    st = "2";
                    break;
            }

            ConnInfo.S = string.Format("串口号：{0},波特率：{1},数据位：{2},校验位：{3},停止位：{4}",
                serial.PortName, serial.BaudRate.ToString(), serial.DataBits.ToString(),
                serial.Parity.ToString().Substring(0, 1), st).Replace(",", "\n");
            return true;
        }
        public override bool Close()
        {
            if (serial != null)
            {
                if (serial.IsOpen)
                    serial.Close();
            }
            return base.Close();
        }

        #endregion

    }

    public class Udp : IirPNet
    {
        public Udp(string name = "") : base(name)
        {
            St = SocketType.Dgram;
            Pt = ProtocolType.Udp;
        }

    }

    public class TCP : IirPNet
    {
        public TCP(string IP = "", int _Port = 9600, string name = "") : base(name)
        {
            if (IP != "")
            {
                IpAddress.S = IP;
                Port.I = _Port;
            }
            St = SocketType.Stream;
            Pt = ProtocolType.Tcp;

        }
    }

    /// <summary>
    ///通讯设备的基类，凡是具有外部（如usb 串口 网口 等）通讯功能的对象都叫设备  比如 plc  数据库 相机  等等 
    /// </summary>
    public abstract class IirPDevice : IirPobject
    {
        protected bool isClient = true;
        protected ValueBase failReTryTime = new ValueBase("", "连接或通讯失败后重试时间间隔", "2000", "连接或通讯失败后重试时间间隔 单位是ms");
        protected ValueBase maxDelayMs_step = new ValueBase("", "单帧超时设置", "500", "设置通信读取单帧返回值的最大超时时间 单位是ms");
        /// <summary>
        /// 执行指令的周期，默认为1
        /// </summary>
        protected ValueBase CmdTime = new ValueBase("", "指令执行周期", "1", "每条指令执行的周期");

        /// <summary>
        /// 当前的线程ID
        /// </summary>
        protected int runThreadID = -1;
        public bool isRunThread
        {
            get
            {
                return (runThreadID == Thread.CurrentThread.ManagedThreadId) ? true : false;
            }
        }

        /// <summary>
        ///  所有的设备的集合
        /// </summary>
        public static List<IirPDevice> listDevice = new List<IirPDevice>();

        /// <summary>
        ///   主扫描函数里面的步骤  默认=0 
        /// </summary>
        public ActStep scanStep = ActStep.Start;

        public LEDState led = null;
        /// <summary>
        /// 动作执行步骤的集合
        /// </summary>
        protected Dictionary<ActStep, string> dictStep = new Dictionary<ActStep, string>();
        /// <summary>
        ///  主扫描函数里面的步骤  
        /// </summary>
        public string CurScanStep
        {
            get
            {
                if (dictStep.ContainsKey(scanStep))
                    return dictStep[scanStep];
                else return "Nc";
            }
        }

        public ValueBase tips = new ValueBase("");

        /// <summary>
        ///  毫秒计数器 用来超时计时  执行时间计算等 
        /// </summary>
        public TimerBase timer = new TimerBase();

        /// <summary>
        ///   配置参数的集合
        /// </summary>
        public ValueList Values = new ValueList();

        /// <summary>
        /// 设备内的通讯单元 串口   tcp/udp  modbus   等
        /// </summary>
        public CommBase Comm = null;

        /// <summary>
        /// 错误信息列表
        /// </summary>
        public List<ExceptionMessage> ExceptionList = new List<ExceptionMessage>();

        /// <summary>
        /// 执行的线程
        /// </summary>
        protected Thread thread = null;

        protected bool Quit = false; //静态变量  控制所有的设备关闭 false 运行中，true 关闭所有设备
        /// <summary>
        /// 线程暂停
        /// </summary>
        public bool Pause = false;

        /// <summary>
        /// 是否实时保存硬件通讯的所有数据
        /// </summary>
        public ValueBase SaveLog = new ValueBase("", "是否保存所有设备的通讯数据", "False", "False 不保存，True保存16进制ASCII");

        /// <summary>
        /// 连接状态标志位  未连接(灰色)  本地打开/监听成功(淡蓝色)，本地打开/监听失败(红色)，远端通信失败/无客户端连接(黄色)，所有成功(绿色)
        /// </summary>
        public ConnectedType ConnType = ConnectedType.None;
        /// <summary>

        /// <summary>
        /// 连接状态的led显示
        /// </summary>
        //public LED led = null;

        public IirPDevice(string name) : base(name)
        {
            dictStep.Add(ActStep.Start, "未打开");
            dictStep.Add(ActStep.Act1, "运行中");
            dictStep.Add(ActStep.End, "关闭");
            listDevice.Add(this);
            Values.Add(CmdTime);
            Values.Add(SaveLog);
            Values.Add(failReTryTime);
            Values.Add(maxDelayMs_step);
        }
        public IirPDevice(string name, CommBase hc) : this(name)
        {

        }
        /// <summary>
        /// 初始化配置参数的组名称和设备的状态提示信息
        /// </summary>
        protected virtual void Init()
        {
            if (ObjName != "")
            {
                Values.SetSetionName(ObjName);
            }
            if (Comm != null)
            {
                if (ObjName != "")
                    Comm.Values.SetSetionName(ObjName);
            }
        }

        /// <summary>
        ///  打开/当前设备
        /// </summary>
        /// <param name="forceOn">强制打开所有设备(当设备内部开关为关闭时，打开内部开关)</param> 
        public void Open(bool forceOn = true)
        {
            if (!Quit || forceOn)
            {
                Quit = false;
                Pause = false;
                if (thread == null)
                {
                    if (!listDevice.Contains(this))
                    {
                        listDevice.Add(this);
                    }
                    thread = new Thread(Run)
                    {
                        IsBackground = true
                    };
                    Thread.Sleep(100);
                    thread.Start();
                }
            }
        }

        /// <summary>
        /// 关闭当前设备
        /// </summary>
        public virtual void Close()
        {
            Quit = true;
            ConnType = ConnectedType.None;
        }

        /// <summary>
        /// 清除设备，释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
            foreach (var a in Values.listValue)
            {
                ValueBase.Valuelist.Remove(a);
            }
            if (Comm != null)
                foreach (var m in Comm.Values.listValue)
                {
                    ValueBase.Valuelist.Remove(m);
                }
            listDevice.Remove(this);
        }
        /// <summary>
        /// 打开/启动 所有的设备
        /// </summary>
        /// <param name="forceOn">强制打开所有设备(当设备内部开关为关闭时，打开内部开关)</param>
        public static void OpenAll(bool forceOn = false)
        {
            foreach (IirPDevice hd in listDevice)
                hd.Open(forceOn);
        }

        /// <summary>
        /// 关闭所有的设备
        /// </summary>
        public static void CloseAll()
        {
            foreach (IirPDevice hd in listDevice)
            {
                hd.Quit = true;
                hd.ConnType = ConnectedType.None;
            }
            listDevice.Clear();
        }

        /// <summary>
        /// 控制设备的动作流程
        /// </summary>
        void Run()
        {
            try
            {
                runThreadID = Thread.CurrentThread.ManagedThreadId;
                Begin();
            }
            catch (Exception ex)
            {
                ex.log("Run");
            }
            while (!Quit)
            {
                try
                {
                    if (!Pause)  //控制设备暂停
                    {
                        MainScan();
                    }
                }
                catch (Exception ex)
                {
                    ex.log("Run");
                }
            }
            scanStep = ActStep.Start;
            End();
            ConnType = ConnectedType.None;
            thread = null;
            log(ObjName + "设备关闭");
        }

        /// <summary>
        /// 线程的主要扫描函数
        /// </summary>
        protected virtual void MainScan()
        {
            Work();
        }

        /// <summary>
        /// 扩展执行函数
        /// </summary>
        protected virtual void Work()
        {

        }

        /// <summary>
        /// 扫描线程开始后执行的初始化
        /// </summary>
        protected virtual void Begin()
        {

        }

        /// <summary>
        /// 扫描线程结束前执行的动作
        /// </summary>
        protected virtual void End()
        {

        }

        /// <summary>
        ///  更新所有的配置参数的组名称
        /// </summary>
        public static void UpdateConfig()
        {
            foreach (IirPDevice id in listDevice)
            {
                id.SetsetionName();
            }
        }

        /// <summary>
        ///  配置参数的组名称
        /// </summary>
        public void SetsetionName()
        {
            Values.SetSetionName(ObjName);
            if (Comm != null) Comm.Values.SetSetionName(ObjName);
        }

        private static object Lockcmd = new object();
        /// <summary>
        /// 保存CMDRETPLC的发送和接收的数据
        /// </summary>
        /// <param name="CMDRETPLC"></param>
        /// <param name="name"></param>
        public void SaveCMDRETPLC(object sender)
        {
            try
            {
                lock (Lockcmd)
                {
                    CMDRETPLC cmd = (CMDRETPLC)sender;
                    if (!SaveLog.B) return;
                    else
                    {
                        File.AppendAllText(IIRSet.dirDevicelog + "/" + ObjName + Vary.DateTimeToStr(Vary.Format.day) + ".txt",
                             Vary.DateTimeToStr(Vary.Format.millisecond) + "-->" + "发送Hex:(" + cmd.TimeSend.ToString("HH:mm:ss.fff") + ")" + cmd.CmdHexStr + "接收Hex:(" + cmd.TimeRead.ToString("HH:mm:ss.fff") + ")" + cmd.RetHexStr.ToString() + "\r\n"
                            );

                    }
                }
            }
            catch (Exception ex)
            {
                log(ex, ":saveCMDRETPLC():");
            }
        }

        private static object LockExcepiton = new object();

        private static object LockExlog = new object();

        /// <summary>
        /// 保存设备异常消息的日志
        /// </summary>
        /// <param name="sender"></param>
        public void SaveExcepition(object sender)
        {
            try
            {
                if (ExceptionList.Count > 0 && SaveLog.B)
                {
                    List<ExceptionMessage> list = new List<ExceptionMessage>();
                    lock (LockExcepiton)
                    {
                        list = ExceptionList;
                        ExceptionList.Clear();
                    }
                    string fn = IIRSet.dirExLog + "/" + ObjName + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    try
                    {
                        lock (LockExlog)
                        {
                            FileStream fs = new FileStream(fn, System.IO.FileMode.Append, System.IO.FileAccess.Write, FileShare.ReadWrite);
                            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

                            if (!File.Exists(fn))
                            {
                                sw.WriteLine(fn, "================header=============");
                            }
                            foreach (ExceptionMessage ex in list)
                            {
                                ex.Message.log("SaveExcepition");
                                sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}--->{ex.Format()}");
                            }
                            sw.Close();
                            fs.Close();
                        }

                    }
                    catch (Exception ex)
                    {
                        log(ex.Message + "SaveExcepition");
                    }
                }
            }
            catch (Exception ex)
            {
                log(ex.Message + "SaveExcepition");
            }
        }

    }

    /// <summary>
    /// 设备客户端
    /// </summary>
    public class IirDeviceClient : IirPDevice
    {

        #region  全局参数

        /// <summary>
        /// 连接是否成功的标志
        /// </summary>
        public bool IsConnected { get; set; } = false;


        public int MaxByte { get; set; } = 100;

        public MemDataCache Cache = new MemDataCache();

        /// <summary>
        /// 通信超时设置
        /// </summary>
        public int Timeout { get { return maxDelayMs_step.I; } set { maxDelayMs_step.I = value; maxDelayMs_step.Save(); } }

        protected object Lockcmd = new object();

        /// <summary>
        /// 临时发送的数据包队列 发送和接收 完成就从队列中自动删除
        /// </summary>
        protected List<CMDRETPLC> CmdList = new List<CMDRETPLC>(); //自定义发送命令缓冲区

        /// <summary>
        /// 自动发送的数据包队列 循环发送和接收  不自动删除
        /// </summary>
        public List<CMDRETPLC> AutoList = new List<CMDRETPLC>();  //自动发送命令缓冲区

        /// <summary>
        /// 当前autolist执行的命令序号
        /// </summary>
        protected ushort CmdIndex { get; set; } = 0;

        /// <summary>
        /// 连续连接失败的次数统计
        /// </summary>
        public int FailReConnectTimes = 0;

        protected object locker = new object();

        #endregion

        #region 构造函数

        public IirDeviceClient(string name) : base(name)
        {

        }
        public IirDeviceClient(string name, CommBase hc) : this(name)
        {
            Comm = hc;
        }

        public void Add(Exception ex)
        {
            ExceptionList.Add(new ExceptionMessage(this, ex));
        }

        public void Add(string ERROR)
        {
            ExceptionList.Add(new ExceptionMessage(this, ERROR));
        }
        public void Add(List<ExceptionMessage> list)
        {
            ExceptionList.AddRange(list);
        }
        #endregion

        #region 发送 + 接收

        protected virtual CMDRETPLC _send(CMDRETPLC cmd)
        {
            //经过数据转换更新cmd
            if (cmd._CMDType != CMDType.WH_Hand)
                cmd.Cmd = TranCmd(cmd);
            try
            {
                cmd.TimeSend = DateTime.Now;
                Comm.Send(cmd.Cmd);
                cmd.State = CmdState.SendOK;
            }
            catch (Exception ex)
            {
                Add(ex);
                log(ex.Message + "_send");
                IsConnected = false;
                cmd.State = CmdState.SendNG;
            }
            return cmd;
        }

        /// <summary>
        /// 接收指令
        /// </summary>
        /// <param name="CMDRETPLC">指令</param>
        /// <returns>指令</returns>
        protected virtual CMDRETPLC _read(CMDRETPLC cmd)
        {
            try
            {
                cmd = ReadData(this, Comm, cmd);
                cmd.TimeRead = DateTime.Now;
            }
            catch (Exception ex)
            {
                Add(ex);
                log(ex.Message + "_read");
                cmd.State = CmdState.ReadNG;
                ConnType = ConnectedType.RemoteNG;
            }

            return cmd;
        }

        /// <summary>
        /// 静态数据接收函数
        /// </summary>
        /// <param name="hd">设备</param>
        /// <param name="hc">数据连接方式</param>
        /// <param name="CMDRETPLC">数据包</param>
        /// <returns></returns>
        private static CMDRETPLC ReadData(IirDeviceClient hd, CommBase hc, CMDRETPLC CMDRETPLC)
        {
            TimerBase t = new TimerBase();
            while (t.D < hd.maxDelayMs_step.I)
            {
                int num = hc.BytesToRead();
                if (num > 0)
                {
                    byte[] buffer = new byte[num];
                    int num0 = hc.Read(ref buffer, 0, Math.Min(num, 2000));
                    CMDRETPLC.ReadBuffer = buffer.ToList();
                    CMDRETPLC.State = hd.CheckRet(CMDRETPLC);
                    t.reset();
                    return CMDRETPLC;
                }
                Thread.Sleep(1);
            }
            hd.IsConnected = false;
            if (CMDRETPLC.State != CmdState.ReadOK)
                CMDRETPLC.State = CmdState.ReadNG;
            return CMDRETPLC;
        }
        /// <summary>
        /// 发送并等待接收返回的指令
        /// </summary>
        /// <param name="cmd">发送的指令</param>
        /// <returns></returns>
        protected virtual CMDRETPLC _getRet(CMDRETPLC cmd)
        {
            try
            {
                cmd.Clear();
                _send(cmd);
                if (cmd.State == CmdState.SendOK)
                {
                    _read(cmd);
                }
                else
                {
                    cmd.State = CmdState.ReadNG;
                }
                if (cmd.State == CmdState.ReadOK)
                {
                    try
                    {
                        lock (lockData)
                        {
                            TranData(cmd);
                        }
                    }
                    catch (Exception ex)
                    {
                        cmd.State = CmdState.TranNG;
                        Log.log("解析函数异常" + ex.Message);
                    }
                }
                ThreadPool.QueueUserWorkItem(new WaitCallback(SaveCMDRETPLC), cmd);   //保存数据包 
            }
            catch (Exception ex)
            {
                ex.log("getret");
            }
            return cmd;
        }

        /// <summary>
        /// 异步等待指令的执行结果
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual CMDRETPLC GetRet(CMDRETPLC cmd)
        {
            if (isRunThread)
            {
                lock (locker)
                {
                    _getRet(cmd);
                }
            }
            else
            {
                int waitNum = CmdList.Count + 1;
                lock (Lockcmd)
                {
                    CmdList.Add(cmd);
                }
                int delayTime = (maxDelayMs_step.I + 1) * waitNum;
                TimerBase t = new TimerBase();
                while (cmd.State != CmdState.ReadOK && t.D < delayTime)
                {
                    IIRSet.Dly(1);
                }
            }
            return cmd;
        }

        public virtual CMDRETPLC GetRet(CMDRETPLC cmd, bool IsAutoList = true)
        {
            lock (Lockcmd)
            {
                AutoList.Add(cmd);
            }
            return cmd;
        }

        public CMDRETPLC GetRet(byte[] bytes)
        {
            return GetRet(new CMDRETPLC(bytes));
        }

        public CMDRETPLC GetRet(string s)
        {
            return GetRet(Vary.StringToBytes(s));
        }

        #endregion

        private delegate bool DgConnecte();

        private DgConnecte dgConnect = null;

        protected override void Begin()
        {
            base.Begin();

            if (Comm != null)
            {
                dgConnect += new DgConnecte(Comm.Connect);
                tips.S = Comm.ConnInfo.S;
            }
            else
            {
                log("tc为空，请先将tc赋值"); Quit = true;
            }


        }
        /// <summary>
        /// 握手函数
        /// </summary>
        /// <param name="hc"></param>
        /// <returns></returns>
        protected virtual bool DoAfterConnect(CommBase hc)
        {
            return true;
        }

        /// <summary>
        /// 转换结果
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected virtual CMDRETPLC TranData(CMDRETPLC cmd)
        {
            return cmd;
        }

        /// <summary>
        ///  设备的扫描函数
        /// </summary>
        protected override void MainScan()
        {
            switch (scanStep)
            {
                case ActStep.Start://新建连接
                    {
                        try  //测试连接并刷新状态
                        {
                            ConnType = ConnectedType.LocalNG;
                            if ((bool)dgConnect?.Invoke())
                            {
                                ConnType = ConnectedType.LocalOK;
                                if (DoAfterConnect(Comm))
                                {
                                    tips.S = "连接成功\n";
                                    ConnType = ConnectedType.RemoteOK;
                                    IsConnected = true;
                                    scanStep = ActStep.Act1;
                                    Log.log(ObjName + "连接成功!");
                                }
                                else
                                {
                                    scanStep = ActStep.Start;
                                    tips.S = "握手失败";
                                    ConnType = ConnectedType.LocalNG;
                                    Thread.Sleep(failReTryTime.I);
                                }
                            }
                            else
                            {
                                scanStep = ActStep.Start;
                                Add(Comm.ExceptionList);
                                ConnType = ConnectedType.LocalNG;
                                Thread.Sleep(failReTryTime.I);
                            }
                        }
                        catch (Exception ex)
                        {
                            Add(ex);
                            log(ex.Message + "MainScan");
                            ConnType = ConnectedType.LocalNG;
                        }
                    }
                    break;
                case ActStep.Act1:
                    {
                        try
                        {
                            if (!IsConnected)
                            {
                                scanStep = ActStep.Start;
                                break;
                            }
                            Work();
                        }
                        catch (Exception ex)
                        {
                            Add(ex);
                            log(ex.Message + "MainScan");
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 循环处理指令发送接收的工作线程
        /// </summary>
        protected override void Work()
        {
            if (CmdList.Count > 0)
            {

                CMDRETPLC cmd = CmdList[0];
                if (cmd != null)
                    _getRet(cmd);
                lock (Lockcmd)
                {
                    CmdList.Remove(cmd);
                }
            }
            if (AutoList.Count > 0)
            {
                lock (Lockcmd)
                {
                    CMDRETPLC CMDRETPLC = AutoList[(CmdIndex++) % AutoList.Count];
                    if (CMDRETPLC != null)
                        _getRet(CMDRETPLC);
                }
            }
            Thread.Sleep(CmdTime.I);
        }

        public object lockData = new object();

        /// <summary>
        /// 发送数据包前对数据包的处理
        /// </summary>
        /// <param name="CMDRETPLC"></param>
        protected virtual byte[] TranCmd(CMDRETPLC cmdRet)
        {
            return cmdRet.Cmd;
        }

        /// <summary>
        /// 检查数据结果是否有异常
        /// </summary>
        /// <param name="CMDRETPLC"></param>
        protected virtual CmdState CheckRet(CMDRETPLC cmdRet)
        {
            if (cmdRet.ReadBuffer.Count >= 1)
                cmdRet.State = CmdState.ReadOK;
            return cmdRet.State;
        }

        protected virtual bool TryConnect()
        {
            bool Flat = false;
            try
            {
                Flat = (bool)dgConnect?.Invoke() ? DoAfterConnect(Comm) : false;//尝试连接
                Comm?.Close();

            }
            catch (Exception ex)
            {
                Flat = false;
                Comm?.Close();
                log("尝试连接失败" + ex.Message);
            }
            return Flat;
        }
    }

    /// <summary>
    /// 数据缓存类
    /// </summary>
    public class MemDataCache
    {
        private object Lock = new object();
      
        private ConcurrentDictionary<string, CacheEntry> _Cache = new ConcurrentDictionary<string, CacheEntry>();


        public void Add(CMDRETPLC cmd)
        {
            CacheEntry oldentry = null;
            if (!_Cache.TryGetValue(cmd.Address, out oldentry) && cmd.State == CmdState.ReadOK)
            {
                CacheEntry entry = new CacheEntry(cmd.Result, cmd.DateType)
                {
                    CreateTime = DateTime.Now
                };
                lock (Lock)
                    _Cache.TryAdd(cmd.Address, entry);
            }
            else
            {
                if (oldentry != null && oldentry.Value != cmd.Result && cmd.State == CmdState.ReadOK)
                {
                    CacheEntry entry = new CacheEntry(cmd.Address, cmd.DateType)
                    {
                        CreateTime = oldentry.CreateTime,
                        UpateTime = DateTime.Now,
                        Value = cmd.Result
                    };
                    lock (Lock)
                        _Cache.TryUpdate(cmd.Address, entry, oldentry);
                }
            }
        }

        public bool GetValue(string Key,out string str)
        {
            bool  Result = false;
            CacheEntry entry = null;
            str = "";
            if (_Cache.TryGetValue(Key, out entry))
            {
                Result = true;
                if (entry.Type.Contains("[]"))
                    str = Vary.ArrayToString(entry.Value);
                else
                    str = entry.Value.ToString();
            }
            return Result;
        }

        public DateTime GetCreateTime(string Key)
        {
            DateTime time = DateTime.Now;
            CacheEntry entry = null;
            time = _Cache.TryGetValue(Key, out entry) ? entry.CreateTime : time;
            return time;
        }

        public DateTime GetUpateTime(string Key)
        {
            DateTime time = DateTime.Now;
            CacheEntry entry = null;
            time = _Cache.TryGetValue(Key, out entry) ? entry.UpateTime : time;
            return time;
        }

        public bool Remove(string Key)
        {
            bool Flat = false;
            lock (Lock)
            {
                CacheEntry cache = null;
                Flat = _Cache.TryRemove(Key, out cache);
            }
            return Flat;
        }

    }

    /// <summary>
    /// 缓存对象
    /// </summary>
    public class CacheEntry
    {
        

        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }


        /// <summary>
        /// 数据类型
        /// </summary>
        public string Type { get; set; }

        public bool Ischanged { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpateTime { get; set; }

        /// <summary>
        /// 更新时间间隔
        /// </summary>
        public double UpateSpace { get { return (DateTime.Now - UpateTime).TotalMilliseconds; } }

        public CacheEntry(object value, string type)
        {
            Value = value;
            Type = type;
        }

    }

}
