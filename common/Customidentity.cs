using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wzgl.Models;

namespace wzgl.common
{
    public class Customidentity
    {
        //由用户登录id判断用户所属的角色，在部门负责人保存所创建项目时使用
       
        public static ApplicationUser User(string loingid)
        {
            using (ApplicationDbContext identitydb1 = new ApplicationDbContext()) //创建一个新的上下文
            {
                ApplicationUser user = identitydb1.Users.Where(s => s.Id == loingid).FirstOrDefault();               
                return (user);
            }            
        }

        public static ApplicationUser User_name(string name)
        {
            using (ApplicationDbContext identitydb1 = new ApplicationDbContext()) //创建一个新的上下文
            {
                ApplicationUser user = identitydb1.Users.Where(s => s.UserName==name).FirstOrDefault();
                return (user);
            }
        }    
    }
}