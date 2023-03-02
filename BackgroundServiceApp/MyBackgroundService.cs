using System.Net.Sockets;

namespace BackgroundServiceApp
{
    public class MyBackgroundService : BackgroundService
    {
        private UdpService udpService;

        public MyBackgroundService()
        {
            this.udpService = new UdpService(new UdpClient(22220));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await udpService.Send();
                await udpService.Receive();
                await Task.Delay(500);
            }
        }
    }
}
