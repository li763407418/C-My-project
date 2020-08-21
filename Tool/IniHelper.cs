using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tool
{
    public class IniHelper
    {

        #region  INI文件
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// 
        //string FilePath = Application.StartupPath + @"\test.ini";   //获取本地ini文件
        public string path;
        public IniHelper(string INIPath)
        {
            //如果不存在 则创建该文件
            FileInfo f = new FileInfo(INIPath);
            if (!f.Exists)
                f.Create();
            path = INIPath;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);



        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="Section">区段</param>
        /// <param name="Key">Key</param>
        /// <param name="Value">Value</param>
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="Section">区段</param>
        /// <param name="Key">Key</param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }
    }

    #endregion
}
