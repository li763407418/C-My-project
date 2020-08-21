using DataBase;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DAL
{
    public class TestDAL
    {
        public static DataTable Get(string name)
        {
            string StoredProcedureName = "xujiaqi";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
                {
                new MySqlParameter("?_Name",MySqlDbType.VarChar),
                };

            Parameter[0].Value = name;

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }

        public static void Delete(string name)
        {
            string StoredProcedureName = "delete";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
                {
                new MySqlParameter("?_Name",MySqlDbType.VarChar),
                };

            Parameter[0].Value = name;

            MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);

        }

        public static void Add(string name, string password)
        {
            string StoredProcedureName = "add";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
                {
                new MySqlParameter("?_Name",MySqlDbType.VarChar),
                new MySqlParameter("?_Password", MySqlDbType.VarChar, 50),
                };

            Parameter[0].Value = name;
            Parameter[1].Value = password;

            MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
        }

        public static void Update(string name, string password)
        {
            string StoredProcedureName = "update";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
                {
                new MySqlParameter("?_Name",MySqlDbType.VarChar),
                new MySqlParameter("?_Password", MySqlDbType.VarChar, 50),
                };

            Parameter[0].Value = name;
            Parameter[1].Value = password;

            MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
        }
    }
}
