using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wzgl.DAL;
using wzgl.Models;

namespace wzgl.Controllers
{
    public class QrcodeController : Controller
    {
        private WzglContent db = new WzglContent();
        // GET: Qrcode
        /// <summary>
        /// 这里在测试时候有个问题，跳转链接  的参数名为 id, 这里接受的参数名为qrcode_id，造成了访问路径无法识别。后期统一改参数名为id
        /// </summary>
        /// <param name="id">入库表中的guid 值</param>
        /// <returns></returns>
        public ActionResult Index(Guid id)             
        {            
            var ruku = db.Rukubiaos.Where(s => s.Qrcode_identity == id).FirstOrDefault();
            
            return View(ruku);
        }

        public ActionResult Test_phone_back()
        {            
            return View();
        }
    }
}