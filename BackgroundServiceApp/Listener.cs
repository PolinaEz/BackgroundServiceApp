using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class Listener : BackgroundService
    {
        private readonly UdpClient udpClient;
        private readonly LbsService lbsService;

        public Listener()
        {
            this.udpClient = new UdpClient(22220);
            this.lbsService = new LbsService();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                lbsService.TryGetLatLng(new Lbs
                {
                    Mcc = 257,
                    Mnc = 2,
                    CellId = 55722,
                    Lac = 84
                }, out double lat, out double lng);
                lbsService.FindLbs(lat, lng);

                var result = await udpClient.ReceiveAsync(stoppingToken);
                var message = Encoding.UTF8.GetString(result.Buffer);
                Console.WriteLine($"Получено {result.Buffer.Length} байт. Данные: {message}");
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
