using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace BlockMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Timer t = new Timer(new TimeSpan(0, 5, 0).TotalMilliseconds);
        readonly Timer t2 = new Timer(new TimeSpan(1, 0, 0).TotalMilliseconds);

        public MainWindow()
        {
            InitializeComponent();
            t.Elapsed += Monitor;
            t.Start();
            t2.Elapsed += ClearScreen;
            t.Start();

            Task.Run(() => { Monitor(this, null); });
        }

        private void Monitor(object sender, ElapsedEventArgs e)
        {
            GetNodesBlockCount();
            AnalyseResults();
        }

        /// <summary>
        /// 判断出块情况
        /// </summary>
        private void AnalyseResults()
        {
            var currentCount = Status.BlockCountList.Max(p => p.BlockCount);
            if (currentCount == Status.BlockCount)
            {
                ConsensusStoped();
            }
            else
            {
                var averageTime = Math.Round((DateTime.Now - Status.Time).TotalSeconds / (currentCount - Status.BlockCount), 1);
                if (averageTime >= 35 && averageTime < 300)
                    ConsensusSlow(averageTime, currentCount);
                else
                    ConsensusNormal(averageTime, currentCount);
            }
        }

        private void ConsensusSlow(double averageTime, int height)
        {
            var msg = $"Neo出块变慢，最近5分钟平均出块时间为{averageTime}秒。PS：异常区间：{Status.BlockCount}~{height}。";
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBox1.WriteLine($"{msg}, {DateTime.Now}");
            }));
            File.AppendAllText(Tools.fileName, msg + "\r\n");
            Tools.SendMail(msg, "Neo出块变慢❗");
            Tools.WeChat(msg);
            Status.BlockCount = height;
            Status.Time = DateTime.Now;
        }

        private void ConsensusStoped()
        {
            var msg = $"Neo停止出块，超过{Math.Round((DateTime.Now - Status.Time).TotalMinutes)}分钟未出块";
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBox1.WriteLine($"{msg}, { DateTime.Now}");
            }));
            File.AppendAllText(Tools.fileName, msg + "\r\n");
            Tools.SendMail(msg, "Neo停止出块❗❗❗");
            Tools.WeChat(msg);
        }

        private void ConsensusNormal(double averageTime, int height)
        {
            Status.BlockCount = height;
            Status.Time = DateTime.Now;
            var msg = $"出块正常，平均出块时间{averageTime}秒 {height}, {DateTime.Now}";
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBox1.WriteLine(msg);
            }));
            File.AppendAllText(Tools.fileName, msg + "\r\n");
        }

        /// <summary>
        /// 获取种子节点的区块高度
        /// </summary>
        private void GetNodesBlockCount()
        {
            var config = JObject.Parse(File.ReadAllText("config.json"));
            Status.BlockCountList.Clear();
            config["nodes"].ToList().ForEach(p => Status.BlockCountList.Add(new NodeBlockCount(p.ToString(), 0)));
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBox0.Clear();
            }));
            Status.BlockCountList.ForEach(node =>
            {
                node.GetBlockCount();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextBox0.WriteLine(node.ToString());
                }));
            });
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBox1.WriteLine($"{DateTime.Now}\t {Status.BlockCountList.Max(p => p.BlockCount)}");
            }));
        }

        private void Call_Click(object sender, RoutedEventArgs e)
        {
            Tools.WeChatTest("【测试】大家好");
        }

        private void ClearScreen(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBox1.Clear();
                TextBox0.Clear();
            }));
        }
    }
}
