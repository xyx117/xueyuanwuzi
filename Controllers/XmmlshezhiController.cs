using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wzgl.DAL;
using wzgl.Models;

namespace wzgl.Controllers
{
    public class XmmlshezhiController : Controller
    {
        // GET: Xmmlshezhi
        public ActionResult Index()
        {
            return View();
        }

        private WzglContent db = new WzglContent();

        //存在就返回false,不存在就返回true
        [HttpPost]
        public String CheckNameIsSame(String name)
        {
            string isOk = "False";

            var xm = db.Mulushezhis.Where(x => x.Name == name).Select(x => x.Name);

            if (!xm.Any())
            {
                isOk = "True";
            }
            return isOk + "";
        }


        // GET: users
        public ActionResult XmmlshezhiIndex()
        {
            //return View(db.xiangmumulus.ToList());
            return View();
        }


        [HttpPost]
        public JsonResult GetMulu()
        {
            int page = (Request.Form["page"] != null) ? Int32.Parse(Request.Form["page"]) : 1;

            int rows = (Request.Form["rows"] != null) ? Int32.Parse(Request.Form["rows"]) : 10;


            var xm = from s in db.Mulushezhis
                     orderby s.Chuangjian_time descending
                     select new { s.ID ,s.Name, s.Chuangjian_time,s.Beizhu };

            try
            {
                // 返回到前台的值必须按照如下的格式包括 total and rows 
                var easyUIPages = new Dictionary<string, object>();
                easyUIPages.Add("total", xm.Count());
                easyUIPages.Add("rows", xm.ToPagedList(page, rows));
                return Json(easyUIPages);

            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]        
        public JsonResult SaveMulu([Bind(Include = "Name,Beizhu")]Mulushezhi Mulushezhi)//在这里的绑定中没有主键ID，因为在数据库中，ID是自动增长得
        {
            Mulushezhi.Chuangjian_time = DateTime.Now;    //目录的创建时间

            //if (xiangmumulu.fgxzshenhefou == "on")
            //{
            //    xiangmumulu.fgxzshenhefou = "审核";
            //}
            //else
            //{
            //    xiangmumulu.fgxzshenhefou = "不审";
            //}

            if (ModelState.IsValid)
            {
                try
                {
                    db.Mulushezhis.Add(Mulushezhi);
                    db.SaveChanges();
                    //return RedirectToAction("Index");
                    //return Json(xiangmumulu);
                    return Json(new { success = true, Msg = "保存目录成功！" }, "text/html");
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }          
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "some error occured." });
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]    [Bind(Include = "ID,Name,Beizhu,fgxzshenhefou,Kaishishijian,Jieshushijian,chuangjian_time")] xiangmumulu xiangmumulu,
        public JsonResult UpdateMulu(int ID)
        {
            //int id = niandu.ID;       //设置断点，测试用
            //string f = niandu.Name;

            //xiangmumulu.chuangjian_time = DateTime.Now;

            var xiangmumulu = db.Mulushezhis.Where(s => s.ID == ID).FirstOrDefault();

            xiangmumulu.Beizhu = Request.Form["Beizhu"];
            //xiangmumulu.Kaishishijian = Convert.ToDateTime(Request.Form["Kaishishijian"]);
            //xiangmumulu.Jieshushijian = Convert.ToDateTime(Request.Form["Jieshushijian"]);

            

            //string fgldshenhefou = Request.Form["fgxzshenhefou"];   //switchbutton传过来的到底是什么类型的值

            //if (fgldshenhefou == "on")
            //{
            //    xiangmumulu.fgxzshenhefou = "审核";
            //}
            //else
            //{
            //    xiangmumulu.fgxzshenhefou = "不审";
            //}

            if (ModelState.IsValid)
            {
                try
                {

                    db.Entry(xiangmumulu).State = EntityState.Modified;
                    db.SaveChanges();
                    //return Json(xiangmumulu);
                    return Json(new { success = true, Msg = "编辑目录保存成功！" }, "text/html");
                }
                catch (Exception ex)
                {
                    //return Json(new { errorMsg = ex.ToString() }, "text/html");
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }
           
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "some error occured." });
        }



        [HttpPost]
        // [ValidateAntiForgeryToken]     这种方式获取不到数据
        public JsonResult DelMulu(FormCollection form,int ID)
        {
            string muluname = form["muluname"];   //删除实际就是按主键类型，查找主键，删除主键记录

            //if (mulucustom.muluisempty(muluname))
            //{                
            //}
            //else
            //{
            //    return Json(new { success = false, errorMsg = "该目录下的内容不为空，不能删除" }, "text/html");
            //}

            try
            {
                Mulushezhi niandu = db.Mulushezhis.Where(s => s.ID == ID).FirstOrDefault();

                db.Mulushezhis.Remove(niandu);
                db.SaveChanges();
                return Json(new { success = true, Msg = "删除目录成功！" }, "text/html");
                //return Json(new { success = true ,errorMsg="sdf"}, "text/html");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                
            }
        }
    }
}