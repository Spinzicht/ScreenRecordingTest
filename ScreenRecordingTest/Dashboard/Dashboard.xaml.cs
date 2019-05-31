using Core;
using Core.Networking;
using Chronos.Networking;

using Core.Data;
using System.Windows;
using System.Net;
using Ludio.VM;

namespace Ludio.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        private Log.Log Log;
        private Overlay Overlay;

        public Dashboard()
        {
            InitializeComponent();

            var vm = VM.Navigator.I;
            DataContext = vm;

            Log = new Log.Log();
            Overlay = new Overlay();
        }

        private void HasLoaded(object sender, RoutedEventArgs e)
        {
            string ip = Constants.ISLAN ? "127.0.0.1" : new WebClient().DownloadString("http://icanhazip.com");

            string name = "Dennis";
#if !DEBUG
            ServerConnection.I.Port = 4444;
            name = "Indie " + name;
#endif
            Core.Util.Log.I.Add(name);
            User.I.Load(ip, PeerConnection.GetLocalIPAddress(), name);

            ServerConnection.I.Connect();

            Navigator.I.Init();

            Log.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Log.Close();
            Overlay.Close();
        }
    }
}
