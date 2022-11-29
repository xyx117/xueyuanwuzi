using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    public class Rukubiao
    {
        //  一个表中有 41 个字段
        /// <summary>
        /// 这里的申请人，经手人，证明人都改为真实名
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        //与项目表的对应字段
        public int? XiangmuzibiaoID { get; set; }

        //项目名称
        public string Xiangmumingcheng { get; set; }

        //申请人
        public string Shenqingren { get; set; }

        ////经手人   购买清单列表的人，部门主管
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
        //public int Shuliang { get; set; }

        //金额
        //public decimal Jine { get; set; }

        //维修或新增
        public string Weixiuhuoxinzeng { get; set; }

        //备注
        public string Beizhu { get; set; }

        //订单收货否   未收货，已收货
        //public string Dingdanshoushuo { get; set; }


        //持有人
        public string Chiyouren { get; set; }

        //持有人id
        public string Chiyouren_Number { get; set; }

        //持有人所属部门
        public string Suoshubumen { get; set; }

        //入库日期
        public DateTime ? Rukuriqi { get; set; }

        //分配日期
        public DateTime ? Fenpeiriqi { get; set; }

        //签收日期
        public DateTime ? Qianshouriqi { get; set; }

        //报损日期
        public DateTime ? Baosunriqi { get; set; }

        //分配状态
        public string Fenpeizhuangtai { get; set; }

        //签收状态
        public string Qianshouzhuangtai { get; set; }

        //为了在  被移交人  签收前，记录还能再  移交人  的列表中显示，加上一个  接收人  字段，配合 持有人  字段显示
        public string Yijiao_Jieshouren { get; set; }

        //报损状态
        public string Baosunzhuangtai { get; set; }

        //草料二维码 url
        public string Qr_pic_url { get; set; }

        //二维码 识别id， 用主键识别在 拨付流程 的添加中无法添加id，这里改用guid
        public Guid Qrcode_identity { get; set; }

        //入库类别  自购流程   拨付流程  分为 自购 和 拨付
        public string Rukuleibie { get; set; }

        //报损备注
        public string Baosunbeizhu { get; set; }
       

        //入库状态  初始是  未入库，部门领导审核通过后改为  已入库
        public string Rukuzhuangtai { get; set; }

        //对应的订单主表 id   ，这是用来更改入库状态的
        public Guid? Dingdan_str { get; set; }
        

        //领用单位
        public string Lingyongdanwei { get; set; }

        //仪器编号
        public string Yiqibianhao { get; set; }

        //分类号
        public string Fenleihao { get; set; }

        //厂家
        public string Changjia { get; set; }

        //购置日期
        public DateTime Gouzhiriqi { get; set; }

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

        //单据号 、发票号
        public string Fapiaohao { get; set; }


        //多对一关系
        public virtual Xiangmuzibiao Xiangmuzibiao { get; set; }
    }
}