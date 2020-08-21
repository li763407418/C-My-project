using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DAL;


namespace BLL
{
    public static class ViewLoadBLL
    {
        /// <summary>
        /// 获取菜单栏设置
        /// </summary>
        /// <returns></returns>
        public static DataTable GetViewButton()
        {
            return ViewLoadDAL.GetViewButton();
        }

        public static DataTable GetViewPage(string parameter)
        {
            return ViewLoadDAL.GetViewPage(parameter);
        }

        /// <summary>
        /// 获取数据库中配置的对应的页面窗体文件名称
        /// </summary>
        /// <param name="_ViewName"></param>
        /// <returns></returns>
        public static DataTable GetPagePath(string _ViewName)
        {
            return ViewLoadDAL.GetPagePath(_ViewName);
        }
    }
}
