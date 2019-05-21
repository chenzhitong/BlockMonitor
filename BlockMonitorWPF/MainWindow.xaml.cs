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
        Timer t = new Timer(new TimeSpan(0, 5, 0).TotalMilliseconds);

        public MainWindow()
        {
            InitializeComponent();
            t.Elapsed += Monitor;
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
            var height = Status.HeightList.Max(p => p.BlockCount);
            if (height == Status.BlockCount)
            {
                ConsensusStoped(height);
            }
            else
            {
                var averageTime = Math.Round((DateTime.Now - Status.Time).TotalSeconds / (height - Status.BlockCount), 1);
                if (averageTime >= 35 && averageTime < 300)
                    ConsensusSlow(averageTime, height);
                else
                    ConsensusNormal(averageTime, height);
            }
        }

        private void ConsensusSlow(double averageTime, int height)
        {
            var msg = $"NEO出块变慢，最近5分钟平均出块时间为{averageTime}秒。<br />PS：异常区间：{Status.BlockCount}~{height}。";
            Dispatcher.BeginInvoke(new Action(() => {
                TextBox1.WriteLine($"{msg}, { DateTime.Now.ToString()}");
            }));
            Tools.SendMail(msg, "NEO出块变慢❗");
            Status.BlockCount = height;
            Status.Time = DateTime.Now;
        }

        private void ConsensusStoped(int block)
        {
            var msg = $"NEO停止出块，超过{Math.Round((DateTime.Now - Status.Time).TotalMinutes)}分钟未出块";
            Dispatcher.BeginInvoke(new Action(() => {
                TextBox1.WriteLine($"{msg}, { DateTime.Now.ToString()}");
            }));
            Tools.SendMail(msg, "NEO停止出块❗❗❗");
            Tools.Call();
        }

        private void ConsensusNormal(double averageTime, int height)
        {
            Status.BlockCount = height;
            Status.Time = DateTime.Now;
            Dispatcher.BeginInvoke(new Action(() => {
                TextBox1.WriteLine($"出块正常，平均出块时间{averageTime}秒 {height}, {DateTime.Now.ToString()}");
            }));
        }

        /// <summary>
        /// 获取种子节点的区块高度
        /// </summary>
        private void GetNodesBlockCount()
        {
            Status.HeightList.ForEach(p => {
                p.Refresh();
                Dispatcher.BeginInvoke(new Action(() => {
                    TextBox1.WriteLine(p.ToString());
                }));
            });
            Dispatcher.BeginInvoke(new Action(() => {
                TextBox1.WriteLine($"{DateTime.Now.ToString()}\t {Status.HeightList.Max(p => p.BlockCount)}");
            }));
        }

        /// <summary>
        /// 加载种子节点
        /// </summary>
        private void LoadSeedNodes()
        {
            var config = JObject.Parse(File.ReadAllText("config.json"));
            config["nodes"].ToList().ForEach(p => Status.HeightList.Add(new NodeBlockCount(p.ToString(), 0)));
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            t.Start();
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    StartMenu.IsEnabled = false;
                }));
                LoadSeedNodes();
                GetNodesBlockCount();
                Status.BlockCount = Status.HeightList.Max(p => p.BlockCount);
                Status.Time = DateTime.Now;
            });
        }
    }
}
