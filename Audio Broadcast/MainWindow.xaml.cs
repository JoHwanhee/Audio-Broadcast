using System;
using System.Windows;
using Audio_Broadcast.Common;

namespace Audio_Broadcast
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server server;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Open(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("서버 시작!");
            server = new Server(Int32.Parse(TextBoxPort.Text));
            server.Run();
            
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
