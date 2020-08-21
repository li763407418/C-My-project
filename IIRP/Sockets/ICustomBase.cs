using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using IIRP.Com;
namespace IIRP.Sockets
{
    public interface ICustomBase
    {
        int Readcount { get; }

        void ByteToDate(byte[] buff, VaryType vary);

        byte[] DataToByte(VaryType vary);

    }
    /// <summary>
    /// 用于读取自定义的类型，该类型继承于IDataVary接口
    /// </summary>
    /// <typeparam name="T">自定义的类型</typeparam>
    /// <param name="address">PLC地址</param>
    /// <returns>自定义的结果对象</returns>
    /// <remarks>
    /// 必须定义一个类继承于IDataVary接口,才能使用这个方法
    /// </remarks>
    public abstract class Custom
    {
        public int Readcount
        {
            get
            {
                return GetReadcount();
            }

        }

        private int GetReadcount()
        {
            System.Type type = GetType();
            int count = 0;
            foreach (PropertyInfo p in type.GetProperties())
            {
                object value = p.GetValue(this, null);
                if (p.Name != "Readcount")
                {
                    if (value.GetType() == typeof(long) || value.GetType() == typeof(ulong) || value.GetType() == typeof(double))
                    {
                        count += 8;
                    }
                    else if (value.GetType() == typeof(int) || value.GetType() == typeof(uint) || value.GetType() == typeof(float))
                    {
                        count += 4;
                    }
                    else if (value.GetType() == typeof(short) || value.GetType() == typeof(ushort))
                    {
                        count += 2;
                    }
                    else if (value.GetType() == typeof(bool))
                    {
                        count += 1;
                    }
                }

            }
            return count;
        }

        public void ByteToDate(object sender, byte[] buff, VaryType vary)
        {
            System.Type type = this.GetType();
            List<byte> Buff = buff.ToList();
            foreach (PropertyInfo p in type.GetProperties())
            {
                object value = p.GetValue(this, null);
                if (p.Name != "Readcount")
                {
                    if (value.GetType() == typeof(double))
                    {
                        byte[] Temp = new byte[8];
                        Array.Copy(Buff.ToArray(), 0, Temp, 0, 8);
                        value = BitConverter.ToDouble(Vary.Vary8Byte(Temp, vary), 0);
                        Buff.RemoveRange(0, 8);

                    }
                    else if (value.GetType() == typeof(int))
                    {
                        byte[] Temp = new byte[4];
                        Array.Copy(Buff.ToArray(), 0, Temp, 0, 4);
                        value = BitConverter.ToInt32(Vary.Vary4Byte(Temp, vary), 0);
                        Buff.RemoveRange(0, 4);
                    }
                    else if (value.GetType() == typeof(uint))
                    {
                        byte[] Temp = new byte[4];
                        Array.Copy(Buff.ToArray(), 0, Temp, 0, 4);
                        value = BitConverter.ToUInt32(Vary.Vary4Byte(Temp, vary), 0);
                        Buff.RemoveRange(0, 4);
                    }
                    else if (value.GetType() == typeof(float))
                    {
                        byte[] Temp = new byte[4];
                        Array.Copy(Buff.ToArray(), 0, Temp, 0, 4);
                        value = BitConverter.ToSingle(Vary.Vary4Byte(Temp, vary), 0);
                        Buff.RemoveRange(0, 4);
                    }
                    else if (value.GetType() == typeof(short))
                    {
                        value = BitConverter.ToInt16(Buff.ToArray(), 0);
                        Buff.RemoveRange(0, 2);
                    }
                    else if (value.GetType() == typeof(ushort))
                    {
                        value = BitConverter.ToUInt16(Buff.ToArray(), 0);
                        Buff.RemoveRange(0, 2);
                    }
                    else if (value.GetType() == typeof(bool))
                    {
                        value = Buff[0] > 1 ? true : false;
                    }
                }

            }
        }

        public byte[] DataToByte(VaryType vary)
        {
            System.Type type = this.GetType();
            List<byte> Buff = new List<byte>();
            foreach (PropertyInfo p in type.GetProperties())
            {
                object value = p.GetValue(this, null);
                if (p.Name != "Readcount")
                {
                    if (value.GetType() == typeof(double))
                    {
                        Buff.AddRange(Vary.ArrayToBytes(new double[1] { (double)value }, vary));
                    }
                    else if (value.GetType() == typeof(int))
                    {
                        Buff.AddRange(Vary.ArrayToBytes(new int[1] { (int)value }, vary));
                    }
                    else if (value.GetType() == typeof(uint))
                    {
                        Buff.AddRange(Vary.ArrayToBytes(new uint[1] { (uint)value }, vary));
                    }
                    else if (value.GetType() == typeof(float))
                    {
                        Buff.AddRange(Vary.ArrayToBytes(new float[1] { (float)value }, vary));
                    }
                    else if (value.GetType() == typeof(short))
                    {
                        Buff.AddRange(Vary.ArrayToBytes(new short[1] { (short)value }));
                    }
                    else if (value.GetType() == typeof(ushort))
                    {
                        Buff.AddRange(Vary.ArrayToBytes(new ushort[1] { (ushort)value }));
                    }
                    else if (value.GetType() == typeof(bool))
                    {
                        Buff.Add((byte)((bool)value==true?1:0 ));
                    }
                }

            }

            return Buff.ToArray();
        }
    }
}
