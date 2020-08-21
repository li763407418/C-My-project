using DataBase;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DAL
{
    public static class ViewLoadDAL
    {
        /// <summary>
        /// 获取菜单栏
        /// </summary>
        /// <returns></returns>
        public static DataTable GetViewButton()
        {
            string StoredProcedureName = "GetViewButton";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {

            };

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }

        public static DataTable GetViewPage(string _ParentView)
        {
            string StoredProcedureName = "GetViewPage";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {
                new MySqlParameter("?_parentView",MySqlDbType.VarChar),
            };

            Parameter[0].Value = _ParentView;
            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }


        /// <summary>
        /// 获取页面的窗体文件信息
        /// </summary>
        /// <param name="_ParentView"></param>
        /// <returns></returns>
        public static DataTable GetPagePath(string _ViewName)
        {
            string StoredProcedureName = "View_GetPagePath";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {
                new MySqlParameter("?_ViewName",MySqlDbType.VarChar),
            };

            Parameter[0].Value = _ViewName;
            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }
    }
}
