using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class UdpService : IDisposable
    {
        private UdpClient udpClient;
        const int port = 22220;

        public UdpService(UdpClient udpClient)
        {
            this.udpClient = udpClient;
        }

        public async Task Send()
        {
            string message = DateTimeOffset.Now.ToString();
            byte[] data = Encoding.UTF8.GetBytes(message);
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            int bytes = await udpClient.SendAsync(data, remotePoint);
            Console.WriteLine($"Отправлено {bytes} байт");
        }

        public async Task Receive()
        {
            var result = await udpClient.ReceiveAsync();
            var message = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Получено {result.Buffer.Length} байт. Данные: {message}");
        }

        public void Dispose()
        {
            udpClient.Dispose();
        }
    }
}
