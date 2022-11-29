using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wzgl.App_Start;
using wzgl.common;
using wzgl.DAL;
using wzgl.Models;
using DotNet.Highcharts;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;

namespace wzgl.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private WzglContent db = new WzglContent();

        public ActionResult BarChart_admin(string loingid)
        {
            //ApplicationDbContext identitydb1 = new ApplicationDbContext();

            //判断登录人的角色，如果是 admin 或者 部门负责人 就可以看到全部人的，否则只能看到自己的信息物资
            var role = Customidentity.User(loingid).Role;            

            if (role == "管理员" || role == "部门主管")//
            {
                //var username = from c in identitydb1.Users
                //               where c.Id!=""
                //               orderby c.Id
                //               select c.Zhenshiname;

                var username = (from s in db.Rukubiaos
                                orderby s.ID
                                select s.Chiyouren).Distinct();

                //有空研究一下这两种写法  Application.Users和 UserManager.Users
                //var xm = from c in UserManager.Users                        
                //         where c.Role == "员工" && c.Suoshubumen == bumen
                //         orderby c.UserName
                //         select new { id = c.Id};            


                if (username.Any())                                  //判断是否有部门存在，没有则转到数据错误页面
                {
                    int username_count = username.Count();

                    string[] y_categories = new string[username_count];  //声明一个部门总数位数的数组

                    y_categories = username.ToArray();              //由部门名称数组化                               

                    object[] zongjianshu = new object[username_count];
                    object[] fenpeishu = new object[username_count];
                    object[] jine = new object[username_count];

                    for (int i = 0; i < username_count; i++)
                    {
                        string geren = y_categories[i];

                        //持有仪器设备
                        zongjianshu[i] = db.Rukubiaos.Where(s => s.Chiyouren == geren).Count();

                        //申请购置仪器设备
                        fenpeishu[i] = db.Rukubiaos.Where(s => s.Shenqingren == geren).Count();

                        //持有资产金额
                        jine[i] = db.Rukubiaos.Where(s => s.Chiyouren == geren && s.Rukuzhuangtai == "已入库").OrderBy(s => s.ID).Sum(s => (decimal?)s.Danjia) ?? 0;  //Sum(t => (decimal?)t.Amount) ?? 0

                        //jine[i] = db.Rukubiaos.Where(s => s.Chiyouren == geren && s.Fenpeizhuangtai == "已分配").OrderBy(s => s.ID).Select(s => s.Danjia).Sum();
                    }

                    Highcharts chart = new Highcharts("chart")
                       .InitChart(new DotNet.Highcharts.Options.Chart { DefaultSeriesType = ChartTypes.Column, Width = 1500 })
                       .SetTitle(new DotNet.Highcharts.Options.Title { Text = "个人物资数量柱形图" })

                       .SetXAxis(new XAxis
                       {
                           Categories = y_categories,
                           Title = new XAxisTitle { Text = string.Empty },
                           Labels = new XAxisLabels { Rotation = 90 },
                           TickWidth = 1,
                       })

                       .SetYAxis(new YAxis
                       {
                           Title = new YAxisTitle
                           {
                               Text = "数量",
                               Align = AxisTitleAligns.High,
                           }
                       })

                       .SetTooltip(new Tooltip
                       {
                       //HeaderFormat = "<span style=\"font-size:10px\">{point.key}</span><table>",
                       //PointFormat = "<tr><td style=\"color:{series.color}\"} >{series.name}:</td><td style=\"padding:0px\"><b>{point.y}个</b></td></tr>",
                       //FooterFormat = "</table>",
                       //Shared = true,
                       //UseHTML = true

                       HeaderFormat = "<span style=\"font-size:10px\">{point.key}</span><table>",
                           PointFormat = "<tr><td style=\"color:{series.color}\"} >{series.name}:</td><td style=\"padding:0px\"><b>{point.y} </b></td></tr>",
                           FooterFormat = "</table>",
                           Shared = true,
                           UseHTML = true
                       })

                       .SetCredits(new Credits { Enabled = false })

                       .SetSeries(new[]
                       {
                        new Series { Name = "持有仪器设备（件）", Data =new Data(zongjianshu) },

                        new Series { Name = "申请购置仪器设备（件）", Data = new Data(fenpeishu) },

                        new Series { Name = "持有资产金额（元）", Data = new Data(jine) }
                       });
                    return View(chart);
                }
                else
                {
                    return View("basicbar_error");
                }
            }
            else       //如果登录人不是  管理员 部门主管
            {
                //因为入库表中的  持有人  是根据 真实名记录的，所以这里要取出登录人的 真实名
                var name = Customidentity.User(loingid).Zhenshiname;                              

                var didian = (from s in db.Rukubiaos where s.Chiyouren == name && s.Lingyongdanwei != "" &&s.Lingyongdanwei != null
                              orderby s.ID
                              select s.Lingyongdanwei).Distinct();
                
                //有空研究一下这两种写法  Application.Users和 UserManager.Users
                //var xm = from c in UserManager.Users                        
                //         where c.Role == "员工" && c.Suoshubumen == bumen
                //         orderby c.UserName
                //         select new { id = c.Id};            


                if (didian.Any())                                  //判断是否有部门存在，没有则转到数据错误页面
                {
                    int didian_count = didian.Count();

                    string[] x_categories = new string[didian_count];  //声明一个部门总数位数的数组

                    x_categories = didian.ToArray();              //由部门名称数组化                               

                    object[] zongjianshu = new object[didian_count];
                    object[] fenpeishu = new object[didian_count];
                    object[] jine = new object[didian_count];

                    for (int i = 0; i < didian_count; i++)
                    {
                        string geren = x_categories[i];

                        //持有仪器设备
                        zongjianshu[i] = db.Rukubiaos.Where(s => s.Chiyouren == name && s.Lingyongdanwei == geren).Count();

                        //申请购置仪器设备
                        fenpeishu[i] = db.Rukubiaos.Where(s => s.Shenqingren == name && s.Lingyongdanwei == geren).Count();

                        //持有资产金额
                        jine[i] = db.Rukubiaos.Where(s => s.Chiyouren == name && s.Rukuzhuangtai == "已入库" && s.Lingyongdanwei == geren).OrderBy(s => s.ID).Sum(s => (decimal?)s.Danjia) ?? 0;  //Sum(t => (decimal?)t.Amount) ?? 0

                        //jine[i] = db.Rukubiaos.Where(s => s.Chiyouren == geren && s.Fenpeizhuangtai == "已分配").OrderBy(s => s.ID).Select(s => s.Danjia).Sum();
                    }

                    Highcharts chart = new Highcharts("chart")
                       .InitChart(new DotNet.Highcharts.Options.Chart { DefaultSeriesType = ChartTypes.Column, Width = 1500 })
                       .SetTitle(new DotNet.Highcharts.Options.Title { Text = "个人物资数量柱形图" })

                       .SetXAxis(new XAxis
                       {
                           Categories = x_categories,
                           Title = new XAxisTitle { Text = string.Empty },
                           Labels = new XAxisLabels { Rotation = 90 },
                           TickWidth = 1,
                       })

                       .SetYAxis(new YAxis
                       {
                           Title = new YAxisTitle
                           {
                               Text = "数量",
                               Align = AxisTitleAligns.High,
                           }
                       })

                       .SetTooltip(new Tooltip
                       {
                           
                           HeaderFormat = "<span style=\"font-size:10px\">{point.key}</span><table>",
                           PointFormat = "<tr><td style=\"color:{series.color}\"} >{series.name}:</td><td style=\"padding:0px\"><b>{point.y} </b></td></tr>",
                           FooterFormat = "</table>",
                           Shared = true,
                           UseHTML = true
                       })

                       .SetCredits(new Credits { Enabled = false })

                       .SetSeries(new[]
                       {
                        new Series { Name = "持有仪器设备（件）", Data =new Data(zongjianshu) },

                        new Series { Name = "申请购置仪器设备（件）", Data = new Data(fenpeishu) },

                        new Series { Name = "持有资产金额（元）", Data = new Data(jine) }
                       });
                    return View(chart);
                }
                else
                {
                    return View("Basicbar_error");
                }
            }            
        }
        
        //public class Mulu
        //{
        //    public int id { get; set; }
        //    public string text { get; set; }
        //    public string iconCls { get; set; }
        //    public string children { get; set; }
        //    public string url { get; set; }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f">为loingid</param>
        /// <returns></returns>        
        public ActionResult Index()
        {
            //using (WzglContent db1 = new WzglContent())
            //{
            //    //var mulu = from c in db1.Mulushezhis
            //    //        orderby c.ID
            //    //        select new
            //    //        {
            //    //            id = c.ID,
            //    //            text = c.Name,
            //    //            iconCls = "icon-search",
            //    //            url = "/Xiangmuguanli/XiangmugualiIndex?muluName=" + c.Name + "&loingid=" + f + ""
            //    //        };
            //    //List<Mulu> mulu_List = new List<Mulu>();
            //    //foreach (var item in mulu)
            //    //{
            //    //    Mulu mulu_str = new Mulu
            //    //    {
            //    //        text = item.text,
            //    //        iconCls = item.iconCls,
            //    //        url = item.url
            //    //    };
            //    //    mulu_List.Add(mulu_str);
            //    //}
            //    //var json = new JObject                //这里在末尾  tostring(),返回页面无法显示
            //    //{
            //    //    new JProperty("text","项目管理"),
            //    //    new JProperty("iconCls","icon-listmanage"),
            //    //    new JProperty("state","open"),
            //    //    new JProperty("children",new JArray(from p in mulu_List
            //    //                                    select new JObject(
            //    //                                        new JProperty("text",p.text),
            //    //                                        new JProperty("iconCls","icon-more"),
            //    //                                        new JProperty("url",p.url)
            //    //                                        ))),                  
            //    //};


            //    //这里是没有  linq  的部分
            //    var json = new JObject                //这里在末尾  tostring(),返回页面无法显示
            //    {
            //        new JProperty("text","项目管理"),
            //        new JProperty("iconCls","icon-listmanage"),
            //        new JProperty("state","open"),
            //        new JProperty("children",new JArray(new JObject(
            //                                            new JProperty("text","自购项目"),
            //                                            new JProperty("iconCls","icon-more"),
            //                                            new JProperty("url","/Xiangmuguanli/Xm_zigou?loingid=" + f + "")),
            //                                            new JObject(
            //                                                new JProperty("text","拨付项目"),
            //                                                new JProperty("iconCls","icon-more"),
            //                                                new JProperty("url","/Xiangmuguanli/Xm_bofu?loingid=" + f + "")
            //                                                )   
            //                                            ))
            //    };
            //    var json2 = new JObject                //这里在末尾  tostring(),返回页面无法显示
            //    {
            //        new JProperty("text","基本设置"),
            //        new JProperty("iconCls","icon-hammer"),
            //        new JProperty("children",new JArray(new JObject(
            //                                            new JProperty("text","目录设置"),
            //                                            new JProperty("iconCls","icon-mulu"),
            //                                            new JProperty("url","/Xmmlshezhi/XmmlshezhiIndex")),
            //                                            new JObject(
            //                                                new JProperty("text","部门设置"),
            //                                                new JProperty("iconCls","icon-bumen"),
            //                                                new JProperty("url","/Bmshezhi/BmshezhiIndex")
            //                                                ),
            //                                            new JObject(
            //                                                new JProperty("text","用户管理"),
            //                                                new JProperty("iconCls","icon-yonghu"),
            //                                                new JProperty("url","/Account/UserIndex?f=" + f + "")
            //                                                ),
            //                                            new JObject(
            //                                                new JProperty("text","流程设置"),
            //                                                new JProperty("iconCls","icon-liucheng"),
            //                                                new JProperty("url","/Liuchengshezhi/LiuchengIndex")
            //                                                ),
            //                                            new JObject(
            //                                                new JProperty("text","角色管理"),
            //                                                new JProperty("iconCls","icon-user"),
            //                                                new JProperty("url","/Account/RoleIndex")
            //                                                ),
            //                                            new JObject(
            //                                                new JProperty("text","密码修改"),
            //                                                new JProperty("iconCls","icon-lock"),
            //                                                new JProperty("url","/Account/Rsetpwdindex")
            //                                                )
            //                                            ))
            //    };
            //    var json3 = new JObject                //这里在末尾  tostring(),返回页面无法显示
            //    {
            //        new JProperty("text","关于我们"),
            //        new JProperty("iconCls","icon-about"),
            //        new JProperty("children",new JArray(new JObject(
            //                                            new JProperty("text","版权信息"),
            //                                            new JProperty("iconCls","icon-copyright"),
            //                                            new JProperty("url","/home/About")),
            //                                            new JObject(
            //                                                new JProperty("text","技术支持"),
            //                                                new JProperty("iconCls","icon-help"),
            //                                                new JProperty("url","/home/Contact")
            //                                                ),
            //                                            new JObject(
            //                                                new JProperty("text","用户手册"),
            //                                                new JProperty("iconCls","icon-shouce"),
            //                                                new JProperty("url","/home/help")
            //                                                )
            //                                            ))
            //    };

            //    ViewBag.json = (JsonConvert.SerializeObject(json)).ToString();
            //    ViewBag.json2 = (JsonConvert.SerializeObject(json2)).ToString();
            //    ViewBag.json3 = (JsonConvert.SerializeObject(json3)).ToString();
            //    //ViewBag.g = Newtonsoft.Json.JsonConvert.SerializeObject(genres.ToList());
            //}            
            return View();            
        }

        /// <summary>
        /// sidemenu加载的post方法
        /// </summary>
        /// <param name="f">为loingid</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Index_json(string f)
        {
            string loingrole = Customidentity.User(f).Role;

            if (loingrole == "管理员")
            {
                var json = new JObject                //这里在末尾  tostring(),返回页面无法显示
                {
                    new JProperty("text","项目管理"),
                    new JProperty("iconCls","icon-listmanage"),
                    new JProperty("state","open"),
                    //new JProperty("children",new JArray(new JObject(
                    //                                    new JProperty("text","自购项目"),
                    //                                    new JProperty("iconCls","icon-more"),
                    //                                    new JProperty("url","/Xiangmuguanli/Xm_zigou?loingid=" + f + "")),
                    //                                    new JObject(
                    //                                        new JProperty("text","拨付项目"),
                    //                                        new JProperty("iconCls","icon-more"),
                    //                                        new JProperty("url","/Xiangmuguanli/Xm_bofu?loingid=" + f + "")
                    //                                        )
                    //                                    ))
                };
                var json2 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                {
                    new JProperty("text","资产管理"),
                    new JProperty("iconCls","icon-project"),
                    new JProperty("state","open"),
                    //new JProperty("children",new JArray(new JObject(
                    //                                    new JProperty("text","资产列表"),
                    //                                    new JProperty("iconCls","icon-more"),
                    //                                    new JProperty("url","/Zichanguanli/Index?loingid=" + f + ""))
                    //                                    ))
                };
                var json3 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                {
                    new JProperty("text","基本设置"),
                    new JProperty("iconCls","icon-hammer"),
                    new JProperty("state","open"),
                    new JProperty("children",new JArray(
                                                        //new JObject(
                                                        //new JProperty("text","目录设置"),
                                                        //new JProperty("iconCls","icon-mulu"),
                                                        //new JProperty("url","/Xmmlshezhi/XmmlshezhiIndex")),
                                                        new JObject(
                                                            new JProperty("text","部门设置"),
                                                            new JProperty("iconCls","icon-bumen"),
                                                            new JProperty("url","/Bmshezhi/BmshezhiIndex")
                                                            ),
                                                        new JObject(
                                                            new JProperty("text","用户管理"),
                                                            new JProperty("iconCls","icon-yonghu"),
                                                            new JProperty("url","/Account/UserIndex?f=" + f + "")
                                                            ),
                                                        new JObject(
                                                            new JProperty("text","项目流程"),
                                                            new JProperty("iconCls","icon-liucheng"),
                                                            new JProperty("url","/Liuchengshezhi/LiuchengIndex")
                                                            ),
                                                        new JObject(
                                                            new JProperty("text","发票流程"),
                                                            new JProperty("iconCls","icon-listmanage"),
                                                            new JProperty("url","/Fapiao_liucheng/Index")
                                                            ),
                                                        new JObject(
                                                            new JProperty("text","角色管理"),
                                                            new JProperty("iconCls","icon-user"),
                                                            new JProperty("url","/Account/RoleIndex")
                                                            ),
                                                        new JObject(
                                                            new JProperty("text","密码修改"),
                                                            new JProperty("iconCls","icon-lock"),
                                                            new JProperty("url","/Account/Rsetpwdindex")
                                                            )
                                                        ))
                };
                var json4 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                {
                    new JProperty("text","关于我们"),
                    new JProperty("iconCls","icon-about"),
                    new JProperty("children",new JArray(new JObject(
                                                        new JProperty("text","版权信息"),
                                                        new JProperty("iconCls","icon-copyright"),
                                                        new JProperty("url","/Home/About")),
                                                        new JObject(
                                                            new JProperty("text","技术支持"),
                                                            new JProperty("iconCls","icon-help"),
                                                            new JProperty("url","/Home/Contact")
                                                            ),
                                                        new JObject(
                                                            new JProperty("text","用户手册"),
                                                            new JProperty("iconCls","icon-shouce"),
                                                            new JProperty("url","/Home/Help")
                                                            )
                                                        ))
                };
                var json1_ = (JsonConvert.SerializeObject(json)).ToString();
                var json2_ = (JsonConvert.SerializeObject(json2)).ToString();
                var json3_ = (JsonConvert.SerializeObject(json3)).ToString();
                var json4_ = (JsonConvert.SerializeObject(json4)).ToString();

                return Json(new { a = json1_, b = json2_, c = json3_, d = json4_ });
            }
            else         
            {
                if (loingrole == "部门领导" || loingrole == "领导")
                {
                    var json = new JObject                //这里在末尾  tostring(),返回页面无法显示
                    {
                        new JProperty("text","项目管理"),
                        new JProperty("iconCls","icon-listmanage"),
                        new JProperty("state","open"),
                        new JProperty("children",new JArray(new JObject(
                                                            new JProperty("text","自购项目"),
                                                            new JProperty("iconCls","icon-more"),
                                                            new JProperty("url","/Xiangmuguanli/Xm_zigou?loingid=" + f + ""))//,
                                                            //new JObject(
                                                            //    new JProperty("text","拨付项目"),
                                                            //    new JProperty("iconCls","icon-more"),
                                                            //    new JProperty("url","/Xiangmuguanli/Xm_bofu?loingid=" + f + "")
                                                            //    )
                                                            ))
                    };
                    var json2 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                    {
                        new JProperty("text","资产管理"),
                        new JProperty("iconCls","icon-project"),
                        new JProperty("state","open"),
                        new JProperty("children",new JArray(new JObject(
                                                            new JProperty("text","资产列表"),
                                                            new JProperty("iconCls","icon-more"),
                                                            new JProperty("url","/Zichanguanli/Index?loingid=" + f + ""))
                                                            ))
                    };
                    var json3 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                    {
                        new JProperty("text","基本设置"),
                        new JProperty("iconCls","icon-hammer"),
                        new JProperty("children",new JArray(
                                                            //new JObject(
                                                            //new JProperty("text","目录设置"),
                                                            //new JProperty("iconCls","icon-mulu"),
                                                            //new JProperty("url","/Xmmlshezhi/XmmlshezhiIndex")),
                                                            //new JObject(
                                                            //    new JProperty("text","部门设置"),
                                                            //    new JProperty("iconCls","icon-bumen"),
                                                            //    new JProperty("url","/Bmshezhi/BmshezhiIndex")
                                                            //    ),
                                                            //new JObject(
                                                            //    new JProperty("text","用户管理"),
                                                            //    new JProperty("iconCls","icon-yonghu"),
                                                            //    new JProperty("url","/Account/UserIndex?f=" + f + "")
                                                            //    ),
                                                            //new JObject(
                                                            //    new JProperty("text","流程设置"),
                                                            //    new JProperty("iconCls","icon-liucheng"),
                                                            //    new JProperty("url","/Liuchengshezhi/LiuchengIndex")
                                                            //    ),
                                                            //new JObject(
                                                            //    new JProperty("text","角色管理"),
                                                            //    new JProperty("iconCls","icon-user"),
                                                            //    new JProperty("url","/Account/RoleIndex")
                                                            //    ),
                                                            new JObject(
                                                                new JProperty("text","密码修改"),
                                                                new JProperty("iconCls","icon-lock"),
                                                                new JProperty("url","/Account/Rsetpwdindex")
                                                                )
                                                            ))
                    };
                    var json4 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                    {
                        new JProperty("text","关于我们"),
                        new JProperty("iconCls","icon-about"),
                        new JProperty("children",new JArray(new JObject(
                                                            new JProperty("text","版权信息"),
                                                            new JProperty("iconCls","icon-copyright"),
                                                            new JProperty("url","/Home/About")),
                                                            new JObject(
                                                                new JProperty("text","技术支持"),
                                                                new JProperty("iconCls","icon-help"),
                                                                new JProperty("url","/Home/Contact")
                                                                ),
                                                            new JObject(
                                                                new JProperty("text","用户手册"),
                                                                new JProperty("iconCls","icon-shouce"),
                                                                new JProperty("url","/Home/Help")
                                                                )
                                                            ))
                    };
                    var json1_ = (JsonConvert.SerializeObject(json)).ToString();
                    var json2_ = (JsonConvert.SerializeObject(json2)).ToString();
                    var json3_ = (JsonConvert.SerializeObject(json3)).ToString();
                    var json4_ = (JsonConvert.SerializeObject(json4)).ToString();

                    return Json(new { a = json1_, b = json2_, c = json3_, d = json4_ });
                }
                else
                {
                    if (loingrole == "部门主管" ) {
                        var json = new JObject                //这里在末尾  tostring(),返回页面无法显示
                        {
                            new JProperty("text","项目管理"),
                            new JProperty("iconCls","icon-listmanage"),
                            new JProperty("state","open"),
                            new JProperty("children",new JArray(new JObject(
                                                                new JProperty("text","自购项目"),
                                                                new JProperty("iconCls","icon-more"),
                                                                new JProperty("url","/Xiangmuguanli/Xm_zigou?loingid=" + f + "")),
                                                                new JObject(
                                                                    new JProperty("text","拨付项目"),
                                                                    new JProperty("iconCls","icon-more"),
                                                                    new JProperty("url","/Xiangmuguanli/Xm_bofu?loingid=" + f + "")
                                                                    )
                                                                ))
                        };
                        var json2 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                        {
                            new JProperty("text","资产管理"),
                            new JProperty("iconCls","icon-project"),
                            new JProperty("state","open"),
                            new JProperty("children",new JArray(new JObject(
                                                                new JProperty("text","资产列表"),
                                                                new JProperty("iconCls","icon-more"),
                                                                new JProperty("url","/Zichanguanli/Index?loingid=" + f + ""))
                                                                ))
                        };
                        var json3 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                        {
                            new JProperty("text","基本设置"),
                            new JProperty("iconCls","icon-hammer"),
                            new JProperty("children",new JArray(
                                                                //new JObject(
                                                                //new JProperty("text","目录设置"),
                                                                //new JProperty("iconCls","icon-mulu"),
                                                                //new JProperty("url","/Xmmlshezhi/XmmlshezhiIndex")),
                                                                //new JObject(
                                                                //    new JProperty("text","部门设置"),
                                                                //    new JProperty("iconCls","icon-bumen"),
                                                                //    new JProperty("url","/Bmshezhi/BmshezhiIndex")
                                                                //    ),
                                                                new JObject(
                                                                    new JProperty("text","用户管理"),
                                                                    new JProperty("iconCls","icon-yonghu"),
                                                                    new JProperty("url","/Account/UserIndex?f=" + f + "")
                                                                    ),
                                                                //new JObject(
                                                                //    new JProperty("text","流程设置"),
                                                                //    new JProperty("iconCls","icon-liucheng"),
                                                                //    new JProperty("url","/Liuchengshezhi/LiuchengIndex")
                                                                //    ),
                                                                //new JObject(
                                                                //    new JProperty("text","角色管理"),
                                                                //    new JProperty("iconCls","icon-user"),
                                                                //    new JProperty("url","/Account/RoleIndex")
                                                                //    ),
                                                                new JObject(
                                                                    new JProperty("text","密码修改"),
                                                                    new JProperty("iconCls","icon-lock"),
                                                                    new JProperty("url","/Account/Rsetpwdindex")
                                                                    )
                                                                ))
                        };
                        var json4 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                        {
                            new JProperty("text","关于我们"),
                            new JProperty("iconCls","icon-about"),
                            new JProperty("children",new JArray(new JObject(
                                                                new JProperty("text","版权信息"),
                                                                new JProperty("iconCls","icon-copyright"),
                                                                new JProperty("url","/Home/About")),
                                                                new JObject(
                                                                    new JProperty("text","技术支持"),
                                                                    new JProperty("iconCls","icon-help"),
                                                                    new JProperty("url","/Home/Contact")
                                                                    ),
                                                                new JObject(
                                                                    new JProperty("text","用户手册"),
                                                                    new JProperty("iconCls","icon-shouce"),
                                                                    new JProperty("url","/Home/Help")
                                                                    )
                                                                ))
                        };
                        var json1_ = (JsonConvert.SerializeObject(json)).ToString();
                        var json2_ = (JsonConvert.SerializeObject(json2)).ToString();
                        var json3_ = (JsonConvert.SerializeObject(json3)).ToString();
                        var json4_ = (JsonConvert.SerializeObject(json4)).ToString();

                        return Json(new { a = json1_, b = json2_, c = json3_, d = json4_ });
                    }
                    else {    //如果角色是  员工
                        var json = new JObject                //这里在末尾  tostring(),返回页面无法显示
                        {
                            new JProperty("text","项目管理"),
                            new JProperty("iconCls","icon-listmanage"),
                            new JProperty("state","open"),
                            new JProperty("children",new JArray(new JObject(
                                                                new JProperty("text","自购项目"),
                                                                new JProperty("iconCls","icon-more"),
                                                                new JProperty("url","/Xiangmuguanli/Xm_zigou?loingid=" + f + "")) //,
                                                                //new JObject(
                                                                //    new JProperty("text","拨付项目"),
                                                                //    new JProperty("iconCls","icon-more"),
                                                                //    new JProperty("url","/Xiangmuguanli/Xm_bofu?loingid=" + f + "")
                                                                //    )
                                                                ))
                        };
                        var json2 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                        {
                            new JProperty("text","资产管理"),
                            new JProperty("iconCls","icon-project"),
                            new JProperty("state","open"),
                            new JProperty("children",new JArray(new JObject(
                                                                new JProperty("text","资产列表"),
                                                                new JProperty("iconCls","icon-more"),
                                                                new JProperty("url","/Zichanguanli/Index?loingid=" + f + ""))
                                                                ))
                        };
                        var json3 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                        {
                            new JProperty("text","基本设置"),
                            new JProperty("iconCls","icon-hammer"),
                            new JProperty("children",new JArray(
                                                                //new JObject(
                                                                //new JProperty("text","目录设置"),
                                                                //new JProperty("iconCls","icon-mulu"),
                                                                //new JProperty("url","/Xmmlshezhi/XmmlshezhiIndex")),
                                                                //new JObject(
                                                                //    new JProperty("text","部门设置"),
                                                                //    new JProperty("iconCls","icon-bumen"),
                                                                //    new JProperty("url","/Bmshezhi/BmshezhiIndex")
                                                                //    ),
                                                                //new JObject(
                                                                //    new JProperty("text","用户管理"),
                                                                //    new JProperty("iconCls","icon-yonghu"),
                                                                //    new JProperty("url","/Account/UserIndex?f=" + f + "")
                                                                //    ),
                                                                //new JObject(
                                                                //    new JProperty("text","流程设置"),
                                                                //    new JProperty("iconCls","icon-liucheng"),
                                                                //    new JProperty("url","/Liuchengshezhi/LiuchengIndex")
                                                                //    ),
                                                                //new JObject(
                                                                //    new JProperty("text","角色管理"),
                                                                //    new JProperty("iconCls","icon-user"),
                                                                //    new JProperty("url","/Account/RoleIndex")
                                                                //    ),
                                                                new JObject(
                                                                    new JProperty("text","密码修改"),
                                                                    new JProperty("iconCls","icon-lock"),
                                                                    new JProperty("url","/Account/Rsetpwdindex")
                                                                    )
                                                                ))
                        };
                        var json4 = new JObject                //这里在末尾  tostring(),返回页面无法显示
                        {
                            new JProperty("text","关于我们"),
                            new JProperty("iconCls","icon-about"),
                            new JProperty("children",new JArray(new JObject(
                                                                new JProperty("text","版权信息"),
                                                                new JProperty("iconCls","icon-copyright"),
                                                                new JProperty("url","/Home/About")),
                                                                new JObject(
                                                                    new JProperty("text","技术支持"),
                                                                    new JProperty("iconCls","icon-help"),
                                                                    new JProperty("url","/Home/Contact")
                                                                    ),
                                                                new JObject(
                                                                    new JProperty("text","用户手册"),
                                                                    new JProperty("iconCls","icon-shouce"),
                                                                    new JProperty("url","/Home/Help")
                                                                    )
                                                                ))
                        };
                        var json1_ = (JsonConvert.SerializeObject(json)).ToString();
                        var json2_ = (JsonConvert.SerializeObject(json2)).ToString();
                        var json3_ = (JsonConvert.SerializeObject(json3)).ToString();
                        var json4_ = (JsonConvert.SerializeObject(json4)).ToString();

                        return Json(new { a = json1_, b = json2_, c = json3_, d = json4_ });
                    }


                    
                }                
            }            
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Help()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public FileResult GetexcelFile(string mobanname)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Content/uploads/moban/";
            string fileName = mobanname + "部门主管及员工用户使用手册.docx";
            return File(path + fileName, "text/plain", fileName);
        }

        //public class Xmname
        //{
        //    public string text { get; set; }
        //}

        //[HttpPost]
        //public JsonResult GetXmmuluData(string loingid)
        //{

        //    //根据ID取出该用户的所属目录
        //    //string suoshumulu = common.customidentity.pingshenmulu(loingid);

        //    ////判断所登录的userid是否是评委角色，是则根据所属目录来显示相关目录
        //    //string shifoupingwei = common.customidentity.userrole(loingid);

        //    string temp;
        //    List<Xmname> xmmuluList = new List<Xmname>();


        //    var query = from s in db.Mulushezhis
        //                orderby s.Chuangjian_time descending     //按照目录创建时间降序排序

        //                select new { xmname = s.Name};

        //    foreach (var item in query)
        //    {               
        //        temp = "<a style=\"margin-left:20px;text-decoration:none\" title=\"申报项目时间\" href=\"javascript:void(0)\" onclick=\"showniandulist('/Xiangmuguanli/XiangmugualiIndex?muluName=" + item.xmname  + "&loingid=" + loingid + "')\" target=\"mainFrame\" >&nbsp;&nbsp;" + item.xmname + "</a>";//glyphicon glyphicon-eye-open
        //                                                                                                                                                                                                                                                                                                                                                                        //margin-left:20px;
        //        //temp = "<a style=\"margin-left:20px;text-decoration:none\" title=\"申报项目时间\" href=\"javascript:void(0)\" onclick=\"showniandulist('/xiangmuguanli/xiangmugualiIndex?muluName=" + item.xmname + "&loingid=" + loingid + "')\" target=\"mainFrame\"><Image src='/Scripts/easyui/themes/icons/zoom.png' Title='分管领导不参与审核'/>&nbsp;&nbsp;" + item.xmname + "</span></a>";

        //        Xmname mulu = new Xmname { text = temp };

        //        xmmuluList.Add(mulu);                
        //    }           
        //    return Json(xmmuluList.ToList());
        //}

    }
}