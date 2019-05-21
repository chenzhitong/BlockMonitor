using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace BlockMonitor
{
    public static class Tools
    {
        public static string HttpPost(string Url, string postData, List<HttpHeader> HttpHeaders = null, int timeOut = 5000)
        {
            WebRequest request = WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;
            request.Timeout = timeOut;
            if (HttpHeaders != null && HttpHeaders.Count > 0)
            {
                foreach (var item in HttpHeaders)
                {
                    switch (item.Name)
                    {
                        case "Accept": break;
                        case "Content-Type": request.ContentType = item.Value; break;
                        default: request.Headers.Add(item.Name, item.Value); break;
                    }
                }
            }
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }

        public static void SendMail(string to, string from, string subject, string body, int times = 1)
        {
            try
            {
                //邮件发送类 
                using (MailMessage mail = new MailMessage())
                {
                    //是谁发送的邮件 
                    mail.From = new MailAddress("chris@neo.org", from);
                    //发送给谁 
                    mail.To.Add(to);
                    //标题 
                    mail.Subject = subject;
                    //内容编码 
                    mail.BodyEncoding = Encoding.UTF8;
                    //邮件内容 
                    mail.Body = body;
                    //是否HTML形式发送 
                    mail.IsBodyHtml = true;

                    //邮件服务器和端口 
                    using (SmtpClient smtp = new SmtpClient("smtp.office365.com", 587))
                    {
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = true;

                        //指定发送方式 
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        //指定登录名和密码 
                        var config = JObject.Parse(File.ReadAllText("config.json"));
                        smtp.Credentials = new NetworkCredential(
                            config["email"]["username"].ToString(),
                            config["email"]["password"].ToString());

                        try
                        {
                            smtp.Send(mail);
                        }
                        catch (SmtpException e)
                        {
                            Console.WriteLine("Error Tools 073: " + e.Message);
                            if (times <= 3)
                                SendMail(to, from, subject, body, ++times);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Tools 082: " + e.Message);
            }
        }

        public static void Call()
        {
            var config = JObject.Parse(File.ReadAllText("config.json"));
            var callList = config["call"];
            foreach (string item in callList)
            {
                Tools.CallAdmin(item);
            }
        }

        private static void CallAdmin(string call)
        {
            var config = JObject.Parse(File.ReadAllText("config.json"));
            var accountSid = config["yuntongxun"]["ACCOUNT SID"].ToString();
            var authToken = config["yuntongxun"]["AUTH TOKEN"].ToString();
            var appId = config["yuntongxun"]["AppID"].ToString();
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var SigParameter = $"{accountSid}{authToken}{timestamp}".MD5Encrypt();
            var url = $"https://app.cloopen.com:8883/2013-12-26/Accounts/{accountSid}/Calls/LandingCalls?sig={SigParameter}";
            var authorization = Convert.ToBase64String(Encoding.Default.GetBytes($"{accountSid}:{timestamp}"));
            var headers = new List<HttpHeader>
            {
                new HttpHeader("Accept", "application/json"),
                new HttpHeader("Content-Type", "application/json;charset=utf-8"),
                new HttpHeader("Authorization", authorization)
            };

            var body = $"{{'mediaName':'warning.wav','to':'{call}','appId':'{appId}'}}";
            var response = HttpPost(url, body, headers);
            var xml = new XmlDocument();
            xml.LoadXml(response);
            var statusCode = xml.SelectSingleNode("Response").SelectSingleNode("statusCode").InnerText;
            if (statusCode == "000000")
            {
                Console.WriteLine($"{call}呼叫成功");
            }
        }

        public static int GetBlockCount(string node)
        {
            string json;
            try
            {
                json = Tools.HttpPost($"{node}", "{'jsonrpc': '2.0', 'method': 'getblockcount', 'params': [], 'id':   1}");
                return (int)JObject.Parse(json)["result"] - 1;
            }
            catch (Exception)
            {
                return 0;
            }

        }
        public static string MD5Encrypt(this string strText)
        {
            byte[] result = Encoding.Default.GetBytes(strText.Trim());
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "").ToUpper();
        }

        public static void SendMail(string msg, string subject)
        {
            var config = JObject.Parse(File.ReadAllText("config.json"));
            foreach (var item in config["contact"])
            {
                Tools.SendMail(item.ToString(), "BlockMonitor", subject, msg);
            }
        }
    }

}
