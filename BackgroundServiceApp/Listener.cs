using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Options;

namespace BackgroundServiceApp
{
    public class Listener : BackgroundService
    {
        private readonly LbsService _lbsService;
        private readonly ListenerOptions _listenerOptions;
        private readonly ILogger<Listener> _logger;

        public Listener(LbsService lbsService, IOptions<ListenerOptions> config, ILogger<Listener> logger)
        {
            this._lbsService = lbsService;
            _listenerOptions = config.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using UdpClient udpClient = new(_listenerOptions.Port);
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = await udpClient.ReceiveAsync(stoppingToken);
                    var message = Encoding.UTF8.GetString(result.Buffer);

                    if (!Point.TryParsePoint(message, out var point)) continue;

                    if (!point!.IsValid())
                    {
                        if (!_lbsService.TryGetStationInfo(point.Lbs, out var stationInfo))
                            continue;

                        point.Coordinates = stationInfo.Coordinates;
                    }

                    _logger.LogInformation("Recv: {point}", point);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation("{e}",e);
            }
        }
    }
}
