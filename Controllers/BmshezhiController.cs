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
    [Authorize]
    public class BmshezhiController : Controller
    {
        
        // GET: Bmshezhi   BmshezhiIndex
        private WzglContent db = new WzglContent();   
        
        [Authorize(Roles = "管理员")]
        public ActionResult BmshezhiIndex()
        {
            //为所辖部门获取部门设置列表
            //var GenreLst = new List<string>();

            //var GenreQry = from d in db.bumenxingzhis
            //               orderby d.ID
            //               select d.xingzhi;
            //GenreLst.AddRange(GenreQry.Distinct());
            ////ViewBag.zxdanwei = new SelectList(GenreLst);
            //ViewBag.xingzhi = GenreLst;

            return View();
        }

        //存在就返回false,不存在就返回true
        [HttpPost]
        public string CheckNameIsSame(string name)
        {
            string isOk = "False";

            var xm = db.Bumenshezhis.Where(x => x.BmName == name).Select(x => x.BmName);

            if (!xm.Any())
            {
                isOk = "True";
            }
            return isOk + "";
        }

        [HttpPost]
        public JsonResult GetBumen(string searchquery)
        {
            int page = (Request.Form["page"] != null) ? Int32.Parse(Request.Form["page"]) : 1;
            int rows = (Request.Form["rows"] != null) ? Int32.Parse(Request.Form["rows"]) : 10;
            var xm = from s in db.Bumenshezhis
                     where s.BmName.Contains(searchquery)
                     orderby s.BmName
                     select new { id = s.ID,bmname=s.BmName, bmxingzhi=s.Bmxingzhi };
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
        public JsonResult SaveBumen([Bind(Include = "BmName,Bmxingzhi")]Bumenshezhi bumenshezhi)
        {
            if (ModelState.IsValid)
            {                              
                try
                {
                    db.Bumenshezhis.Add(bumenshezhi);
                    db.SaveChanges();
                    
                    return Json(new { success = true, Msg = "新增部门成功！" }, "text/html");
                }
                catch (Exception ex)
                {                    
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }            
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "新增部门失败，请再试一试！" }, "text/html");
        }
        

        [HttpPost]        
        public JsonResult UpdateBumen(string bmname)  //string bmname 好像多余
        {
            var bumen = db.Bumenshezhis.Where(s => s.BmName == bmname).FirstOrDefault();

            bumen.Bmxingzhi = Request.Form["Bmxingzhi"];

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(bumen).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true, Msg = "更改部门信息成功！" }, "text/html");
                    //return Json(bumen1);                 
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, Msg = ex.ToString() }, "text/html");
                }
            }            
            //底下这个方法更简洁
            return Json(new { success = false, Msg = "更改部门信息失败，请再试一试！" }, "text/html");
        }



        [HttpPost]
        // [ValidateAntiForgeryToken]     这种方式获取不到数据
        public JsonResult DelBumen(int id)
        {
            Bumenshezhi bumen = db.Bumenshezhis.Find(id);

            try
            {
                db.Bumenshezhis.Remove(bumen);
                db.SaveChanges();
                return Json(new { success = true });
                //return Json(bumen);
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.ToString() }, "text/html");
            }
        }
    }
}