using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    public class Shenhezibiao
    {
        /// <summary>
        /// 这里是的审核子表原先是想作为审核流中的一个记录表，后来发现  审核流程只不过是与
        /// 一个int  的审核序列有关，所以已经把 int 值得审核序列移到审核主表中，这里只是作为 审核 日志使用
        /// </summary>

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        //审核主表对应字段        
        public int XiangmubiaoID { get; set; }

        //审核意见
        public string Shenheyijian { get; set; }

        //审核状态
        public string Shenhezhuangtai { get; set; }

        //审核角色
        public string Shenhejuese { get; set; }

        //审核流程序列
        public int Shenhexulie { get; set; }

        //审核提交人
        public string Shenheren { get; set; }

        //日期
        public DateTime Riqi { get; set; }

        //审核节点
        public string Shenhejiedian { get; set; }


        //多对一关系
        public virtual Xiangmubiao Xiangmubiao { get; set; }
    }
}