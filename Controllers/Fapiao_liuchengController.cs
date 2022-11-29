using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wzgl.DAL;
using PagedList;
using wzgl.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using wzgl.App_Start;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;

namespace wzgl.Controllers
{
    public class Fapiao_liuchengController : Controller
    {
        private WzglContent db = new WzglContent();

        // GET: Fapiao_liucheng
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public JsonResult Get_liucheng()
        {
            int page = (Request.Form["page"] != null) ? Int32.Parse(Request.Form["page"]) : 1;
            int rows = (Request.Form["rows"] != null) ? Int32.Parse(Request.Form["rows"]) : 10;
            var xm = from s in db.Fapiao_liuchneg_zhubiaos
                         //where s.Mingcheng.Contains(searchquery)
                     orderby s.ID descending
                     select new
                     {
                         id = s.ID,
                         mingcheng = s.Mingcheng,
                         beizhu = s.Beizhu,
                         riqi = s.Riqi                         
                     };
            try
            {
                // 返回到前台的值必须按照如下的格式包括 total and rows 
                var easyUIPages = new Dictionary<string, object>
                {
                    { "total", xm.Count() },
                    { "rows", xm.ToPagedList(page, rows) }
                };
                return Json(easyUIPages);
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]
        public string CheckNameIsSame(string name)
       {
            string isOk = "False";

            var xm = db.Liuchengzhubiaos.Where(x => x.Mingcheng == name).Select(x => x.Mingcheng);

            if (!xm.Any())
            {
                isOk = "True";
            }
            return isOk + "";
        }


        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {         //这里定义了一个连接数据库连接字符串的类，有关于人员的操作可以直接使用这个类
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        //[HttpPost]      
        //跳转到 流程子表页面
        public ActionResult Zibiaoindex(int id, ApplicationDbContext context)
        {
            //获取流程主表的  所属部门
            //var suoshubumen = db.Liuchengzhubiaos.Where(s => s.ID == id).FirstOrDefault().Suoshubumen;

            //获取角色列表
            var GenreLst = new List<string>();
            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<AppRole>(context));
            var GenreQry = from c in roleMgr.Roles
                           orderby c.Name
                           select c.Name;
            GenreLst.AddRange(GenreQry.Distinct());
            GenreLst.Remove("管理员");    //排除管理员用户角色
            GenreLst.Remove("员工");    //排除管理员用户角色

            //获取用户名列表   过滤的用户 的 shuoshubumen  字段要符合  主表的  suoshubumen  值
            var GenreLst_username = new List<string>();
            ApplicationUser user = new ApplicationUser();

            //var GenrQry_username = from c in UserManager.Users
            //                       where c.Role != "员工" && (c.Suoshubumen==suoshubumen || c.Suoshubumen.Contains(suoshubumen))
            //                       orderby c.Id
            //                       select c.Zhenshiname;

            var userName = from c in UserManager.Users
                           where c.Role != "员工" && c.Role != "管理员"
                           orderby c.Id
                           select c;

            //GenreLst_username.AddRange(GenrQry_username.Distinct());
            //GenreLst_username.Remove("admin");  //排除管理员用户            

            ViewBag.role = GenreLst;
            //ViewBag.username = GenreLst_username;

            ViewBag.username = userName;

            ViewBag.zhubiaoid = id;

            return View();
        }


        [HttpPost]
        public JsonResult Saveliucheng([Bind(Include = "Mingcheng,Beizhu")]Fapiao_liucheng_zhubiao fapiaoliuchneg)
        {
            fapiaoliuchneg.Riqi = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    db.Fapiao_liuchneg_zhubiaos.Add(fapiaoliuchneg);
                    db.SaveChanges();

                    return Json(new { success = true, Msg = "新增流程成功！" }, "text/html");
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "新增流程失败，请再试一试！" }, "text/html");
        }


        [HttpPost]
        public JsonResult Updateliucheng(int id)  //string bmname 好像多余
        {
            var liucheng = db.Fapiao_liuchneg_zhubiaos.Where(s => s.ID == id).FirstOrDefault();

            liucheng.Beizhu = Request.Form["beizhu"];
            
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(liucheng).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true, Msg = "更改流程成功！" }, "text/html");
                    //return Json(bumen1);                 
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "更改流程失败，请再试一试！" }, "text/html");
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]     这种方式获取不到数据
        public JsonResult Delliucheng(int id)
        {
            Fapiao_liucheng_zhubiao Liuchengzhubiaos = db.Fapiao_liuchneg_zhubiaos.Find(id);

            try
            {
                db.Fapiao_liuchneg_zhubiaos.Remove(Liuchengzhubiaos);
                db.SaveChanges();
                return Json(new { success = true });                
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.ToString() }, "text/html");
            }
        }


        [HttpPost]
        public JsonResult Getzibiao(int zhubiaoid)
        {
            int page = (Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1;
            int rows = (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10;
            var xm = from s in db.Fapiao_liuchneg_zibiaos
                     where s.Fapiao_liucheng_zhubiaoID == zhubiaoid
                     orderby s.ID descending
                     select new
                     {
                         id = s.ID,
                         buzhouming = s.Buzhouming,
                         liuchengxulie = s.Liuchengxulie,
                         beizhu = s.Beizhu,
                         rolename = s.Rolename,
                         liuchengUsername = s.LiuchengUsername,
                         zhenshiname = s.ZhenshiName
                     };
            try
            {
                // 返回到前台的值必须按照如下的格式包括 total and rows 
                var easyUIPages = new Dictionary<string, object>
                {
                    { "total", xm.Count() },
                    { "rows", xm.ToPagedList(page, rows) }
                };
                return Json(easyUIPages);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult Savezibiao([Bind(Include = "Buzhouming,Rolename,LiuchengUsername,Beizhu")]Fapiao_liuchneg_zibiao liuchengzibiao, int zhubiaoid)
        {
            liuchengzibiao.Fapiao_liucheng_zhubiaoID = zhubiaoid;            
            var liuchengUsername = Request.Form["liuchengUsername"];
            var c = common.Customidentity.User_name(liuchengUsername).Role;

            //通过username取出用户真实名
            liuchengzibiao.ZhenshiName = common.Customidentity.User_name(liuchengUsername).Zhenshiname;
            if (c != Request.Form["rolename"])
            {
                return Json(new { success = false, Msg = "“审核用户”角色与“审核角色”不相符，请重新“审核用户”！" }, "text/html");
            };

            //var xulie_max = db.Liuchengzibiaos.Where(s => s.LiuchengzhubiaoID == zhubiaoid).FirstOrDefault().Liuchengxulie;
            using (WzglContent db1 = new WzglContent())
            {
                var xulie = from s in db1.Fapiao_liuchneg_zibiaos
                            where s.Fapiao_liucheng_zhubiaoID == zhubiaoid
                            orderby s.ID
                            select s.Liuchengxulie;
                if (xulie.Count() > 0)
                {
                    var xulie_max = xulie.Max();
                    liuchengzibiao.Liuchengxulie = xulie_max + 1;
                }
                else
                {
                    liuchengzibiao.Liuchengxulie = 1;
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        db1.Fapiao_liuchneg_zibiaos.Add(liuchengzibiao);
                        db1.SaveChanges();
                        return Json(new { success = true, Msg = "新增子流程成功！" }, "text/html");
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                    }
                }
            }
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "新增子流程失败，请再试一试！" }, "text/html");
        }




        [HttpPost]
        public JsonResult Updatezibiao(int id)
        {
            var zibiao = db.Fapiao_liuchneg_zibiaos.Where(s => s.ID == id).FirstOrDefault();

            zibiao.Buzhouming = Request.Form["buzhouming"];

            //zibiao.Liuchengxulie = Convert.ToInt32( Request.Form["liuchengxulie"]);

            zibiao.Rolename = Request.Form["rolename"];

            zibiao.LiuchengUsername = Request.Form["LiuchengUsername"];

            zibiao.Beizhu = Request.Form["beizhu"];

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(zibiao).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true, Msg = "更改流程成功！" }, "text/html");                                    
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "更改流程失败，请再试一试！" }, "text/html");
        }


        [HttpPost]
        public JsonResult Delzibiao(int id)
        {            
            Fapiao_liuchneg_zibiao zibiao = db.Fapiao_liuchneg_zibiaos.Find(id);

            try
            {
                db.Fapiao_liuchneg_zibiaos.Remove(zibiao);
                db.SaveChanges();
                return Json(new { success = true });                
            }
            catch (Exception ex)
            {
                return Json(new { Msg = ex.ToString() }, "text/html");
            }
        }
    }
}