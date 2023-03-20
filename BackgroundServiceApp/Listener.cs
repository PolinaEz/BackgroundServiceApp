using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class Listener : BackgroundService
    {
        private readonly LbsService lbsService;
        private readonly List<Point> points = new();

        public Listener()
        {
            this.lbsService = new LbsService();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using UdpClient udpClient = new(22220);
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await udpClient.ReceiveAsync(stoppingToken);
                var message = Encoding.UTF8.GetString(result.Buffer);

                var point = new Point().Parse(message);

                if (point != null && point.Sat < 3)
                {
                    lbsService.TryGetStationInfo(point.Lbs, out StationInfo stationInfo);
                    point.Сoordinates = stationInfo.Coordinates;
                }

                Console.WriteLine($"Receive: {point}");
                Console.WriteLine();

                if (point != null)
                {
                    points.Add(point);
                }               
            }
        }
    }
}
