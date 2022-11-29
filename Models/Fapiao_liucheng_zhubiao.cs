using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    public class Fapiao_liucheng_zhubiao
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        //流程名称
        public string Mingcheng { get; set; }

        //备注
        public string Beizhu { get; set; }

        //日期
        public DateTime Riqi { get; set; }

        //一对多关系
        public virtual ICollection<Fapiao_liuchneg_zibiao> Fapiao_liuchneg_zibiaos { get; set; }
    }
}