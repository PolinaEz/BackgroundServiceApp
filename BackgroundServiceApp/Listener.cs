using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class Listener : BackgroundService
    {
        private readonly UdpClient udpClient;

        public Listener(UdpClient udpClient)
        {
            this.udpClient = udpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await udpClient.ReceiveAsync(stoppingToken);
                var message = Encoding.UTF8.GetString(result.Buffer);
                Console.WriteLine($"Получено {result.Buffer.Length} байт. Данные: {message}");
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
