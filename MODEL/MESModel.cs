using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MODEL
{
    public class MESModel
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
        /// 产品编码（模组）
        /// </summary>
        public string productCode { set; get; }

        /// <summary>
        /// 产品数量
        /// </summary>
        public int productCount { set; get; }

        /// <summary>
        /// 产品质量：0，不合格；1，合格；
        /// </summary>
        public int productQuality { set; get; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public string produceDate { set; get; }

        /// <summary>
        /// 生产开始时间
        /// </summary>
        public string startTime { set; get; }

        /// <summary>
        /// 生产结束时间
        /// </summary>
        public string endTime { set; get; }

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
        /// 备注
        /// </summary>
        public string Remarks { set; get; }

        /// <summary>
        /// 租户ID，甲方提供租户ID供乙方使用
        /// </summary>
        public int tenantID { set; get; }

        public List<produceInEntity> produceInEntityList = new List<produceInEntity>();

        public List<produceParamEntity> produceParamEntityList = new List<produceParamEntity>();
    }

    public class produceInEntity
    {
        /// <summary>
        /// 产品编码（原材料）
        /// </summary>
        public string productCode { set; get; }

        /// <summary>
        /// 产品数量
        /// </summary>
        public int productCount { set; get; }

    }

    public class produceParamEntity
    {
        /// <summary>
        /// 	产品编码（模组或电芯，根据关键控制点归属）
        /// </summary>
        public string productCode { set; get; }

        /// <summary>
        /// 工艺参数名称。甲方提供工艺参数名称列表供乙方使用
        /// </summary>
        public string technicsParamName { set; get; }

        /// <summary>
        /// 工艺参数编码。甲方提供工艺参数编码列表供乙方使用
        /// </summary>
        public string technicsParamCode { set; get; }

        /// <summary>
        /// 工艺参数值
        /// </summary>
        public string technicsParamValue { set; get; }

        /// <summary>
        /// 描述
        /// </summary>
        public string desc { set; get; }


    }
}
