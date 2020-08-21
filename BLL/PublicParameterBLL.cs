using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace BLL
{
    public class PublicParameterBLL
    {
        public static DataTable Get()
        {
            return PublicParameterDAL.Get();
        }
    }
}
