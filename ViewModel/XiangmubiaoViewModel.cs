using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wzgl.ViewModel
{
    public class XiangmubiaoViewModel
    {
        //项目名称
        public string Xiangmumingcheng { get; set; }
                
        //规格
        public string Guige { get; set; }

        //单价
        public decimal Danjia { get; set; }

        //数量
        public int Shuliang { get; set; }

        //金额
        public decimal Jine { get; set; }

        //维修或新增
        public string Weixiuhuoxinzeng { get; set; }

        //备注
        public string Beizhu { get; set; }

        //经手人
        public string Jingshouren { get; set; }

        //证明人
        public string Zhengmingren { get; set; }


        //申请人
        public string Shenqingren { get; set; }

        //申请日期
        public DateTime Shenqingriqi { get; set; }


        //流程主表
        public int LiuchengzhubiaoID { get; set; }

       
        //所属目录
        public string Mulu { get; set; }

        //申请人id
        public string Userid { get; set; }
    }
}