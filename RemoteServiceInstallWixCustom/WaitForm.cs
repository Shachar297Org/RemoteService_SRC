using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RemoteServiceInstallWixCustom
{
    public partial class WaitForm : Form
    {
        public WaitForm()
        {
            InitializeComponent();
        }

        public void SetDone()
        {
            lblDone.Text = "Done";
        }

        public void ShowCountdownInfo()
        {
            lblCountdown.Visible = true;
        }

        public void Countdown(int i)
        {
            lblCountdown.Text = String.Format("System will be restarted at: {0} seconds", i); 
        }
    }
}
