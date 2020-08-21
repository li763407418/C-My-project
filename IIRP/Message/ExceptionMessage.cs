using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IIRP.Com;
namespace IIRP.Message
{
    public class ExceptionMessage
    {
        private int Index = 0;
        /// <summary>
        /// 异常编号
        /// </summary>
        public int ExceptionCode { get { return Index; } set { Index++; }} 

        /// <summary>
        /// 异常消息
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 引发异常的对象名称
        /// </summary>
        public string ObjectName { get; set; } = "";

        /// <summary>
        /// 引发异常的时间
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;

        public ExceptionMessage(IirPobject iirPobject,Exception ex)
        {
            ExceptionCode = Index;
            ObjectName= iirPobject.ObjName;
            Message = ex.Message;
            DateTime = DateTime.Now;
        }
        public ExceptionMessage(IirPobject iirPobject, string ERROR)
        {
            ExceptionCode = Index;
            ObjectName = iirPobject.ObjName;
            Message = ERROR;
            DateTime = DateTime.Now;
        }
        public ExceptionMessage(int _ExceptionCode, string _Message, string _Type, DateTime dateTime)
        {
            ExceptionCode = _ExceptionCode;
            Message = _Message;
            ObjectName = _Type;
            DateTime = dateTime;
        }

        public string Format()
        {
            if (Message == "" && ObjectName == "") return "对象为空";
            return $"<异常对象:{ObjectName}>-发生时间:{DateTime} 异常代码:{ExceptionCode} 异常消息:{Message}";
        }
    }
}
