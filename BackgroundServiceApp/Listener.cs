using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class Listener : BackgroundService
    {
        private readonly LbsService _lbsService;

        public Listener()
        {
            this._lbsService = new LbsService();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using UdpClient udpClient = new(22220);
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await udpClient.ReceiveAsync(stoppingToken);
                var message = Encoding.UTF8.GetString(result.Buffer);

                if (!Point.TryParsePoint(message, out var point)) continue;

                if (point!.Sat >= 3)
                {
                    _lbsService.TryGetStationInfo(point.Lbs, out var stationInfo);
                    point.Coordinates = stationInfo.Coordinates;
                }

                Console.WriteLine($"Receive: {point}");
                Console.WriteLine();
            }
        }
    }
}
