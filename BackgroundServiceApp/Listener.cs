using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class Listener : BackgroundService
    {
        private readonly LbsService lbsService;
        private List<Point> points = new();

        public Listener()
        {
            this.lbsService = new LbsService();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using UdpClient udpClient = new UdpClient(22220);
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await udpClient.ReceiveAsync(stoppingToken);
                var message = Encoding.UTF8.GetString(result.Buffer);
                Console.WriteLine(points[^1].ToString());
                points.Add(new Point().Parse(message));
                //if (points[^1].Sat < 3)
                //{
                //    points[^1].Lbs = lbsService.FindLbs(points[^1].Lat, points[^1].Long);
                //}
                //Console.WriteLine(points[^1].ToString());
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
