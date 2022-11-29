using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    public class Xiangmubiao
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        //项目名称
        public string Xiangmumingcheng { get; set; }

        //申请人
        public string Shenqingren { get; set; }

        //申请人真是名
        public string ZhenshiName { get; set; }

        //申请人id
        public string Userid { get; set; }

        //申请日期
        public DateTime Shenqingriqi { get; set; }       

        //金额
        public decimal Jine { get; set; }

        ////维修或新增
        //public string Weixiuhuoxinzeng { get; set; }

        //备注
        public string Beizhu { get; set; }        

        //流程主表
        public int Liuchengzhubiao { get; set; }

        //流程表名
        public string Liuchengbiaoname { get; set; }

        //审核主表  初始值为0，每审核一次值加1
        public int Shenhexulie { get; set; }


        //审核序列的最大值，
        //public int Shenhexulie_max { get; set; }

        //初始撤回人           
        //如果有多个撤回人一起撤回，这里就 把他们的 id 连接起来，在前端判断 这字段中是否包含有 登录人 的ID，避免在这个人
        //在第一次 撤回后 还能多次撤回
        public string Chehui_user { get; set; }

        //撤回人 真实名
        public string Chehui_realname { get; set; }

        //撤回意见
        public string Chehui_yijian { get; set; }

        //评审不通过的用户
        public string Weiguo_user { get; set; }

        //评审没有通过用户的真实名
        public string Weiguo_realname { get; set; }

        //所属目录
        public string Mulu { get; set; }

        

        //所属部门
        public string Suoshubumen { get; set; }

        //员工提交   未提交，已提交
        //public string Yuangongtijiao { get; set; }

        //审核状态    未提交，已提交，未审核，审核中，通过，未通过，撤回
        public string Shenhezhuangtai { get; set; }

        //购买状态，初始值为null，当部门主管  购买后 此值改为 “已购”
        public string Goumaizhuangtai { get; set; }

        //订单收货否   未收货，已收货
        public string Dingdanshoushuo { get; set; }       

        //与订单主表对应关系  的外键
        //public long ? DingdanzhubiaoID { get; set; }

        public Guid? DingdanzhubiaoID { get; set; }

        //与审核主表的一对一关系
        //[Required]
        //public virtual Shenhezhubiao Shenhezhubiao { get; set; }

        //与审核子表的一对多关系
        public virtual ICollection<Shenhezibiao> Shenhezibiaos { get; set; }

        //与项目子表的一对多关系
        public virtual ICollection<Xiangmuzibiao> Xiangmuzibiaos { get; set; }

        //附件表
        public virtual ICollection<File_upload> Files { get; set; }

        //与订单主表的对应关系，这里是多
        public virtual Dingdanzhubiao Dingdanzhubiao { get; set; }

        //与上传发票表的对应关系
        //public virtual ICollection<Fapiao_upload> Fapiao_uploads { get; set; }

        //与fiaopiao_webtext表的  一对一关系
        //public virtual ICollection<Fapiao_webtext> Fapiao_Webtexts { get; set; }

    }
}