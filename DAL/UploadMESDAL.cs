using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using DataBase;

namespace DAL
{
    public static class UploadMESDAL
    {
        public static DataTable Get()
        {
            string StoredProcedureName = "GetUploadMES";

            List<MySqlParameter> Parameter = new List<MySqlParameter>()
                {
                    
                };

            DataSet ds = MySQLHelper.RunStoredProcedure(StoredProcedureName, Parameter);
            return ds.Tables[0];
        }

        public static bool UpDate(string barcode, int tag)
        {
            string StoredProcedureName = $"update injectbatterinfo set IsUpMes = '{tag}'  where barcode = '{ barcode }';";

            //MySQLHelper.Update(StoredProcedureName);
            return MySQLHelper.Update(StoredProcedureName) > 0 ? true : false;
        }

    }
}
