using System.Globalization;
using System.Net.Sockets;
using System.Text;
using GsmLibrary;
using Microsoft.Extensions.Options;
using Point = GsmLibrary.Point;

namespace SenderPoint
{
    public class Sender : BackgroundService
    {
        private readonly LbsService _lbsService;
        private readonly ILogger<Sender> _logger;
        private readonly SenderOptions _senderOptions;
        private List<Point>? _points = new();

        public Sender(LbsService lbsService, IOptions<SenderOptions> config, ILogger<Sender> logger)
        {
            this._lbsService = lbsService;
            this._logger = logger;
            _senderOptions = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var udpClient = new UdpClient(_senderOptions.Host, _senderOptions.Port);

            var isInvalid = true;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, stoppingToken);
                    Console.WriteLine($"/////////////////////////////\n {(isInvalid ? "Invalid" : "Valid")} data \n /////////////////////////////\n");

                    foreach (var t in _points!)
                    {
                        t.Time = DateTime.Now;
                        t.Sat = isInvalid ? 2 : 5;
                        var message = t.ToString();

                        var data = Encoding.UTF8.GetBytes(message);
                        await udpClient.SendAsync(data, stoppingToken);
                        _logger.LogInformation("Send: {message}", message);
                        await Task.Delay(1000, stoppingToken);
                    }

                    isInvalid = !isInvalid;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }
        }
    }
}
