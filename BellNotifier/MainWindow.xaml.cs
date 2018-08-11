using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BellNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string m_url;
        private Timer m_timer;
        private string m_channel;
        private SoundPlayer m_player;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_url = "http://bd.youxidudu.com/db/api/get_gd.php?type=5";
            m_timer = new Timer(new TimerCallback(Timer_Elapsed), null, 0, 10000);
            m_channel = null;
            m_player = new SoundPlayer("bell.wav");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            m_timer.Dispose();
        }

        private void Timer_Elapsed(object state)
        {
            ResponseStruct stru;

            try
            {
                stru = MakeRequest();
            }
            catch (WebException err)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    StatusText.Text = err.Message;
                }));

                return;
            }

            Dispatcher.BeginInvoke(new Action(delegate
            {
                StatusText.Text = stru.Message;
            }));

            if (stru.Code != "0")
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(delegate
            {
                ChannelText.Text = stru.Channel ?? "查無資料";
                TimeText.Text = stru.Time;
            }));

            if (stru.Channel != m_channel)
            {
                m_channel = stru.Channel;

                if (m_channel != null)
                {
                    m_player.Play();
                }
            }
        }

        private ResponseStruct MakeRequest()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_url);
            string text;
            dynamic json, data;
            ResponseStruct stru;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    text = reader.ReadToEnd();
                }
            }

            json = JsonConvert.DeserializeObject(text);
            stru = new ResponseStruct
            {
                Code = json["err"],
                Message = json["errmes"],
            };

            data = json["data"];

            if (data != null)
            {
                Regex regex = new Regex("([0-9]+)分([0-9]+)秒");
                Match match;
                string channel, time;
                TimeSpan t1, t2;

                t1 = TimeSpan.MinValue;

                foreach (dynamic item in data)
                {
                    channel = item["xianlu"];
                    time = item["GoldenBellTime"];

                    if (t1 == TimeSpan.MinValue)
                    {
                        stru.Channel = channel;
                        stru.Time = time;
                    }

                    match = regex.Match(time);

                    if (match != null && match.Success)
                    {
                        t2 = new TimeSpan(0, int.Parse(match.Groups[1].Value),
                            int.Parse(match.Groups[2].Value));

                        if (t2 > t1)
                        {
                            stru.Channel = channel;
                            stru.Time = time;
                        }
                    }
                }
            }

            return stru;
        }
    }

    class ResponseStruct
    {
        public string Code;
        public string Message;
        public string Channel;
        public string Time;
    }
}
