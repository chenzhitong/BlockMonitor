using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;

namespace BlockMonitor
{
    public static class Tools
    {
        public static string LogFileName = $"Log/{DateTime.Now:yyyyMMdd}.txt";

        public static string HttpPost(string Url, string postData, List<HttpHeader> HttpHeaders = null, int timeOut = 1000)
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
            var config = JObject.Parse(File.ReadAllText("config.json"));
            try
            {
                //邮件发送类 
                using (MailMessage mail = new MailMessage())
                {
                    //是谁发送的邮件 
                    mail.From = new MailAddress(config["email"]["username"].ToString(), from);
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
                        smtp.Credentials = new NetworkCredential(
                            config["email"]["username"].ToString(),
                            config["email"]["password"].ToString());

                        try
                        {
                            smtp.Send(mail);
                        }
                        catch (SmtpException e)
                        {
                            Log("Error Tools 090: " + e.Message);
                            if (times <= 3)
                                SendMail(to, from, subject, body, ++times);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log("Error Tools 099: " + e.Message);
            }
        }

        public static int GetBlockCount(string node)
        {
            string json;
            try
            {
                json = HttpPost($"{node}", "{'jsonrpc': '2.0', 'method': 'getblockcount', 'params': [], 'id':   1}");
                return (int)JObject.Parse(json)["result"];
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
                SendMail(item.ToString(), "BlockMonitor", subject, msg);
            }
        }

        public static void WeChat(string msg)
        {
            try
            {
                HttpPost("https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=ce226718-4e85-482f-99ee-18ed7876ed8a", $"{{\"msgtype\": \"text\",\"text\": {{\"content\": \"{msg}\",\"mentioned_list\":[\"@all\"]}}}}");
            }
            catch (Exception e)
            {
                Log("Error Tools 142: " + e.Message);
            }
        }

        public static void WeChatTest(string msg)
        {
            try
            {
                HttpPost("https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=de61b23c-1c05-4594-be29-20160445babc", $"{{\"msgtype\": \"text\",\"text\": {{\"content\": \"{msg}\",\"mentioned_list\":[\"@all\"]}}}}");
            }
            catch (Exception e)
            {
                Log("Error Tools 154: " + e.Message);
            }
        }

        public static void WriteLine(this TextBox textbox1, string text)
        {
            textbox1.AppendText(text.EndsWith("\n") ? text : text + "\n");
            textbox1.ScrollToEnd();
        }

        public static void Log(string msg)
        {
            File.AppendAllText(LogFileName, DateTime.Now + "\t" + msg + "\r\n");
        }
    }

}
