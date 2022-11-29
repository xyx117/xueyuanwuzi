using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    public class Dingdanzhubiao
    {
        [Key]
        [Required]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]    //唯一性string  ID
        public Guid ID { get; set; }

        //订单名，就是发票名    就是各个项目表名 连接起来的  名
        public string Dingdanming { get; set; }

        //订购人  
        public string Dingdanren { get; set; }

        //订单人id
        public string DingdanreID { get; set; }

        //订单人真实名
        public string DingdanrenName { get; set; }

        //订单金额
        public decimal Dingdanjine { get; set; }

        //订单日期
        public DateTime Dingdanriqi { get; set; }

        //收件人
        public string Shoujianren { get; set;}

        //收件地址
        public string Shoujiandizhi { get; set; }

        //订单数量   这里应该是订单所包含的  项目数量
        public int Dingdanshuliang { get; set; }   

        //订单确认收件数量
        public int Dingdanshoujian { get; set; }

        //项目ID 组成的数组字符串，用于在添加  订单子表的时候 以这个字段作为条件 查询 这条记录的 ID
        public string Xmid_shuzu_str { get; set; }

        //这是后面加上去的，是否用到待定
        public string Mulu { get; set; }

        //这是后期加上去的，是否用到待定
        public string Suoshubumen { get; set; }

        //订单收货否   未收货，已收货
        public string Dingdanshoushuo { get; set; }

        //指定采购人 ,这里是一个 id字符串，，真实名是有重名可能的，所以这里需要一个  ID 确地唯一性
        public string Caigouren { get; set; }

        //采购人真实名   ，真实名是有重名可能的，所以这里需要一个  ID 确地唯一性
        public string Caigouren_name { get; set; }        

        //报销意见
        public string Baoxiaoyijian { get; set; }

        //备注
        public string Beizhu { get; set; }

        //报销状态   未报销，申请报销，同意报销、不同意报销
        public string Baoxiaozhuangtai { get; set; }

        //报销审核人  这里后面可以不用
        //public string Baoxiao_shenheren { get; set; }

        //报销审核序列
        public int Shenhexulie { get; set; }

        //流程主表
        public int Liuchengzhubiao { get; set; }

        //流程表名
        public string Liuchengbiaoname { get; set; }

        //订单主表所对应的  发票流程主表  的主键值
        public int Fapiao_liuchneg { get; set; }

        //审核时候，在撤回状态下 ，记录撤回的人,这里记录的是  撤回人的ID
        //如果有多个撤回人一起撤回，这里就 把他们的 id 连接起来，在前端判断 这字段中是否包含有 登录人 的ID，避免在这个人
        //在第一次 撤回后 还能多次撤回
        public string Chehui_user { get; set; }

        //这里记录的是撤回人的  真实名
        public string Chehui_realname { get; set; }

        //订单发票号
        public string Fapiaohao { get; set; }

        //和订单子表的对应关系
        //public virtual ICollection<Dingdanzibiao> Dingdanzibiaos { get; set; }

        //和发票上传文件对应关系
        public virtual ICollection<Fapiao_upload> Fapiao_uploads { get; set; }

        //与项目表的对应关系
        public virtual ICollection<Xiangmubiao> Xiangmubiaos { get; set; }

        public virtual ICollection<Shenhezibiao_Fapiao> Shenhezibiao_Fapiaos { get; set; }
    }
}