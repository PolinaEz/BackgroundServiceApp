using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class Sender : BackgroundService
    {
        private readonly UdpClient udpClient;

        public Sender()
        {
            this.udpClient = new UdpClient("127.0.0.1", 22220);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string message = DateTimeOffset.Now.ToString();
                byte[] data = Encoding.UTF8.GetBytes(message);
                await udpClient.SendAsync(data, stoppingToken);
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
