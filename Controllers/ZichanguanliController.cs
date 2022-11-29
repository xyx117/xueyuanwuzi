using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wzgl.DAL;
using wzgl.Models;
using PagedList;
using System.Data.SqlClient;
using System.Transactions;

namespace wzgl.Controllers
{
    public class ZichanguanliController : Controller
    {
        private WzglContent db = new WzglContent();
        // GET: Zichanguanli
        public ActionResult Index(string loingid)
        {
            return View();
        }
        
        //登录人查看所有入库物资信息
        public ActionResult Ruku_all(string loingid)
        {
            ApplicationDbContext identitydb1 = new ApplicationDbContext();
            var suoshubumen = identitydb1.Users.Where(s => s.Id == loingid).FirstOrDefault().Suoshubumen;

            //var NameLst = new List<string>();
            var userName = from c in identitydb1.Users
                           where c.Suoshubumen == suoshubumen || c.Suoshubumen.Contains(suoshubumen)
                           orderby c.Id
                           select c;

            return View(userName);
        }

        //管理员查看所有入库  报损 物资信息
        public ActionResult Baosun_admin(string loingid)
        {
            return View();
        }
        
        /// <summary>
        /// 管理员看到全部资产
        /// </summary>
        /// <param name="searchquery"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get_ruku_all(string searchquery, string loingid)  //string mulu,
        {
            var name_real = common.Customidentity.User(loingid).Zhenshiname;
            var role = common.Customidentity.User(loingid).Role;
            
            if(role=="部门主管")
            {
                if (searchquery == "")
                {
                    try
                    {
                        var xm2 = from c in db.Rukubiaos
                                  where c.Chiyouren == name_real   //c.Mulu == mulu && 
                                  orderby c.ID descending
                                  select new
                                  {
                                      id = c.ID,
                                      xiangmumingcheng = c.Xiangmumingcheng,
                                      //shenqingren = c.Shenqingren,     //申请人就是登陆人本人，没有必要有申请人
                                      jingshouren = c.Jingshouren,
                                      zhengmingren = c.Zhengmingren,
                                      guige = c.Guige,
                                      danjia = c.Danjia,
                                      weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
                                      beizhu = c.Beizhu,
                                      chiyouren = c.Chiyouren,
                                      rukuriqi = c.Rukuriqi,
                                      fenpeizhuangtai = c.Fenpeizhuangtai,
                                      qianshouzhuangtai = c.Qianshouzhuangtai,
                                      qr_pic_url = c.Qr_pic_url,
                                      lingyongdanwei = c.Lingyongdanwei,                                      
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
                                  where c.Xiangmumingcheng.Contains(searchquery) && c.Chiyouren == name_real  //c.Mulu == mulu && c.Shenqingren == username_real &&
                                  orderby c.ID descending
                                  select new
                                  {
                                      id = c.ID,
                                      xiangmumingcheng = c.Xiangmumingcheng,
                                      //shenqingren = c.Shenqingren,     //申请人就是登陆人本人，没有必要有申请人
                                      jingshouren = c.Jingshouren,
                                      zhengmingren = c.Zhengmingren,
                                      guige = c.Guige,
                                      danjia = c.Danjia,
                                      weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
                                      beizhu = c.Beizhu,
                                      chiyouren = c.Chiyouren,
                                      rukuriqi = c.Rukuriqi,
                                      fenpeizhuangtai = c.Fenpeizhuangtai,
                                      qianshouzhuangtai = c.Qianshouzhuangtai,
                                      qr_pic_url = c.Qr_pic_url,
                                      lingyongdanwei = c.Lingyongdanwei
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
            else    //如果登录角色是其他人，如 员工，部门领导，领导 
            {
                if (searchquery == "")
                {
                    try
                    {
                        var xm2 = from c in db.Rukubiaos
                                  where c.Chiyouren == name_real   //c.Mulu == mulu && 
                                  orderby c.ID descending
                                  select new
                                  {
                                      id = c.ID,
                                      xiangmumingcheng = c.Xiangmumingcheng,
                                      //shenqingren = c.Shenqingren,     //申请人就是登陆人本人，没有必要有申请人
                                      jingshouren = c.Jingshouren,
                                      zhengmingren = c.Zhengmingren,
                                      guige = c.Guige,
                                      danjia = c.Danjia,
                                      weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
                                      beizhu = c.Beizhu,
                                      chiyouren = c.Chiyouren,
                                      rukuriqi = c.Rukuriqi,
                                      fenpeizhuangtai = c.Fenpeizhuangtai,
                                      qianshouzhuangtai = c.Qianshouzhuangtai,
                                      qr_pic_url = c.Qr_pic_url,
                                      lingyongdanwei = c.Lingyongdanwei,
                                      yijiao_jieshouren = c.Yijiao_Jieshouren
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
                                  where c.Xiangmumingcheng.Contains(searchquery) && c.Chiyouren == name_real //c.Mulu == mulu  &&
                                  orderby c.ID descending
                                  select new
                                  {
                                      id = c.ID,
                                      xiangmumingcheng = c.Xiangmumingcheng,
                                      //shenqingren = c.Shenqingren,     //申请人就是登陆人本人，没有必要有申请人
                                      jingshouren = c.Jingshouren,
                                      zhengmingren = c.Zhengmingren,
                                      guige = c.Guige,
                                      danjia = c.Danjia,
                                      weixiuhuoxinzeng = c.Weixiuhuoxinzeng,
                                      beizhu = c.Beizhu,
                                      chiyouren = c.Chiyouren,
                                      rukuriqi = c.Rukuriqi,
                                      fenpeizhuangtai = c.Fenpeizhuangtai,
                                      qianshouzhuangtai = c.Qianshouzhuangtai,
                                      qr_pic_url = c.Qr_pic_url,
                                      lingyongdanwei = c.Lingyongdanwei,
                                      yijiao_jieshouren = c.Yijiao_Jieshouren
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
        }

        


        public JsonResult Sql()
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                transaction.Complete();
            }

            string ConString = System.Configuration.ConfigurationManager.ConnectionStrings["wzglContent"].ConnectionString;
            SqlConnection con = new SqlConnection(ConString);

            string sqldel = "delete from Rukubiao where Rukuleibie = '拨付'";

            try
            {
                con.Open();
                SqlCommand sqlcmddel = new SqlCommand(sqldel, con);
                sqlcmddel.ExecuteNonQuery();   //删除了现有的名单
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return Json(new { success = false, errorMsg = error }, "text/html", JsonRequestBehavior.AllowGet);
            }
            finally
            {
                con.Close(); //无论如何都要执行的语句。
            }
            return Json(new { success = true, errorMsg = "删除成功！" }, "text/html", JsonRequestBehavior.AllowGet);
        }
    }
}