//using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Support_request_app
{
    /// <summary>
    /// Interaction logic for requestControl.xaml
    /// </summary>
    public partial class requestControl : UserControl
    {
       // private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        public static readonly DependencyProperty RequestModelProperty =
        DependencyProperty.Register("RequestSupport", typeof(RequestModel), typeof(requestControl), null);

        public RequestModel RequestSupport
        {
            set { SetValue(RequestModelProperty, value); }
            get { return (RequestModel)GetValue(RequestModelProperty); }
        }

        public requestControl()
        {
            InitializeComponent();
            RequestSupport = new RequestModel();
        }

        private void Invite_click(object sender, RoutedEventArgs e)
        {
            try
            {
                RequestSupport.RequestSupport();
            }
            catch (Exception ex)
            {

            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RequestSupport.StopSupport();
            }
            catch (Exception ex)
            {

            }
        }

        private void ResetSessionTime_Click(object sender, RoutedEventArgs e)
        {
            RequestSupport.RenewSessionLimit();
        }
    }
}
