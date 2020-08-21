using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MODEL
{
    public class PublicParameter
    {
        /// <summary>
        /// 生产工单编码
        /// </summary>
        public string produceOrderCode { set; get; }

        /// <summary>
        /// 工序编码。甲方提供工序编码列表供乙方使用
        /// </summary>
        public string technicsProcessCode { set; get; }

        /// <summary>
        /// 工序名称。甲方提供工序名称列表供乙方使用
        /// </summary>
        public string technicsProcessName { set; get; }

        /// <summary>
        /// 工步编码
        /// </summary>
        public string technicsStepCode { set; get; }

        /// <summary>
        /// 工步名称
        /// </summary>
        public string technicsStepName { set; get; }

        /// <summary>
        /// 产品数量
        /// </summary>
        public int productCount { set; get; }


        /// <summary>
        /// 用户名称，甲方提供
        /// </summary>
        public string username { set; get; }

        /// <summary>
        /// 用户账号，甲方提供
        /// </summary>
        public string userAccount { set; get; }

        /// <summary>
        /// 设备编码。甲方提供设备编码列表供乙方使用
        /// </summary>
        public string deviceCode { set; get; }

        /// <summary>
        /// 设备名称。甲方提供设备名称列表供乙方使用
        /// </summary>
        public string deviceName { set; get; }

        /// <summary>
        /// 租户ID，甲方提供租户ID供乙方使用
        /// </summary>
        public int tenantID { set; get; }
    }
}
