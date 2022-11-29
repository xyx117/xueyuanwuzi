using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    public class Fapiao_upload
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]            //因为与项目总表是一对多的关系，所以不考虑使用主键自动加一
        public int ID { get; set; }


        public Guid DingdanzhubiaoID { get; set; }

        //订单主表ID
        //public int XiangmubiaoID { get; set; }

        //文件大小
        public string Filesize { get; set; }

        //上传时间        
        public string Uploadtime { get; set; }

        //文件保存路径
        public string Filepath { get; set; }

        // 原始文件名称
        public string Filename { get; set; }

        // 文件扩展名
        public string Fileextension { get; set; }

        // 保存文件名称
        public string Savename { get; set; }

        //备注
        public  string Beizhu { get; set; }

        //目录
        public string Mulu { get; set; }

        //上传人id
        public string Loingid { get; set; }

        //上传人
        public string Username { get; set; }

        //所属部门
        public string Suoshubumen { get; set; }



       
        //与项目表的对应关系
        //public virtual Xiangmubiao Xiangmubiao { get; set; }

        //与订单主表的对应关系
        public virtual Dingdanzhubiao Dingdanzhubiao { get; set; }

    }
}