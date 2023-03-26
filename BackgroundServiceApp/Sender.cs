using Aspose.Gis;
using Aspose.Gis.Geometries;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace BackgroundServiceApp
{
    public class Sender : BackgroundService
    {
        private readonly LbsService _lbsService;
        private readonly List<Point> _points = new();
        private readonly SenderOptions _senderOptions;

        public Sender(LbsService lbsService, IOptions<SenderOptions> config)
        {
            this._lbsService = lbsService;
            _senderOptions = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var pointsGpx = Drivers.Gpx.OpenLayer(@"GraphHopper-Track-2023-03-16-3km.gpx");
                foreach (var pointGpx in pointsGpx)
                {
                    if (pointGpx.Geometry.GeometryType == GeometryType.MultiLineString)
                    {
                        var lines = (MultiLineString)pointGpx.Geometry;
                        ParseMultiLineString(lines.AsText());
                    }
                }

                using var udpClient = new UdpClient(_senderOptions.Host, _senderOptions.Port);

                var isInvalid = true;

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                    Console.WriteLine($"/////////////////////////////\n {(isInvalid ? "Invalid" : "Valid")} data \n /////////////////////////////\n");

                    foreach (var t in _points)
                    {
                        t.Time = DateTime.Now;
                        t.Sat = isInvalid ? 0 : 5;
                        var message = t.ToString();

                        var data = Encoding.UTF8.GetBytes(message);
                        await udpClient.SendAsync(data, stoppingToken);
                        Console.WriteLine($"Send: {message}");
                        await Task.Delay(1000, stoppingToken);
                    }

                    isInvalid = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void ParseMultiLineString(string line)
        {
            var lineSpan = line.AsSpan();

            var indexOfParenthesis = line.LastIndexOf('(');
            if (indexOfParenthesis == -1)
                return;

            var indexOfComma = indexOfParenthesis;

            do
            {
                int indexOfSpace;
                if (
                    TryParseDouble(indexOfComma, indexOfSpace = NextIndex(indexOfComma + 1, line, ' '), lineSpan, out var lon) &&
                    TryParseDouble(indexOfSpace, NextIndex(indexOfSpace, line, ' '), lineSpan, out var lat)
                    )
                {
                    var lbs = _lbsService.FindLbs(new Coordinates() { Lat = lat, Lon = lon });
                    _points.Add(new Point
                    {
                        Coordinates = new Coordinates() { Lat = lat, Lon = lon},
                        Lbs = lbs
                    });
                }
            } while ((indexOfComma = NextIndex(indexOfComma, line, ',')) != -1);

            static int NextIndex(int index, string line, char separator)
            {
                return line.IndexOf(separator, index + 1);
            }
            static bool TryParseDouble(int index, int nextIndex, ReadOnlySpan<char> lineSpan, out double result)
            {
                return double.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            }
        }
    }
}
