using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class Sender : BackgroundService
    {
        private readonly UdpClient udpClient;
        const int port = 22220;

        public Sender(UdpClient udpClient)
        {
            this.udpClient = udpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string message = DateTimeOffset.Now.ToString();
                byte[] data = Encoding.UTF8.GetBytes(message);
                IPEndPoint remotePoint = new(IPAddress.Parse("127.0.0.1"), port);
                int bytes = await udpClient.SendAsync(data, remotePoint, stoppingToken);
                Console.WriteLine($"Отправлено {bytes} байт");
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
