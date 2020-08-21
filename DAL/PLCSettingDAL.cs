using DataBase;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DAL
{
    public static class PLCSettingDAL
    {
        /// <summary>
        /// 获取数据库中的PLC配置信息
        /// </summary>
        /// <returns></returns>
        public static DataTable GetPLCSetting()
        {
            string StoredProcedureName = "PLCSetting_Get";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {

            };

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }
    }
}
