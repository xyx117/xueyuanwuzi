using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace wzgl.Controllers
{
    public class PythonController : Controller
    {
        // GET: Python
        public ActionResult Index()
        {
            return View();
        }


        // 这里使用websockt 通信 执行 python  脚本
        readonly ClientWebSocket webSocket = new ClientWebSocket();
        readonly CancellationToken _cancellation = new CancellationToken();
        [HttpPost]
        public async Task<ContentResult> Py_Jingdong(string py_str)  //, int Capacity = 1024 * 1024 * 1
        {
            try
            {
                //建立连接
                //var url = "ws://localhost:10240";  //?str="+str+"

                var url = "ws://127.0.0.1:10240";

                //var url = "210.37.0.94:9876";

                await webSocket.ConnectAsync(new Uri(url), _cancellation);


                if (webSocket.State == WebSocketState.Open)
                {

                    // 发送这里ok
                    string userMsg = py_str;
                    ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(userMsg));
                    await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);


                    //接收 及处理接收的数据
                    var result = new byte[4096 * 20];    //定义一个类似消息缓存区的字节数组，  这个消息缓存区会比实际接收到的数据大很多

                    var rec = await webSocket.ReceiveAsync(new ArraySegment<byte>(result), new CancellationToken());//接受数据

                    var rec_coutn = rec.Count;   //查找接收数据的大小

                    byte[] bRec = new byte[rec_coutn];   //定新义一个和实际接收数据大小的 字节数组
                    Array.Copy(result, bRec, rec_coutn);  // 将 实际有效的数据 从初始的 数组 拷贝 到后面定义的数组，其大小是 实际有效消息的大小

                    //Encoding.Default.GetString(bRec)

                    webSocket.Dispose(); //关闭websocket链接

                    var str1 = Encoding.UTF8.GetString(bRec);  //对接收到的数据进行转码

                    //ViewBag.bt = str1;
                    //var str2 = JsonConvert.SerializeObject(str1);

                    return Content(str1);  //= "{index:8,shoudian:777,jiage:888,mingcheng:999}"
                }
                else
                {
                    //webSocket.Dispose(); //关闭websocket链接
                    
                    return Content("");
                }

            }
            catch (Exception ex)
            {

                //webSocket.Dispose(); //关闭websocket链接
                
                return Content(ex.ToString());
            }
        }







        public ContentResult Py_Jingdong_old(string py_str)
        {
            JObject sta = new JObject();          //新建一个JObject对象你 这里有问题呢吗    这些是不用的，为啥，没用啊 sta是json对象呀
            JArray arr1 = new JArray();                  //新建一个JArray对象   你接着你把这些注释去掉
            Dictionary<string, int> lstTrueField = new Dictionary<string, int>();  //新建键值对对象

            string temppath = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
            Process p = new Process();
            p.StartInfo.FileName = temppath + "python_jd_bs4.exe";   //  obj\\Debug\\
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.CreateNoWindow = false;            
            p.StartInfo.Arguments = "\"" + py_str + "\"";
            p.Start();

            int m = 0;
            string succeed = "false";

            try
            {
                while (p.StandardOutput.Peek() > -1)
                {
                    m = m + 1;

                    string str_tmp = p.StandardOutput.ReadLine(); //好像空格是没规律的 多个空格有影响,怎么办？pandas可以直接输出html吗
                    string[] split = System.Text.RegularExpressions.Regex.Split(str_tmp, @"\s{2,}");  //应该可以用2个以上的空格来划分

                    if (m > 1)
                    {
                        arr1.Add(new JObject(new JProperty("xuhao", split[0]), new JProperty("shangjia", split[1]), new JProperty("jiage", split[2]), new JProperty("shangpin", split[3])));
                    }
                    succeed = "true";
                }
                
                sta.Add(new JProperty("localdata", arr1));    //这个 locdata 是什么东西，它其实是一个键，arr是值   "locdata"  多｛｝

                p.WaitForExit();
                p.Close();            //它这里就退出去了，但它应该在前面就把值给来output才对3.exe里是怎么写的，有没有输出值，你打开例子
            }
            catch (Exception)
            {
                succeed = "false";
            }
            sta.Add(new JProperty("succeed", succeed));
            var sta_json = JsonConvert.SerializeObject(sta);  //对拼接后的json字符串进行序列化，才能进行网络传输

            return Content(sta_json);    //不对，应该先把output处理成jons格式 好了
        }


        


    }
}