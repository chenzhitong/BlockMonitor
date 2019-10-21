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

        public static void SendMail(string to, string subject, string body)
        {
            try
            {
                var config = JObject.Parse(File.ReadAllText("config.json"));
                using MailMessage mail = new MailMessage
                {
                    From = new MailAddress(config["email"]["username"].ToString(), config["email"]["from"].ToString()),
                    Subject = subject,
                    BodyEncoding = Encoding.UTF8,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(to);

                using SmtpClient smtp = new SmtpClient("smtp.office365.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = true,

                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(
                    config["email"]["username"].ToString(),
                    config["email"]["password"].ToString())
                };

                try
                {
                    smtp.Send(mail);
                }
                catch (SmtpException e)
                {
                    Console.WriteLine("SmtpException" + e.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        public static int GetBlockCount(string node)
        {
            string json;
            try
            {
                json = HttpPost($"{node}", "{'jsonrpc': '2.0', 'method': 'getblockcount', 'params': [], 'id':   1}");
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
            using MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "").ToUpper();
        }

        public static void SendMail(string msg, string subject)
        {
            var config = JObject.Parse(File.ReadAllText("config.json"));
            foreach (var item in config["contact"])
            {
                SendMail(item.ToString(), subject, msg);
            }
        }
    }

}
