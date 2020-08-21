using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataBase
{
    public class MySQLHelper
    {
        /// <summary>
        /// 执行储存过程，返回结果
        /// </summary>
        /// <param name="StoredProcedureName">存储过程名</param>
        /// <param name="mySqlParameter">传入参数</param>
        /// <returns></returns>
        public static DataSet RunStoredProcedure(string StoredProcedureName, List<MySqlParameter> mySqlParameter)
        {
            try
            {
                MySqlConnection mysqlcon = new MySqlConnection();
                MySqlCommand mysqlCommand = new MySqlCommand();
                //string str = "server= localhost;User ID=root;password=geesun;Database=test";
                string str = ConfigurationManager.ConnectionStrings["MySQL"].ToString();
                mysqlcon = new MySqlConnection(str);
                mysqlcon.Open();
                mysqlCommand.Connection = mysqlcon;

                mysqlCommand.CommandText = StoredProcedureName;
                mysqlCommand.CommandType = CommandType.StoredProcedure;
                foreach (MySqlParameter a in mySqlParameter)
                {
                    if (a == null)
                        break;
                    mysqlCommand.Parameters.Add(a);
                }

                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = mysqlCommand;
                DataSet ds = new DataSet();
                adapter.Fill(ds, mysqlCommand.CommandText);

                mysqlCommand.Dispose();
                mysqlcon.Close();
                mysqlcon.Dispose();
                return ds;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw ex;
            }
        }

        public static int Update(string StoredProcedureName)
        {
            try
            {
                int result;
                MySqlConnection mysqlcon = new MySqlConnection();
                MySqlCommand mysqlCommand = new MySqlCommand();
                //string str = "server= localhost;User ID=root;password=geesun;Database=test";
                string str = ConfigurationManager.ConnectionStrings["MySQL"].ToString();
                mysqlcon = new MySqlConnection(str);
                mysqlcon.Open();
                mysqlCommand.Connection = mysqlcon;

                mysqlCommand.CommandText = StoredProcedureName;
                mysqlCommand.CommandType = CommandType.Text;

                result = mysqlCommand.ExecuteNonQuery();
                mysqlCommand.Dispose();
                mysqlcon.Close();
                mysqlcon.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
