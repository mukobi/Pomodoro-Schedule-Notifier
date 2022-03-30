using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PomodoroScheduleNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon TrayIcon = new NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            TrayIcon.Icon = new Icon(@"Resources/Icon.ico");
            TrayIcon.Visible = true;
            TrayIcon.Text = "Tray Application";
            TrayIcon.Click += TrayIcon_Click;
        }

        private void TrayIcon_Click(object? sender, EventArgs e)
        {
            Debug.WriteLine("Tray Icon Clicked");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Debug.WriteLine("Deactivated");
        }
    }
}
