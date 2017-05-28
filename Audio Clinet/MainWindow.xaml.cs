using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Audio_Clinet
{
    public partial class MainWindow : Window
    {
        private AudioClient client;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ip = textBoxIp.Text;
                int port = Int32.Parse(textBoxPort.Text);

                client = new AudioClient(ip, port);
                client.Run();
                

                this.labelIsConnected.Content = "연결, 라디오 수신중..";

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception + "");
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // TODO 고쳐야함.
            //client.Stop();
        }
    }


}
