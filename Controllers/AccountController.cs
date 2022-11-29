using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wzgl.App_Start;
using wzgl.Models;
using PagedList;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Transactions;
using wzgl.DAL;
using System.Drawing;
using ThoughtWorks.QRCode.Codec;
using System.Text;

namespace wzgl.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// 根据链接获取二维码
        /// </summary>
        /// <param name="link">链接</param>
        /// <returns>返回二维码图片</returns>
        //public Bitmap GetDimensionalCode()
        //{
        //    //string link = "http://jingyan.baidu.com/user/npublic?un=%E8%BE%B9%E7%BC%98%E6%B2%B3%E5%9B%BE";
        //    string link = "http://localhost:45822/";

        //    Bitmap bmp = null;
            
        //    try
        //    {

        //        QRCodeEncoder qrCodeEncoder = new QRCodeEncoder
        //        {
        //            QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE,

        //            QRCodeScale = 4,

        //            //int version = Convert.ToInt16(cboVersion.Text);
        //            QRCodeVersion = 7,

        //            QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M
        //        };

        //        Bitmap img = qrCodeEncoder.Encode(link, Encoding.UTF8);//指定utf-8编码， 支持中文   EAN_13-test

        //        bmp = qrCodeEncoder.Encode(link);

        //        string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "EAN_13-" + "test111" + ".jpg";


        //        img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
        //    }
        //    catch (Exception ex)
        //    {                
        //        //MessageBox.Show("Invalid version !");                   
        //    }
        //    return bmp;
        //}



        //QRCodeEncoder qrCodeEncoder = new QRCodeEncoder
        //{
        //    QRCodeVersion = 0
        //};
        //Bitmap img = qrCodeEncoder.Encode(text, Encoding.UTF8);//指定utf-8编码， 支持中文   EAN_13-test

        //string filePath1 = string.Format("~/QR_code/{0}/{1}/{2}", mulu, ruku.Xiangmumingcheng, ruku.ID);//通过参数组建一个路径格式的字符串
        //string filePath2 = Server.MapPath(filePath1);//将路径格式的字符串通过函数转化为服务器路径
        //                                             //string filePath = filePath2 + "\\EAN_13-" + "test" + ".jpg";
        //string filename = "\\" + ruku.Xiangmumingcheng + "_" + ruku.ID + ".jpg";

        //        //string filePath3 = string.Format("QR_code\\{0}\\{1}\\{2}", mulu, ruku.Xiangmumingcheng, ruku.ID);

        //        if (!Directory.Exists(filePath2))            //判断路径是否存在，如果不存在，则根据路径建立路径文件夹，这里只需要检验到文件夹
        //        {
        //            Directory.CreateDirectory(filePath2);
        //        }

        ////string filePath = filePath2 + "\\EAN_13-" + "test" + ".jpg";
        //string filePath = filePath2 + filename;

        ////string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "EAN_13-" + "test111" + ".jpg";
        //img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);


            

        private WzglContent db = new WzglContent();

        public ActionResult Rsetpwdindex()
        {
            return View();
        }

        //初始化化密码，初始化为123456
        [HttpPost]
        //[AllowAnonymous]
        public async Task<JsonResult> Resetpassword_csh(string name)
        {
            if (ModelState.IsValid)
            {
                string errormsg = "";
                var user = await UserManager.FindByNameAsync(name);//这里多一步，可以直接通过登录人信息取得用户id（主键）
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);//根据user.id产生code,在forgotpassword中有

                if (user == null)
                {
                    //return Json(new { errorMsg = "找不到用户。" }, "text/html");
                    return Json(new { success = false, errorMsg = "找不到用户。" }, "text/html");
                }
                IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, code, "123456");//ResetPasswordAsync方法需要有三个参数
                if (result.Succeeded)
                {
                    //return Json(new { Succeeded = "OK！" }, "text/html");
                    return Json(new { success = true, errorMsg = "OK！初始化密码成功！" }, "text/html");
                }
                else
                {
                    //AddErrors(result);
                    foreach (var error in result.Errors)
                    {
                        errormsg = errormsg + error;
                    }
                    //return Json(new { errorMsg = errormsg }, "text/html");
                    return Json(new { success = false, errorMsg = errormsg }, "text/html");
                }
            }
            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            //return View(model);
            //return Json(new { errorMsg = "重置密码有误！" }, "text/html");
            return Json(new { success = false, errorMsg = "重置密码有误！" }, "text/html");
        }


        //登录人员修改密码
        [HttpPost]
        //[AllowAnonymous]
        public async Task<JsonResult> Setpwd(string userid)
        {
            if (ModelState.IsValid)
            {
                string errormsg = "";
                var user = await UserManager.FindByIdAsync(userid);//这里多一步，可以直接通过登录人信息取得用户id（主键）

                string password = Request.Form["password"];

                string code = await UserManager.GeneratePasswordResetTokenAsync(userid);//根据user.id产生code,在forgotpassword中有

                if (user == null)
                {
                    return Json(new { success = false, Msg = "找不到用户。" }, "text/html");
                }
                IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, code, password);//ResetPasswordAsync方法需要有三个参数
                if (result.Succeeded)
                {
                    return Json(new { success = true, Msg = "修改密码成功！" }, "text/html");
                }
                else
                {
                    //AddErrors(result);
                    foreach (var error in result.Errors)
                    {
                        errormsg = errormsg + error;
                    }
                    return Json(new { success = false, errorMsg = errormsg });
                }
            }
            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            //return View(model);
            return Json(new { success = false, Msg = "重置密码有误！" }, "text/html");
        }


        public ActionResult Addrole(ApplicationDbContext context)
        {
            string role = Request.Form["RoleName"];

            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<AppRole>(context));
            
            if (!roleMgr.RoleExists(role))
            {
                try
                {
                    roleMgr.Create(new AppRole(role));

                    return Json(new { success = true, Msg = "已添加角色！" }, "text/html");
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }
            else
            {
                return Json(new { success = false, Msg = "角色已经存在！" }, "text/html");
            }                          
        }

        [HttpPost]
        public JsonResult EditRole(ApplicationDbContext context,string id)
        {
            string rolename = Request.Form["RoleName"];

            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<AppRole>(context));

            var role1 = roleMgr.FindById(id);

            role1.Name = rolename;

            try
            {
                //roleMgr.Create(new AppRole(role));

                //roleMgr.Update(new AppRole (role1.Name=role));

                roleMgr.Update(role1);

                return Json(new { success = true, Msg = "已编辑角色！" }, "text/html");
            }
            catch (Exception ex)
            {

                return Json(new { success = false, Msg = ex.ToString() }, "text/html");
            }           
        }

        [HttpPost]
        public JsonResult DelRole( string id,string rolename, ApplicationDbContext context)
        {
            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<AppRole>(context));

            var role1 = roleMgr.FindById(id);

            try
            {
                roleMgr.Delete(role1);

                return Json(new { success = true, Msg = "已删除角色！" }, "text/html");
            }
            catch (Exception ex)
            {

                return Json(new { success = false, Msg = ex.ToString() }, "text/html");
            }           
        }

        public ActionResult Index()
        {
            return View();
        }


        //角色管理
        [Authorize(Roles = "管理员")]
        public ActionResult RoleIndex()
        {
            return View();
        }

        /// <summary>
        /// 用户管理
        /// </summary>
        /// string f为loingid
        [Authorize(Roles = "管理员,部门主管")]
        public ActionResult UserIndex(ApplicationDbContext context,string f)
        {
            //为所辖部门获取部门设置列表
            var GenreLst = new List<string>();            
            
            var role = common.Customidentity.User(f).Role;
            var bumen = common.Customidentity.User(f).Suoshubumen;

            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<AppRole>(context));
            if (role == "管理员")
            {                
                var GenreQry = from c in roleMgr.Roles
                               where c.Name!="员工"
                               orderby c.Name
                               select c.Name;
                GenreLst.AddRange(GenreQry.Distinct());
                //GenreLst.Remove("员工");
                ViewBag.role = GenreLst;

                var GenreLst_bm = new List<string>();
                var GenreQry_bm = from d in db.Bumenshezhis
                                  orderby d.BmName
                                  select d.BmName;
                GenreLst_bm.AddRange(GenreQry_bm.Distinct());
                ViewBag.bumen = GenreQry_bm;
            }
            else
            {
                var GenreQry = from c in roleMgr.Roles
                               where c.Name == "员工"
                               orderby c.Name
                               select c.Name;
                GenreLst.AddRange(GenreQry.Distinct());                
                ViewBag.role = GenreLst;

                var GenreLst_bm = new List<string>();
                var GenreQry_bm = from d in db.Bumenshezhis
                                  where d.BmName == bumen
                                  orderby d.BmName
                                  select d.BmName;
                GenreLst_bm.AddRange(GenreQry_bm.Distinct());
                ViewBag.bumen = GenreQry_bm;
            }                  

            return View();
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


        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {            
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.Name, model.Password);
                string username = model.Name;
                DateTime logintime = DateTime.Now;

                //根据username取出loingid
                string loingid ;
                using (ApplicationDbContext identitydb1 = new ApplicationDbContext()) //创建一个新的上下文
                {
                    loingid = identitydb1.Users.Where(s => s.UserName == username).FirstOrDefault().Id;
                }

                //这里直接得到正在使用的ipv4地址，简单粗暴
                string ip2 = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork")).ToString();

                if (user != null)
                {   
                    await SignInAsync(user, model.RememberMe);
                    //return RedirectToLocal("/Home/Index?f="+loingid+"");
                    return RedirectToLocal("/Home/Index");
                }
                else
                {
                    ModelState.AddModelError("", "用户名或密码无效。");
                }
            }
            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public class Userinfo
        {
            public string Username { get; set; }
            public string Userrole { get; set; }
            public string Bumen { get; set; }
            public string Zhenshiname { get; set; }
        }
        //返回登录人的信息
        [HttpPost]
        public JsonResult Getusernameandrole(string id)
        {
            var user = UserManager.Users.Where(c => c.Id == id).Select(c => new Userinfo { Username = c.UserName, Userrole = c.Role, Bumen = c.Suoshubumen ,Zhenshiname = c.Zhenshiname}).FirstOrDefault();
            
            if (user == null)
            {
                return Json(new { success = false, msg = "用户不存在！" }, "text/html");
            }
            return Json(new { success = true, loginrole = user.Zhenshiname + "/"+ user.Bumen + "/" + user.Userrole, loginname = user.Username }, "text/html");
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }


        [HttpPost]
        public JsonResult GetRole(ApplicationDbContext context)
        {
            
            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<AppRole>(context));

            int page = (Request.Form["page"] != null) ? Int32.Parse(Request.Form["page"]) : 1;
            int rows = (Request.Form["rows"] != null) ? Int32.Parse(Request.Form["rows"]) : 10;

            var xm = from c in roleMgr.Roles
                     orderby c.Name
                     select new { RoleID=c.Id,RoleName =c.Name};                  

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

        [Authorize(Roles = "管理员,部门主管")]
        [HttpPost]
        public JsonResult GetUser(string userid)
        {

            int page = (Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1;
            int rows = (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10;

            //var user = UserManager.FindById(userid);//通过userid找出部门负责人
            //int myid = user.MyuserID;//取出部门负责人的myuserID

            //var xm = UserManager.Users.Where(c => c.parentID == myid).OrderBy(c=>c.UserName);  //找出parentID等于其部门负责人myuserID的人员

            var role = common.Customidentity.User(userid).Role;
            var bumen = common.Customidentity.User(userid).Suoshubumen;
            
            if (role == "管理员")
            {
                var xm = from c in UserManager.Users
                         where c.Role != "员工"       //   && c.UserName!="admin"  原本是想这样过滤掉admin ，但是考虑到admin忘记密码的问题
                         orderby c.UserName
                         select new { id = c.Id, username = c.UserName, zhenshiname = c.Zhenshiname, suoshubumen = c.Suoshubumen, role = c.Role };
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
            if (role == "部门主管")
            {
                var xm = from c in UserManager.Users
                         where c.Role == "员工" && c.Suoshubumen == bumen
                         orderby c.UserName
                         select new { id = c.Id, username = c.UserName, zhenshiname = c.Zhenshiname, suoshubumen = c.Suoshubumen, role = c.Role };
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
            return Json(new { success = false, Msg = "" }, "text/html");
        }
        

        [HttpPost]
        //[ValidateAntiForgeryToken]   这条语句不能用！！！！！
        public JsonResult AddUser(AdminRegisterViewModel model)
        {
            //同名验证改为在后端进行，首先 验证同名性            
            //var xm = UserManager.FindByName(model.UserName);
            //if (xm!=null)
            //{
            //    return Json(new { success = false, Msg = "您输入的用户名已经存在！请改名。" }, "text/html");
            //}

            //string role = Request.Form["role"];  //新增领导分管学院的时候，根据新增用户时所分配的角色，判断角色属于领导的，在所属学院上加一个all
            string suoshubumen = Request.Form["suoshubumen"];//模型无法接受到多选值，此处通过表单接收多选值     

            bool zhuangtai = true;
            string msg = "";

            IdentityResult result;

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser() { UserName = model.UserName, Suoshuxueyuan = "信息学院",Suoshubumen= suoshubumen, Email = model.UserName + "@qq.com", Zhenshiname = model.Zhenshiname, Role = model.Role, ParentID = -1, Usercount = 0, PhoneNumber = model.PhoneNumber };
                
                using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
                {
                    try
                    {
                        if (model.Password == null && model.ConfirmPassword == null)
                        {
                            result = UserManager.Create(user, "123456");//根据对象user和password创建新用户
                        }
                        else
                        {
                            result = UserManager.Create(user, model.Password);//根据对象user和password创建新用户
                        }
                        if (result.Succeeded)
                        {
                            if (!UserManager.IsInRole(user.Id, model.Role))   //在业务专员添加人员工时，判断添加角色是否属于员工，不是则添加到角色
                            {
                                UserManager.AddToRole(user.Id, model.Role);
                            };
                            transaction.Complete();
                            msg = "保存成功！";
                        }
                        else
                        {
                            zhuangtai = false;
                            foreach (var error in result.Errors)
                            {
                                msg = msg + error;
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        //return Json(new { errorMsg = ex.ToString() }, "text/html");
                        return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
                return Json(new { success = zhuangtai, Msg = msg }, "text/html");
            }
            return Json(new { success = false, Msg = "您输入的值有错误！" }, "text/html");
        }


        [HttpPost]
        public async Task<JsonResult> Edit_user(string id)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            

            if (user != null)
            {
                try
                {
                    //更改  用户名  代码
                    //var old_username = user.UserName;   //用户原来的名字
                    //var username = Request.Form["username"];   //  当前输入框的值

                    //var newuser = UserManager.FindByName(username);   // 当前输入框所代表的用户

                    //if (username != old_username)  //当前名字已经发生改动，
                    //{
                    //    if(newuser==null)
                    //    {
                    //        user.UserName = username;
                    //    }
                    //    else                         //并且是已经存在的用户名，返回错误
                    //    {
                    //        return Json(new { success = false, Msg = "您输入的用户名已经存在！请改名。" }, "text/html");
                    //    }                    
                    //}                    

                    user.Zhenshiname = Request.Form["zhenshiname"];

                    user.Role = Request.Form["role"];

                    user.Suoshubumen = Request.Form["suoshubumen"];

                    IdentityResult result = await UserManager.UpdateAsync(user);

                    return Json(new { success = true, Msg = "编辑保存成功！" }, "text/html");
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }
            else
            {                
                return Json(new { success = false, Msg = "您编辑的用户不存在！" }, "text/html");
            }            
        }
        
        
        [HttpPost]
        public JsonResult Deluser(string yuangongid)
        {
            string errormsg0 = "";
            string errormsg1 = "";
            string errormsg2 = "";
            bool zhuantai = true;

            var yuangong = UserManager.FindById(yuangongid);  //找出这个员工，通过登录的id，id为加密值           

            IdentityResult resultyg;//定义一个表示identity操作的值                 

            using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
            {
                try
                {                    
                    resultyg = UserManager.Delete(yuangong);//删除选出的员工

                    if (resultyg.Succeeded)
                    {
                        errormsg0 = "删除用户成功！";
                        transaction.Complete();
                    }
                    else
                    {
                        foreach (var error in resultyg.Errors)
                        {
                            errormsg2 = errormsg2 + error;
                        }
                        zhuantai = false;
                        errormsg0 = errormsg1 + errormsg2;
                    };
                }
                catch (Exception ex)
                {
                    return Json(new { Succeeded = false, Msg = ex.ToString() }, "text/html");
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            return Json(new { Succeeded = zhuantai, Msg = errormsg0 }, "text/html");
        }


        public static AppRoleManager RoleMgr(ApplicationDbContext context)
        {
            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<AppRole>(context));

            return roleMgr; 
        }               

        [HttpPost]
        public string CheckUser(ApplicationDbContext context, string name)
        {
            string isOk = "False";            
            var role = RoleMgr(context).FindByName(name);
            if (role == null)
            {
                isOk = "True";
            }
            return isOk + "";
        }


        //判断用户名存在就返回false,不存在就返回true
        [HttpPost]
        public string CheckName(string name) 
        {
            var namelist = (from c in UserManager.Users
                           orderby c.Id
                           select c.Zhenshiname).ToArray();

            string isOk = "True";               //返回true时为不可用
            for (int i= 0; i < namelist.Count(); i++)
            {
                var namelist_item = namelist[i];
                if (namelist_item == name)         //这里加一个for 循环，如果有一个值与数组中的值相同，直接返回结果，
                {
                    isOk = "False ";
                    return isOk + "";
                }
                else
                {
                    isOk = "True";
                }               
            }
            return isOk + "";


            //string isOk = "False";

            //var xm = UserManager.FindByName(name);

            //if (xm == null)
            //{
            //    isOk = "True";
            //}
            //return isOk + "";
        }    
    }
}