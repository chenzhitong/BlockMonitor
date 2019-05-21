using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BlockMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始监控");
            Timer t = new Timer(new TimeSpan(0, 5, 0).TotalMilliseconds);
            t.Elapsed += T_Elapsed;
            t.Start();
            T_Elapsed(null, null);
            Console.ReadLine();
        }

        private static void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            Status.HeightList.Clear();
            var config = JObject.Parse(File.ReadAllText("config.json"));
            
            foreach (var item in config["nodes"])
            {
                var h = Tools.GetBlockCount(item.ToString());
                Console.WriteLine($"{item}\t{h}");
                Status.HeightList.Add(h);
            }
            int height = Status.HeightList.Max();

            if (Status.BlockCount > 0)
            {
                if (height == Status.BlockCount)
                {
                    var msg = $"NEO停止出块，超过{Math.Round((DateTime.Now - Status.Time).TotalMinutes)}分钟未出块";
                    Console.WriteLine($"{msg}, { DateTime.Now.ToString()}");
                    Tools.SendMail(msg, "NEO停止出块❗❗❗");
                    Tools.Call();
                    return;
                }

                var timeSpan = Math.Round((DateTime.Now - Status.Time).TotalSeconds / (height - Status.BlockCount), 1);

                if (timeSpan >= 35 && timeSpan < 300)
                {
                    var msg = $"NEO出块变慢，最近5分钟平均出块时间为{timeSpan}秒。<br />PS：异常区间：{Status.BlockCount}~{height}。";
                    Console.WriteLine($"{msg.Replace("<br />", "\n")}, { DateTime.Now.ToString()}");
                    Tools.SendMail(msg, "NEO出块变慢❗");
                    Status.BlockCount = height;
                    Status.Time = DateTime.Now;
                }
                else
                {
                    Console.WriteLine($"出块正常，平均出块时间{timeSpan}秒 {height}, {DateTime.Now.ToString()}");
                    Status.BlockCount = height;
                    Status.Time = DateTime.Now;
                }
            }
            else
            {
                Status.BlockCount = height;
                Status.Time = DateTime.Now;
                Console.WriteLine($"出块正常 {height}, {DateTime.Now.ToString()}");
            }
        }        
    }
}
