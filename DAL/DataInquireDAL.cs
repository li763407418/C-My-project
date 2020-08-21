using DataBase;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DAL
{
    public static class DataInquireDAL
    {
        public static DataTable GetDataByTime(string bgeinTime , string endTime)
        {
            string StoredProcedureName = "injectbatterinfo_GetDataByTime";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {
                new MySqlParameter("?_bgeinTime",MySqlDbType.VarChar),
                new MySqlParameter("?_endTime",MySqlDbType.VarChar),
            };

            Parameter[0].Value = bgeinTime;
            Parameter[1].Value = endTime;

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }



        public static DataTable GetDataByCode(string barCode)
        {
            string StoredProcedureName = "injectbatterinfo_GetDataByCode";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {
                new MySqlParameter("?_barCode",MySqlDbType.VarChar),
            };

            Parameter[0].Value = barCode;

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }
    }
}
