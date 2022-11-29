using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    public class Xiangmuzibiao
    {
        [Key]
        [Required]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        //与项目表的对应字段
        public int XiangmubiaoID { get; set; }

        //项目名称
        public string Xiangmumingcheng { get; set; }        

        //申请人
        public string Shenqingren { get; set; }

        //申请人真实名
        public string ZhenshiName { get; set; }


        //经手人   购买清单列表的人，部门主管
        public string Jingshouren { get; set; }

        //证明人   收货的人，员工
        public string Zhengmingren { get; set; }

        //申请日期
        public DateTime Shenqingriqi { get; set; }

        //规格
        public string Guige { get; set; }

        //型号
        public string Xinghao { get; set; }

        //放置地点
        public string Fangzhididian { get; set; }

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

        //订单收货否   未收货，已收货
        public string Dingdanshoushuo { get; set; }


        //领用单位
        public string Lingyongdanwei { get; set; }

        //仪器编号
        public string Yiqibianhao { get; set; }

        //分类号
        public string Fenleihao { get; set; }

        //厂家
        public string Changjia { get; set; }


        //购置日期
        public DateTime ? Gouzhiriqi { get; set; }

        //现状
        public string Xianzhuang { get; set; }

        //经费科目
        public string Jingfeikemu { get; set; }

        //设备来源
        public string Shebeilaiyuan { get; set; }

        //记账人
        public string Jizhangren { get; set; }

        //入库人
        public string Rukuren { get; set; }

        //领用人
        public string Lingyongren { get; set; }

        //单据号 、发票号
        public string Fapiaohao { get; set; }




        //与入库表的一对多关系，这里是一
        public virtual ICollection<Rukubiao> Rukubiaos { get; set; }

        //多对一关系
        public virtual Xiangmubiao Xiangmubiao { get; set; }

    }
}