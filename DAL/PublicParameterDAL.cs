using DataBase;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data;

namespace DAL
{
    public class PublicParameterDAL
    {
        public static DataTable Get()
        {
            string StoredProcedureName = "PublicParameter_Get";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
            {

            };

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }
    }
}
