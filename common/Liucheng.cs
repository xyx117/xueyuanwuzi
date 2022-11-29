using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wzgl.DAL;
using wzgl.Models;

namespace wzgl.common
{
    public class mulucustom
    {
        //返回流程子表的流程序列值
        public static int Liuchengxulie(int zhubiaoid,string username,string role)
        {
            Liuchengzibiao zibiao = new Liuchengzibiao();
            
            using (WzglContent db1 = new WzglContent()) //创建一个新的上下文
            {
                 zibiao = db1.Liuchengzibiaos.Where(s => s.LiuchengzhubiaoID == zhubiaoid&&s.Rolename==role&&s.LiuchengUsername==username).FirstOrDefault();                
            }
            return zibiao.Liuchengxulie;
        }

    }
}