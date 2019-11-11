using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Lumenis.RemoteServiceApi;
using Lumenis.RemoteServiceApi.LumenisRemoteService;
using System.ServiceProcess;
using System.Linq;

namespace AppSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RemoteAPI _remoteApi = null;
        private bool _isRunning = true;
        private bool _isEnabled = true;

        public MainWindow()
        {
            InitializeComponent();
            _remoteApi = new RemoteAPI();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                //ServiceController controller = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "Lumenis Remote Service");

                _remoteApi.Open();

                UpdateStatus();
            }
            catch (Exception ex)
            {
                Debug.Print("Window_Loaded - exception: {0}", ex.Message);
            }
        }

        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register("IsRunning", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public string IsRunning
        {
            get { return (string)this.GetValue(IsRunningProperty); }
            set { this.SetValue(IsRunningProperty, value); }
        }

        public static readonly DependencyProperty IsRemoteEnabledProperty =
            DependencyProperty.Register("IsRemoteEnabled", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public string IsRemoteEnabled
        {
            get { return (string)this.GetValue(IsRemoteEnabledProperty); }
            set { this.SetValue(IsRemoteEnabledProperty, value); }
        }

        public static readonly DependencyProperty HasTokenProperty =
            DependencyProperty.Register("HasToken", typeof(string), typeof(MainWindow), new UIPropertyMetadata(string.Empty));

        public string HasToken
        {
            get { return (string)this.GetValue(HasTokenProperty); }
            set { this.SetValue(HasTokenProperty, value); }
        }

        public void UpdateStatus()
        {
            try
            {
                RemoteStatus status = _remoteApi.GetStatus();
                _isEnabled = status.IsEnabled;
                _isRunning = status.IsConnected;

                IsRunning = _isRunning ? "Running" : "Stopped";
                IsRemoteEnabled = _isEnabled ? "Enabled" : "Disabled";
            }
            catch (Exception ex)
            {
                Debug.Print("UpdateStatus - exception: {0}", ex.ToString());
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _remoteApi.StartRemoteConnection();
            }
            catch (Exception ex)
            {
                Debug.Print("btnStart_Click - exception: {0}", ex.Message);
            }
            Thread.Sleep(1000);
            UpdateStatus();

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _remoteApi.StopRemoteConnection();
            }
            catch (Exception ex)
            {
                Debug.Print("btnStop_Click - exception: {0}", ex.Message);
            }
            Thread.Sleep(1000);
            UpdateStatus();

        }

        private void btnEnable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _remoteApi.Enable(true);
            }
            catch (Exception ex)
            {
                Debug.Print("btnEnable_Click - exception: {0}", ex.Message);
            }
            Thread.Sleep(1000);
            UpdateStatus();

        }

        private void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _remoteApi.Enable(false);
            }
            catch (Exception ex)
            {
                Debug.Print("btnDisable_Click - exception: {0}", ex.Message);
            }
            Thread.Sleep(1000);
            UpdateStatus();
        }

        private void btnHasToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool hasToken = _remoteApi.HasFeature(10001);
                if (hasToken)
                {
                    HasToken = "Token Exists";
                }
                else
                {
                    HasToken = "NO Token";
                }
            }
            catch (Exception ex)
            {
                Debug.Print("btnHasToken_Click - exception: {0}", ex.Message);
            }
        }

        private void btnRemoveToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _remoteApi.RemoveFeature(10001);
            }
            catch (Exception ex)
            {
                Debug.Print("btnRemoveToken_Click - exception: {0}", ex.Message);
            }
        }
    }
}
