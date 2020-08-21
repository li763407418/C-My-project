using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL;
using System.Data;

namespace BLL
{
    public static class PLCSettingBLL
    {
        public static DataTable GetPLCSetting()
        {
            return PLCSettingDAL.GetPLCSetting();
        }
    }
}
