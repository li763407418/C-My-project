using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DAL;

namespace BLL
{
    public static class UploadMESBLL
    {
        public static DataTable Get()
        {
            return UploadMESDAL.Get();
        }

        public static bool UpDate(string barcode, int tag)
        {
            return UploadMESDAL.UpDate(barcode,tag);
        }
    }
}
