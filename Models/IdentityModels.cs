using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using wzgl.App_Start;

namespace wzgl.Models
{
    //public class IdentityModels
    //{

    //}
    public class ApplicationUser : IdentityUser
    {
        //添加自定义属性
        public string Suoshuxueyuan { get; set; }

        public string Suoshubumen { get; set; }

        public int ParentID { get; set; }

        public string Zhenshiname { get; set; }

        public string Role { get; set; }

        //myuserID 和parentID为父子关系
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MyuserID { get; set; }

        public int Usercount { get; set; }

        public string Pingshenmulu { get; set; }


        //评委评审任务
        public string Pingshenrenwu { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // 请注意，authenticationType 必须与 CookieAuthenticationOptions.AuthenticationType 中定义的相应项匹配
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在此处添加自定义用户声明
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("IdentityDb", throwIfV1Schema: false)
        {
        }

        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new IdentityDbInit());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class IdentityDbInit : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            PerformInitialSetup(context);
            base.Seed(context);
        }
        public void PerformInitialSetup(ApplicationDbContext context)
        {
            //初始化

            ApplicationUserManager userMgr = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<AppRole>(context));

            //int[] numbers = new int[]{1,2,3,4,5,6};
            string[] roleNames = new string[] { "员工", "部门主管", "管理员", "部门领导", "领导", "评委" };

            for (int i = 0; i < roleNames.Length; i++)
            {
                if (!roleMgr.RoleExists(roleNames[i]))
                {
                    roleMgr.Create(new AppRole(roleNames[i]));
                }
            }



            //string roleName = "Administrators";

            string userName = "admin";
            string password = "123456";
            string email = "admin@example.com";


            ApplicationUser user = userMgr.FindByName(userName);
            if (user == null)
            {

                //UserName = model.name, suoshuxueyuan = xueyuan, Email = model.name + "@qq.com", zhenshiname = model.zhenshiname, role = model.role, parentID = -1, usercount = 0
                userMgr.Create(new ApplicationUser { UserName = userName, Email = email, Suoshuxueyuan = "all", Zhenshiname = "admin", Role = "管理员", ParentID = -1, Usercount = 0,Suoshubumen="all" },
                    password);
                user = userMgr.FindByName(userName);
            }

            if (!userMgr.IsInRole(user.Id, roleNames[2]))//在这里判断admin是不是在数组中的角色，不是就添加到第三下标的角色
            {
                userMgr.AddToRole(user.Id, roleNames[2]);
            }

            //foreach (ApplicationUser dbUser in userMgr.Users)
            //{
            //    dbUser.City = Cities.PARIS;
            //}
            context.SaveChanges();

        }
    }
}