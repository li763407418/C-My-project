using DataBase;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DAL
{
    public static class MoveDataDAL
    {
        /// <summary>
        /// 查询临时表将要移除的数据
        /// </summary>
        public static DataTable Get()
        {
            string StoredProcedureName = "GetMoveData";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {

            };

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }

        /// <summary>
        /// 移动临时表数据到历史表
        /// </summary>
        public static void Move()
        {
            string StoredProcedureName = "MoveData";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {

            };

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
        }

        /// <summary>
        /// 删除临时表数据
        /// </summary>
        public static void Delete()//DeleteData
        {
            string StoredProcedureName = "DeleteData";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {

            };

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
        }
    }
}
