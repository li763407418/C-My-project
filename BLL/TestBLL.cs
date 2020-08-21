
//using DAL;
using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BLL
{
    public class TestBLL
    {
        public static DataTable Get(string name)
        {
            return TestDAL.Get(name);
        }

        public static void Delete(string name)
        {
            TestDAL.Delete(name);
        }

        public static void add(string name, string password)
        {
            TestDAL.Add(name, password);
        }

        public static void update(string name, string password)
        {
            TestDAL.Update(name, password);
        }
    }
}
