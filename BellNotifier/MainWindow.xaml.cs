using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_url = "http://bd.youxidudu.com/db/api/get_gd.php?type=5";
            m_timer = new Timer(new TimerCallback(Timer_Elapsed), null, 0, 10000);
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
        }

        private ResponseStruct MakeRequest()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_url);
            string text;
            dynamic json;
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

            return stru;
        }
    }

    class ResponseStruct
    {
        public string Code;
        public string Message;
    }
}
