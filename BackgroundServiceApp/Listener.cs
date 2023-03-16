using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class Listener : BackgroundService
    {
        private readonly UdpClient udpClient;
        private readonly LbsService lbsService;
        private List<Point> points = new();

        public Listener()
        {
            this.udpClient = new UdpClient(22220);
            this.lbsService = new LbsService();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //lbsService.TryGetLatLng(new Lbs
                //{
                //    Mcc = 257,
                //    Mnc = 2,
                //    CellId = 55722,
                //    Lac = 84
                //}, out double lat, out double lng);
                //var lbs = lbsService.FindLbs(lat, lng);

                //var point = new Point().Parse("20.07.2015 11:43:33,29.478378,54.703674,3,0,0,0,0");
                //point.Lbs = lbsService.FindLbs(point.Lat, point.Long);
                //Console.WriteLine(point.ToString());

                var result = await udpClient.ReceiveAsync(stoppingToken);
                var message = Encoding.UTF8.GetString(result.Buffer);

                points.Add(new Point().Parse(message));
                if (points[^1].Sat < 3)
                {
                    points[^1].Lbs = lbsService.FindLbs(points[^1].Lat, points[^1].Long);
                }
                Console.WriteLine(points[^1].ToString());
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
