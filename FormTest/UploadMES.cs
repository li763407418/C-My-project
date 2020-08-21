using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;
using Newtonsoft.Json;
using Tool;
using System.Threading;
using MODEL;

namespace FormTest
{
    public partial class UploadMES : Form
    {
        /// <summary>
        /// MES上传状态位
        /// </summary>
        private bool status = false;
        private object statusLock = new object();
        public UploadMES()
        {
            InitializeComponent();
        }

        private void UploadMES_Load(object sender, EventArgs e)
        {
            Thread UploadMES = new Thread(UpData);
            UploadMES.IsBackground = true;
            UploadMES.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lock (statusLock)
            {
                status = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lock (statusLock)
            {
                status = false;
            }
        }

        public void UpData()
        {
            string url = "http://172.16.162.22:8020/api/produce/produce/open/add";

            PublicParameter data = new PublicParameter();
            DataTable dt = PublicParameterBLL.Get();//获取公共参数
            data.produceOrderCode = dt.Rows[0]["value"].ToString();
            data.technicsProcessCode = dt.Rows[1]["value"].ToString();
            data.technicsProcessName = dt.Rows[2]["value"].ToString();
            data.technicsStepCode = dt.Rows[3]["value"].ToString();
            data.technicsStepName = dt.Rows[4]["value"].ToString();
            data.productCount = int.Parse(dt.Rows[5]["value"].ToString());
            data.username = dt.Rows[6]["value"].ToString();
            data.userAccount = dt.Rows[7]["value"].ToString();
            data.deviceCode = dt.Rows[8]["value"].ToString();
            data.deviceName = dt.Rows[9]["value"].ToString();
            data.tenantID = int.Parse(dt.Rows[10]["value"].ToString());

            while (true)
            {
                try
                {
                    if (status)
                    {
                        dt = UploadMESBLL.Get();//获取需要上传的信息
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            MODEL.MESModel MesData = new MODEL.MESModel();
                            MesData.produceOrderCode = data.produceOrderCode;
                            MesData.technicsProcessCode = data.technicsProcessCode;
                            MesData.technicsProcessName = data.technicsProcessName;
                            MesData.technicsStepCode = data.technicsStepCode;
                            MesData.technicsStepName = data.technicsStepName;
                            MesData.productCode = dt.Rows[i]["BarCode"].ToString();
                            MesData.productCount = 1;
                            MesData.productQuality = dt.Rows[i]["InjectStatusText"].ToString() == "正常" ? 1 : 0;
                            MesData.produceDate = Convert.ToDateTime(dt.Rows[i]["AfterWeightEndTime"].ToString().Substring(0, dt.Rows[i]["AfterWeightEndTime"].ToString().Count() - 4)).ToString("yyyy-MM-dd");
                            MesData.startTime = Convert.ToDateTime(dt.Rows[i]["ScanBeginTime"].ToString().Substring(0, dt.Rows[i]["ScanBeginTime"].ToString().Count() - 4)).ToString("yyyy-MM-dd HH:mm:ss");
                            MesData.endTime = Convert.ToDateTime(dt.Rows[i]["AfterWeightEndTime"].ToString().Substring(0, dt.Rows[i]["AfterWeightEndTime"].ToString().Count() - 4)).ToString("yyyy-MM-dd HH:mm: ss");
                            MesData.username = data.username;
                            MesData.userAccount = data.userAccount;
                            MesData.deviceCode = data.deviceCode;
                            MesData.deviceName = data.deviceName;
                            MesData.Remarks = "";
                            MesData.tenantID = data.tenantID;

                            MODEL.produceInEntity ProduceInEntityData = new MODEL.produceInEntity();
                            ProduceInEntityData.productCode = MesData.productCode;
                            ProduceInEntityData.productCount = 1;
                            MesData.produceInEntityList.Add(ProduceInEntityData);



                            MODEL.produceParamEntity ProduceParamEntity = new MODEL.produceParamEntity();
                            ProduceParamEntity.productCode = MesData.productCode;
                            ProduceParamEntity.technicsParamName = "批次号";
                            ProduceParamEntity.technicsParamCode = "SCDNJ_ETYBTH_RES";
                            ProduceParamEntity.technicsParamValue = dt.Rows[i]["BatchNo"].ToString();
                            ProduceParamEntity.desc = "";
                            MesData.produceParamEntityList.Add(ProduceParamEntity);

                            MODEL.produceParamEntity code = new MODEL.produceParamEntity();
                            code.productCode = MesData.productCode;
                            code.technicsParamName = "条码";
                            code.technicsParamCode = "SCDNJ_CELCOD_RES";
                            code.technicsParamValue = MesData.productCode;
                            code.desc = "";
                            MesData.produceParamEntityList.Add(code);

                            MODEL.produceParamEntity BeginTime = new MODEL.produceParamEntity();
                            BeginTime.productCode = MesData.productCode;
                            BeginTime.technicsParamName = "开始时间";
                            BeginTime.technicsParamCode = "SCDNJ_STATIM_RES";
                            BeginTime.technicsParamValue = MesData.startTime;
                            BeginTime.desc = "";
                            MesData.produceParamEntityList.Add(BeginTime);

                            MODEL.produceParamEntity EndTime = new MODEL.produceParamEntity();
                            EndTime.productCode = MesData.productCode;
                            EndTime.technicsParamName = "结束时间";
                            EndTime.technicsParamCode = "SCDNJ_ENDTIM_RES";
                            EndTime.technicsParamValue = MesData.endTime;
                            EndTime.desc = "";
                            MesData.produceParamEntityList.Add(EndTime);

                            MODEL.produceParamEntity BeforeWeigth = new MODEL.produceParamEntity();
                            BeforeWeigth.productCode = MesData.productCode;
                            BeforeWeigth.technicsParamName = "注液前重量";
                            BeforeWeigth.technicsParamCode = "SCDNJ_WEIBEFINJ_RES";
                            BeforeWeigth.technicsParamValue = dt.Rows[i]["BeforeWeight"].ToString();
                            BeforeWeigth.desc = "";
                            MesData.produceParamEntityList.Add(BeforeWeigth);

                            MODEL.produceParamEntity AfterWeigth = new MODEL.produceParamEntity();
                            AfterWeigth.productCode = MesData.productCode;
                            AfterWeigth.technicsParamName = "注液后重量";
                            AfterWeigth.technicsParamCode = "SCDNJ_WEIAFTINJ_RES";
                            AfterWeigth.technicsParamValue = dt.Rows[i]["AfterWeight"].ToString();
                            AfterWeigth.desc = "";
                            MesData.produceParamEntityList.Add(AfterWeigth);

                            MODEL.produceParamEntity InjectValue = new MODEL.produceParamEntity();
                            InjectValue.productCode = MesData.productCode;
                            InjectValue.technicsParamName = "注液量";
                            InjectValue.technicsParamCode = "SCDNJ_WEIINJ_RES";
                            InjectValue.technicsParamValue = dt.Rows[i]["InjectValue"].ToString();
                            InjectValue.desc = "";
                            MesData.produceParamEntityList.Add(InjectValue);

                            MODEL.produceParamEntity AllInjectValue = new MODEL.produceParamEntity();
                            AllInjectValue.productCode = MesData.productCode;
                            AllInjectValue.technicsParamName = "总注液量";
                            AllInjectValue.technicsParamCode = "SCDNJ_WEIINJ_RES";
                            AllInjectValue.technicsParamValue = dt.Rows[i]["ActualInjectValue"].ToString();
                            AllInjectValue.desc = "";
                            MesData.produceParamEntityList.Add(AllInjectValue);

                            MODEL.produceParamEntity PosPre = new MODEL.produceParamEntity();
                            PosPre.productCode = MesData.productCode;
                            PosPre.technicsParamName = "正压值";
                            PosPre.technicsParamCode = "SCDNJ_POSPRE_RES";
                            PosPre.technicsParamValue = "1";
                            PosPre.desc = "机台没有该数值";
                            MesData.produceParamEntityList.Add(PosPre);

                            MODEL.produceParamEntity NegPre = new MODEL.produceParamEntity();
                            NegPre.productCode = MesData.productCode;
                            NegPre.technicsParamName = "负压值";
                            NegPre.technicsParamCode = "SCDNJ_NEGPRE_RES";
                            NegPre.technicsParamValue = "1";
                            NegPre.desc = "机台没有该数值";
                            MesData.produceParamEntityList.Add(NegPre);

                            MODEL.produceParamEntity TotQua = new MODEL.produceParamEntity();
                            TotQua.productCode = MesData.productCode;
                            TotQua.technicsParamName = "总数量";
                            TotQua.technicsParamCode = "SCDNJ_TOTQUA_RES";
                            TotQua.technicsParamValue = "1";
                            TotQua.desc = "";
                            MesData.produceParamEntityList.Add(TotQua);

                            MODEL.produceParamEntity QuaQua = new MODEL.produceParamEntity();
                            QuaQua.productCode = MesData.productCode;
                            QuaQua.technicsParamName = "合格数量";
                            QuaQua.technicsParamCode = "SCDNJ_QUAQUA_RES";
                            QuaQua.technicsParamValue = MesData.productQuality == 1 ? "1" : "0";
                            QuaQua.desc = "";
                            MesData.produceParamEntityList.Add(QuaQua);

                            MODEL.produceParamEntity TotNGQua = new MODEL.produceParamEntity();
                            TotNGQua.productCode = MesData.productCode;
                            TotNGQua.technicsParamName = "总NG数量";
                            TotNGQua.technicsParamCode = "SCDNJ_TOTNGQUA_RES";
                            TotNGQua.technicsParamValue = MesData.productQuality == 0 ? "1" : "0";
                            TotNGQua.desc = "";
                            MesData.produceParamEntityList.Add(TotNGQua);

                            MODEL.produceParamEntity Celsta = new MODEL.produceParamEntity();
                            Celsta.productCode = MesData.productCode;
                            Celsta.technicsParamName = "电芯状态";
                            Celsta.technicsParamCode = "SCDNJ_CELSTA_RES";
                            Celsta.technicsParamValue = dt.Rows[i]["InjectStatusText"].ToString();
                            Celsta.desc = "";
                            MesData.produceParamEntityList.Add(Celsta);

                            MODEL.produceParamEntity Helretpre = new MODEL.produceParamEntity();
                            Helretpre.productCode = MesData.productCode;
                            Helretpre.technicsParamName = "回氦气压力";
                            Helretpre.technicsParamCode = "SCDNJ_HELRETPRE_RES";
                            Helretpre.technicsParamValue = "1";
                            Helretpre.desc = "机台没有该数值";
                            MesData.produceParamEntityList.Add(Helretpre);

                            string jsonStr = JsonConvert.SerializeObject(MesData);

                            GotionAPI gotionAPI = new GotionAPI();

                            //string response = gotionAPI.Post(url, jsonStr);

                            //Dictionary<string, string> mesResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                            //if (mesResult["code"] == "200")
                            //{
                                UploadMESBLL.UpDate(MesData.productCode,1);
                            //}
                            //else
                            //{
                                //string message = $"数据上传失败,原因是:{mesResult["message"]}";
                                //UploadMESBLL.UpDate(MesData.productCode, 2);
                            //}
                        }
                    }
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
    }
}
