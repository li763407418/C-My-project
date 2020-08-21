using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DAL;

namespace BLL
{
    public static class DataInquireBLL
    {
        public static DataTable GetDataByTime(string bgeinTime, string endTime)
        {
            return DataInquireDAL.GetDataByTime(bgeinTime, endTime);
        }

        public static DataTable GetDataByCode(string barCode)
        {
            return DataInquireDAL.GetDataByCode(barCode);
        }
    }
}
