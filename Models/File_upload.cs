using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace wzgl.Models
{
    public class File_upload
    {
        [Key]
        public int ID { get; set; }        

        //与项目表关联字段 
        public int XiangmubiaoID { get; set; }

        //文件大小
        public string File_size { get; set; }
        
        public string Uploadtime { get; set; }


        //文件保存路径
        public string File_path { get; set; }

        // 原始文件名称
        public string File_name { get; set; }

        // 文件扩展名
        public string File_exten{ get; set; }

        // 保存文件名称
        public string Save_name { get; set; }


        //关联到项目管理表
        public virtual Xiangmubiao Xiangmubiao { get; set; }//这个延时加载暂时保留
    }
}