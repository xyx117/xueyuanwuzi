using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    public class Fapiao_liuchneg_zibiao
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        //流程主表
        public int Fapiao_liucheng_zhubiaoID { get; set; }

        //流程序列
        public int Liuchengxulie { get; set; }

        //步骤名
        public string Buzhouming { get; set; }

        //备注
        public string Beizhu { get; set; }

        //角色名
        public string Rolename { get; set; }

        //流程角色  具体什么作用待定
        public string LiuchengUsername { get; set; }

        //在项目显示流程时候，改为显示  真实名
        public string ZhenshiName { get; set; }

        //多对一关系
        public virtual Fapiao_liucheng_zhubiao Fapiao_liucheng_zhubiao { get; set; }
    }
}