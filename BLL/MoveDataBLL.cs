using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL;
using System.Data;

namespace BLL
{
    public static class MoveDataBLL
    {

        /// <summary>
        /// 查询临时表将要移除的数据
        /// </summary>
        public static int Get()
        {
            int count = 0;
            DataTable dt = MoveDataDAL.Get();
            int.TryParse(dt.Rows[0][0].ToString(), out count);
            return count;
        }

        /// <summary>
        /// 移动临时表数据到历史表
        /// </summary>
        public static void Move()
        {
            MoveDataDAL.Move();
        }

        /// <summary>
        /// 删除临时表数据
        /// </summary>
        public static void Delete()
        {
            MoveDataDAL.Delete();
        }
    }
}
