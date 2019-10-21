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
            using (Timer t = new Timer(new TimeSpan(0, 5, 0).TotalMilliseconds))
            {
                t.Elapsed += Elapsed;
                t.Start();
            }
            Elapsed(null, null);
            Console.ReadLine();
        }

        private static void Elapsed(object sender, ElapsedEventArgs e)
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

            if (Status.BlockCount > 0 && height == Status.BlockCount)
            {
                var msg = $"neowish 停止运行 {Math.Round((DateTime.Now - Status.Time).TotalMinutes)} 分钟";
                Console.WriteLine($"{msg}, { DateTime.Now.ToString()}");
                Tools.SendMail(msg, "neowish 停止运行❗❗❗");
                return;
            }
        }        
    }
}
