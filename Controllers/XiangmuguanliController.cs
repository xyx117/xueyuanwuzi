using Aspose.Cells;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using ThoughtWorks.QRCode.Codec;
using wzgl.common;
using wzgl.DAL;
using wzgl.Models;

namespace wzgl.Controllers
{
    public class XiangmuguanliController : Controller
    {
        private WzglContent db = new WzglContent();
        // GET: xiangmuguanli        
        public ActionResult Index()
        {
            return View();
        }
         
        

        //项目自购类
        public ActionResult Xm_zigou(string loingid)       //string muluName,
        {
            //ViewBag.mulu = muluName;

            string loingrole = Customidentity.User(loingid).Role;

            switch (loingrole)
            {
                case "员工": return View("Yuangong");

                case "部门主管": return View("Bm_Zhuguan");

                case "管理员": return View("Admin");
                    
                case "部门领导": return View("Bm_Lindao");

                case "领导": return View("Bm_Lindao");

                default: return View("Shenhe");
            }
        }

        //项目拨付类
        public ActionResult Xm_bofu(string loingid)       //string muluName,
        {
            //ViewBag.mulu = muluName;

            string loingrole = Customidentity.User(loingid).Role;

            switch (loingrole)
            {
                case "员工": return View("Xm_bofu");

                case "部门主管": return View("");

                case "管理员": return View("Xm_bofu");

                case "部门领导": return View("Xm_bofu");

                default: return View("Xm_bofu");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmid">项目表id</param>
        /// <returns></returns>
        public ActionResult File_upload(int xmid)
        {
            //用来控制是否显示上传按钮
            var shenhezhuangtai = db.Xiangmubiaos.Where(s => s.ID == xmid).FirstOrDefault().Shenhezhuangtai;

            var picList = db.File_uploads.Where(s => s.XiangmubiaoID == xmid);

            ViewBag.id = xmid;
            ViewBag.shenhezhuangtai = shenhezhuangtai;
            return View(picList);
        }

        //项目表 附件上传
        [HttpPost]
        public ActionResult Upload_file(HttpPostedFileBase file, int id, string loingid)
        {
            var xmming = db.Xiangmubiaos.Where(s => s.ID == id).FirstOrDefault().Xiangmumingcheng;
            var mulu = DateTime.Now.ToString("yyyy");

            if (Request.Files.Count == 0)
            {
                return Json(new { jsonrpc = 2.0, success = false, message = "请选择要上传的文件。" }, "text/html");
            }
            string fileName;

            File_upload upload = new File_upload();

            if (file != null)
            {
                string filePath1 = string.Format("~/Uploads_file/{0}/{1}/", mulu, xmming);//通过参数组建一个路径格式的字符串

                // 文件上传后的保存路径
                string filePath = Server.MapPath(filePath1);//将路径格式的字符串通过函数转化为服务器路径

                if (!Directory.Exists(filePath))            //判断路径是否存在，如果不存在，则根据路径建立路径文件夹，这里只需要检验到文件夹
                {
                    Directory.CreateDirectory(filePath);
                }
                string fileseze = (file.ContentLength / 1024).ToString();

                fileName = Path.GetFileName(file.FileName);// 原始文件名称

                string fileExtension = Path.GetExtension(fileName); // 文件扩展名

                //string saveName = Guid.NewGuid().ToString() + fileExtension; // 保存文件名称

                string webPath = string.Format("~/Uploads_file/{0}/{1}/{2}", mulu, xmming, fileName);//这里只是字符串，判断参数，用于判断上传的文件是否存在，这里需要检验到四层目录

                string generateFilePath = Server.MapPath(webPath);//这里是真实的路径，由字符串转化为路径

                if (System.IO.File.Exists(generateFilePath))//判断文件是否存在，如果存在返回提示json字符串
                {
                    return Json(new { jsonrpc = 2.0, success = false, message = fileName + "这个文件已经存在！保存失败" }, "text/html");
                }
                try
                {
                    file.SaveAs(generateFilePath);//saveas保存的参数是服务器根路径，webpath不是路径   通过路径和源文件名做参数，保存上传文件
                    upload.File_path = string.Format("Uploads_file/{0}/{1}/", mulu, xmming);
                    upload.File_name = fileName;
                    upload.File_exten = fileExtension;
                    upload.Uploadtime = DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss");
                    upload.File_size = fileseze + "kb";
                    upload.XiangmubiaoID = id;
                    db.File_uploads.Add(upload);
                    db.SaveChanges();
                    return Json(new { jsonrpc = "2.0", success = true, filePath = webPath }, "text/html");
                }
                catch (Exception ex)
                {
                    if (System.IO.File.Exists(generateFilePath))//判断文件是否存在，如果存在返回提示json字符串
                    {
                        System.IO.File.Delete(generateFilePath);
                    }
                    //return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
                    return Json(new { jsonrpc = 2.0, success = false, message = ex.Message }, "text/html");
                }
            }
            else
            {
                return Json(new { jsonrpc = 2.0, success = false, message = "请选择要上传的文件！" }, "text/html");
            }
        }

        //项目表附件删除
        [HttpPost]
        public JsonResult Feupld_del_file(int id)
        {
            File_upload fileupload = db.File_uploads.Find(id);
            var path = fileupload.File_path;
            var name = fileupload.File_name;

            try
            {
                var path1 = path.Replace(@"/", @"\");//D:\\32位激活工具\\xmkgl\\xmkgl\\Uploads/2015ll/a1/也能正常删除，不过改为反向双斜杠

                string filepath = AppDomain.CurrentDomain.BaseDirectory + path1;//D:\\32位激活工具\\xmkgl\\xmkgl\\Uploads/2015ll/a1/

                if (System.IO.File.Exists(filepath + name))
                {
                    System.IO.File.Delete(filepath + name);
                }
                db.File_uploads.Remove(fileupload);
                db.SaveChanges();
                return Json(new { success = true, errorMsg = "文件删除成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString() });
            }
        }

        //项目表 下载附件        
        public FileResult Get_fujian(string filename, string filepath)
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory + "Content/uploads/moban/";
            //string fileName = "预算总表导入模板.xls";
            //return File(path + fileName, "text/plain", fileName);
            filepath = filepath.Replace(@"/", @"\");
            string path = AppDomain.CurrentDomain.BaseDirectory + filepath;

            return File(path + filename, "text/plain", filename);//fileName应该是下载出来后的名字
        }


        //部门主管结算
        /// <summary>
        /// 部门结算操作三个表
        /// 1、生成一条 订单主表的 记录  2、根据订单表主键 修改对应的 项目表 订单外键  3、生成审核日志表 
        /// </summary>
        /// <param name="loingid"></param>
        /// <param name="xmnames"></param>
        /// <param name="heji"></param>
        /// <param name="xmid">多个项目ID 组成的 ID字符串 </param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Jiesuan(string loingid, string xmnames, string heji, string xmid)
        {
            string[] xmid_fenli = xmid.Split(',');
            int xmid_fenli_conut = xmid_fenli.Count();
            string suoshubume = Customidentity.User(loingid).Suoshubumen;
            string username = Customidentity.User(loingid).Zhenshiname;
            
            var mulu = DateTime.Now.ToString("yyyy");
            //var baoxiaoren = Request.Form["baoxiao_shenhe"];

            // 取值，设置 订单 所对应的 审核流程
            var liuchengname = Request.Form["shenheliucheng"];
            var liuchengid = db.Fapiao_liuchneg_zhubiaos.Where(c => c.Mingcheng == liuchengname).Select(c => c.ID).FirstOrDefault();

            //这里判断，如果指定采购人是为空的，则默认指定人为结算人，就是部门主管
            //这里的  采购人就是 经手人，把经手人写入 项目子表
            var userid = "";
            var name = "";
            int shenhexulie ;
            if (Request.Form["caigouren"].Length == 0)   //如果不选，默认 采购人是部门主管本人
            {
                userid = loingid;
                name = common.Customidentity.User(loingid).Zhenshiname;

                //根据采购人是谁设置 审核序列的开始值
                shenhexulie = 0;
            }
            else      //如果是非空，就判断非空 的值，是属于  部门主管 还是 员工
            {
                userid = Request.Form["caigouren"];
                name = Customidentity.User(userid).Zhenshiname;
                var role = Customidentity.User(userid).Role;

                if (role == "员工")
                {
                    //设置审核序列开始值                
                    shenhexulie = -1;
                }
                else
                {
                    //设置审核序列开始值                
                    shenhexulie = 0;
                }               
            }
                                 
            Guid str = Guid.NewGuid();

            //订单主表操作
            Dingdanzhubiao zhubiao = new Dingdanzhubiao
            {
                ID = str,
                Dingdanming = xmnames,
                Dingdanren = username,    //生成这个订单的人， 这里和  采购人  可以是同一个人，也可能不是同一个人
                DingdanreID = loingid,
                Dingdanjine = Convert.ToDecimal(heji),
                //Dingdanriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                Dingdanriqi = DateTime.Now,
                Shoujianren = username,
                Shoujiandizhi = "海南师范大学信息学院教学楼",
                Dingdanshuliang = xmid_fenli_conut,
                Dingdanshoujian = 0,
                Xmid_shuzu_str = xmid,
                Mulu = mulu,
                Suoshubumen = suoshubume,
                Dingdanshoushuo = "未收货",                
                Beizhu = Request.Form["beizhu"],
                Caigouren = userid,     //员工登录，指定采购人的页面  的数据过滤，以采购人为准
                Caigouren_name = name,
                //Baoxiao_shenheren = baoxiaoren,

                Baoxiaozhuangtai = "未报销",
                Shenhexulie = shenhexulie,    //设置审核序列开始的值

                Liuchengzhubiao = liuchengid,   //设置 订单表审核所对应的  审核流程 ID 和所要显示 的流程名
                Liuchengbiaoname = liuchengname
            };

            //这里的事务回滚   添加 订单主表、 修改了 项目表、 修改了 项目子表 、添加了 审核子表
            using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
            {
                try
                {
                    db.Dingdanzhubiaos.Add(zhubiao);
                    //db.SaveChanges();

                    //这里的操作是，把刚 添加 保存 成功 的数据马上通过 项目ID  字符串 提取出  刚刚生成的  随机 数 guid，也就是  订单主键
                    Dingdanzhubiao zhubiao_id = db.Dingdanzhubiaos.Where(s => s.Xmid_shuzu_str == xmid).FirstOrDefault();
                    //Guid zhubiaoid = zhubiao_id.ID;

                    for (int i = 0; i < xmid_fenli_conut; i++)
                    {
                        int id = Convert.ToInt32(xmid_fenli[i]);  //项目主表ID

                        //项目表操作
                        var xm = db.Xiangmubiaos.Where(s => s.ID == id).FirstOrDefault();
                        xm.Goumaizhuangtai = "已购买";    //修改项目表中的购买状态
                        //xm.DingdanzhubiaoID = zhubiaoid;
                        xm.DingdanzhubiaoID = str;


                        //在结算时 设置，谁采购 谁就是 经手人
                        //项目子表中找出  对应 项目主表 同一 ID 记录
                        var zibiao = (from c in db.Xiangmuzibiaos
                                     where c.XiangmubiaoID == id
                                     orderby c.ID
                                     select c.ID).ToArray();
                        for (int j=0;j<zibiao.Count();j++)
                        {
                            int zibiao_id = zibiao[j];
                            var zibiao_item = db.Xiangmuzibiaos.Where(s => s.ID == zibiao_id).FirstOrDefault();
                            zibiao_item.Jingshouren = name;
                            db.Entry(zibiao_item).State = EntityState.Modified;
                        }                        

                        //审核子表操作
                        Shenhezibiao shenhezibiao = new Shenhezibiao
                        {
                            XiangmubiaoID = id,
                            Shenheyijian = "结算",
                            Shenhezhuangtai = "结算",
                            Shenhejuese = Customidentity.User(loingid).Role,
                            //Shenhexulie = xm.Shenhexulie + 1,
                            Shenheren = username,
                            Riqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                            Shenhejiedian = username + "结算"
                        };

                        db.Entry(xm).State = EntityState.Modified;
                        db.Shenhezibiaos.Add(shenhezibiao);                        
                    }
                    db.SaveChanges();
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
                finally
                {
                    transaction.Dispose();
                }
                return Json(new { success = true, Msg = "已完成后台结算操作" }, "text/html");
            }
        }



        public ActionResult Shenhe_rizhi(string xmname, int xmid)
        {
            ViewBag.xmid = xmid;

            ViewBag.xmname = xmname;

            var xm = db.Xiangmubiaos.Where(s => s.ID == xmid).FirstOrDefault();
            string shenhezhuangtai = xm.Shenhezhuangtai;
            string goumaizhuangtai = xm.Goumaizhuangtai;
            string dingdanshouhuo = xm.Dingdanshoushuo;

            //xiangmubiao.Shenhezhuangtai = "未提交";
            //xiangmubiao.Goumaizhuangtai = "未购买";
            //xiangmubiao.Dingdanshoushuo = "未收货";                    

            if (dingdanshouhuo == "已收货")
            {
                ViewBag.step = 8;
            }
            else
            {
                if (goumaizhuangtai == "已购买")
                {
                    ViewBag.step = 6;
                }
                else
                {
                    if (shenhezhuangtai == "通过" || shenhezhuangtai == "未通过")
                    {
                        ViewBag.step = 5;
                    }
                    else
                    {
                        if (shenhezhuangtai == "审核中")
                        {
                            ViewBag.step = 4;
                        }
                        else
                        {
                            if (shenhezhuangtai == "已提交")
                            {
                                ViewBag.step = 3;
                            }
                            else
                            {
                                ViewBag.step = 2;
                            }
                        }
                    }
                }
            }
            return View();
        }



        public ActionResult Shenhe_Fapiao_rizhi(string xmname, Guid xmid)
        {
            ViewBag.xmid = xmid;

            ViewBag.xmname = xmname;

            var xm = db.Dingdanzhubiaos.Where(s => s.ID == xmid).FirstOrDefault();
            string shenhezhuangtai = xm.Baoxiaozhuangtai;
            //string goumaizhuangtai = xm.Goumaizhuangtai;
            //string dingdanshouhuo = xm.Dingdanshoushuo;

            if (shenhezhuangtai == "同意报销")
            {
                ViewBag.step = 6;
            }
            else
            {
                if (shenhezhuangtai == "审核中")
                {
                    ViewBag.step = 4;
                }
                else
                {
                    if (shenhezhuangtai == "申请报销")
                    {
                        ViewBag.step = 3;
                    }
                    else
                    {
                        ViewBag.step = 2;
                    }
                }
            }
            return View();
        }


        //获取审核日志
        [HttpPost]
        public JsonResult Get_xmrizhi(int xmid)
        {
            try
            {
                var xm = from s in db.Shenhezibiaos
                         where s.XiangmubiaoID == xmid
                         orderby s.ID descending
                         select new
                         {
                             id = s.ID,
                             shenheyijian = s.Shenheyijian,
                             shenhezhuangtai = s.Shenhezhuangtai,
                             shenhejuese = s.Shenhejuese,
                             shenheren = s.Shenheren,
                             shenheriqi = s.Riqi,
                             shenhejiedian = s.Shenhejiedian
                         };
                // 返回到前台的值必须按照如下的格式包括 total and rows 
                return Json(new Dictionary<string, object>
                {
                    { "total", xm.Count() },
                    { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                });
            }
            catch (Exception)
            {
                throw;
            }
        }


        //获取 发票审核 中的 审核日志
        [HttpPost]
        public JsonResult Get_Fapiao_rizhi(Guid xmid) 
        {
            try
            {
                var xm = from s in db.Shenhezibiao_Fapiaos
                         where s.DingdanzhubiaoID == xmid
                         orderby s.ID descending
                         select new
                         {
                             id = s.ID,
                             shenheyijian = s.Shenheyijian,
                             shenhezhuangtai = s.Shenhezhuangtai,
                             shenhejuese = s.Shenhejuese,
                             shenheren = s.Shenheren,
                             shenheriqi = s.Riqi,
                             shenhejiedian = s.Shenhejiedian
                         };
                // 返回到前台的值必须按照如下的格式包括 total and rows 
                return Json(new Dictionary<string, object>
                {
                    { "total", xm.Count() },
                    { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult Shenqing_Yg(string userid, DateTime time)  //string mulu,
        {
            //取出用户的 suoshubumen
            var suoshubumen = Customidentity.User(userid).Suoshubumen;

            //赛选出来的主流程 suoshubumen 字段值，要与 用户的 suoshubumen  字段值一致
            var GenreLst = new List<string>();
            var GenreQry = from c in db.Liuchengzhubiaos
                           where c.Suoshubumen == suoshubumen
                           orderby c.ID
                           select c.Mingcheng;

            GenreLst.AddRange(GenreQry.Distinct());
            ViewBag.liucheng = GenreLst;
            //ViewBag.mulu = mulu;

            return View();
        }

        public ActionResult Tuihui_Yg(DateTime time)  
        {
            return View();
        }

        public ActionResult Weitongguo_Yg(DateTime time)
        {
            return View();
        }

        public ActionResult Tongguo_Yg(DateTime time)  
        {
            //ViewBag.mulu = mulu;
            return View();
        }

        public ActionResult Shouhuo_Yg(DateTime time)  
        {
            //ViewBag.mulu = mulu;
            return View();
        }

        //指定采购人，员工页面
        public ActionResult Caigou_Yg(DateTime time)
        {
            //ViewBag.mulu = mulu;
            return View();
        }

        //员工已经提交 报销审核的页面 
        public ActionResult Fapiao_Zaishen_Yg(DateTime time)
        {            
            return View();
        }

        //发票审核，撤回到员工的页面
        public ActionResult Fapiao_Chehui_Yg(DateTime time)
        {
            //ViewBag.mulu = mulu; 
            return View();
        }

        public ActionResult Fapiao_Tongguo_Yg(DateTime time)
        {
            //ViewBag.mulu = mulu;
            return View();
        }

        public ActionResult Yiqianshou_Yg(DateTime time,string loingid)  //string mulu,
        {
            //取出同一个部门下的所有  用户名
            //using () //创建一个新的上下文
            //{ }
            ApplicationDbContext identitydb1 = new ApplicationDbContext();
            var suoshubumen = identitydb1.Users.Where(s => s.Id == loingid).FirstOrDefault().Suoshubumen;

            //var NameLst = new List<string>();
            var userName = from c in identitydb1.Users
                            where c.Suoshubumen == suoshubumen || c.Suoshubumen.Contains(suoshubumen)
                            orderby c.Id
                            select c; 

                

            //这里不能去重，一去重，如果有重名 的真实名用户，那就会报错了
            //NameLst.AddRange(userName.Distinct());
            //NameLst.AddRange(userName.Distinct());


            //有空研究一下这两种写法  Application.Users和 UserManager.Users
            //var xm = from c in UserManager.Users                        
            //         where c.Role == "员工" && c.Suoshubumen == bumen
            //         orderby c.UserName
            //         select new { id = c.Id};

            //ViewBag.username = NameLst;
            //ViewBag.username = userName;

            return View(userName);
                        
        }

        public ActionResult Daiqianshou_Yg(DateTime time,string loingid)
        {
            //ApplicationDbContext identitydb1 = new ApplicationDbContext();
            //string suoshubumen = identitydb1.Users.Where(s => s.Id == loingid).FirstOrDefault().Suoshubumen;
            //var role = common.Customidentity.User(loingid).Role;

            //var NameLst = new List<string>();
            //if (role == "员工"||role=="部门主管")
            //{
            //    var userName = from c in identitydb1.Users
            //                    where c.Suoshubumen == suoshubumen || c.Suoshubumen.Contains(suoshubumen)
            //                    orderby c.Id
            //                    select c.UserName;
            //    NameLst.AddRange(userName.Distinct());
            //}
            //else
            //{
            //    var userName = from c in identitydb1.Users
            //                    where c.Role !="员工"
            //                    orderby c.Id
            //                    select c.UserName;
            //    NameLst.AddRange(userName.Distinct());
            //}                            
            //ViewBag.username = NameLst;


            ApplicationDbContext identitydb1 = new ApplicationDbContext();
            var suoshubumen = identitydb1.Users.Where(s => s.Id == loingid).FirstOrDefault().Suoshubumen;
            var role = common.Customidentity.User(loingid).Role;

            if (role == "员工" || role == "部门主管")
            {
                var userName = from c in identitydb1.Users
                               where c.Suoshubumen == suoshubumen || c.Suoshubumen.Contains(suoshubumen)
                               orderby c.Id
                               select c;

                return View(userName);
            }
            else
            {
                var userName = from c in identitydb1.Users
                               where c.Role!="员工"
                               orderby c.Id
                               select c;

                return View(userName);
            }             
        }        

        /// <summary>
        /// 部门主管  加载框架  转到的视图
        /// </summary>        
        public ActionResult Nishen_Zg(DateTime time)
        {
            return View();
        }
        public ActionResult Daishen_Zg(DateTime time)
        {
            return View();
        }

        public ActionResult Tuihui_Zg(DateTime time)
        {
            return View();
        }
         
        public ActionResult Weitongguo_Zg(DateTime time)
        {
            return View();
        }

        public ActionResult Yishen_Zg(DateTime time)
        {            
            return View();
        }

        public ActionResult Daigou_Zg(DateTime time, string loingid)
        {
            ApplicationDbContext identitydb1 = new ApplicationDbContext();
            var suoshubumen = identitydb1.Users.Where(s => s.Id == loingid).FirstOrDefault().Suoshubumen;

            //var NameLst = new List<string>();
            var userName = from c in identitydb1.Users
                           where (c.Suoshubumen == suoshubumen || c.Suoshubumen.Contains(suoshubumen)) && (c.Role == "员工"||c.Role=="部门主管")
                           orderby c.Id
                           select c;
           
            var GenreLst = new List<string>();
            var GenreQry = from c in db.Fapiao_liuchneg_zhubiaos
                               //where c.Suoshubumen == suoshubumen
                           orderby c.ID
                           select c.Mingcheng;           //这里如果选多个值 传回页面会报错

            GenreLst.AddRange(GenreQry.Distinct());
            ViewBag.liucheng = GenreLst;

            return View(userName);          //一个页面是否可以接受多个 model，如何设置接受多个model
        }

        public ActionResult Yigou_Zg(DateTime time,string username)
        {
            ViewBag.username = username;
            return View();
        }

        // 部门主管已经审过的发票，或者部门主管 自己提交的发票    就是审核序列大于或等于一
        public ActionResult Fapiao_Ys_zg(DateTime time) 
        {            
            return View();
        }

        public ActionResult Fapiao_Chehui_zg(DateTime time) 
        {
            return View();
        }


        public ActionResult Fapiao_Tongyi_zg(DateTime time)
        {
            return View();
        }


        /// <param name="biaoxiao">报销状态</param>
        /// <param name="id">订单主表  主键ID</param>
        /// <returns></returns>
        public ActionResult Fapiao_upload_Zg(string biaoxiao,Guid id)
        {
            var picList = db.Fapiao_uploads.Where(s => s.DingdanzhubiaoID == id);

            var dingdan = db.Dingdanzhubiaos.Where(s => s.ID == id).FirstOrDefault();
            
            //提取订单发票号
            ViewBag.fapiao = dingdan.Fapiaohao;

            ViewBag.biaoxiao = biaoxiao;
            ViewBag.id = id;
            return View(picList);
        }


        public ActionResult Yishen_Bmld(string mulu, string userid, DateTime time)
        {
            ViewBag.mulu = mulu;
            return View();
        }

        public ActionResult Fapiao_Bmld( DateTime time)
        {
            return View();
        }

        public ActionResult Fapiao_Yishen_Bmld(DateTime time) 
        {
            return View();
        }

        //点击项目名，跳转到项目子表页面
        public ActionResult Xiangmuzibiao(int xmid)
        {            
            var shenhezhuangtai = db.Xiangmubiaos.Where(s => s.ID == xmid).FirstOrDefault().Shenhezhuangtai;
            ViewBag.shenhezhuangtai = shenhezhuangtai;
            ViewBag.xmid = xmid;
            return View();
        }

        /// <summary>
        /// 由订单表点击跳转到项目表页面，筛选出订单ID   对应的项目
        /// </summary>
        /// <param name="id">订单主表ID</param>
        /// <returns></returns>
        public ActionResult Xiangmubiao(Guid id)
        {
            ViewBag.dingdanID = id;
            return View();
        }

        public ActionResult Xiangmuzibiao_shouhuo(string loingid,int xmid)  //string mulu,
        {
            //这里是想，在收货的时候，有个下拉框选择 项目子表的经手人，但是  后来 又个 指定 采购人 功能，采购人就是经手人。经手人在指定采购人的时候 ，直接给相应的子表赋值经手人
            //ApplicationDbContext identitydb1 = new ApplicationDbContext();
            //var suoshubumen = identitydb1.Users.Where(s => s.Id == loingid).FirstOrDefault().Suoshubumen;            
            //var userName = from c in identitydb1.Users
            //               where c.Suoshubumen == suoshubumen && c.Role != "部门领导"&&c.Role!="领导"
            //               orderby c.Id
            //               select c;


            //有空研究一下这两种写法  Application.Users和 UserManager.Users
            //var xm = from c in UserManager.Users                        
            //         where c.Role == "员工" && c.Suoshubumen == bumen
            //         orderby c.UserName
            //         select new { id = c.Id};
            
            ViewBag.xmid = xmid;
            return View();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">订单主表ID</param>
        /// <returns></returns>
        public ActionResult Fapiao_upload_bmld(Guid id)
        {            
            var pic_list = db.Fapiao_uploads.Where(s => s.DingdanzhubiaoID == id);
            var dingdan = db.Dingdanzhubiaos.Where(s => s.ID == id).FirstOrDefault();

            //提取订单发票号
            ViewBag.fapiao = dingdan.Fapiaohao;

            return View(pic_list);
        }
        
        
        
        
        /// <summary>
        /// 先根据xmid 判断发票表中是否有对应的记录，没有返回提示先增加发票记录
        /// 此处xmid为订单主表ID        /// 
        /// 部门主管 待审报销 和 员工 待审报销 和员工 撤回 提交报销  公用了这个方法
        /// 这里只相当于 员工 和部门主管 的初始 报销 提交
        /// </summary>        
        [HttpPost]
        public JsonResult Tijiao_Fp_Baoxiao(string loingid, string username,Guid dingdanID)
        {             
            var fapiao = db.Fapiao_uploads.Where(c => c.DingdanzhubiaoID == dingdanID);                    

            if (fapiao.Count() > 0)
            {
                using (WzglContent new_db = new WzglContent())
                {
                    var username_rel = Customidentity.User_name(username).Zhenshiname;
                    var dingdan = new_db.Dingdanzhubiaos.Where(c => c.ID == dingdanID).FirstOrDefault();


                    // 提交的时候判断是否已经是审核的最后一步
                    var liuchengzhubiaoID = dingdan.Liuchengzhubiao;
                    var liuchengzibiao = from s in new_db.Fapiao_liuchneg_zibiaos
                                         where s.Fapiao_liucheng_zhubiaoID == liuchengzhubiaoID
                                         orderby s.ID
                                         select s.Liuchengxulie;
                    var xulie_max = liuchengzibiao.Max();

                    if (dingdan.Shenhexulie + 1 == xulie_max)  //判断是最后一步审核
                    {
                        dingdan.Shenhexulie = dingdan.Shenhexulie + 1;
                        dingdan.Baoxiaozhuangtai = "同意报销";

                        //这里加一个功能，只有在 部门领导审核同意之后才能入库
                        //if (Request.Form["shenhe"] == "同意报销")
                        //{                            
                        //}

                        // 最后一步是部门主管的提交，那就是 通过报销，物资入库
                        var ruku_arry = new_db.Rukubiaos.Where(s => s.Dingdan_str == dingdanID).OrderBy(s => s.ID).Select(s => s.ID).ToArray();
                        for (int i = 0; i < ruku_arry.Length; i++)
                        {
                            int rukuid = ruku_arry[i];
                            Rukubiao ruku = new Rukubiao();
                            ruku = new_db.Rukubiaos.Where(s => s.ID == rukuid).FirstOrDefault();
                            ruku.Rukuzhuangtai = "已入库";
                            new_db.Entry(entity: ruku).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        dingdan.Baoxiaozhuangtai = "申请报销";
                        dingdan.Shenhexulie = dingdan.Shenhexulie + 1;

                        dingdan.Chehui_realname = "";      //撤回的发票，当重新提交审核时候，要把 之前的撤回人 清空，也就是 重新走一次审核
                        dingdan.Chehui_user = "";
                    }                    
                  
                    try
                    {
                        Shenhezibiao_Fapiao fapiaorizhi = new Shenhezibiao_Fapiao
                        {
                            DingdanzhubiaoID = dingdanID,
                            Shenheyijian = "申请报销",
                            Shenhezhuangtai = "申请报销",
                            Shenhejuese = Customidentity.User(loingid).Role,
                            Shenheren = username_rel,
                            Riqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                            Shenhejiedian = username_rel + "申请报销",
                            //Shenhexulie = dingdan.Shenhexulie + 1
                        };

                        new_db.Shenhezibiao_Fapiaos.Add(fapiaorizhi);    
                        new_db.Entry(dingdan).State = EntityState.Modified;
                        new_db.SaveChanges();
                        return Json(new { success = true, Msg = "提交成功" }, "text/html");                    
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                    }
                }
            }
            else
            {
                return Json(new { success = false, Msg = "请先点击发票按钮上传发票记录！" }, "text/html");
            }       
        }

        /// <summary>
        /// 员工 到 部门主管  ，部门主管的审核
        /// </summary>
        /// <param name="loingid"></param>
        /// <param name="username"></param>
        /// <param name="xmid">订单主表 ID</param>
        /// <returns></returns>        
        [HttpPost]
        public JsonResult Fapiao_Shenhe(string loingid, Guid xmid)
        {
            var shenhe = Request.Form["shenhe"];  //  部门负责人：通过/撤回   部门领导：同意报销/撤回
            if (shenhe == null || shenhe == "")
            {
                return Json(new { success = false, Msg = "请选择审核状态后确认！" }, "text/html");
            }
            var shenheyijian = Request.Form["yijian"];
            var usernam_real = Customidentity.User(loingid).Zhenshiname;
            if (shenheyijian == "")
            {
                shenheyijian = shenhe;
            }
            using (WzglContent db_new = new WzglContent()) //创建一个新的上下文
            {
                var dingdan = db_new.Dingdanzhubiaos.Where(s => s.ID == xmid).FirstOrDefault();
                var liuchengzhubiaoID = dingdan.Liuchengzhubiao;

                var shenhezibiao = new Shenhezibiao_Fapiao     //  shenhezibiao表就是日志表
                {
                    DingdanzhubiaoID = xmid,
                    Shenheyijian = shenheyijian,
                    Shenhezhuangtai = shenhe,
                    Shenhejuese = Customidentity.User(loingid).Role,
                    //Shenhexulie = dingdan.Shenhexulie + 1,
                    Shenheren = usernam_real,            //这里的审核人要显示真名
                    Riqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                    Shenhejiedian = usernam_real + "审核发票"    //这里的审核人要显示真名
                };

                //取出项目对应流程子表中的最大序列数，判断审核是否进行到了最后一步
                var liuchengzibiao = from s in db_new.Fapiao_liuchneg_zibiaos
                                     where s.Fapiao_liucheng_zhubiaoID == liuchengzhubiaoID
                                     orderby s.ID
                                     select s.Liuchengxulie;
                var xulie_max = liuchengzibiao.Max();
                if (shenhe == "撤回")
                {
                    dingdan.Baoxiaozhuangtai = "撤回";
                    var yijian = usernam_real + ":" + shenheyijian + "；";

                    if (dingdan.Chehui_user == "" || dingdan.Chehui_user == null)
                    {
                        dingdan.Shenhexulie = dingdan.Shenhexulie - 1;                   //这里是为了审核不通过时返回上一级做的设置
                        dingdan.Chehui_user = loingid;
                        dingdan.Chehui_realname = common.Customidentity.User(loingid).Zhenshiname;    //这里写入的第一个撤回的人            
                    }
                    else
                    {
                        var chehui = dingdan.Chehui_user;
                        dingdan.Chehui_user = chehui + "&&" + loingid;     //为了避免在撤回的时候重复撤回，这里是把 撤回人 的loingid都 连接起来
                        dingdan.Shenhexulie = dingdan.Shenhexulie - 1;     //这里是为了审核不通过时返回上一级做的设置                                             
                    }
                }
                else //通过的
                {
                    if (dingdan.Shenhexulie + 1 == xulie_max)  //最后一步审核
                    {
                        dingdan.Shenhexulie = dingdan.Shenhexulie + 1;
                        dingdan.Baoxiaozhuangtai = shenhe;


                        //这里加一个功能，只有在 部门领导审核同意之后才能入库
                        if (Request.Form["shenhe"] == "同意报销")
                        {
                            var ruku_arry = db_new.Rukubiaos.Where(s => s.Dingdan_str == xmid).OrderBy(s => s.ID).Select(s => s.ID).ToArray();
                            for (int i = 0; i < ruku_arry.Length; i++)
                            {
                                int rukuid = ruku_arry[i];
                                Rukubiao ruku = new Rukubiao();
                                ruku = db_new.Rukubiaos.Where(s => s.ID == rukuid).FirstOrDefault();
                                ruku.Rukuzhuangtai = "已入库";
                                db_new.Entry(entity: ruku).State = EntityState.Modified;
                            }
                        }
                    }
                    else    //不是最后一步审核
                    {
                        dingdan.Shenhexulie = dingdan.Shenhexulie + 1;
                        dingdan.Baoxiaozhuangtai = "审核中";

                        //这里的意思是，如果有撤回记录， 但是这登录人并没有将这条记录 接着往下撤回到  最初开始提交的人，就直接审核通过了
                        dingdan.Chehui_user = "";
                        dingdan.Chehui_realname = "";
                    }
                }
                using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
                {
                    try
                    {
                        db_new.Entry(dingdan).State = EntityState.Modified;
                        db_new.Shenhezibiao_Fapiaos.Add(shenhezibiao);
                        db_new.SaveChanges();
                        transaction.Complete();
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
                return Json(new { success = true, Msg = "审核保存成功！" }, "text/html");
            }
        }       
        

        ///// <summary>
        ///// 部门领导  审核是否同意报销
        ///// </summary>
        ///// <param name="loingid"></param>
        ///// <param name="xmid">订单主表 主键 ID</param>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonResult Baoxiao_Shenhe(string loingid,Guid xmid)  
        //{
        //    if (Request.Form["shenhe"] == null)
        //    {
        //        return Json(data: new { success = false, Msg = "请选择审核状态后在确认！" }, contentType: "text/html");
        //    }

        //    var username_rel = common.Customidentity.User(loingid).Zhenshiname;

        //    //订单主表 关联的项目表  的审核记录
        //    //using (WzglContent new_db1 = new WzglContent())
        //    //{
        //    //    for (int i = 0; i < db.Xiangmubiaos.Where(predicate: c => c.DingdanzhubiaoID == xmid).ToArray().Count(); i++)
        //    //    {
        //    //        new_db1.Shenhezibiaos.Add(new Shenhezibiao
        //    //        {
        //    //            XiangmubiaoID = db.Xiangmubiaos.Where(predicate: c => c.DingdanzhubiaoID == xmid).ToArray()[i].ID,
        //    //            //Shenheyijian = shenheyijian,
        //    //            Shenhezhuangtai = Request.Form["shenhe"],
        //    //            Shenhejuese = Customidentity.User(loingid).Role,
        //    //            //Shenhexulie = xm.Shenhexulie + 1,
        //    //            Shenheren = username_rel,
        //    //            Riqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
        //    //            Shenhejiedian = username_rel + "审核报销"
        //    //        });
        //    //        new_db1.SaveChanges();
        //    //    };
        //    //}            

        //    using (WzglContent db_new = new WzglContent()) //创建一个新的上下文
        //    {
        //        //这里加一个功能，只有在 审核最后一步 同意之后才能入库
        //        db_new.Dingdanzhubiaos.Where(s => s.ID == xmid).FirstOrDefault().Baoxiaozhuangtai = Request.Form["shenhe"];
        //        db_new.Dingdanzhubiaos.Where(s => s.ID == xmid).FirstOrDefault().Baoxiaoyijian = Request.Form["yijian"];

        //        //取出项目对应流程子表中的最大序列数，判断审核是否进行到了最后一步
        //        using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
        //        {
        //            try
        //            {
        //                var shenheyijian_lst = "";
        //                if(Request.Form["yijian"]==""|| Request.Form["yijian"] == null)
        //                {
        //                    if (Request.Form["shenhe"] == "同意报销")
        //                    {
        //                        shenheyijian_lst = "同意报销";
        //                    }
        //                    else
        //                    {
        //                        shenheyijian_lst = "不同意报销";
        //                    }
        //                }
        //                else
        //                {
        //                    shenheyijian_lst = Request.Form["yijian"];
        //                }                       
        //                for (int i = 0; i < db_new.Xiangmubiaos.Where(predicate: c => c.DingdanzhubiaoID == xmid).ToArray().Count(); i++)
        //                {
        //                    db_new.Shenhezibiaos.Add(new Shenhezibiao
        //                    {
        //                        XiangmubiaoID = db_new.Xiangmubiaos.Where(predicate: c => c.DingdanzhubiaoID == xmid).ToArray()[i].ID,
        //                        Shenheyijian = shenheyijian_lst,
        //                        Shenhezhuangtai = Request.Form["shenhe"],
        //                        Shenhejuese = Customidentity.User(loingid).Role,
        //                        //Shenhexulie = xm.Shenhexulie + 1,                                
        //                        Shenheren = username_rel,
        //                        Riqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
        //                        Shenhejiedian = username_rel + "审核报销"
        //                    });                            
        //                    //db_new.SaveChanges();
        //                };


        //                //这里加一个功能，只有在 部门领导审核同意之后才能入库
        //                if (Request.Form["shenhe"] == "同意报销")
        //                {                       
        //                    var ruku_arry = db_new.Rukubiaos.Where(s => s.Dingdan_str == xmid).OrderBy(s => s.ID).Select(s => s.ID).ToArray();
        //                    for (int i = 0; i < ruku_arry.Length; i++)
        //                    {
        //                        int rukuid = ruku_arry[i];
        //                        Rukubiao ruku = new Rukubiao();
        //                        ruku = db_new.Rukubiaos.Where(s => s.ID == rukuid).FirstOrDefault();                                
        //                        ruku.Rukuzhuangtai = "已入库";
        //                        db_new.Entry(entity: ruku).State = EntityState.Modified;
        //                    }                            
        //                }
                        
        //                db_new.Entry(entity: db_new.Dingdanzhubiaos.Where(predicate: s => s.ID == xmid).FirstOrDefault()).State = EntityState.Modified;
        //                //db_new.Shenhezibiaos.Add(shenhezibiao);
        //                db_new.SaveChanges();
        //                transaction.Complete();
        //            }
        //            catch (Exception ex)
        //            {
        //                return Json(data: new { success = false, Msg = ex.ToString() }, contentType: "text/html");
        //            }
        //            finally
        //            {
        //                transaction.Dispose();
        //            }
        //        }
        //        return Json(data: new { success = true, Msg = "审核保存成功！" }, contentType: "text/html");
        //    }
        //}


        /// <summary>
        /// 在设置 部门领导同意报销 后才能入库时，在员工收货方法中为  ruku.cs表保留了订单主表的  guid  ID 主键字段
        /// 这里直接在ruku.cs表中搜素  订单主表主键就可以
        /// </summary>
        /// <param name="id">订单主表 guid  ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Fapiaohao_edt(Guid id, string fapiaohao)
        {
            //var fapiaohao = Request.Form["fapiaohao"];

            using (WzglContent new_db = new WzglContent())
            {
                var dingdanzhubiao = new_db.Dingdanzhubiaos.Where(s => s.ID == id).FirstOrDefault();
                dingdanzhubiao.Fapiaohao = fapiaohao;
                var ruku = (from c in new_db.Rukubiaos
                            where c.Dingdan_str == id
                            orderby c.ID
                            select c.ID).ToArray();
                for (int j = 0; j < ruku.Count(); j++)
                {
                    var ruku_id = ruku[j];
                    var ruku_item = new_db.Rukubiaos.Where(s => s.ID == ruku_id).FirstOrDefault();
                    try
                    {
                        ruku_item.Fapiaohao = fapiaohao;  //发票表和入库表的关系是，一张发票对应多个入库物资，通过 Fapiaohao 这个字段 可以查找
                        new_db.Entry(ruku_item).State = EntityState.Modified;
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = true, Msg = ex.ToString() }, "text/html");
                    }
                }
                new_db.Entry(dingdanzhubiao).State = EntityState.Modified;
                new_db.SaveChanges();
            }
            return Json(new { success = true, Msg = "提交成功" }, "text/html");
        }

        /// <summary> 
        /// 员工申请的项目   ，员工 显示未通过项目是为了可以再次提交
        /// </summary>        
        [HttpPost]
        public JsonResult Get_shenqing_xm(string searchquery,string loingid)  //,string mulu
        {
            if (searchquery == "")
            {
                try
                {
                    var xm = from c in db.Xiangmubiaos
                             where c.Userid == loingid && c.Shenhezhuangtai != "未通过" && c.Shenhezhuangtai!="撤回"    //这里取出的是在审核主表中没有记录的项目，就是审核过的项目
                             orderby c.ID descending
                             select new
                             {
                                 id = c.ID,
                                 xiangmumingcheng = c.Xiangmumingcheng,
                                 jine = c.Jine,
                                 shenqingren = c.ZhenshiName,
                                 riqi = c.Shenqingriqi,
                                 shenheliucheng = c.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                 liuchengID = c.Liuchengzhubiao,
                                 beizhu = c.Beizhu,
                                 //yuangongtijiao =c.Yuangongtijiao,
                                 shenhezhuangtai = c.Shenhezhuangtai,
                                 goumaizhuangtai = c.Goumaizhuangtai,
                                 dingdanshouhuo = c.Dingdanshoushuo,
                                 shenhexulie = c.Shenhexulie
                             };
                    // 返回到前台的值必须按照如下的格式包括 total and rows 
                    return Json(data: new Dictionary<string, object>
                    {
                        { "total", xm.Count() },
                        { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm = from c in db.Xiangmubiaos
                             where c.Userid == loingid && c.Shenhezhuangtai != "未通过" && c.Shenhezhuangtai != "撤回" && c.Xiangmumingcheng.Contains(searchquery)    //这里取出的是在审核主表中没有记录的项目，就是审核过的项目
                             orderby c.ID descending
                             select new
                             {
                                 id = c.ID,
                                 xiangmumingcheng = c.Xiangmumingcheng,
                                 jine = c.Jine,
                                 shenqingren = c.ZhenshiName,
                                 riqi = c.Shenqingriqi,
                                 shenheliucheng = c.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                 liuchengID = c.Liuchengzhubiao,
                                 beizhu = c.Beizhu,
                                 //yuangongtijiao =c.Yuangongtijiao,
                                 shenhezhuangtai = c.Shenhezhuangtai,
                                 goumaizhuangtai = c.Goumaizhuangtai,
                                 dingdanshouhuo = c.Dingdanshoushuo
                             };
                    // 返回到前台的值必须按照如下的格式包括 total and rows 
                    return Json(data: new Dictionary<string, object>
                    {
                        { "total", xm.Count() },
                        { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }

        /// <summary>
        /// 员工登录，得到的退回项目
        /// </summary>
        /// <param name="searchquery"></param>
        /// <param name="loingid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get_tuihui_Yg(string searchquery, string loingid)  //,string mulu
        {
            if (searchquery == "")
            {
                try
                {
                    var xm = from c in db.Xiangmubiaos
                             where c.Userid == loingid && c.Shenhezhuangtai=="撤回" &&c.Shenhexulie == -1   //这里取出的是在审核主表中没有记录的项目，就是审核过的项目
                             orderby c.ID descending
                             select new
                             {
                                 id = c.ID,
                                 xiangmumingcheng = c.Xiangmumingcheng,
                                 jine = c.Jine,
                                 shenqingren = c.ZhenshiName,
                                 riqi = c.Shenqingriqi,
                                 shenheliucheng = c.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                 liuchengID = c.Liuchengzhubiao,
                                 beizhu = c.Beizhu,
                                 //yuangongtijiao =c.Yuangongtijiao,
                                 shenhezhuangtai = c.Shenhezhuangtai,
                                 goumaizhuangtai = c.Goumaizhuangtai,
                                 dingdanshouhuo = c.Dingdanshoushuo,
                                 //chehuiren = c.Chehui_user,
                                 chehui_realname = c.Chehui_realname
                             };
                    // 返回到前台的值必须按照如下的格式包括 total and rows 
                    return Json(data: new Dictionary<string, object>
                    {
                        { "total", xm.Count() },
                        { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm = from c in db.Xiangmubiaos
                             where c.Userid == loingid && c.Shenhezhuangtai == "撤回" && c.Shenhexulie == -1 && c.Xiangmumingcheng.Contains(searchquery)    //这里取出的是在审核主表中没有记录的项目，就是审核过的项目
                             orderby c.ID descending
                             select new
                             {
                                 id = c.ID,
                                 xiangmumingcheng = c.Xiangmumingcheng,
                                 jine = c.Jine,
                                 shenqingren = c.ZhenshiName,
                                 riqi = c.Shenqingriqi,
                                 shenheliucheng = c.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                 liuchengID = c.Liuchengzhubiao,
                                 beizhu = c.Beizhu,
                                 //yuangongtijiao =c.Yuangongtijiao,
                                 shenhezhuangtai = c.Shenhezhuangtai,
                                 goumaizhuangtai = c.Goumaizhuangtai,
                                 dingdanshouhuo = c.Dingdanshoushuo,
                                 //chehuiren = c.Chehui_user,
                                 chehui_realname = c.Chehui_realname
                             };
                    // 返回到前台的值必须按照如下的格式包括 total and rows 
                    return Json(data: new Dictionary<string, object>
                    {
                        { "total", xm.Count() },
                        { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary> 
        /// 员工申请的项目   ，员工 显示未通过项目是为了可以再次提交
        /// </summary>        
        [HttpPost]
        public JsonResult Get_weiguo_yg(string searchquery, string loingid,string username)  //,string mulu
        {
            if (searchquery == "")
            {
                try
                {
                    var xm = from c in db.Xiangmubiaos
                             where c.Userid == loingid && c.Shenhezhuangtai == "未通过"    //这里取出的是在审核主表中没有记录的项目，就是审核过的项目
                             orderby c.ID descending
                             select new
                             {
                                 id = c.ID,
                                 xiangmumingcheng = c.Xiangmumingcheng,
                                 jine = c.Jine,
                                 shenqingren = c.ZhenshiName,
                                 riqi = c.Shenqingriqi,
                                 shenheliucheng = c.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                 liuchengID = c.Liuchengzhubiao,
                                 beizhu = c.Beizhu,
                                 //yuangongtijiao =c.Yuangongtijiao,
                                 shenhezhuangtai = c.Shenhezhuangtai,
                                 goumaizhuangtai = c.Goumaizhuangtai,
                                 dingdanshouhuo = c.Dingdanshoushuo,
                                 foujueren = c.Weiguo_realname
                             };
                    // 返回到前台的值必须按照如下的格式包括 total and rows 
                    return Json(data: new Dictionary<string, object>
                    {
                        { "total", xm.Count() },
                        { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm = from c in db.Xiangmubiaos
                             where c.Userid == loingid && c.Shenhezhuangtai == "未通过" && c.Xiangmumingcheng.Contains(searchquery)    //这里取出的是在审核主表中没有记录的项目，就是审核过的项目
                             orderby c.ID descending
                             select new
                             {
                                 id = c.ID,
                                 xiangmumingcheng = c.Xiangmumingcheng,
                                 jine = c.Jine,
                                 shenqingren = c.ZhenshiName,
                                 riqi = c.Shenqingriqi,
                                 shenheliucheng = c.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                 liuchengID = c.Liuchengzhubiao,
                                 beizhu = c.Beizhu,
                                 //yuangongtijiao =c.Yuangongtijiao,
                                 shenhezhuangtai = c.Shenhezhuangtai,
                                 goumaizhuangtai = c.Goumaizhuangtai,
                                 dingdanshouhuo = c.Dingdanshoushuo,
                                 foujueren = c.Weiguo_realname
                             };
                    // 返回到前台的值必须按照如下的格式包括 total and rows 
                    return Json(data: new Dictionary<string, object>
                    {
                        { "total", xm.Count() },
                        { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        /// <summary>
        ///   审核不通过的项目，
        /// </summary>
        [HttpPost]
        public JsonResult Get_weiguo_zg(string searchquery, string loingid, string username)
        {
            //这里两个值在linq中直接赋值无法识别
            var role = Customidentity.User(loingid).Role;
            var suoshubume = Customidentity.User(loingid).Suoshubumen;

            if (searchquery == "")
            {
                var xm2 = from x in db.Xiangmubiaos
                          join lc_zi in db.Liuchengzibiaos
                          on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.LiuchengzhubiaoID } into zonbiao
                          from c in zonbiao.DefaultIfEmpty()
                          where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && x.Shenhexulie+1 >= c.Liuchengxulie && (x.Shenhezhuangtai == "未通过")
                          orderby x.ID descending
                          select new
                          {
                              //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                              id = x.ID,
                              xiangmumingcheng = x.Xiangmumingcheng,
                              jine = x.Jine,
                              shenqingren = x.ZhenshiName,
                              riqi = x.Shenqingriqi,
                              shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                              liuchengID = x.Liuchengzhubiao,
                              beizhu = x.Beizhu,
                              shenhezhuangtai = x.Shenhezhuangtai,
                              goumaizhuangtai = x.Goumaizhuangtai,
                              dingdanshouhuo = x.Dingdanshoushuo,
                              foujueren = x.Weiguo_realname
                          };
                try
                {
                    // 返回到前台的值必须按照如下的格式包括 total and rows                   
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                var xm2 = from x in db.Xiangmubiaos
                          join lc_zi in db.Liuchengzibiaos
                          on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.LiuchengzhubiaoID } into zonbiao
                          from c in zonbiao.DefaultIfEmpty()
                          where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && x.Shenhexulie + 1 >= c.Liuchengxulie && (x.Shenhezhuangtai == "未通过" ) && x.Xiangmumingcheng.Contains(searchquery)
                          orderby x.ID descending
                          select new
                          {
                              //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                              id = x.ID,
                              xiangmumingcheng = x.Xiangmumingcheng,
                              jine = x.Jine,
                              shenqingren = x.Shenqingren,
                              riqi = x.Shenqingriqi,
                              shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                              liuchengID = x.Liuchengzhubiao,
                              beizhu = x.Beizhu,
                              shenhezhuangtai = x.Shenhezhuangtai,
                              goumaizhuangtai = x.Goumaizhuangtai,
                              dingdanshouhuo = x.Dingdanshoushuo,
                              foujueren = x.Weiguo_realname
                          };
                try
                {
                    // 返回到前台的值必须按照如下的格式包括 total and rows                   
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 员工提交项目，这里要同时操作三个表，修改项目表内容，增加审核主表记录，取审核主表主键增加审核子表记录
        /// 这里的重点是怎么在添加审核子表的时候，匹配到流程子表中的记录
        /// </summary>
        [HttpPost]
        public JsonResult Yg_tijiao(string loingid, string username, int xmid)
        {
            using(WzglContent db1 = new WzglContent())
            {
                string role = Customidentity.User(loingid).Role;      //这里做内联  无效

                var real_name = common.Customidentity.User_name(username).Zhenshiname;

                //判断项目对应的清单是否有 数据，没有数据，就返回提示让 用户添加数据
                if (db1.Xiangmuzibiaos.Where(s => s.XiangmubiaoID == xmid).Count() == 0)
                {
                    return Json(data: new { success = false, Msg = "请先添加子项目！" }, contentType: "text/html");
                }

                //int shenhexulie = db1.Xiangmubiaos.Where(c => c.ID == xmid).FirstOrDefault().Shenhexulie;   //这里做内联  无效
                db1.Xiangmubiaos.Where(c => c.ID == xmid).FirstOrDefault().Shenhexulie = 0;
                db1.Xiangmubiaos.Where(c => c.ID == xmid).FirstOrDefault().Shenhezhuangtai = "已提交";
                db1.Xiangmubiaos.Where(c => c.ID == xmid).FirstOrDefault().Chehui_realname = "";
                db1.Xiangmubiaos.Where(c => c.ID == xmid).FirstOrDefault().Chehui_user = "";

                using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
                {
                    try
                    {
                        db1.Entry(entity: db1.Xiangmubiaos.Where(c => c.ID == xmid).FirstOrDefault()).State = EntityState.Modified;
                        
                        db1.Shenhezibiaos.Add(entity: new Shenhezibiao
                        {
                            XiangmubiaoID = xmid,
                            Shenhezhuangtai = "已提交",
                            Shenhexulie = 0,              //员工提交的默认流程序列值为0
                            Shenhejuese = role,
                            Shenheren = real_name,
                            Riqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                            Shenhejiedian = "员工提交",
                            Shenheyijian ="员工提交"
                        });
                        db1.SaveChanges();
                        transaction.Complete();
                    }
                    catch (Exception ex)
                    {
                        return Json(data: new { success = false, Msg = ex.ToString() }, contentType: "text/html");
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
            };            
            return Json(data: new { success = true, Msg = "提交成功" }, contentType: "text/html");
        }

        
        /// <summary>
        /// 员工收货，这里操作了
        /// 1、更改  项目子表 的收货字段状态 收货的同时 在入库表 插入 入库信息（这里后面移到部门领导通过后再进行），在更改审核 日志状态
        /// 2、判断项目子表收货是否完成，更改项目表  对应记录 的收货状态
        /// 3、判断项目 表的 收货是否完成，更改 对应 的收货状态
        /// </summary>        
        /// <param name="zibiaoid">项目子表ID</param>
        /// <param name="id">项目表ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Yg_shouhuo(string loingid,int zibiaoid,int id)  //string mulu,
        {
            DateTime riqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            string role = Customidentity.User(loingid).Role;  //这里做内联无效
            string username = Customidentity.User(loingid).Zhenshiname;    //这里做内联无效，取用户真实名

            //string bumen = Customidentity.User(loingid).Suoshubumen;         
            //ApplicationDbContext identitydb1 = new ApplicationDbContext();
            //string jinshouren = identitydb1.Users.Where(s => s.Suoshubumen == bumen&&s.Role=="部门主管").FirstOrDefault().Zhenshiname;            

            //var zhi_id = Request.Form["jingshouren"];    //这里取到的是真实名的 经手人姓名
            //var jinshouren = Customidentity.User(zhi_id).Zhenshiname;

            //项目子表的收货
            using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
            {
                try
                {
                    using (WzglContent db_new2 = new WzglContent()) //创建一个新的上下文
                    {
                        try
                        {                                                        
                            Xiangmuzibiao xiangmuzibiao = db_new2.Xiangmuzibiaos.Where(s => s.ID == zibiaoid).FirstOrDefault();

                            //获取对应订单主表的  guid 。  这里主要是为在部门领导审核发票通过时，一次性改变入库表  物资  的入库状态                            
                            var dingdanbiao = db_new2.Xiangmubiaos.Where(c => c.ID == xiangmuzibiao.XiangmubiaoID).FirstOrDefault().DingdanzhubiaoID;
                            var samell = dingdanbiao.ToString();                                                  

                            xiangmuzibiao.Dingdanshoushuo = "已收货";
                            xiangmuzibiao.Zhengmingren = username;
                            //xiangmuzibiao.Jingshouren = jinshouren;
                            db_new2.Entry(entity: xiangmuzibiao).State = EntityState.Modified;

                            //项目子表的 维修或新增 状态，维修的状态 不做入库处理   
                            if (xiangmuzibiao.Weixiuhuoxinzeng == "新增")
                            {
                                //入库信息
                                int qingdan_count = xiangmuzibiao.Shuliang;
                                for (int i = 0; i < qingdan_count; i++)
                                {
                                    Guid qrcode_id = new Guid();
                                    //Rukubiao ruku = new Rukubiao();
                                    //ruku = db.Rukubiaos.Where(s => s.ID == id).FirstOrDefault();                                
                                    //ruku.Rukuleibie = "自购";
                                    //ruku.Qrcode_identity = qrcode_id;

                                    //var mulu = DateTime.Now.ToString("yyyy");
                                    var mulu = "zigou";
                                    var str = xiangmuzibiao.Lingyongdanwei;
                                    if (xiangmuzibiao.Lingyongdanwei.Contains(":"))
                                    {
                                        str = xiangmuzibiao.Lingyongdanwei.Replace(":", "");
                                    }

                                    //string link = "http://localhost:45822/Qrcode/Index?id=" + qrcode_id;
                                    //210.37.0.0.94

                                    string link = "http://210.37.0.94/Qrcode/Index?id=" + qrcode_id;

                                    QRCodeEncoder qrCodeEncoder = new QRCodeEncoder
                                    {
                                        QRCodeVersion = 0
                                    };
                                    Bitmap img = qrCodeEncoder.Encode(link, Encoding.UTF8);//指定utf-8编码， 支持中文   EAN_13-test


                                    //string filePath1 = string.Format("~/QR_code/{0}/{1}/{2}", mulu, xiangmuzibiao.Xiangmumingcheng, qrcode_id);//通过参数组建一个路径格式的字符串
                                    string filePath1 = string.Format("~/QR_code/{0}/{1}", mulu, str);//通过参数组建一个路径格式的字符串

                                    string filePath2 = Server.MapPath(filePath1);//将路径格式的字符串通过函数转化为服务器路径


                                    string filename = "\\" + xiangmuzibiao.Xiangmumingcheng + "_" + qrcode_id + ".jpg";  //构建 保存文件  的格式化 文件名  字符串

                                    //判断服务器路径是否存在， 不加上这个判断有  个关于 gid  的错误
                                    if (!Directory.Exists(filePath2))            //判断路径是否存在，如果不存在，则根据路径建立路径文件夹，这里只需要检验到文件夹
                                    {
                                        Directory.CreateDirectory(filePath2);
                                    }

                                    string filePath = filePath2 + filename;    //此为完整的  服务器  文件保存路径，  保存地址 + 文件名

                                    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);  //保存在系统根路径文件夹

                                    string db_path = filePath1.Replace("~", "") + filename.Replace("\\", "/");  //这是个可以在页面上被识别出来的 路径

                                    //ruku.Qr_pic_url = db_path;

                                    db_new2.Rukubiaos.Add(new Rukubiao
                                    {
                                        XiangmuzibiaoID = zibiaoid,
                                        Lingyongdanwei = xiangmuzibiao.Lingyongdanwei,

                                        Xiangmumingcheng = xiangmuzibiao.Xiangmumingcheng,

                                        //这里的三个人名都要用真实名
                                        Shenqingren = username,
                                        Jingshouren = xiangmuzibiao.Jingshouren,
                                        Zhengmingren = username,  //

                                        Shenqingriqi = xiangmuzibiao.Shenqingriqi,
                                        Guige = xiangmuzibiao.Guige,
                                        Xinghao = xiangmuzibiao.Xinghao,
                                        Fangzhididian = xiangmuzibiao.Fangzhididian,
                                        Changjia = xiangmuzibiao.Changjia,
                                        Gouzhiriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                                        Jingfeikemu = xiangmuzibiao.Jingfeikemu,


                                        Danjia = xiangmuzibiao.Danjia,
                                        Weixiuhuoxinzeng = xiangmuzibiao.Weixiuhuoxinzeng,
                                        Beizhu = xiangmuzibiao.Beizhu,

                                        Fenpeizhuangtai = "未分配",
                                        Qianshouzhuangtai = "已签收",
                                        Baosunzhuangtai = "未报损",
                                        Rukuriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),

                                        Qr_pic_url = db_path,
                                        Qrcode_identity = qrcode_id,
                                        Rukuleibie = "自购",

                                        //部门领导审核发票通过时才入库
                                        Rukuzhuangtai = "未入库",
                                        Dingdan_str = dingdanbiao,

                                        //在 部门领导 同意报销 时候，这两个值不方便获取，这里先赋值，chiyouren_Number 在资产管理时做过滤使用
                                        Chiyouren = username,
                                        Chiyouren_Number = loingid


                                    });
                                }
                                db_new2.Shenhezibiaos.Add(new Shenhezibiao
                                {
                                    XiangmubiaoID = id,
                                    Shenheyijian = "项目清单列表收货",
                                    Shenhezhuangtai = "项目清单列表收货",
                                    Shenhejuese = role,
                                    //Shenhexulie = xm.Shenhexulie + 1,
                                    Shenheren = username,
                                    Riqi = riqi,
                                    Shenhejiedian = "项目清单列表收货"
                                });
                            }
                            else
                            {
                                db_new2.Shenhezibiaos.Add(new Shenhezibiao
                                {
                                    XiangmubiaoID = id,
                                    Shenheyijian = "项目清单列表维修",
                                    Shenhezhuangtai = "项目清单列表维修",
                                    Shenhejuese = role,
                                    //Shenhexulie = xm.Shenhexulie + 1,
                                    Shenheren = username,
                                    Riqi = riqi,
                                    Shenhejiedian = "项目清单列表维修"
                                });
                            }
                                                        
                            db_new2.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            return Json(data: new { success = false, Msg = ex.ToString() }, contentType: "text/html");
                        }
                    }                    
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return Json(data: new { success = false, Msg = ex.ToString() }, contentType: "text/html");
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
            {
                try
                {
                    using (WzglContent db_new1 = new WzglContent()) //创建一个新的上下文
                    {
                        try
                        {
                            //判断项目所对应的 项目子表 是否全部收货,  以操作项目表
                            //var str = zi_shouhuofou_all.Distinct().ToString();    //判断去重后是否等于  “已收货”
                            //判断去重后是否只有一个值
                            if ((from x in db_new1.Xiangmuzibiaos
                                 where x.XiangmubiaoID == id
                                 orderby x.ID
                                 select x.Dingdanshoushuo).Distinct().Count() == 1)
                            {
                                Xiangmubiao xm = db_new1.Xiangmubiaos.Where(d => d.ID == id).FirstOrDefault();
                                xm.Dingdanshoushuo = "已收货";                                
                                
                                //审核子表写入
                                db_new1.Shenhezibiaos.Add(new Shenhezibiao
                                {
                                    XiangmubiaoID = id,
                                    Shenheyijian = "项目收货",
                                    Shenhezhuangtai = "项目收货",
                                    Shenhejuese = role,
                                    //Shenhexulie = xm.Shenhexulie + 1,
                                    Shenheren = username,
                                    Riqi = riqi,
                                    Shenhejiedian = "项目收货"
                                });

                                db_new1.Entry(xm).State = EntityState.Modified;
                                db_new1.SaveChanges();
                            }
                            else
                            {
                                return Json(new { success = true, Msg = "提交成功" }, "text/html");
                            }
                        }
                        catch (Exception ex)
                        {
                            return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                        }
                    }
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return Json(data: new { success = false, Msg = ex.ToString() }, contentType: "text/html");
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
            {
                try
                {
                    //判断项目子表所对应的  
                    using (WzglContent db_new = new WzglContent()) //创建一个新的上下文
                    {
                        try
                        {
                            //判断订单所对应的  项目表  是否全部收货      这里在做内联时出现问题，找不出原因
                            Guid? dingdanid = db_new.Xiangmubiaos.Where(predicate: d => d.ID == id).FirstOrDefault().DingdanzhubiaoID;

                            if ((from z in db_new.Xiangmubiaos
                                 where z.DingdanzhubiaoID == dingdanid
                                 orderby z.ID
                                 select z.Dingdanshoushuo).Distinct().Count() == 1)
                            {
                                Dingdanzhubiao dingdanzhubiao = db_new.Dingdanzhubiaos.Where(predicate: s => s.ID == dingdanid).FirstOrDefault();
                                dingdanzhubiao.Dingdanshoushuo = "已收货";
                                db_new.Entry(dingdanzhubiao).State = EntityState.Modified;

                                //审核子表写入
                                db_new.Shenhezibiaos.Add(new Shenhezibiao
                                {
                                    XiangmubiaoID = id,
                                    Shenheyijian = "订单收货",
                                    Shenhezhuangtai = "订单收货",
                                    Shenhejuese = role,
                                    //Shenhexulie = xm.Shenhexulie + 1,
                                    Shenheren = username,
                                    Riqi = riqi,
                                    Shenhejiedian = "订单收货"
                                });

                                db_new.SaveChanges();
                            }
                            else
                            {
                                return Json(data: new { success = true, Msg = "提交成功" }, contentType: "text/html");
                            }
                        }
                        catch (Exception ex)
                        {
                            return Json(data: new { success = false, Msg = ex.ToString() }, contentType: "text/html");
                        }
                    }
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return Json(data: new { success = false, Msg = ex.ToString() }, contentType: "text/html");
                }
                finally
                {
                    transaction.Dispose();
                }
            }                                                        
            return Json(new { success = true, Msg = "提交成功" }, "text/html");
        }
        

        //员工分配物资  分配和移交在后台实际上是同一个方法
        // 员工的移交要使用  真实名
        [HttpPost]
        public JsonResult Yg_yijiao(int id)
        {           
            try
            {
                Rukubiao ruku = new Rukubiao();
                ruku = db.Rukubiaos.Where(s => s.ID == id).FirstOrDefault();
                var userid = Request.Form["chiyouren"];

                //这里的两个赋值，放在签收的时候 才写入
                //ruku.Chiyouren = common.Customidentity.User(userid).Zhenshiname;                
                //ruku.Beizhu = Request.Form["beizhu"];                
                //ruku.Chiyouren_Number = userid;


                ruku.Yijiao_Jieshouren = userid;

                ruku.Qianshouzhuangtai = "未签收";

                db.Entry(entity: ruku).State = EntityState.Modified;
                db.SaveChanges();
                
                return Json(new { success = true, Msg = "操作完成" }, "text/html");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, Msg = ex.ToString() }, "text/html");               
            }            
        }

        //员工批量分配物资
        [HttpPost]
        public JsonResult Yijiao_Piliang(string rukuid)
        {
            string[] xmid_fenli = rukuid.Split(',');
            var xmid_fenli_conut = xmid_fenli.Count();
            
            var chiyouren_id = Request.Form["chiyouren"];       //a1ce54bc-a063-4092-86c0-d8fb48375bac 这里传过来的是  userid ，显示的是username
            var beizhu = Request.Form["beizhu"];            
            //var chiyouren = Customidentity.User(chiyouren_id).Zhenshiname;

            if (chiyouren_id == null)
            {
                return Json(new { success = false, errorMsg = "请选持有人后再提交！" }, "text/html");
            }
            else
            {
                using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
                {
                    try
                    {
                        for (int i = 0; i < xmid_fenli_conut; i++)
                        {
                            int id = Convert.ToInt32(xmid_fenli[i]);
                            Rukubiao ruku = new Rukubiao();
                            ruku = db.Rukubiaos.Where(s => s.ID == id).FirstOrDefault();
                            //ruku.Chiyouren = chiyouren;
                            //ruku.Chiyouren_Number = chiyouren_id;
                            ruku.Yijiao_Jieshouren = chiyouren_id;
                            ruku.Beizhu = beizhu;
                            ruku.Fenpeizhuangtai = "已分配";
                            ruku.Qianshouzhuangtai = "未签收";
                            ruku.Fenpeiriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                            db.Entry(entity: ruku).State = EntityState.Modified;                            
                        }
                        db.SaveChanges();
                        transaction.Complete();
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
                return Json(new { success = true, Msg = "操作完成" }, "text/html");
            }           
        }

        //员工签收物资
        [HttpPost]
        public JsonResult Yg_qianshou(int id)
        {
            //string ip2 = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork")).ToString();

            try
            {
                Rukubiao ruku = new Rukubiao();
                ruku = db.Rukubiaos.Where(s => s.ID == id).FirstOrDefault();
                var jieshouren = ruku.Yijiao_Jieshouren;

                //接收人接手后，把持有人的 真实名，和loingid 写入，同时把 移交接收人的 值 清空
                ruku.Chiyouren_Number = jieshouren;
                ruku.Chiyouren = Customidentity.User(jieshouren).Zhenshiname;
                ruku.Yijiao_Jieshouren = "";

                ruku.Qianshouriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                ruku.Qianshouzhuangtai = "已签收";

                db.Entry(entity: ruku).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, Msg = "操作完成" }, "text/html");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, Msg = ex.ToString() }, "text/html");
            }
        }

        //员工批量签收物资
        [HttpPost]
        public JsonResult Qianshou_Piliang(string rukuid)
        {
            string[] xmid_fenli = rukuid.Split(',');
            var xmid_fenli_conut = xmid_fenli.Count();

            using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
            {
                try
                {
                    for (int i = 0; i < xmid_fenli_conut; i++)
                    {
                        int id = Convert.ToInt32(xmid_fenli[i]);
                        Rukubiao ruku = new Rukubiao();
                        ruku = db.Rukubiaos.Where(s => s.ID == id).FirstOrDefault();

                        var jieshouren = ruku.Yijiao_Jieshouren;

                        //接收人接手后，把持有人的 真实名，和loingid 写入，同时把 移交接收人的 值 清空
                        ruku.Chiyouren_Number = jieshouren;
                        ruku.Chiyouren = Customidentity.User(jieshouren).Zhenshiname;
                        ruku.Yijiao_Jieshouren = "";


                        ruku.Qianshouriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                        ruku.Qianshouzhuangtai = "已签收";
                        db.Entry(entity: ruku).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            return Json(new { success = true, Msg = "签收物资完成" }, "text/html");
        }

        //一个生成二维码的例子，与本项目 无用，仅仅作为参考
        public ActionResult Qr_code()
        {
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder
            {
                QRCodeVersion = 0
            };
            Bitmap img = qrCodeEncoder.Encode("nihao", Encoding.UTF8);//指定utf-8编码， 支持中文
            
            string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "\\EAN_13-" + "test" + ".jpg";

            img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

            return View();
        }

        [HttpPost]
        public JsonResult Edt_ruku(int id)  //string bmname 好像多余
        {
            var ruku = db.Rukubiaos.Where(c => c.ID == id).FirstOrDefault();

            ruku.Xiangmumingcheng = Request.Form["xiangmumingcheng"];
            ruku.Zhengmingren = Request.Form["zhengmingren"];
            ruku.Danjia = Convert.ToDecimal( Request.Form["danjia"]);
            ruku.Guige = Request.Form["guige"];
            ruku.Xinghao = Request.Form["xinghao"];
            ruku.Fangzhididian = Request.Form["fangzhididian"];
            ruku.Beizhu = Request.Form["beizhu"];

            try
            {
                db.Entry(ruku).State = EntityState.Modified;
                db.SaveChanges();                
            }
            catch (Exception ex)
            {
                return Json(new { success = false, Msg = ex.ToString() }, "text/html");
            }
            return Json(new { success = true, Msg = "更改项目成功！" }, "text/html");
        }

        [HttpPost]
        public JsonResult Del_ruku(int id)
        {
            var ruku = db.Rukubiaos.Where(s => s.ID == id).FirstOrDefault();
            var path = ruku.Qr_pic_url;
            var name = ruku.Xiangmumingcheng;
            path = path.Replace(@"/", @"\");   //D:\\32位激活工具\\xmkgl\\xmkgl\\Uploads/2015ll/a1/也能正常删除，不过改为反向双斜杠
            var path1 = path.Substring(1);
            string filepath = AppDomain.CurrentDomain.BaseDirectory + path1;  //D:\\32位激活工具\\xmkgl\\xmkgl\\Uploads/2015ll/a1/

            try
            {   //F:\xueyuanwuziguanli\wzgl\wzgl\QR_code\2019\测试\0ea4d6d4-45df-4158-8428-22197c9385e6
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }               
                db.Rukubiaos.Remove(ruku);
                db.SaveChanges();

                return Json(new { success = true, errorMsg = "文件删除成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString() }, "text/html");
            }
        }

        //获取 项目子表的excel上传  的模板
        [Authorize(Roles = "部门主管,员工")]
        public FileResult Get_exl_ruku(string mobanname)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Content/uploads/moban/";
            string fileName = mobanname + ".xlsx";
            return File(path + fileName, "text/plain", fileName);
        }

        /// <summary>
        /// 物资拨付  电子表格入库
        /// </summary>
        /// <param name="form"></param>
        /// <param name="loingid"></param>
        /// <param name="username"></param>
        /// <param name="upFileBase"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "")]
        public JsonResult Exl_ruku(FormCollection form, string loingid, string username, HttpPostedFileBase upFileBase)     // 
        {
            ViewBag.error = "";

            //持有人赋值
            var chiyouren = Customidentity.User(loingid).Zhenshiname;

            HttpPostedFileBase fileBase = Request.Files["files"];

            if (fileBase == null || fileBase.ContentLength <= 0)
            {
                ViewBag.error = "文件不能为空";
                return Json(new { success = false, errorMsg = ViewBag.error }, "text/html");
            }

            string filename = Path.GetFileName(fileBase.FileName);    //获得文件全名
            //int filesize = file.ContentLength;//获取上传文件的大小单位为字节byte
            string fileEx = Path.GetExtension(filename);     //获取上传文件的扩展名
            string NoFileName = Path.GetFileNameWithoutExtension(filename);   //获取无扩展名的文件名

            string FileName = NoFileName + DateTime.Now.ToString("yyyyMMddhhmmss") + fileEx;   //这里是一个 文件名+时间+扩展名  的新文件名，作为保存在服务器中的  文件名

            string path = AppDomain.CurrentDomain.BaseDirectory + "Ruku_uploads";  // 这里获得的是文件的物理路径;
            string savePath = Path.Combine(path, FileName);
            fileBase.SaveAs(savePath);

            Workbook BuildReport_WorkBook = new Workbook();
            BuildReport_WorkBook.Open(savePath);//fileFullName            

            Worksheets sheets = BuildReport_WorkBook.Worksheets;

            //试题表
            Worksheet workSheetQuestion = BuildReport_WorkBook.Worksheets["Sheet1"];   //  sheet1
            Cells cellsQuestion = workSheetQuestion.Cells;    //单元格

            //int xmid = 1;
            //引用事务机制，出错时，事物回滚
            using (TransactionScope transaction = new TransactionScope())
            {
                if (form["gengxinmoshi"].ToString() == "fugai")  
                {
                    string ConString = System.Configuration.ConfigurationManager.ConnectionStrings["wzglContent"].ConnectionString;
                    SqlConnection con = new SqlConnection(ConString);
                    string sqldel = "delete from Rukubiao where Rukuleibie = '拨付'";

                    //删除对应二维码图片  这里的思路是， 自购的二维码和拨付的二维码放在不同的文件夹中
                    // 拨付 覆盖 时候，直接删除  bofu  文件夹                 
                    string filepath = AppDomain.CurrentDomain.BaseDirectory + "QR_code\\bofu\\";                     
                    if (Directory.Exists(filepath))
                    {
                        Directory.Delete(filepath, true);
                    }

                    int j;    //提示报错的行
                    try
                    {
                        con.Open();
                        SqlCommand sqlcmddel = new SqlCommand(sqldel, con);
                        sqlcmddel.ExecuteNonQuery();   //删除了现有的名单
                        con.Close(); 


                        var ruku = new List<Rukubiao>();
                        //试题表 
                        for (int i = 1; i < cellsQuestion.MaxRow + 1; i++)
                        {
                            j = i + 1;
                            try
                            {
                                Guid qrcode_id = Guid.NewGuid();

                                var mulu = "bofu";

                                //string link = "http://localhost:45822/Qrcode/Index?id=" + qrcode_id;

                                string link = "http://210.37.0.94/Qrcode/Index?id=" + qrcode_id;

                                QRCodeEncoder qrCodeEncoder = new QRCodeEncoder
                                {
                                    QRCodeVersion = 0
                                };
                                Bitmap img = qrCodeEncoder.Encode(link, Encoding.UTF8);//指定utf-8编码， 支持中文   EAN_13-test
                                                                                       //构建格式化路径字符串
                                //判断文件夹命名是否包含特殊字符，有就用 方法换掉
                                var str = cellsQuestion[i, 0].StringValue;
                                
                                if (str.Contains(":"))
                                {
                                    str = cellsQuestion[i, 0].StringValue.Replace(":", "");
                                }
                                                                                             
                                var linyongren = cellsQuestion[i, 12].StringValue;
                                if (linyongren.Contains("*"))
                                {
                                    linyongren = "_";
                                }
                                var wujian = cellsQuestion[i, 2].StringValue;

                                //string filePath1 = string.Format("~/QR_code/{0}/{1}/{2}", mulu, str, qrcode_id);//通过参数组建一个路径格式的字符串
                                string filePath1 = string.Format("~/QR_code/{0}/{1}", mulu, str);//通过参数组建一个路径格式的字符串
                                                                                                                //把路径字符串转成  服务器 路径字符串
                                string filePath2 = Server.MapPath(filePath1);//将路径格式的字符串通过函数转化为服务器路径
                                                                             //string filePath = filePath2 + "\\EAN_13-" + "test" + ".jpg";
                                                                             //构建 保存文件  的格式化 文件名  字符串
                                string filename1 = "\\" + str + "_" + linyongren + "_" + wujian + "_" + qrcode_id + ".jpg";

                                //判断服务器路径是否存在， 不加上这个判断有  个关于 gid  的错误
                                if (!Directory.Exists(filePath2))            //判断路径是否存在，如果不存在，则根据路径建立路径文件夹，这里只需要检验到文件夹
                                {
                                    Directory.CreateDirectory(filePath2);
                                }
                                //此为完整的  服务器  文件保存路径，  保存地址 + 文件名
                                string filePath = filePath2 + filename1;

                                //string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "EAN_13-" + "test111" + ".jpg";     //保存在系统根路径文件夹
                                img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                                //这是个可以在页面上被识别出来的 路径
                                string db_path = filePath1.Replace("~", "") + filename1.Replace("\\", "/");


                                ruku.Add(new Rukubiao
                                {
                                    Lingyongdanwei = cellsQuestion[i, 0].StringValue,
                                    Yiqibianhao = cellsQuestion[i, 1].StringValue,
                                    Xiangmumingcheng = cellsQuestion[i, 2].StringValue,
                                    Fenleihao = cellsQuestion[i, 3].StringValue,
                                    //型号
                                    Xinghao = cellsQuestion[i, 4].StringValue,
                                    //规格
                                    Guige = cellsQuestion[i, 5].StringValue,
                                    Danjia = Convert.ToDecimal(cellsQuestion[i, 6].StringValue),
                                    //厂家
                                    Changjia = cellsQuestion[i, 7].StringValue,
                                    Gouzhiriqi = Convert.ToDateTime(cellsQuestion[i, 8].StringValue),
                                    //现状
                                    Xianzhuang = cellsQuestion[i, 9].StringValue,
                                    //经费科目
                                    Jingfeikemu = cellsQuestion[i, 10].StringValue,
                                    //设备来源
                                    Shebeilaiyuan = cellsQuestion[i, 11].StringValue,
                                    //领用人
                                    Chiyouren = cellsQuestion[i, 12].StringValue,
                                    //经手人
                                    Jingshouren = cellsQuestion[i, 13].StringValue,
                                    //记账人
                                    Jizhangren = cellsQuestion[i, 14].StringValue,
                                    //入库时间
                                    Rukuriqi = Convert.ToDateTime(cellsQuestion[i, 15].StringValue),
                                    Fapiaohao = cellsQuestion[i, 16].StringValue,
                                    //存放地名称
                                    Fangzhididian = cellsQuestion[i, 17].StringValue,
                                    //备注
                                    Beizhu = cellsQuestion[i, 18].StringValue,
                                    Weixiuhuoxinzeng = "新增",
                                    Suoshubumen = "",
                                    Qrcode_identity = qrcode_id,
                                    Shenqingriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                                    Qianshouriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                                    Rukuleibie = "拨付",
                                    Baosunzhuangtai = "未报损",
                                    Qianshouzhuangtai = "已签收",
                                    Rukuzhuangtai = "已入库",
                                    Qr_pic_url = db_path
                                });

                                db.Rukubiaos.AddRange(ruku);
                                //db.BulkSaveChanges();
                            }
                            catch (Exception ex)
                            {
                                string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！建议使用模板上传！";
                                string ex_str = ex.ToString();
                                return Json(new { success = false, errorMsg = error }, "text/html");
                            }
                        }
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                        return Json(new { success = false, errorMsg = error }, "text/html", JsonRequestBehavior.AllowGet);
                    }
                    finally
                    {
                        /*con.Close();*/ //无论如何都要执行的语句。
                    }
                    transaction.Complete();
                }
                else   //追加模式
                {
                    int j;    //提示报错的行
                    try
                    {
                        var ruku = new List<Rukubiao>();
                        //试题表 
                        for (int i = 1; i < cellsQuestion.MaxRow + 1; i++)
                        {
                            j = i + 1;
                            try
                            {
                                Guid qrcode_id = Guid.NewGuid();

                                var mulu = "bofu";

                                //string link = "http://localhost:45822/Qrcode/Index?id=" + qrcode_id;

                                string link = "http://210.37.0.94/Qrcode/Index?id=" + qrcode_id;

                                QRCodeEncoder qrCodeEncoder = new QRCodeEncoder
                                {
                                    QRCodeVersion = 0
                                };
                                Bitmap img = qrCodeEncoder.Encode(link, Encoding.UTF8);//指定utf-8编码， 支持中文   EAN_13-test
                                                                                       //构建格式化路径字符串
                                //var str = cellsQuestion[i, 0].StringValue.Replace(":", "");//这里文件命名不支持  特殊符号

                                //判断文件夹命名是否包含特殊字符，有就用 方法换掉
                                var str = cellsQuestion[i, 0].StringValue;

                                if (str.Contains(":"))
                                {
                                    str = cellsQuestion[i, 0].StringValue.Replace(":", "");
                                }

                                var linyongren = cellsQuestion[i, 12].StringValue;
                                if(linyongren.Contains("*"))
                                {
                                    linyongren = "_";
                                }
                                var wujian = cellsQuestion[i, 2].StringValue;

                                string filePath1 = string.Format("~/QR_code/{0}/{1}", mulu, str);//通过参数组建一个路径格式的字符串
                                                                                                                //把路径字符串转成  服务器 路径字符串
                                string filePath2 = Server.MapPath(filePath1);//将路径格式的字符串通过函数转化为服务器路径
                                                                             //string filePath = filePath2 + "\\EAN_13-" + "test" + ".jpg";
                                                                             //构建 保存文件  的格式化 文件名  字符串
                                
                                string filename1 = "\\" + str + "_" + linyongren + "_"  + wujian + "_" + qrcode_id + ".jpg";

                                //判断服务器路径是否存在， 不加上这个判断有  个关于 gid  的错误
                                if (!Directory.Exists(filePath2))            //判断路径是否存在，如果不存在，则根据路径建立路径文件夹，这里只需要检验到文件夹
                                {
                                    Directory.CreateDirectory(filePath2);
                                }
                                //此为完整的  服务器  文件保存路径，  保存地址 + 文件名
                                string filePath = filePath2 + filename1;

                                //string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "EAN_13-" + "test111" + ".jpg";     //保存在系统根路径文件夹
                                img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                                //这是个可以在页面上被识别出来的 路径
                                string db_path = filePath1.Replace("~", "") + filename1.Replace("\\", "/");


                                ruku.Add(new Rukubiao {
                                    Lingyongdanwei = cellsQuestion[i, 0].StringValue,
                                    Yiqibianhao = cellsQuestion[i, 1].StringValue,
                                    Xiangmumingcheng = cellsQuestion[i, 2].StringValue,
                                    Fenleihao = cellsQuestion[i, 3].StringValue,
                                    //型号
                                    Xinghao = cellsQuestion[i, 4].StringValue,
                                    //规格
                                    Guige = cellsQuestion[i, 5].StringValue,
                                    Danjia = Convert.ToDecimal(cellsQuestion[i, 6].StringValue),
                                    //厂家
                                    Changjia = cellsQuestion[i, 7].StringValue,
                                    Gouzhiriqi = Convert.ToDateTime(cellsQuestion[i, 8].StringValue),
                                    //现状
                                    Xianzhuang = cellsQuestion[i, 9].StringValue,
                                    //经费科目
                                    Jingfeikemu = cellsQuestion[i, 10].StringValue,
                                    //设备来源
                                    Shebeilaiyuan = cellsQuestion[i, 11].StringValue,
                                    //领用人
                                    Chiyouren = cellsQuestion[i, 12].StringValue,
                                    //经手人
                                    Jingshouren = cellsQuestion[i, 13].StringValue,
                                    //记账人
                                    Jizhangren = cellsQuestion[i, 14].StringValue,
                                    //入库时间
                                    Rukuriqi = Convert.ToDateTime(cellsQuestion[i, 15].StringValue),
                                    Fapiaohao = cellsQuestion[i, 16].StringValue,
                                    //存放地名称
                                    Fangzhididian = cellsQuestion[i, 17].StringValue,
                                    //备注
                                    Beizhu = cellsQuestion[i, 18].StringValue,
                                    Weixiuhuoxinzeng = "新增",
                                    Suoshubumen = "",
                                    Qrcode_identity = qrcode_id,
                                    Shenqingriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                                    Qianshouriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                                    Rukuleibie = "拨付",
                                    Baosunzhuangtai = "未报损",
                                    Qianshouzhuangtai = "已签收",
                                    Rukuzhuangtai = "已入库",
                                    Qr_pic_url = db_path
                                });

                                db.Rukubiaos.AddRange(ruku);
                                //db.BulkSaveChanges();
                            }
                            catch (Exception ex)
                            {
                                string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！建议使用模板上传！";
                                string ex_str = ex.ToString();
                                return Json(new { success = false, errorMsg = error }, "text/html");
                            }
                        }
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                        return Json(new { success = false, errorMsg = error }, "text/html");
                    }
                    transaction.Complete();
                }
                
            }
            return Json(new { success = true, errorMsg = "祝贺您，本次信息导入成功！" }, "text/html");
        }

        /// <summary>
        /// 物资拨付  入库
        /// </summary>
        /// <param name="ruku"></param>
        /// <param name="username"></param>
        /// <param name="loingid"></param>
        /// <returns></returns>
        [HttpPost]   
        public JsonResult Add_ruku([Bind(Include = "Xiangmumingcheng,Lingyongdanwei,Guige,Xinghao,Changjia,Danjia,Weixiuhuoxinzeng,Jingfeikemu,Beizhu")]Rukubiao ruku,string username,string loingid)
        {                     
            if (ModelState.IsValid)
            {
                Guid qrcode_id = Guid.NewGuid();
                ruku.Shenqingren = username;
                ruku.Jingshouren = username;
                ruku.Chiyouren = username;
                ruku.Chiyouren_Number = loingid;
                ruku.Weixiuhuoxinzeng = "新增";
                ruku.Fenpeizhuangtai = "已分配";
                ruku.Qianshouzhuangtai = "已签收";
                ruku.Baosunzhuangtai = "未报损";
                ruku.Qrcode_identity = qrcode_id;
                ruku.Rukuleibie = "拨付";
                ruku.Rukuzhuangtai = "已入库";
                ruku.Shenqingriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                ruku.Rukuriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                ruku.Fenpeiriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                ruku.Qianshouriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

                //var mulu = DateTime.Now.ToString("yyyy");

                //string link = "http://localhost:45822/Qrcode/Index?id=" + qrcode_id;

                string link = "http://210.37.0.94/Qrcode/Index?id=" + qrcode_id;
                
                //string link = "http://192.168.0.1:7777/Qrcode/Index?id=" + qrcode_id;

                QRCodeEncoder qrCodeEncoder = new QRCodeEncoder
                {
                    QRCodeVersion = 0
                };
                Bitmap img = qrCodeEncoder.Encode(link, Encoding.UTF8);//指定utf-8编码， 支持中文   EAN_13-test

                string mulu = "bofu";
                string str = Request.Form["lingyongdanwei"];

                //构建格式化路径字符串
                //string filePath1 = string.Format("~/QR_code/{0}/{1}/{2}", mulu, ruku.Xiangmumingcheng, qrcode_id);//通过参数组建一个路径格式的字符串
                string filePath1 = string.Format("~/QR_code/{0}/{1}", mulu, str);

                //把路径字符串转成  服务器 路径字符串
                string filePath2 = Server.MapPath(filePath1);//将路径格式的字符串通过函数转化为服务器路径
                                                             //string filePath = filePath2 + "\\EAN_13-" + "test" + ".jpg";
                                                             //构建 保存文件  的格式化 文件名  字符串
                string filename = "\\" + ruku.Xiangmumingcheng + "_" + qrcode_id + ".jpg";

                //判断服务器路径是否存在， 不加上这个判断有  个关于 gid  的错误
                if (!Directory.Exists(filePath2))            //判断路径是否存在，如果不存在，则根据路径建立路径文件夹，这里只需要检验到文件夹
                {
                    Directory.CreateDirectory(filePath2);
                }
                //此为完整的  服务器  文件保存路径，  保存地址 + 文件名
                string filePath = filePath2 + filename;

                //string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "EAN_13-" + "test111" + ".jpg";     //保存在系统根路径文件夹
                img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                //这是个可以在页面上被识别出来的 路径
                string db_path = filePath1.Replace("~", "") + filename.Replace("\\", "/");

                ruku.Qr_pic_url = db_path;

                using (TransactionScope transaction = new TransactionScope())  //原子操作，事物错误回滚
                {
                    try
                    {
                        db.Rukubiaos.Add(ruku);
                        db.SaveChanges();
                        transaction.Complete();
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
                return Json(new { success = true, Msg = "新增拨付项目成功！" }, "text/html");
            }
            return Json(new { success = false, Msg = "新增拨付项目失败，请再试一试！" }, "text/html");
        }


        

        /// <summary>
        /// 这里是除了员工和管理员角色之外的角色使用的页面加载方法
        /// 部门主管  要审核的项目
        /// { b = x.Shenhexulie + 1 } equals new {  b = lc_zi.Liuchengxulie} 是为了判断审核流程进行到了哪一步
        /// </summary>        
        [HttpPost]
        public JsonResult Get_liucheng_xm(string searchquery, string loingid,string username)
        {
            //这里两个值在linq中直接赋值无法识别
            string role = Customidentity.User(loingid).Role;
            string suoshubume = Customidentity.User(loingid).Suoshubumen;
            if (searchquery == "")
            {
                try
                {
                    var xm2 = from x in db.Xiangmubiaos
                              join lc_zi in db.Liuchengzibiaos
                              on new { a = x.Liuchengzhubiao, b = x.Shenhexulie + 1 } equals new { a = lc_zi.LiuchengzhubiaoID, b = lc_zi.Liuchengxulie } into zonbiao
                              from c in zonbiao.DefaultIfEmpty()
                              where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen))&&x.Shenhezhuangtai!="撤回"&&x.Shenhezhuangtai!="未通过"&&x.Shenhezhuangtai!="通过"
                              orderby x.ID descending
                              select new
                              {
                                  //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                                  id = x.ID,
                                  xiangmumingcheng = x.Xiangmumingcheng,
                                  jine = x.Jine,
                                  shenqingren = x.ZhenshiName,
                                  riqi = x.Shenqingriqi,
                                  shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                  liuchengID = x.Liuchengzhubiao,
                                  beizhu = x.Beizhu,
                                  //yuangongtijiao = x.Yuangongtijiao,
                                  shenhezhuangtai = x.Shenhezhuangtai,
                                  goumaizhuangtai = x.Goumaizhuangtai,
                                  dingdanshouhuo = x.Dingdanshoushuo
                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm2 = from x in db.Xiangmubiaos
                              join lc_zi in db.Liuchengzibiaos
                              on new { a = x.Liuchengzhubiao, b = x.Shenhexulie + 1 } equals new { a = lc_zi.LiuchengzhubiaoID, b = lc_zi.Liuchengxulie } into zonbiao
                              from c in zonbiao.DefaultIfEmpty()
                              where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && x.Shenhezhuangtai != "撤回" && x.Shenhezhuangtai != "未通过" && x.Shenhezhuangtai != "通过" && x.Xiangmumingcheng.Contains(searchquery)
                              orderby x.ID descending
                              select new
                              {
                                  //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                                  id = x.ID,
                                  xiangmumingcheng = x.Xiangmumingcheng,
                                  jine = x.Jine,
                                  shenqingren = x.Shenqingren,
                                  riqi = x.Shenqingriqi,
                                  shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                  liuchengID = x.Liuchengzhubiao,
                                  beizhu = x.Beizhu,
                                  //yuangongtijiao = x.Yuangongtijiao,
                                  shenhezhuangtai = x.Shenhezhuangtai,
                                  goumaizhuangtai = x.Goumaizhuangtai,
                                  dingdanshouhuo = x.Dingdanshoushuo
                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }


        [HttpPost]
        public JsonResult Get_tuihui_Zg(string searchquery, string loingid, string username)
        {
            //这里两个值在linq中直接赋值无法识别
            var role = Customidentity.User(loingid).Role;
            var suoshubume = Customidentity.User(loingid).Suoshubumen;

            if (searchquery == "")
            {
                var xm2 = from x in db.Xiangmubiaos
                          join lc_zi in db.Liuchengzibiaos
                          on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.LiuchengzhubiaoID } into zonbiao
                          from c in zonbiao.DefaultIfEmpty()
                          where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && (x.Shenhexulie +1 <= c.Liuchengxulie ) && (x.Shenhezhuangtai == "撤回" )
                          orderby x.ID descending
                          select new
                          {
                              //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                              id = x.ID,
                              xiangmumingcheng = x.Xiangmumingcheng,
                              jine = x.Jine,
                              shenqingren = x.ZhenshiName,
                              riqi = x.Shenqingriqi,
                              shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                              liuchengID = x.Liuchengzhubiao,
                              beizhu = x.Beizhu,
                              shenhezhuangtai = x.Shenhezhuangtai,
                              goumaizhuangtai = x.Goumaizhuangtai,
                              dingdanshouhuo = x.Dingdanshoushuo,
                              shenhexulie = x.Shenhexulie,          //这里由  shenhexulie 和shenhe_max 值的关系 判断哪些是属于登录人 本人 退回的，本人退回的                              
                              chehuiren = x.Chehui_user,
                              chehui_realname = x.Chehui_realname
                          };                                         
                try
                {
                    // 返回到前台的值必须按照如下的格式包括 total and rows                   
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                var xm2 = from x in db.Xiangmubiaos
                          join lc_zi in db.Liuchengzibiaos
                          on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.LiuchengzhubiaoID } into zonbiao
                          from c in zonbiao.DefaultIfEmpty()
                          where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && (x.Shenhexulie + 1 == c.Liuchengxulie ) && (x.Shenhezhuangtai == "撤回") && x.Xiangmumingcheng.Contains(searchquery)
                          orderby x.ID descending
                          select new
                          {
                              //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                              id = x.ID,
                              xiangmumingcheng = x.Xiangmumingcheng,
                              jine = x.Jine,
                              shenqingren = x.Shenqingren,
                              riqi = x.Shenqingriqi,
                              shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                              liuchengID = x.Liuchengzhubiao,
                              beizhu = x.Beizhu,
                              shenhezhuangtai = x.Shenhezhuangtai,
                              goumaizhuangtai = x.Goumaizhuangtai,
                              dingdanshouhuo = x.Dingdanshoushuo,
                              shenhexulie = x.Shenhexulie,          //这里由  shenhexulie 和shenhe_max 值的关系 判断哪些是属于登录人 本人 退回的，本人退回的
                              chehuiren = x.Chehui_user,
                              chehui_realname = x.Chehui_realname
                          };                                        
                try
                {
                    // 返回到前台的值必须按照如下的格式包括 total and rows                   
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
               
        /// <summary>
        /// 部门主管  审核过的项目，但不包括 审核不通过 的项目 Get_yishen_zg
        /// </summary>
        [HttpPost]
        public JsonResult Get_yishen_zg(string searchquery, string loingid,string username)
        {
            //这里两个值在linq中直接赋值无法识别
            var role = Customidentity.User(loingid).Role;
            var suoshubume = Customidentity.User(loingid).Suoshubumen;

            if (searchquery=="")
            {
                var xm2 = from x in db.Xiangmubiaos
                          join lc_zi in db.Liuchengzibiaos
                          on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.LiuchengzhubiaoID } into zonbiao
                          from c in zonbiao.DefaultIfEmpty()
                          where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && x.Shenhexulie >= c.Liuchengxulie && (x.Shenhezhuangtai == "通过" || x.Shenhezhuangtai == "审核中")
                          orderby x.ID descending
                          select new
                          {
                              //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                              id = x.ID,
                              xiangmumingcheng = x.Xiangmumingcheng,
                              jine = x.Jine,
                              shenqingren = x.ZhenshiName,
                              riqi = x.Shenqingriqi,
                              shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                              liuchengID = x.Liuchengzhubiao,
                              beizhu = x.Beizhu,
                              shenhezhuangtai = x.Shenhezhuangtai,
                              goumaizhuangtai = x.Goumaizhuangtai,
                              dingdanshouhuo = x.Dingdanshoushuo
                          };
                try
                {
                    // 返回到前台的值必须按照如下的格式包括 total and rows                   
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                var xm2 = from x in db.Xiangmubiaos
                          join lc_zi in db.Liuchengzibiaos
                          on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.LiuchengzhubiaoID } into zonbiao
                          from c in zonbiao.DefaultIfEmpty()
                          where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && x.Shenhexulie >= c.Liuchengxulie && (x.Shenhezhuangtai == "通过" || x.Shenhezhuangtai == "审核中") && x.Xiangmumingcheng.Contains(searchquery)
                          orderby x.ID descending
                          select new
                          {
                              //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                              id = x.ID,
                              xiangmumingcheng = x.Xiangmumingcheng,
                              jine = x.Jine,
                              shenqingren = x.Shenqingren,
                              riqi = x.Shenqingriqi,
                              shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                              liuchengID = x.Liuchengzhubiao,
                              beizhu = x.Beizhu,
                              shenhezhuangtai = x.Shenhezhuangtai,
                              goumaizhuangtai = x.Goumaizhuangtai,
                              dingdanshouhuo = x.Dingdanshoushuo
                          };
                try
                {
                    // 返回到前台的值必须按照如下的格式包括 total and rows                   
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }

        /// <summary>
        /// 员工登录，获得审核通过项目 方法        
        /// </summary>  
        [HttpPost]
        public JsonResult Get_tongguo_yg(string searchquery, string loingid)   /*string mulu,*/
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;
            if (searchquery == "")
            {
                try
                {
                    var xm2 = from x in db.Xiangmubiaos
                              where x.Suoshubumen == suoshubume && x.Userid == loingid && x.Shenhezhuangtai == "通过"
                              orderby x.ID descending
                              select new
                              {
                                  //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                                  id = x.ID,
                                  xiangmumingcheng = x.Xiangmumingcheng,
                                  jine = x.Jine,
                                  shenqingren = x.ZhenshiName,
                                  riqi = x.Shenqingriqi,
                                  shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                  liuchengID = x.Liuchengzhubiao,
                                  beizhu = x.Beizhu,
                                  shenhezhuangtai = x.Shenhezhuangtai,
                                  goumaizhuangtai = x.Goumaizhuangtai,
                                  dingdanshouhuo = x.Dingdanshoushuo
                              };

                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm2 = from x in db.Xiangmubiaos
                              where x.Suoshubumen == suoshubume && x.Userid == loingid && x.Shenhezhuangtai == "通过" &&x.Xiangmumingcheng.Contains(searchquery)
                              orderby x.ID descending
                              select new
                              {
                                  //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                                  id = x.ID,
                                  xiangmumingcheng = x.Xiangmumingcheng,
                                  jine = x.Jine,
                                  shenqingren = x.ZhenshiName,
                                  riqi = x.Shenqingriqi,
                                  shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                  liuchengID = x.Liuchengzhubiao,
                                  beizhu = x.Beizhu,
                                  shenhezhuangtai = x.Shenhezhuangtai,
                                  goumaizhuangtai = x.Goumaizhuangtai,
                                  dingdanshouhuo = x.Dingdanshoushuo
                              };

                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }

        /// <summary>
        /// 员工要收货的页面，需要加载的数据
        /// </summary>       
        [HttpPost] 
        public JsonResult Get_shouhuo_yg(string searchquery, string loingid)  //string mulu,
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;

            //这里是groupview的过滤，也是可用的
            //var xm1 = from x in db.Xiangmubiaos where x.Userid==loingid && x.Goumaizhuangtai =="已购买"
            //          join s in db.Xiangmuzibiaos 
            //          on new { a=x.ID}equals new {a=s.XiangmubiaoID} into xinbiao
            //          from c in xinbiao.DefaultIfEmpty()
            //          orderby x.ID
            //          select new
            //          {
            //              //项目主表内容
            //              id = x.ID,
            //              zongbiaomingcheng = x.Xiangmumingcheng,                          
            //              //项目子表内容
            //              zibiaoid = c.ID,
            //              shouhuofou = c.Dingdanshoushuo,
            //              jine = c.Jine,
            //              zibiaomingcheng =c.Xiangmumingcheng,
            //              guige =c.Guige,
            //              danjia = c.Danjia,
            //              shuliang = c.Shuliang,
            //              shenqingren = c.Shenqingren,
            //              weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
            //              beizhu = c.Beizhu,
            //              shenqingriqi = c.Shenqingriqi                          
            //          };

            if (searchquery == "")
            {
                try
                {
                    var xm2 = from x in db.Xiangmubiaos
                              where x.Userid == loingid && x.Suoshubumen == suoshubume && x.Shenhezhuangtai == "通过" && x.Goumaizhuangtai == "已购买"
                              orderby x.ID descending
                              select new
                              {
                                  id = x.ID,
                                  xiangmumingcheng = x.Xiangmumingcheng,
                                  jine = x.Jine,
                                  shenqingren = x.ZhenshiName,
                                  riqi = x.Shenqingriqi,
                                  beizhu = x.Beizhu,
                                  shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                  liuchengID = x.Liuchengzhubiao,
                                  shouhuofou = x.Dingdanshoushuo,
                                  shenhezhuangtai = x.Shenhezhuangtai,
                                  goumaizhuangtai = x.Goumaizhuangtai,
                                  dingdanshouhuo = x.Dingdanshoushuo

                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm2 = from x in db.Xiangmubiaos
                              where x.Userid == loingid && x.Suoshubumen == suoshubume && x.Shenhezhuangtai == "通过" && x.Goumaizhuangtai == "已购买" && x.Xiangmumingcheng.Contains(searchquery)
                              orderby x.ID descending
                              select new
                              {
                                  id = x.ID,
                                  xiangmumingcheng = x.Xiangmumingcheng,
                                  jine = x.Jine,
                                  shenqingren = x.ZhenshiName,
                                  riqi = x.Shenqingriqi,
                                  beizhu = x.Beizhu,
                                  shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                  liuchengID = x.Liuchengzhubiao,
                                  shouhuofou = x.Dingdanshoushuo,
                                  shenhezhuangtai = x.Shenhezhuangtai,
                                  goumaizhuangtai = x.Goumaizhuangtai,
                                  dingdanshouhuo = x.Dingdanshoushuo

                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }

        [HttpPost]
        public JsonResult Get_yiqianshou(string searchquery, string username,string loingid)  //string mulu,
        {
            var username_real = Customidentity.User_name(username).Zhenshiname;
            if (searchquery == "")
            { 
                try
                {              //这里只过滤持有人，申请人不做过滤，就是说移交出去的东西不需要再看到了
                    var xm2 = from c in db.Rukubiaos
                              where c.Chiyouren_Number == loingid && c.Rukuzhuangtai=="已入库" &&c.Qianshouzhuangtai=="已签收"&&c.Rukuleibie=="自购" //c.Mulu == mulu && 
                              orderby c.ID descending
                              select new
                              {
                                  id = c.ID,
                                  xiangmumingcheng = c.Xiangmumingcheng,
                                  //shenqingren = c.Shenqingren,     //申请人就是登陆人本人，没有必要有申请人
                                  jingshouren = c.Jingshouren,
                                  zhengmingren = c.Zhengmingren,
                                  guige = c.Guige,
                                  xinghao = c.Xinghao,
                                  danjia = c.Danjia,
                                  weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
                                  beizhu = c.Beizhu,
                                  chiyouren = c.Chiyouren,
                                  rukuriqi = c.Rukuriqi,
                                  fenpeizhuangtai = c.Fenpeizhuangtai,
                                  qianshouzhuangtai = c.Qianshouzhuangtai,
                                  qr_pic_url = c.Qr_pic_url
                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {            //这里只过滤持有人，申请人不做过滤，就是说移交出去的东西不需要再看到了
                    var xm2 = from c in db.Rukubiaos
                              where c.Chiyouren_Number == loingid && c.Rukuzhuangtai == "已入库"&& c.Qianshouzhuangtai == "已签收" && c.Rukuleibie == "自购" && c.Xiangmumingcheng.Contains(searchquery)  //c.Mulu == mulu && 
                              orderby c.ID descending
                              select new
                              {
                                  id = c.ID,
                                  xiangmumingcheng = c.Xiangmumingcheng,
                                  //shenqingren = c.Shenqingren,     //申请人就是登陆人本人，没有必要有申请人
                                  jingshouren = c.Jingshouren,
                                  zhengmingren = c.Zhengmingren,
                                  guige = c.Guige,
                                  xinghao = c.Xinghao,
                                  danjia = c.Danjia,
                                  weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
                                  beizhu = c.Beizhu,
                                  chiyouren = c.Chiyouren,
                                  rukuriqi = c.Rukuriqi,
                                  fenpeizhuangtai = c.Fenpeizhuangtai,
                                  qianshouzhuangtai = c.Qianshouzhuangtai,
                                  qr_pic_url = c.Qr_pic_url
                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }

        // 待签收，获取页面数据
        [HttpPost]
        public JsonResult Get_DaiQianshou(string searchquery, string loingid)    //string mulu,
        {
            if (searchquery == "")
            {
                try
                {
                    var xm2 = from c in db.Rukubiaos
                              where c.Yijiao_Jieshouren == loingid&&c.Rukuzhuangtai =="已入库" && (c.Rukuleibie == "自购" || c.Rukuleibie == "拨付") && c.Qianshouzhuangtai=="未签收"  //c.Mulu == mulu && 
                              orderby c.ID descending
                              select new
                              {
                                  id = c.ID,
                                  xiangmumingcheng = c.Xiangmumingcheng,
                                  shenqingren = c.Shenqingren,
                                  jingshouren = c.Jingshouren,
                                  zhengmingren = c.Zhengmingren,
                                  //shenqingriqi = c.Shenqingriqi,
                                  guige = c.Guige,                                  
                                  xinghao = c.Xinghao,
                                  danjia = c.Danjia,
                                  weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
                                  beizhu = c.Beizhu,
                                  chiyouren = c.Chiyouren,
                                  //rukuriqi = c.Rukuriqi,
                                  //fenpeiriqi = c.Fenpeiriqi,
                                  //fenpeizhuangtai = c.Fenpeizhuangtai,
                                  qianshouzhuangtai = c.Qianshouzhuangtai,
                                  baosunzhuangtai = c.Baosunzhuangtai,
                                  qr_pic_url = c.Qr_pic_url
                              };

                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm2 = from c in db.Rukubiaos
                              where c.Yijiao_Jieshouren == loingid && c.Rukuzhuangtai == "已入库" && (c.Rukuleibie == "自购"||c.Rukuleibie=="拨付") && c.Qianshouzhuangtai == "未签收" && c.Xiangmumingcheng.Contains(searchquery)   //c.Mulu == mulu && 
                              orderby c.ID descending
                              select new
                              {
                                  id = c.ID,
                                  xiangmumingcheng = c.Xiangmumingcheng,
                                  shenqingren = c.Shenqingren,
                                  jingshouren = c.Jingshouren,
                                  zhengmingren = c.Zhengmingren,
                                  //shenqingriqi = c.Shenqingriqi,
                                  guige = c.Guige,
                                  xinghao = c.Xinghao,
                                  danjia = c.Danjia,
                                  weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
                                  beizhu = c.Beizhu,
                                  chiyouren = c.Chiyouren,
                                  //rukuriqi = c.Rukuriqi,
                                  //fenpeiriqi = c.Fenpeiriqi,
                                  //fenpeizhuangtai = c.Fenpeizhuangtai,
                                  qianshouzhuangtai = c.Qianshouzhuangtai,
                                  baosunzhuangtai = c.Baosunzhuangtai,
                                  qr_pic_url = c.Qr_pic_url
                              };

                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }


        [HttpPost]
        public JsonResult Get_xm_bofu(string searchquery, string loingid)    //string mulu,
        {
            if (searchquery == "")
            {
                try
                {
                    var xm2 = from c in db.Rukubiaos
                              where c.Rukuleibie == "拨付"   //c.Mulu == mulu && 
                              orderby c.ID descending
                              select new
                              {
                                  id = c.ID,
                                  xiangmumingcheng = c.Xiangmumingcheng,
                                  zhengmingren = c.Zhengmingren,
                                  guige = c.Guige,
                                  xinghao = c.Xinghao,
                                  danjia = c.Danjia,
                                  beizhu = c.Beizhu,
                                  fenpeiriqi = c.Fenpeiriqi,
                                  fenpeizhuangtai = c.Fenpeizhuangtai,
                                  qianshouzhuangtai = c.Qianshouzhuangtai,
                                  baosunzhuangtai = c.Baosunzhuangtai,
                                  qr_pic_url = c.Qr_pic_url,
                                  rukuriqi = c.Rukuriqi
                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm2 = from c in db.Rukubiaos
                              where c.Rukuleibie == "拨付" && c.Xiangmumingcheng.Contains(searchquery)  //c.Mulu == mulu && 
                              orderby c.ID descending
                              select new
                              {
                                  id = c.ID,
                                  xiangmumingcheng = c.Xiangmumingcheng,
                                  zhengmingren = c.Zhengmingren,
                                  guige = c.Guige,
                                  xinghao = c.Xinghao,
                                  danjia = c.Danjia,
                                  beizhu = c.Beizhu,
                                  fenpeiriqi = c.Fenpeiriqi,
                                  fenpeizhuangtai = c.Fenpeizhuangtai,
                                  qianshouzhuangtai = c.Qianshouzhuangtai,
                                  baosunzhuangtai = c.Baosunzhuangtai,
                                  qr_pic_url = c.Qr_pic_url,
                                  rukuriqi = c.Rukuriqi
                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }
        


        /// <summary>
        /// 部门主管，拟神项目数据
        /// </summary>        
        [HttpPost]
        public JsonResult Get_Nishen_xm(string searchquery, string loingid)
        {
            var suoshubume = Customidentity.User(loingid).Suoshubumen;
            if (searchquery == "")
            {
                try
                {
                    var xm = from c in db.Xiangmubiaos
                             where c.Suoshubumen == suoshubume && (c.Shenhezhuangtai == "未提交" || c.Shenhezhuangtai == "未通过")    //这里取出的是在审核主表中没有记录的项目，就是审核过的项目
                             orderby c.ID descending
                             select new
                             {
                                 id = c.ID,
                                 xiangmumingcheng = c.Xiangmumingcheng,
                                 jine = c.Jine,
                                 shenqingren = c.Shenqingren,
                                 riqi = c.Shenqingriqi,
                                 shenheliucheng = c.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                 liuchengID = c.Liuchengzhubiao,
                                 beizhu = c.Beizhu,
                                 shenhezhuangtai = c.Shenhezhuangtai,
                                 goumaizhuangtai = c.Goumaizhuangtai,
                                 dingdanshouhuo = c.Dingdanshoushuo

                             };
                    // 返回到前台的值必须按照如下的格式包括 total and rows 
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm.Count() },
                        { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm = from c in db.Xiangmubiaos
                             where c.Suoshubumen == suoshubume && (c.Shenhezhuangtai == "未提交" || c.Shenhezhuangtai == "未通过")&& c.Xiangmumingcheng.Contains(searchquery)    //这里取出的是在审核主表中没有记录的项目，就是审核过的项目
                             orderby c.ID descending
                             select new
                             {
                                 id = c.ID,
                                 xiangmumingcheng = c.Xiangmumingcheng,
                                 jine = c.Jine,
                                 shenqingren = c.Shenqingren,
                                 riqi = c.Shenqingriqi,
                                 shenheliucheng = c.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                 liuchengID = c.Liuchengzhubiao,
                                 beizhu = c.Beizhu,
                                 shenhezhuangtai = c.Shenhezhuangtai,
                                 goumaizhuangtai = c.Goumaizhuangtai,
                                 dingdanshouhuo = c.Dingdanshoushuo

                             };
                    // 返回到前台的值必须按照如下的格式包括 total and rows 
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm.Count() },
                        { "rows", xm.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }

        /// <summary>
        /// 部门主管  待购  数据
        /// </summary>       
        [HttpPost]
        public JsonResult Get_Daigou_Zg(string searchquery, string loingid)
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;
            if (searchquery == "")
            {
                try
                {
                    var xm2 = from x in db.Xiangmubiaos
                              where x.Suoshubumen == suoshubume && x.Shenhezhuangtai == "通过" && x.Goumaizhuangtai != "已购买"
                              orderby x.ID descending
                              select new
                              {
                                  //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                                  id = x.ID,
                                  xiangmumingcheng = x.Xiangmumingcheng,
                                  jine = x.Jine,
                                  shenqingren = x.Shenqingren,
                                  riqi = x.Shenqingriqi,
                                  shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                  liuchengID = x.Liuchengzhubiao,
                                  beizhu = x.Beizhu,
                                  shenhezhuangtai = x.Shenhezhuangtai,
                                  goumaizhuangtai = x.Goumaizhuangtai,
                                  dingdanshouhuo = x.Dingdanshoushuo,
                                  
                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm2 = from x in db.Xiangmubiaos
                              where x.Suoshubumen == suoshubume && x.Shenhezhuangtai == "通过" && x.Goumaizhuangtai != "已购买" && x.Xiangmumingcheng.Contains(searchquery)
                              orderby x.ID descending
                              select new
                              {
                                  //FirstName = (ur.FirstName == null) ? "N/A" : ur.FirstName,
                                  id = x.ID,
                                  xiangmumingcheng = x.Xiangmumingcheng,
                                  jine = x.Jine,
                                  shenqingren = x.Shenqingren,
                                  riqi = x.Shenqingriqi,
                                  shenheliucheng = x.Liuchengbiaoname,  //还有一个liuchengbiaoID，
                                  liuchengID = x.Liuchengzhubiao,
                                  beizhu = x.Beizhu,
                                  shenhezhuangtai = x.Shenhezhuangtai,
                                  goumaizhuangtai = x.Goumaizhuangtai,
                                  dingdanshouhuo = x.Dingdanshoushuo
                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm2.Count() },
                        { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }


        /// <summary>
        /// 待审数据
        /// 部门主管  待购  数据，订单主表和订单子表join起来的数据    
        /// 员工  已购 的数据
        /// </summary>        
        [HttpPost]
        public JsonResult Get_Yigou_Zg(string searchquery, string loingid)
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;
            var username = Customidentity.User(loingid).UserName;
            if (searchquery == "")
            {
                //这里要做一个判断，如果登录人是 部门主管 的，看到的已购项目就必须是全部，如果是员工登录，看到的就是员工自己的采购项目
                if (common.Customidentity.User(loingid).Role == "部门主管")
                {
                    try
                    { 
                        var xm2 = from c in db.Dingdanzhubiaos
                                  where c.Suoshubumen == suoshubume && (c.Baoxiaozhuangtai == "未报销"|| (c.Baoxiaozhuangtai == "申请报销"&&c.Caigouren!=loingid)) //采购人是谁无所谓，是部门主管 自己结算产生的 发票他都要看到。申请报销的 只看到，员工提交上来的
                                  orderby c.Dingdanriqi descending                   //这里不能用ID作为排列对象，ID为guid类型
                                  select new
                                  {
                                      id = c.ID,
                                      dingdanming = c.Dingdanming,
                                      dingdanjine = c.Dingdanjine,
                                      dingdanriqi = c.Dingdanriqi,
                                      dingdanshuliang = c.Dingdanshuliang,
                                      dingdanshouhuo = c.Dingdanshoushuo,
                                      mulu = c.Mulu,
                                      dingdanren = c.Dingdanren,  // 这里是已经封装好的数据源，不能这样赋值 common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                      baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                      baoxiaoyijian = c.Baoxiaoyijian,
                                      beizhu = c.Beizhu,
                                      caigouren = c.Caigouren_name,
                                      caigourenid = c.Caigouren,
                                      liuchengID = c.Liuchengzhubiao,
                                      shenheliucheng = c.Liuchengbiaoname
                                  };
                        // 返回到前台的值必须按照如下的格式包括 total and rows                         
                        var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                        return Json(easyUIPages);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {   //如果登录人不是部门主管，登录人是 员工
                    try
                    {
                        var xm2 = from c in db.Dingdanzhubiaos
                                  where c.Suoshubumen == suoshubume && c.Caigouren == loingid && c.Baoxiaozhuangtai =="未报销"           //&& c.Dingdanren == username
                                  orderby c.Dingdanriqi descending                   //这里不能用ID作为排列对象，ID为guid类型
                                  select new
                                  {
                                      id = c.ID,
                                      dingdanming = c.Dingdanming,
                                      dingdanjine = c.Dingdanjine,
                                      dingdanriqi = c.Dingdanriqi,
                                      dingdanshuliang = c.Dingdanshuliang,
                                      dingdanshouhuo = c.Dingdanshoushuo,
                                      mulu = c.Mulu,
                                      dingdanren = c.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                      baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                      baoxiaoyijian = c.Baoxiaoyijian,
                                      beizhu = c.Beizhu,
                                      caigouren = c.Caigouren_name,
                                      caigourenid = c.Caigouren, 
                                      liuchengID = c.Liuchengzhubiao,
                                      shenheliucheng = c.Liuchengbiaoname

                                  };
                        // 返回到前台的值必须按照如下的格式包括 total and rows                         
                        var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                        return Json(easyUIPages);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }                
            }
            else
            {   //这里要做一个判断，如果登录人是 部门主管 的，看到的已购项目就必须是全部，如果是员工登录，看到的就是员工自己的采购项目
                if (common.Customidentity.User(loingid).Role == "部门主管")
                {
                    try
                    {
                        var xm2 = from c in db.Dingdanzhubiaos  //
                                  where c.Suoshubumen == suoshubume && (c.Baoxiaozhuangtai == "未报销" || (c.Baoxiaozhuangtai == "申请报销" && c.Caigouren != loingid)) && c.Dingdanming.Contains(searchquery)
                                  orderby c.Dingdanriqi descending
                                  select new
                                  {
                                      id = c.ID,
                                      dingdanming = c.Dingdanming,
                                      dingdanjine = c.Dingdanjine,
                                      dingdanriqi = c.Dingdanriqi,
                                      dingdanshuliang = c.Dingdanshuliang,
                                      dingdanshouhuo = c.Dingdanshoushuo,
                                      mulu = c.Mulu,
                                      dingdanren = c.Dingdanren,
                                      baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                      baoxiaoyijian = c.Baoxiaoyijian,
                                      beizhu = c.Beizhu,
                                      caigouren = c.Caigouren_name,
                                      caigourenid = c.Caigouren,
                                      liuchengID = c.Liuchengzhubiao,
                                      shenheliucheng = c.Liuchengbiaoname
                                  };
                        // 返回到前台的值必须按照如下的格式包括 total and rows                         
                        var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                        return Json(easyUIPages);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        var xm2 = from c in db.Dingdanzhubiaos
                                  where c.Suoshubumen == suoshubume && c.Caigouren == loingid && c.Dingdanming.Contains(searchquery) && c.Baoxiaozhuangtai != "撤回" && c.Baoxiaozhuangtai != "不同意报销"
                                  orderby c.Dingdanriqi descending
                                  select new
                                  {
                                      id = c.ID,
                                      dingdanming = c.Dingdanming,
                                      dingdanjine = c.Dingdanjine,
                                      dingdanriqi = c.Dingdanriqi,
                                      dingdanshuliang = c.Dingdanshuliang,
                                      dingdanshouhuo = c.Dingdanshoushuo,
                                      mulu = c.Mulu,
                                      dingdanren = c.Dingdanren,
                                      baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                      baoxiaoyijian = c.Baoxiaoyijian,
                                      beizhu = c.Beizhu,
                                      caigouren = c.Caigouren_name,
                                      caigourenid = c.Caigouren,
                                      liuchengID = c.Liuchengzhubiao,
                                      shenheliucheng = c.Liuchengbiaoname
                                  };
                        // 返回到前台的值必须按照如下的格式包括 total and rows                         
                        var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                        return Json(easyUIPages);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }                
            }            
        }


        /// <summary>
        /// 部门主管 数据，已经提交审核的发票  
        /// </summary>         
        [HttpPost] 
        public JsonResult Get_Fp_Yishen_Zg(string searchquery, string loingid,string username)
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;
            var role = Customidentity.User(loingid).Role;
            
            if (searchquery == "")
            {
                try
                {
                    var xm2 = from x in db.Dingdanzhubiaos
                              join lc_zi in db.Fapiao_liuchneg_zibiaos
                              on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.Fapiao_liucheng_zhubiaoID } into zonbiao
                              from c in zonbiao.DefaultIfEmpty()
                              where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && x.Shenhexulie >= c.Liuchengxulie && x.Baoxiaozhuangtai != "未报销" && x.Baoxiaozhuangtai != "撤回" && x.Baoxiaozhuangtai != "同意报销"
                              orderby x.Dingdanriqi descending
                              select new
                              {
                                  id = x.ID,
                                  dingdanming = x.Dingdanming,
                                  dingdanjine = x.Dingdanjine,
                                  dingdanriqi = x.Dingdanriqi,
                                  dingdanshuliang = x.Dingdanshuliang,
                                  dingdanshouhuo = x.Dingdanshoushuo,
                                  mulu = x.Mulu,
                                  dingdanren = x.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                  baoxiaozhuangtai = x.Baoxiaozhuangtai,
                                  baoxiaoyijian = x.Baoxiaoyijian,
                                  beizhu = x.Beizhu,
                                  caigouren = x.Caigouren_name,
                                  caigourenid = x.Caigouren,
                                  chehui_user = x.Chehui_user,
                                  chehui_realname = x.Chehui_realname,
                                  liuchengID = x.Liuchengzhubiao,
                                  shenheliucheng = x.Liuchengbiaoname                                  
                              };  
                    
                    // 返回到前台的值必须按照如下的格式包括 total and rows                         
                    var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                    return Json(easyUIPages);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm2 = from x in db.Dingdanzhubiaos
                              join lc_zi in db.Fapiao_liuchneg_zibiaos
                              on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.Fapiao_liucheng_zhubiaoID } into zonbiao
                              from c in zonbiao.DefaultIfEmpty()
                              where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && x.Shenhexulie >= c.Liuchengxulie && x.Baoxiaozhuangtai != "未报销" && x.Baoxiaozhuangtai != "撤回" && x.Baoxiaozhuangtai != "同意报销" && x.Dingdanming.Contains(searchquery)
                              orderby x.Dingdanriqi descending
                              select new
                              {
                                  id = x.ID,
                                  dingdanming = x.Dingdanming,
                                  dingdanjine = x.Dingdanjine,
                                  dingdanriqi = x.Dingdanriqi,
                                  dingdanshuliang = x.Dingdanshuliang,
                                  dingdanshouhuo = x.Dingdanshoushuo,
                                  mulu = x.Mulu,
                                  dingdanren = x.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                  baoxiaozhuangtai = x.Baoxiaozhuangtai,
                                  baoxiaoyijian = x.Baoxiaoyijian,
                                  beizhu = x.Beizhu,
                                  caigouren = x.Caigouren_name,
                                  caigourenid = x.Caigouren,
                                  chehui_user = x.Chehui_user,
                                  chehui_realname = x.Chehui_realname,
                                  liuchengID = x.Liuchengzhubiao,
                                  shenheliucheng = x.Liuchengbiaoname
                              };

                    // 返回到前台的值必须按照如下的格式包括 total and rows                         
                    var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                    return Json(easyUIPages);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// 部门主管 获取 撤回 发票记录，可以是上一级撤回给部门主管的记录，也可以是部门主管撤回给 员工的记录
        /// </summary>        
        [HttpPost] 
        public JsonResult Get_Fp_Chehui_Zg(string searchquery, string loingid,string username)
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;
            var role = Customidentity.User(loingid).Role;

            if(role=="部门领导"||role == "领导")   //如果登录人的角色是 部门领导或者领导
            {
                if (searchquery == "")
                {
                    try
                    { 
                        var xm2 = from x in db.Dingdanzhubiaos
                                  join lc_zi in db.Fapiao_liuchneg_zibiaos
                                  on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.Fapiao_liucheng_zhubiaoID } into zonbiao
                                  from c in zonbiao.DefaultIfEmpty()
                                  where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && (x.Shenhexulie + 1 <= c.Liuchengxulie) && (x.Baoxiaozhuangtai == "撤回")
                                  orderby x.Dingdanriqi descending
                                  select new
                                  {
                                      id = x.ID,
                                      dingdanming = x.Dingdanming,
                                      dingdanjine = x.Dingdanjine,
                                      dingdanriqi = x.Dingdanriqi,
                                      dingdanshuliang = x.Dingdanshuliang,
                                      dingdanshouhuo = x.Dingdanshoushuo,
                                      mulu = x.Mulu,
                                      dingdanren = x.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                      baoxiaozhuangtai = x.Baoxiaozhuangtai,
                                      baoxiaoyijian = x.Baoxiaoyijian,
                                      beizhu = x.Beizhu,
                                      caigouren = x.Caigouren_name,
                                      caigourenid = x.Caigouren,
                                      chehui_realname = x.Chehui_realname,
                                      chehui_user = x.Chehui_user,
                                      liuchengID = x.Liuchengzhubiao,
                                      shenheliucheng = x.Liuchengbiaoname
                                  };
                        
                        // 返回到前台的值必须按照如下的格式包括 total and rows                         
                        var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                        return Json(easyUIPages);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        var xm2 = from x in db.Dingdanzhubiaos
                                  join lc_zi in db.Fapiao_liuchneg_zibiaos
                                  on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.Fapiao_liucheng_zhubiaoID } into zonbiao
                                  from c in zonbiao.DefaultIfEmpty()
                                  where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && (x.Shenhexulie + 1 <= c.Liuchengxulie) && (x.Baoxiaozhuangtai == "撤回") && x.Dingdanming.Contains(searchquery)
                                  orderby x.Dingdanriqi descending
                                  select new
                                  {
                                      id = x.ID,
                                      dingdanming = x.Dingdanming,
                                      dingdanjine = x.Dingdanjine,
                                      dingdanriqi = x.Dingdanriqi,
                                      dingdanshuliang = x.Dingdanshuliang,
                                      dingdanshouhuo = x.Dingdanshoushuo,
                                      mulu = x.Mulu,
                                      dingdanren = x.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                      baoxiaozhuangtai = x.Baoxiaozhuangtai,
                                      baoxiaoyijian = x.Baoxiaoyijian,
                                      beizhu = x.Beizhu,
                                      caigouren = x.Caigouren_name,
                                      caigourenid = x.Caigouren,
                                      chehui_realname = x.Chehui_realname,
                                      chehui_user = x.Chehui_user,
                                      liuchengID = x.Liuchengzhubiao,
                                      shenheliucheng = x.Liuchengbiaoname
                                  };
                        // 返回到前台的值必须按照如下的格式包括 total and rows                         
                        var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                        return Json(easyUIPages);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            else
            {
                if (role == "部门主管")      //如果登录人角色是 部门主管
                {
                    if (searchquery == "")
                    {
                        try
                        {
                            var xm2 = from x in db.Dingdanzhubiaos
                                      join lc_zi in db.Fapiao_liuchneg_zibiaos
                                      on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.Fapiao_liucheng_zhubiaoID } into zonbiao
                                      from c in zonbiao.DefaultIfEmpty()
                                      where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && (x.Shenhexulie + 1 <= c.Liuchengxulie) && (x.Baoxiaozhuangtai == "撤回")
                                      orderby x.Dingdanriqi descending
                                      select new
                                      {
                                          id = x.ID,
                                          dingdanming = x.Dingdanming,
                                          dingdanjine = x.Dingdanjine,
                                          dingdanriqi = x.Dingdanriqi,
                                          dingdanshuliang = x.Dingdanshuliang,
                                          dingdanshouhuo = x.Dingdanshoushuo,
                                          mulu = x.Mulu,
                                          dingdanren = x.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                          baoxiaozhuangtai = x.Baoxiaozhuangtai,
                                          baoxiaoyijian = x.Baoxiaoyijian,
                                          beizhu = x.Beizhu,
                                          caigouren = x.Caigouren_name,
                                          caigourenid = x.Caigouren,
                                          chehui_realname = x.Chehui_realname,
                                          chehui_user = x.Chehui_user,
                                          liuchengID = x.Liuchengzhubiao,
                                          shenheliucheng = x.Liuchengbiaoname
                                      };
                            // 返回到前台的值必须按照如下的格式包括 total and rows                         
                            var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                            return Json(easyUIPages);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        try
                        {
                            var xm2 = from x in db.Dingdanzhubiaos
                                      join lc_zi in db.Fapiao_liuchneg_zibiaos
                                      on new { a = x.Liuchengzhubiao } equals new { a = lc_zi.Fapiao_liucheng_zhubiaoID } into zonbiao
                                      from c in zonbiao.DefaultIfEmpty()
                                      where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubume || suoshubume.Contains(x.Suoshubumen)) && (x.Shenhexulie + 1 <= c.Liuchengxulie) && (x.Baoxiaozhuangtai == "撤回") && x.Dingdanming.Contains(searchquery)
                                      orderby x.Dingdanriqi descending
                                      select new
                                      {
                                          id = x.ID,
                                          dingdanming = x.Dingdanming,
                                          dingdanjine = x.Dingdanjine,
                                          dingdanriqi = x.Dingdanriqi,
                                          dingdanshuliang = x.Dingdanshuliang,
                                          dingdanshouhuo = x.Dingdanshoushuo,
                                          mulu = x.Mulu,
                                          dingdanren = x.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                          baoxiaozhuangtai = x.Baoxiaozhuangtai,
                                          baoxiaoyijian = x.Baoxiaoyijian,
                                          beizhu = x.Beizhu,
                                          caigouren = x.Caigouren_name,
                                          caigourenid = x.Caigouren,
                                          chehui_realname = x.Chehui_realname,
                                          chehui_user = x.Chehui_user,
                                          liuchengID = x.Liuchengzhubiao,
                                          shenheliucheng = x.Liuchengbiaoname
                                      };
                            // 返回到前台的值必须按照如下的格式包括 total and rows                         
                            var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                            return Json(easyUIPages);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
                else  // 登录人角色是员工
                {
                    if (searchquery == "")
                    {
                        try
                        {
                            var xm2 = from c in db.Dingdanzhubiaos
                                      where c.Suoshubumen == suoshubume && c.Caigouren == loingid && c.Baoxiaozhuangtai == "撤回" && c.Shenhexulie == -1
                                      orderby c.Dingdanriqi descending                   //这里不能用ID作为排列对象，ID为guid类型
                                      select new
                                      {
                                          id = c.ID,
                                          dingdanming = c.Dingdanming,
                                          dingdanjine = c.Dingdanjine,
                                          dingdanriqi = c.Dingdanriqi,
                                          dingdanshuliang = c.Dingdanshuliang,
                                          dingdanshouhuo = c.Dingdanshoushuo,
                                          mulu = c.Mulu,
                                          dingdanren = c.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                          baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                          baoxiaoyijian = c.Baoxiaoyijian,
                                          beizhu = c.Beizhu,
                                          caigouren = c.Caigouren_name,
                                          caigourenid = c.Caigouren,
                                          chehui_user = c.Chehui_user,
                                          chehui_realname = c.Chehui_realname,
                                          liuchengID = c.Liuchengzhubiao,
                                          shenheliucheng = c.Liuchengbiaoname
                                      };
                            // 返回到前台的值必须按照如下的格式包括 total and rows                         
                            var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                            return Json(easyUIPages);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        try
                        {
                            var xm2 = from c in db.Dingdanzhubiaos
                                      where c.Suoshubumen == suoshubume && c.Caigouren == loingid && c.Baoxiaozhuangtai == "撤回" && c.Shenhexulie == -1 && c.Dingdanming.Contains(searchquery)
                                      orderby c.Dingdanriqi descending
                                      select new
                                      {
                                          id = c.ID,
                                          dingdanming = c.Dingdanming,
                                          dingdanjine = c.Dingdanjine,
                                          dingdanriqi = c.Dingdanriqi,
                                          dingdanshuliang = c.Dingdanshuliang,
                                          dingdanshouhuo = c.Dingdanshoushuo,
                                          mulu = c.Mulu,
                                          dingdanren = c.Dingdanren,
                                          baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                          baoxiaoyijian = c.Baoxiaoyijian,
                                          beizhu = c.Beizhu,
                                          caigouren = c.Caigouren_name,
                                          caigourenid = c.Caigouren,
                                          liuchengID = c.Liuchengzhubiao,
                                          shenheliucheng = c.Liuchengbiaoname
                                      };
                            // 返回到前台的值必须按照如下的格式包括 total and rows                         
                            var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                            return Json(easyUIPages);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }            
        }


        /// <summary>
        /// 发票审核，撤回到员工重新提交的数据 
        /// </summary>        
        [HttpPost] 
        public JsonResult Get_Fp_Chehui_Yg(string searchquery, string loingid)
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;            
            if (searchquery == "")
            {
                try
                {
                    var xm2 = from c in db.Dingdanzhubiaos
                              where c.Suoshubumen == suoshubume && c.Caigouren == loingid && c.Baoxiaozhuangtai == "撤回" && c.Shenhexulie == -1
                              orderby c.Dingdanriqi descending                   //这里不能用ID作为排列对象，ID为guid类型
                              select new
                              {
                                  id = c.ID,
                                  dingdanming = c.Dingdanming,
                                  dingdanjine = c.Dingdanjine,
                                  dingdanriqi = c.Dingdanriqi,
                                  dingdanshuliang = c.Dingdanshuliang,
                                  dingdanshouhuo = c.Dingdanshoushuo,
                                  mulu = c.Mulu,
                                  dingdanren = c.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                  baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                  baoxiaoyijian = c.Baoxiaoyijian,
                                  beizhu = c.Beizhu,
                                  caigouren = c.Caigouren_name,
                                  caigourenid = c.Caigouren,
                                  chehui_user = c.Chehui_user,
                                  chehui_realname = c.Chehui_realname,
                                  liuchengID = c.Liuchengzhubiao,
                                  shenheliucheng = c.Liuchengbiaoname
                              };
                    // 返回到前台的值必须按照如下的格式包括 total and rows                         
                    var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                    return Json(easyUIPages);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm2 = from c in db.Dingdanzhubiaos
                              where c.Suoshubumen == suoshubume && c.Caigouren == loingid && c.Baoxiaozhuangtai == "撤回" && c.Shenhexulie == -1 && c.Dingdanming.Contains(searchquery)
                              orderby c.Dingdanriqi descending
                              select new
                              {
                                  id = c.ID,
                                  dingdanming = c.Dingdanming,
                                  dingdanjine = c.Dingdanjine,
                                  dingdanriqi = c.Dingdanriqi,
                                  dingdanshuliang = c.Dingdanshuliang,
                                  dingdanshouhuo = c.Dingdanshoushuo,
                                  mulu = c.Mulu,
                                  dingdanren = c.Dingdanren,
                                  baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                  baoxiaoyijian = c.Baoxiaoyijian,
                                  beizhu = c.Beizhu,
                                  caigouren = c.Caigouren_name,
                                  caigourenid = c.Caigouren,
                                  liuchengID = c.Liuchengzhubiao,
                                  shenheliucheng = c.Liuchengbiaoname
                              };
                    // 返回到前台的值必须按照如下的格式包括 total and rows                         
                    var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                    return Json(easyUIPages);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        
        /// <summary>
        /// 员工页面 数据，已经提交审核的发票  
        /// </summary>         
        [HttpPost]
        public JsonResult Get_Fp_Zaishen_Yg(string searchquery, string loingid)
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;
            if (searchquery == "")
            {
                try
                {
                    var xm2 = from c in db.Dingdanzhubiaos
                              where c.Suoshubumen == suoshubume && c.Caigouren == loingid && c.Baoxiaozhuangtai != "未报销"&&c.Baoxiaozhuangtai!="撤回" && c.Baoxiaozhuangtai != "同意报销"
                              orderby c.Dingdanriqi descending                   //这里不能用ID作为排列对象，ID为guid类型
                              select new
                              {
                                  id = c.ID,
                                  dingdanming = c.Dingdanming,
                                  dingdanjine = c.Dingdanjine,
                                  dingdanriqi = c.Dingdanriqi,
                                  dingdanshuliang = c.Dingdanshuliang,
                                  dingdanshouhuo = c.Dingdanshoushuo,
                                  mulu = c.Mulu,
                                  dingdanren = c.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                  baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                  baoxiaoyijian = c.Baoxiaoyijian,
                                  beizhu = c.Beizhu,
                                  caigouren = c.Caigouren_name,
                                  caigourenid = c.Caigouren,
                                  chehui_user = c.Chehui_user,
                                  chehui_realname = c.Chehui_realname,
                                  liuchengID = c.Liuchengzhubiao,
                                  shenheliucheng = c.Liuchengbiaoname
                              };
                    // 返回到前台的值必须按照如下的格式包括 total and rows                         
                    var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                    return Json(easyUIPages);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm2 = from c in db.Dingdanzhubiaos
                              where c.Suoshubumen == suoshubume && c.Caigouren == loingid && c.Baoxiaozhuangtai != "未报销" && c.Baoxiaozhuangtai != "撤回" && c.Baoxiaozhuangtai != "同意报销"&& c.Dingdanming.Contains(searchquery)
                              orderby c.Dingdanriqi descending
                              select new
                              {
                                  id = c.ID,
                                  dingdanming = c.Dingdanming,
                                  dingdanjine = c.Dingdanjine,
                                  dingdanriqi = c.Dingdanriqi,
                                  dingdanshuliang = c.Dingdanshuliang,
                                  dingdanshouhuo = c.Dingdanshoushuo,
                                  mulu = c.Mulu,
                                  dingdanren = c.Dingdanren,
                                  baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                  baoxiaoyijian = c.Baoxiaoyijian,
                                  beizhu = c.Beizhu,
                                  caigouren = c.Caigouren_name,
                                  caigourenid = c.Caigouren,
                                  liuchengID = c.Liuchengzhubiao,
                                  shenheliucheng = c.Liuchengbiaoname
                              };
                    // 返回到前台的值必须按照如下的格式包括 total and rows                         
                    var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                    return Json(easyUIPages);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        //发票通过的订单
        [HttpPost]
        public JsonResult Get_Fp_Tg_Yg(string searchquery, string loingid)
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;
            var username = Customidentity.User(loingid).UserName;
            //这里要做一个判断，如果登录人是 部门主管 的，看到的已购项目就必须是全部，如果是员工登录，看到的就是员工自己的采购项目
            if (Customidentity.User(loingid).Role == "部门主管")
            {
                try
                {
                    var xm2 = from c in db.Dingdanzhubiaos
                              where c.Suoshubumen == suoshubume && c.Baoxiaozhuangtai == "同意报销"        //&& c.Dingdanren == username
                              orderby c.Dingdanriqi descending                   //这里不能用ID作为排列对象，ID为guid类型
                              select new
                              {
                                  id = c.ID,
                                  dingdanming = c.Dingdanming,
                                  dingdanjine = c.Dingdanjine,
                                  dingdanriqi = c.Dingdanriqi,
                                  dingdanshuliang = c.Dingdanshuliang,
                                  dingdanshouhuo = c.Dingdanshoushuo,
                                  mulu = c.Mulu,
                                  dingdanren = c.Dingdanren,  // 这里是已经封装好的数据源，不能这样赋值 common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                  baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                  baoxiaoyijian = c.Baoxiaoyijian,
                                  beizhu = c.Beizhu,
                                  caigouren = c.Caigouren_name,
                                  caigourenid = c.Caigouren,
                                  liuchengID = c.Liuchengzhubiao,
                                  shenheliucheng = c.Liuchengbiaoname
                              };
                    // 返回到前台的值必须按照如下的格式包括 total and rows                         
                    var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                    return Json(easyUIPages);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                if (Customidentity.User(loingid).Role == "员工")
                {
                    //如果登录人不是部门主管，登录人是 员工
                    try
                    {
                        var xm2 = from c in db.Dingdanzhubiaos
                                  where c.Suoshubumen == suoshubume && c.Caigouren == loingid && c.Baoxiaozhuangtai == "同意报销"           //&& c.Dingdanren == username
                                  orderby c.Dingdanriqi descending                   //这里不能用ID作为排列对象，ID为guid类型
                                  select new
                                  {
                                      id = c.ID,
                                      dingdanming = c.Dingdanming,
                                      dingdanjine = c.Dingdanjine,
                                      dingdanriqi = c.Dingdanriqi,
                                      dingdanshuliang = c.Dingdanshuliang,
                                      dingdanshouhuo = c.Dingdanshoushuo,
                                      mulu = c.Mulu,
                                      dingdanren = c.Dingdanren,          //common.Customidentity.User(c.DingdanreID).Zhenshiname,
                                      baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                      baoxiaoyijian = c.Baoxiaoyijian,
                                      beizhu = c.Beizhu,
                                      caigouren = c.Caigouren_name,
                                      caigourenid = c.Caigouren,
                                      liuchengID = c.Liuchengzhubiao,
                                      shenheliucheng = c.Liuchengbiaoname
                                  };
                        // 返回到前台的值必须按照如下的格式包括 total and rows                         
                        var easyUIPages = new Dictionary<string, object>
                            {
                                { "total", xm2.Count() },
                                { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                            };
                        return Json(easyUIPages);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else          //如果登录人是 部门领导或者领导
                {
                    try
                    {
                        var xm2 = from c in db.Dingdanzhubiaos
                                  where (c.Suoshubumen == suoshubume||suoshubume.Contains(c.Suoshubumen)) && c.Baoxiaozhuangtai == "同意报销"
                                  orderby c.ID descending
                                  select new
                                  {
                                      id = c.ID,
                                      dingdanming = c.Dingdanming,
                                      dingdanjine = c.Dingdanjine,
                                      dingdanriqi = c.Dingdanriqi,
                                      dingdanshuliang = c.Dingdanshuliang,
                                      dingdanshouhuo = c.Dingdanshoushuo,
                                      mulu = c.Mulu,
                                      dingdanren = c.Dingdanren,
                                      baoxiaozhuangtai = c.Baoxiaozhuangtai,
                                      baoxiaoyijian = c.Baoxiaoyijian,
                                      beizhu = c.Beizhu,
                                      caigouren = c.Caigouren_name,
                                      caigourenid = c.Caigouren,
                                      liuchengID = c.Liuchengzhubiao,
                                      shenheliucheng = c.Liuchengbiaoname
                                  };
                        // 返回到前台的值必须按照如下的格式包括 total and rows                         
                        var easyUIPages = new Dictionary<string, object>
                        {
                            { "total", xm2.Count() },
                            { "rows", xm2.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                        };
                        return Json(easyUIPages);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// 部门领导，以及领导 的登录页面，在datagrid中加载的数据
        /// </summary>        
        [HttpPost]
        public JsonResult Get_Fapiao_Bmld(string searchquery, string loingid,string username)
        {
            var real_name = common.Customidentity.User(loingid).Zhenshiname;
            string role = Customidentity.User(loingid).Role;
            string suoshubumen = Customidentity.User(loingid).Suoshubumen;
            if (searchquery == "")
            {                              
                try
                {
                    var xm1 = from x in db.Dingdanzhubiaos
                              join lc_zi in db.Fapiao_liuchneg_zibiaos
                              on new { a = x.Liuchengzhubiao, b = x.Shenhexulie + 1 } equals new { a = lc_zi.Fapiao_liucheng_zhubiaoID, b = lc_zi.Liuchengxulie } into zonbiao
                              from c in zonbiao.DefaultIfEmpty()
                              where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubumen || suoshubumen.Contains(x.Suoshubumen)) && x.Baoxiaozhuangtai != "通过" && x.Baoxiaozhuangtai != "撤回"
                              orderby x.Dingdanriqi descending
                              select new
                              {
                                  id = x.ID,
                                  dingdanming = x.Dingdanming,
                                  dingdanren = x.Dingdanren,
                                  dingdanjine = x.Dingdanjine,
                                  dingdanriqi = x.Dingdanriqi,
                                  dingdanshuliang = x.Dingdanshuliang,
                                  dingdanshouhuo = x.Dingdanshoushuo,
                                  suoshubumen = x.Suoshubumen,
                                  baoxiaoyijian = x.Baoxiaoyijian,
                                  baoxiaozhuangtai = x.Baoxiaozhuangtai,
                                  beizhu = x.Beizhu,
                                  caigouren = x.Caigouren_name,
                                  liuchengID = x.Liuchengzhubiao,
                                  shenheliucheng = x.Liuchengbiaoname
                              }; 
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm1.Count() },
                        { "rows", xm1.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    var xm1 = from x in db.Dingdanzhubiaos
                              join lc_zi in db.Fapiao_liuchneg_zibiaos
                              on new { a = x.Liuchengzhubiao, b = x.Shenhexulie + 1 } equals new { a = lc_zi.Fapiao_liucheng_zhubiaoID, b = lc_zi.Liuchengxulie } into zonbiao
                              from c in zonbiao.DefaultIfEmpty()
                              where c.Rolename == role && c.LiuchengUsername == username && (x.Suoshubumen == suoshubumen || suoshubumen.Contains(x.Suoshubumen)) && x.Baoxiaozhuangtai != "通过" && x.Baoxiaozhuangtai != "撤回"
                              orderby x.Dingdanriqi descending
                              select new
                              {
                                  id = x.ID,
                                  dingdanming = x.Dingdanming,
                                  dingdanren = x.Dingdanren,
                                  dingdanjine = x.Dingdanjine,
                                  dingdanriqi = x.Dingdanriqi,
                                  dingdanshuliang = x.Dingdanshuliang,
                                  dingdanshouhuo = x.Dingdanshoushuo,
                                  suoshubumen = x.Suoshubumen,
                                  baoxiaoyijian = x.Baoxiaoyijian,
                                  baoxiaozhuangtai = x.Baoxiaozhuangtai,
                                  beizhu = x.Beizhu,
                                  liuchengID = x.Liuchengzhubiao,
                                  shenheliucheng = x.Liuchengbiaoname
                              };
                    return Json(new Dictionary<string, object>
                    {
                        { "total", xm1.Count() },
                        { "rows", xm1.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }
        
        /// <summary>
        /// 部门主管  上传发票
        /// </summary>        
        [HttpPost]
        public JsonResult Getupload_Zg(Guid id, string loingid, string mulu)
        {
            //这里两个值在linq中直接赋值无法识别
            var suoshubume = Customidentity.User(loingid).Suoshubumen;
            try
            {
                var xm1 = from s in db.Fapiao_uploads
                          where s.DingdanzhubiaoID == id
                          orderby s.ID
                          select new
                          {
                              id = s.ID,
                              filename = s.Filename,
                              fileextension = s.Fileextension,
                              filesize = s.Filesize,
                              uploadtime = s.Uploadtime,
                              beizhu = s.Beizhu,
                              filepath = s.Filepath
                          };
                // 返回到前台的值必须按照如下的格式包括 total and rows                 
                return Json(new Dictionary<string, object>
                {
                    { "total", xm1.Count() },
                    { "rows", xm1.ToPagedList((Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1, (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10) }
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]   //string id, string name, string type, string lastModifiedDate, int size,
        public ActionResult UpLoadProcessfile( HttpPostedFileBase file, Guid id, string loingid)
        {
            //var xmming = db.Xiangmuzibiaos.Where(s=>s.ID==id).FirstOrDefault().Xiangmumingcheng;
            var xmming = db.Dingdanzhubiaos.Where(s => s.ID == id).FirstOrDefault().Dingdanming;
            var mulu = DateTime.Now.ToString("yyyy");

            if (Request.Files.Count == 0)
            {
                return Json(new { jsonrpc = 2.0, success = false, message = "请选择要上传的文件。" }, "text/html");
            }
            string fileName;

            Fapiao_upload upload = new Fapiao_upload();

            if (file != null)
            {                
                string filePath1 = string.Format("~/Uploads/{0}/{1}/", mulu, xmming);//通过参数组建一个路径格式的字符串

                // 文件上传后的保存路径
                string filePath = Server.MapPath(filePath1);//将路径格式的字符串通过函数转化为服务器路径

                if (!Directory.Exists(filePath))            //判断路径是否存在，如果不存在，则根据路径建立路径文件夹，这里只需要检验到文件夹
                {
                    Directory.CreateDirectory(filePath);
                }
                string fileseze = (file.ContentLength / 1024).ToString();

                fileName = Path.GetFileName(file.FileName);// 原始文件名称

                string fileExtension = Path.GetExtension(fileName); // 文件扩展名

                //string saveName = Guid.NewGuid().ToString() + fileExtension; // 保存文件名称

                string webPath = string.Format("~/Uploads/{0}/{1}/{2}", mulu, xmming, fileName);//这里只是字符串，判断参数，用于判断上传的文件是否存在，这里需要检验到四层目录

                string generateFilePath = Server.MapPath(webPath);//这里是真实的路径，由字符串转化为路径

                if (System.IO.File.Exists(generateFilePath))//判断文件是否存在，如果存在返回提示json字符串
                {
                    return Json(new { jsonrpc = 2.0, success = false, message = fileName + "这个文件已经存在！保存失败"}, "text/html");
                }
                try
                {
                    file.SaveAs(generateFilePath);//saveas保存的参数是服务器根路径，webpath不是路径   通过路径和源文件名做参数，保存上传文件
                    
                    upload.Filepath = string.Format("Uploads/{0}/{1}/", mulu, xmming);
                    upload.Filename = fileName;
                    upload.Fileextension = fileExtension;
                    
                    upload.DingdanzhubiaoID = id;          //这里要给xiangmuguanliID赋值，存在一对多关系

                    upload.Uploadtime = DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss");

                    upload.Filesize = fileseze + "kb";
                    upload.Mulu = mulu;
                    upload.Suoshubumen = Customidentity.User(loingid).Suoshubumen;
                    upload.Loingid = loingid;
                    upload.Username = Customidentity.User(loingid).UserName;
                    db.Fapiao_uploads.Add(upload);
                    db.SaveChanges();
                    
                    return Json(new { jsonrpc = "2.0", success = true, filePath = webPath }, "text/html");
                }
                catch (Exception ex)
                {
                    if (System.IO.File.Exists(generateFilePath))//判断文件是否存在，如果存在返回提示json字符串
                    {
                        System.IO.File.Delete(generateFilePath);
                    }
                    //return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
                    return Json(new { jsonrpc = 2.0, success = false, message = ex.Message}, "text/html");
                }
            }
            else
            {
                return Json(new { jsonrpc = 2.0, success = false, message = "请选择要上传的文件！" }, "text/html");                
            }
        }

        /// <summary>
        /// 删除上传的文件
        /// </summary>        
        [HttpPost]         
        public JsonResult Feupld_del(int zhu_id, string path, string name)
        {
            path = path.Replace(@"/", @"\");//D:\\32位激活工具\\xmkgl\\xmkgl\\Uploads/2015ll/a1/也能正常删除，不过改为反向双斜杠

            string filepath = AppDomain.CurrentDomain.BaseDirectory + path;//D:\\32位激活工具\\xmkgl\\xmkgl\\Uploads/2015ll/a1/
            
            try
            {
                if (System.IO.File.Exists(filepath + name))
                {
                    System.IO.File.Delete(filepath + name);
                }
                Fapiao_upload fileupload = db.Fapiao_uploads.Find(zhu_id);

                db.Fapiao_uploads.Remove(fileupload);

                db.SaveChanges();

                return Json(new { success = true, errorMsg = "文件删除成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString() }, "text/html");
            }
        }


        [HttpPost]
        public JsonResult Feupld_del_bootstrap(int id)
        {
            Fapiao_upload fileupload = db.Fapiao_uploads.Find(id);

            var path = fileupload.Filepath;
            var name = fileupload.Filename;
            Guid guid = fileupload.DingdanzhubiaoID;

            try
            {                            

                var path1 = path.Replace(@"/", @"\");//D:\\32位激活工具\\xmkgl\\xmkgl\\Uploads/2015ll/a1/也能正常删除，不过改为反向双斜杠

                string filepath = AppDomain.CurrentDomain.BaseDirectory + path1;//D:\\32位激活工具\\xmkgl\\xmkgl\\Uploads/2015ll/a1/

                if (System.IO.File.Exists(filepath + name))
                {
                    System.IO.File.Delete(filepath + name);
                }
                db.Fapiao_uploads.Remove(fileupload);
                db.SaveChanges();

                return Json(new { success = true, errorMsg = "文件删除成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.ToString() });
            }            
        }


        /// <summary>
        /// 下载上传的文件
        /// </summary>      
        public FileResult GetFile(string filename, string filepath)
        {           
            filepath = filepath.Replace(@"/", @"\");
            string path = AppDomain.CurrentDomain.BaseDirectory + filepath;
            return File(path + filename, "text/plain", filename);//fileName应该是下载出来后的名字
        }


        /// <summary>
        /// 部门主管修改上传发票的备注
        /// </summary>       
        [HttpPost]
        public JsonResult Fapiao_edit_beizhu(int id,string beizhu)  //string bmname 好像多余
        {
            var fapiao = db.Fapiao_uploads.Where(c => c.ID == id).FirstOrDefault();
            
            fapiao.Beizhu = beizhu;
            
            try
            {
                db.Entry(fapiao).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, Msg = "更改发票备注成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, Msg = ex.ToString() });
                //return Json(data: new { success = false, Msg = ex.ToString() }, contentType: "text/html");   //这样的写法对bootstrap页面接受有问题
            }            
        }

        

        /// <summary>
        /// 部门主管审核
        /// 需要判断是不是最后一步审核
        /// 需要判断审核是否通过，是否需要初始化项目以便员工重新修改提交
        /// </summary>        
        [HttpPost]
        public JsonResult Shenhe_liucheng(string loingid,string username,int xmid)
        {
            var shenhe = Request.Form["shenhe"];
            //var shenheyijian = Request.Form["yijian"];
            var shenheyijian = Request.Form["yijian"];

            var usernam_real = common.Customidentity.User_name(username).Zhenshiname;

            if (shenheyijian == "")
            {
                shenheyijian = shenhe;
            }

            if (shenhe == null || shenhe == "")
            {
                return Json(new { success = false, Msg = "请选择审核状态后确认！" }, "text/html");
            }

            using (WzglContent db_new = new WzglContent()) //创建一个新的上下文
            {                           
                var xm = db_new.Xiangmubiaos.Where(s => s.ID == xmid).FirstOrDefault();
                var shenhezibiao = new Shenhezibiao    //  shenhezibiao表就是日志表
                {
                    XiangmubiaoID = xmid,
                    Shenheyijian = shenheyijian,
                    Shenhezhuangtai = shenhe,
                    Shenhejuese = Customidentity.User(loingid).Role,
                    Shenhexulie = xm.Shenhexulie + 1,
                    Shenheren = usernam_real,            //这里的审核人要显示真名
                    Riqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                    Shenhejiedian = usernam_real + "审核"    //这里的审核人要显示真名
                };

                //取出项目对应流程子表中的最大序列数，判断审核是否进行到了最后一步
                var liuchengzibiao = from s in db_new.Liuchengzibiaos
                                     where s.LiuchengzhubiaoID == xm.Liuchengzhubiao
                                     orderby s.ID
                                     select s.Liuchengxulie;
                var xulie_max = liuchengzibiao.Max();

                if (shenhe == "撤回")
                {
                    xm.Shenhezhuangtai = "撤回";
                    var yijian = xm.Chehui_realname + ":" + shenheyijian + "；";
                    if (xm.Chehui_user == ""|| xm.Chehui_user==null)
                    {
                        xm.Shenhexulie = xm.Shenhexulie - 1;                   //这里是为了审核不通过时返回上一级做的设置
                        
                        xm.Chehui_user = loingid;     

                        xm.Chehui_realname = common.Customidentity.User(loingid).Zhenshiname;    //这里写入的第一个撤回的人                
                        
                    }
                    else
                    {
                        var chehui = xm.Chehui_user;
                        xm.Chehui_user = chehui +"&&" +loingid;
                        xm.Shenhexulie = xm.Shenhexulie - 1;                   //这里是为了审核不通过时返回上一级做的设置                                             
                    }                   
                }
                else
                {
                    if (xm.Shenhexulie + 1 == xulie_max)  //最后一步审核
                    {
                        if (shenhe == "未通过")
                        {
                            //xm.Shenhexulie = -1;
                            //xm.Yuangongtijiao = "未提交";

                            //xm.Shenhexulie = xm.Shenhexulie - 1;                   //这里是为了审核不通过时返回上一级做的设置
                            
                            xm.Shenhezhuangtai = "未通过";
                            xm.Weiguo_user = loingid;
                            xm.Weiguo_realname = common.Customidentity.User(loingid).Zhenshiname;
                        }
                        else
                        {
                            xm.Shenhexulie = xm.Shenhexulie + 1;
                            xm.Shenhezhuangtai = shenhe;
                        }
                    }
                    else    //不是最后一步审核
                    {
                        if (shenhe == "通过")
                        {
                            xm.Shenhexulie = xm.Shenhexulie + 1;
                            xm.Shenhezhuangtai = "审核中";                           
                        }
                        else
                        {
                            xm.Shenhezhuangtai = "未通过";

                            xm.Weiguo_user = loingid;
                            xm.Weiguo_realname = common.Customidentity.User(loingid).Zhenshiname;

                            //这里是项目未通过时，初始化员工提交的项目       原先不通过项目会直接返回给员工，后来改为返回给上一层
                            //xm.Shenhexulie = -1;
                            //xm.Yuangongtijiao = "未提交";

                            //xm.Shenhexulie = xm.Shenhexulie - 1;                   //这里是为了审核不通过时返回上一级做的设置                            
                        }
                    }
                }                
                using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
                {
                    try
                    {
                        db_new.Entry(xm).State = EntityState.Modified;
                        db_new.Shenhezibiaos.Add(shenhezibiao);
                        db_new.SaveChanges();
                        transaction.Complete();
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
                return Json(new { success = true, Msg = "审核保存成功！" }, "text/html");
            }                       
        }

        

        //存在就返回false,不存在就返回true
        [HttpPost]
        public string CheckNameIsSame(string name)
        {
            string isOk = "False";
            var xm = db.Xiangmubiaos.Where(x => x.Xiangmumingcheng == name).Select(x => x.Xiangmumingcheng);
            if (!xm.Any())
            {
                isOk = "True";
            }
            return isOk + "";
        }

        [HttpPost]
        public JsonResult Save_xm([Bind(Include = "Xiangmumingcheng,Guige,Danjia,Shuliang,Beizhu")]Xiangmubiao xiangmubiao, string username,string loingid)  //,string mulu
        {
            using(WzglContent db1 = new WzglContent())
            {
                var liuchengname = Request.Form["shenheliucheng"];

                var liuchengid = db1.Liuchengzhubiaos.Where(c => c.Mingcheng == liuchengname).Select(c => c.ID).FirstOrDefault();

                xiangmubiao.Liuchengzhubiao = liuchengid;    //流程主表的主键 ID

                xiangmubiao.Liuchengbiaoname = liuchengname;  //流程主表的 mingcheng 

                xiangmubiao.Shenqingriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

                xiangmubiao.Shenqingren = username;

                xiangmubiao.ZhenshiName = common.Customidentity.User_name(username).Zhenshiname;

                xiangmubiao.Userid = loingid;

                //xiangmubiao.Mulu = mulu;

                xiangmubiao.Suoshubumen = Customidentity.User(loingid).Suoshubumen;

                //xiangmubiao.Yuangongtijiao = "未提交";

                //xiangmubiao.Baoxiaozhuangtai = "未报销";

                xiangmubiao.Shenhexulie = -1;                

                xiangmubiao.Shenhezhuangtai = "未提交";

                xiangmubiao.Goumaizhuangtai = "未购买";

                xiangmubiao.Dingdanshoushuo = "未收货";

                //xiangmubiao.DingdanzhubiaoID = Convert.ToInt64();

                ////审核主表部分，建立一个项目相当于新建一个审核主表记录
                //var shenhezhubiao = new Shenhezhubiao();

                //shenhezhubiao.XiangmubiaoID=

                if (ModelState.IsValid)
                {
                    try
                    {
                        db1.Xiangmubiaos.Add(xiangmubiao);
                        db1.SaveChanges();
                        return Json(new { success = true, Msg = "新增项目成功！" }, "text/html");
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                    }
                }
            };
            
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "新增项目失败，请再试一试！" }, "text/html");
        }

        [HttpPost]
        public JsonResult Update_xm([Bind(Include = "ID,Xiangmumingcheng,Guige,Danjia,Shuliang,Weixiuhuoxinzeng,Beizhu")]Xiangmubiao xiangmubiao1, int id)  //string bmname 好像多余
        {
            var xiangmubiao = db.Xiangmubiaos.Where(c=>c.ID==id).FirstOrDefault();

            xiangmubiao.Xiangmumingcheng = Request.Form["xiangmumingcheng"];            

            xiangmubiao.Beizhu = Request.Form["beizhu"];

            var liuchengname = Request.Form["shenheliucheng"];

            var liuchengid = db.Liuchengzhubiaos.Where(c => c.Mingcheng == liuchengname).Select(c => c.ID).FirstOrDefault();

            xiangmubiao.Liuchengzhubiao = liuchengid;    //流程主表的主键 ID

            xiangmubiao.Liuchengbiaoname = liuchengname;  //流程主表的 mingcheng 

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(xiangmubiao).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true, Msg = "更改项目成功！" }, "text/html");                                    
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "更改项目失败，请再试一试！" }, "text/html");
        }

        [HttpPost]
        public JsonResult Del_xm(int id)
        {
            Xiangmubiao xiangmubiao = db.Xiangmubiaos.Find(id);

            try
            {
                db.Xiangmubiaos.Remove(xiangmubiao);
                db.SaveChanges();
                return Json(new { success = true });                
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.ToString() }, "text/html");
            }
        }

        
        /// <summary>
        ///   项目子表内容
        /// </summary>        
        [HttpPost]
        public JsonResult Get_xmzibiao(string searchquery, int xmid)
        {            
            int page = (Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1;
            int rows = (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10;

            var xm = from c in db.Xiangmuzibiaos
                     where c.XiangmubiaoID == xmid
                     orderby c.ID
                     select new
                     {
                         id = c.ID,
                         xiangmumingcheng = c.Xiangmumingcheng,
                         guige = c.Guige,
                         shuliang = c.Shuliang,
                         danjia = c.Danjia,
                         jine = c.Jine,
                         shenqingren = c.ZhenshiName,
                         jingshouren = c.Jingshouren,
                         weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
                         riqi = c.Shenqingriqi,
                         beizhu = c.Beizhu,
                         shouhuofou=c.Dingdanshoushuo,
                         xinghao = c.Xinghao,
                         lingyongdanwei = c.Lingyongdanwei
                      };
            try
            {                
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
        public JsonResult Get_xmbiao( Guid dingdanID)
        {
            int page = (Request.Form["page"] != null) ? int.Parse(Request.Form["page"]) : 1;
            int rows = (Request.Form["rows"] != null) ? int.Parse(Request.Form["rows"]) : 10;

            //Guid id = dingdanID
            var xm = from c in db.Xiangmubiaos
                     where c.DingdanzhubiaoID == dingdanID
                     orderby c.ID
                     select new
                     {
                         id = c.ID,
                         xiangmumingcheng = c.Xiangmumingcheng,                        
                         jine = c.Jine,
                         shenqingren = c.ZhenshiName,                         
                         riqi = c.Shenqingriqi,
                         beizhu = c.Beizhu,
                         shenhezhuangtai = c.Shenhezhuangtai,
                         goumaizhuangtai = c.Goumaizhuangtai,
                         dingdanshouhuo = c.Dingdanshoushuo,
                         liuchengID= c.Liuchengzhubiao,
                         shenheliucheng=c.Liuchengbiaoname
                     };
            try
            {
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
        public JsonResult Save_xmzibiao([Bind(Include = "Xiangmumingcheng,Lingyongdanwei,Guige,Xinghao,Shuliang,Changjia,Danjia,Weixiuhuoxinzeng,Jingfeikemu,Beizhu")]Xiangmuzibiao zibiao, string username,int xmid)
        {
            Guid str = Guid.NewGuid();
            //zibiao.ID = str;
            zibiao.Shenqingriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            zibiao.Shenqingren = Customidentity.User_name(username).Zhenshiname;
            zibiao.Lingyongren = Customidentity.User_name(username).Zhenshiname;
            zibiao.ZhenshiName = Customidentity.User_name(username).Zhenshiname;   // 获取真实名
            zibiao.XiangmubiaoID = xmid;
            zibiao.Jine = zibiao.Danjia * zibiao.Shuliang;
            zibiao.Dingdanshoushuo = "未收货";
            if (ModelState.IsValid)
            {
                using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
                {
                    try
                    {
                        db.Xiangmuzibiaos.Add(zibiao);
                        db.SaveChanges();
                        using (WzglContent db_new = new WzglContent()) //创建一个新的上下文
                        {
                            var xmbiao = db_new.Xiangmubiaos.Where(s => s.ID == xmid).FirstOrDefault();
                            var zibiao_heji = from s in db_new.Xiangmuzibiaos
                                              where s.XiangmubiaoID == xmid
                                              orderby s.ID
                                              select s.Jine;
                            xmbiao.Jine = zibiao_heji.Sum();
                            db_new.Entry(xmbiao).State = EntityState.Modified;
                            db_new.SaveChanges();
                        }
                        transaction.Complete();
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
                return Json(new { success = true, Msg = "添加购置设备成功！" }, "text/html");                
            }            
            return Json(new { success = false, Msg = "添加设备失败，请再试一试！" }, "text/html");
        }


        [HttpPost]
        public JsonResult Update_xmzibiao(int id, string username, int xmid)  //string bmname 好像多余
        {
            var xiangmubiao = db.Xiangmuzibiaos.Where(c => c.ID == id).FirstOrDefault();

            xiangmubiao.Xiangmumingcheng = Request.Form["xiangmumingcheng"];

            xiangmubiao.Guige = Request.Form["guige"];

            xiangmubiao.Shuliang = Convert.ToInt32(Request.Form["shuliang"]);

            xiangmubiao.Danjia = Convert.ToDecimal(Request.Form["danjia"]);

            xiangmubiao.Weixiuhuoxinzeng = Request.Form["weixiuhuoxinzeng"];

            xiangmubiao.Beizhu = Request.Form["beizhu"];

            xiangmubiao.Xinghao = Request.Form["xinghao"];

            xiangmubiao.Lingyongdanwei = Request.Form["lingyongdanwei"];

            xiangmubiao.Shenqingriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

            //xiangmubiao.Shenqingren = username;

            xiangmubiao.Jine = xiangmubiao.Danjia * xiangmubiao.Shuliang;

            using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
            {
                try
                {
                    db.Entry(xiangmubiao).State = EntityState.Modified;
                    db.SaveChanges();
                    using (WzglContent db_new = new WzglContent()) //创建一个新的上下文
                    {
                        var xmbiao = db_new.Xiangmubiaos.Where(s => s.ID == xmid).FirstOrDefault();
                        var zibiao_heji = from s in db_new.Xiangmuzibiaos
                                          where s.XiangmubiaoID == xmid
                                          orderby s.ID
                                          select s.Jine;
                        xmbiao.Jine = zibiao_heji.Sum();
                        db_new.Entry(xmbiao).State = EntityState.Modified;
                        db_new.SaveChanges();
                    }
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            return Json(new { success = true, Msg = "更改项目成功！" }, "text/html");                   
        }


        [HttpPost]
        public JsonResult Del_xmzibiao(int id,int xmid)
        {
            Xiangmuzibiao xiangmubiao = db.Xiangmuzibiaos.Find(id);

            using (TransactionScope transaction = new TransactionScope())//原子操作，事物错误回滚
            {
                try
                {
                    db.Xiangmuzibiaos.Remove(xiangmubiao);
                    db.SaveChanges();
                    using (WzglContent db_new = new WzglContent()) //创建一个新的上下文
                    {
                        var xmbiao = db_new.Xiangmubiaos.Where(s => s.ID == xmid).FirstOrDefault();
                        var zibiao_heji = from s in db_new.Xiangmuzibiaos
                                          where s.XiangmubiaoID == xmid
                                          orderby s.ID
                                          select s.Jine;
                        xmbiao.Jine = zibiao_heji.Sum();
                        db_new.Entry(xmbiao).State = EntityState.Modified;
                        db_new.SaveChanges();
                    }
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            return Json(new { success = true }, "text/html");
        }

        [HttpPost]
        [Authorize(Roles = "部门主管,员工")]
        public JsonResult Excel_zibiao(FormCollection form, int xmid,string username,HttpPostedFileBase upFileBase)     // 
        {
            ViewBag.error = "";
            
            HttpPostedFileBase fileBase = Request.Files["files"];

            if (fileBase == null || fileBase.ContentLength <= 0)
            {
                ViewBag.error = "文件不能为空";
                return Json(new { success = false, errorMsg = ViewBag.error }, "text/html");
            }

            //申请人
            var username_real = common.Customidentity.User_name(username).Zhenshiname;

            string filename = Path.GetFileName(fileBase.FileName);    //获得文件全名
            //int filesize = file.ContentLength;//获取上传文件的大小单位为字节byte
            string fileEx = Path.GetExtension(filename);     //获取上传文件的扩展名
            string NoFileName = Path.GetFileNameWithoutExtension(filename);   //获取无扩展名的文件名

            string FileName = NoFileName + DateTime.Now.ToString("yyyyMMddhhmmss") + fileEx;   //这里是一个 文件名+时间+扩展名  的新文件名，作为保存在服务器中的  文件名

            string path = AppDomain.CurrentDomain.BaseDirectory + "content/uploads/excel/";  // 这里获得的是文件的物理路径;
            string savePath = Path.Combine(path, FileName);
            fileBase.SaveAs(savePath);

            Workbook BuildReport_WorkBook = new Workbook();
            BuildReport_WorkBook.Open(savePath);//fileFullName            

            Worksheets sheets = BuildReport_WorkBook.Worksheets;

            //试题表
            Worksheet workSheetQuestion = BuildReport_WorkBook.Worksheets["Sheet1"];   //  sheet1
            Cells cellsQuestion = workSheetQuestion.Cells;    //单元格

            //引用事务机制，出错时，事物回滚
            using (TransactionScope transaction = new TransactionScope())
            {                
                if (form["gengxinmoshi"].ToString() == "fugai")
                {
                    string ConString = System.Configuration.ConfigurationManager.ConnectionStrings["wzglContent"].ConnectionString;
                    SqlConnection con = new SqlConnection(ConString);

                    string sqldel = "delete from Xiangmuzibiao where xiangmubiaoID=" + xmid.ToString();  //这个筛选方式写法是否正确待检验

                    int j;    //提示报错的行
                    try
                    {
                        con.Open();
                        SqlCommand sqlcmddel = new SqlCommand(sqldel, con);
                        sqlcmddel.ExecuteNonQuery();   //删除了现有的名单

                        var ruku = new List<Rukubiao>();
                        var zibiao_range = new List<Xiangmuzibiao>();

                        //试题表 
                        for (int i = 1; i < cellsQuestion.MaxDataRow + 1; i++)
                        {
                            j = i + 1;
                            try
                            {
                                //电子表格少一个数量，
                                //zibiao_range.Add(new Xiangmuzibiao
                                //{
                                //    Lingyongdanwei = cellsQuestion[i, 0].StringValue,                                    
                                //    Xiangmumingcheng = cellsQuestion[i, 1].StringValue,                                    
                                //    //型号
                                //    Xinghao = cellsQuestion[i, 2].StringValue,
                                //    //规格
                                //    Guige = cellsQuestion[i, 3].StringValue,
                                //    //单价
                                //    Danjia = Convert.ToDecimal(cellsQuestion[i, 4].StringValue),
                                //    //数量
                                //    Shuliang = Convert.ToInt32(cellsQuestion[i, 5].StringValue),
                                //    //厂家
                                //    Changjia = cellsQuestion[i, 6].StringValue,
                                //    Gouzhiriqi = Convert.ToDateTime(cellsQuestion[i, 7].StringValue),
                                //    //现状
                                //    Xianzhuang = "在用",
                                //    //经费科目
                                //    Jingfeikemu = cellsQuestion[i, 8].StringValue,                                                                      
                                //    //设备来源
                                //    Shebeilaiyuan = "购置",
                                //    //领用人
                                //    Lingyongren = cellsQuestion[i, 9].StringValue,
                                //    //经手人
                                //    Jingshouren = cellsQuestion[i, 10].StringValue,
                                //    Weixiuhuoxinzeng = cellsQuestion[i, 11].StringValue,
                                //    //存放地名称
                                //    Fangzhididian = cellsQuestion[i, 12].StringValue,
                                //    //备注
                                //    Beizhu = cellsQuestion[i, 13].StringValue,                                                                      
                                //    Shenqingriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                                //    XiangmubiaoID =xmid,
                                //    Shenqingren = username_real,
                                //    ZhenshiName = username_real,
                                //    Dingdanshoushuo = "未收货",
                                //    Jine = Convert.ToDecimal(cellsQuestion[i, 4].StringValue)* Convert.ToInt32 (cellsQuestion[i, 5].StringValue),                                    
                                //});


                                Xiangmuzibiao zibiao = new Xiangmuzibiao();
                               
                                if (cellsQuestion[i, 0].StringValue == null || cellsQuestion[i, 0].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“领用单位”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Lingyongdanwei = cellsQuestion[i, 0].StringValue;
                                }
                                
                                if (cellsQuestion[i, 1].StringValue == null || cellsQuestion[i, 1].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“仪器名称”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Xiangmumingcheng = cellsQuestion[i, 1].StringValue;
                                }

                                if (cellsQuestion[i, 2].StringValue == null || cellsQuestion[i, 2].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“型号”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Xinghao = cellsQuestion[i, 2].StringValue;
                                }

                                if (cellsQuestion[i, 3].StringValue == null || cellsQuestion[i, 3].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“规格”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Guige = cellsQuestion[i, 3].StringValue;
                                }

                                if (cellsQuestion[i, 4].StringValue == "")//cellsQuestion[i, 3].IntValue == null ||   ，int的类型永不等于null
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“单价”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Danjia = Convert.ToDecimal(cellsQuestion[i, 4].StringValue);
                                }

                                if (cellsQuestion[i, 5].StringValue == null || cellsQuestion[i,5].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“数量”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Shuliang = Convert.ToInt32(cellsQuestion[i, 5].StringValue);
                                }                                

                                if (cellsQuestion[i, 6].StringValue == null || cellsQuestion[i, 6].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“厂家”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Changjia = cellsQuestion[i, 6].StringValue;
                                }

                                zibiao.Gouzhiriqi =Convert.ToDateTime( cellsQuestion[i, 7].StringValue);
                                zibiao.Jingfeikemu = cellsQuestion[i, 8].StringValue;
                                zibiao.Lingyongren = cellsQuestion[i, 9].StringValue;
                                zibiao.Jingshouren = cellsQuestion[i, 10].StringValue;
                                zibiao.Weixiuhuoxinzeng = cellsQuestion[i, 11].StringValue;
                                zibiao.Fangzhididian = cellsQuestion[i, 12].StringValue;
                                zibiao.Beizhu = cellsQuestion[i, 13].StringValue;


                                zibiao.XiangmubiaoID = xmid;
                                zibiao.Shenqingren = username_real;                                
                                zibiao.ZhenshiName = username_real;
                                zibiao.Dingdanshoushuo = "未收货";
                                zibiao.Jine = zibiao.Danjia * zibiao.Shuliang;
                                zibiao.Shenqingriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                                db.Xiangmuzibiaos.Add(zibiao);                                
                            }
                            //db.SaveChanges();放在这里有问题
                            catch (Exception ex)
                            {
                                string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“配置数量”、“单价”列为数值格式，建议使用模板上传！";
                                string ex_str = ex.ToString();
                                return Json(new { success = false, errorMsg = error }, "text/html");
                            }
                        }
                        db.SaveChanges();      
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                        return Json(new { success = false, errorMsg = error }, "text/html");
                    }
                    finally
                    {
                        con.Close(); //无论如何都要执行的语句。
                    }
                    //db.SaveChanges();

                    //这里用  addrange添加有问题
                    using (WzglContent db_new = new WzglContent()) //创建一个新的上下文
                    {
                        var xmbiao = db_new.Xiangmubiaos.Where(s => s.ID == xmid).FirstOrDefault();
                        var zibiao_heji = from s in db_new.Xiangmuzibiaos
                                          where s.XiangmubiaoID == xmid
                                          orderby s.ID
                                          select s.Jine;
                        xmbiao.Jine = zibiao_heji.Sum();
                        db_new.Entry(xmbiao).State = EntityState.Modified;
                        db_new.SaveChanges();
                    }
                    // ViewBag.fugai = "离开覆盖了";
                }
                else  //追加模式
                {
                    int j;    //提示报错的行
                    try
                    {
                        //试题表 
                        for (int i = 1; i < cellsQuestion.MaxRow + 1; i++)
                        {
                            j = i + 1;
                            try
                            {
                                Xiangmuzibiao zibiao = new Xiangmuzibiao();

                                if (cellsQuestion[i, 0].StringValue == null || cellsQuestion[i, 0].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“领用单位”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Lingyongdanwei = cellsQuestion[i, 0].StringValue;
                                }

                                if (cellsQuestion[i, 1].StringValue == null || cellsQuestion[i, 1].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“仪器名称”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Xiangmumingcheng = cellsQuestion[i, 1].StringValue;
                                }

                                if (cellsQuestion[i, 2].StringValue == null || cellsQuestion[i, 2].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“型号”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Xinghao = cellsQuestion[i, 2].StringValue;
                                }

                                if (cellsQuestion[i, 3].StringValue == null || cellsQuestion[i, 3].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“规格”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Guige = cellsQuestion[i, 3].StringValue;
                                }

                                if (cellsQuestion[i, 4].StringValue == "")//cellsQuestion[i, 3].IntValue == null ||   ，int的类型永不等于null
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“单价”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Danjia = Convert.ToDecimal(cellsQuestion[i, 4].StringValue);
                                }

                                if (cellsQuestion[i, 5].StringValue == null || cellsQuestion[i, 5].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“数量”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Shuliang = Convert.ToInt32(cellsQuestion[i, 5].StringValue);
                                }

                                if (cellsQuestion[i, 6].StringValue == null || cellsQuestion[i, 6].StringValue == "")
                                {
                                    string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“厂家”列为不能为空，建议使用模板上传！";
                                    return Json(new { success = false, errorMsg = error }, "text/html");
                                }
                                else
                                {
                                    zibiao.Changjia = cellsQuestion[i, 6].StringValue;
                                }

                                zibiao.Gouzhiriqi = Convert.ToDateTime(cellsQuestion[i, 7].StringValue);
                                zibiao.Jingfeikemu = cellsQuestion[i, 8].StringValue;
                                zibiao.Lingyongren = cellsQuestion[i, 9].StringValue;
                                zibiao.Jingshouren = cellsQuestion[i, 10].StringValue;
                                zibiao.Weixiuhuoxinzeng = cellsQuestion[i, 11].StringValue;
                                zibiao.Fangzhididian = cellsQuestion[i, 12].StringValue;
                                zibiao.Beizhu = cellsQuestion[i, 13].StringValue;


                                zibiao.XiangmubiaoID = xmid;
                                zibiao.Shenqingren = username_real;
                                zibiao.ZhenshiName = username_real;
                                zibiao.Dingdanshoushuo = "未收货";
                                zibiao.Jine = zibiao.Danjia * zibiao.Shuliang;
                                zibiao.Shenqingriqi = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                                db.Xiangmuzibiaos.Add(zibiao);
                            }
                            catch (Exception ex)
                            {
                                string error = "第" + j + "行记录插入有误，请认真检查格式后再导入！“配置数量”、“单价”列为数值格式，建议使用模板上传！";
                                string ex_str = ex.ToString();
                                return Json(new { success = false, errorMsg = error }, "text/html");
                            }
                        }
                        db.SaveChanges();

                        using (WzglContent db_new = new WzglContent()) //创建一个新的上下文
                        {
                            var xmbiao = db_new.Xiangmubiaos.Where(s => s.ID == xmid).FirstOrDefault();
                            var zibiao_heji = from s in db_new.Xiangmuzibiaos
                                              where s.XiangmubiaoID == xmid
                                              orderby s.ID
                                              select s.Jine;
                            xmbiao.Jine = zibiao_heji.Sum();
                            db_new.Entry(xmbiao).State = EntityState.Modified;
                            db_new.SaveChanges();
                        }                            
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                        return Json(new { success = false, errorMsg = error }, "text/html");
                    }
                }
                transaction.Complete();
            }            
            return Json(new { success = true, errorMsg = "祝贺您，本次信息导入成功！" }, "text/html");
        }

        //获取 项目子表的excel上传  的模板
        [Authorize(Roles = "部门主管,员工")]
        public FileResult GetexcelFile(string mobanname)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Content/uploads/moban/";
            string fileName = mobanname + ".xlsx";
            return File(path + fileName, "text/plain", fileName);
        }


        [HttpPost]
        public JsonResult Liucheng_tooltip(int liuchengID)
        {                     
            var zibiao_str = from s in db.Liuchengzibiaos
                             where s.LiuchengzhubiaoID == liuchengID
                             orderby s.ID
                             select new {name= s.ZhenshiName };
            string yuangong = "";

            if (zibiao_str.Any())
            {
                foreach (var item in zibiao_str)
                {
                    yuangong = yuangong+ item.name + "；";
                }
            }
            return Json(new { success = true, Msg ="参与审核人员依次为： "+ yuangong }, "text/html");            
        }


        [HttpPost]
        public JsonResult Fapiao_Liucheng_tooltip(int liuchengID) 
        {
            var zibiao_str = from s in db.Fapiao_liuchneg_zibiaos
                             where s.Fapiao_liucheng_zhubiaoID == liuchengID
                             orderby s.ID
                             select new { name = s.ZhenshiName };
            string yuangong = "";

            if (zibiao_str.Any())
            {
                foreach (var item in zibiao_str)
                {
                    yuangong = yuangong + item.name + "；";
                }
            }
            return Json(new { success = true, Msg = "参与审核人员依次为： " + yuangong }, "text/html");
        }

    }
}