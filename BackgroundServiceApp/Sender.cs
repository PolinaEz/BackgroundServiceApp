﻿using Aspose.Gis;
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
        private readonly ILogger<Sender> _logger;
        private readonly SenderOptions _senderOptions;
        private List<Point>? _points = new();

        public Sender(LbsService lbsService, IOptions<SenderOptions> config, ILogger<Sender> _logger)
        {
            this._lbsService = lbsService;
            this._logger = _logger;
            _senderOptions = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var pointsGpx = Drivers.Gpx.OpenLayer(@"GraphHopper-Track-2023-03-16-3km.gpx");
                foreach (var pointGpx in pointsGpx)
                {
                    if (pointGpx.Geometry.GeometryType != GeometryType.MultiLineString) continue;
                    var lines = (MultiLineString)pointGpx.Geometry;
                    _points = ParseMultiLineString(lines.AsText());
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
                        _logger.LogInformation("Send: {message}", message);
                        await Task.Delay(1000, stoppingToken);
                    }

                    isInvalid = !isInvalid;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private List<Point>? ParseMultiLineString(string line)
        {
            var points = new List<Point>();

            var lineSpan = line.AsSpan();
            var indexOfParenthesis = line.LastIndexOf('(');
            if (indexOfParenthesis == -1)
                return null;

            var indexOfComma = indexOfParenthesis;

            do
            {
                int indexOfSpace;
                if (!TryParseDouble(indexOfComma, indexOfSpace = NextIndex(indexOfComma + 1, line, ' '), lineSpan,
                        out var lon) ||
                    !TryParseDouble(indexOfSpace, NextIndex(indexOfSpace, line, ' '), lineSpan, out var lat)) continue;

                var lbs = _lbsService.FindLbs(new Coordinates() { Lat = lat, Lon = lon });
                points.Add(new Point
                {
                    Coordinates = new Coordinates() { Lat = lat, Lon = lon},
                    Lbs = lbs
                });

            } while ((indexOfComma = NextIndex(indexOfComma, line, ',')) != -1);

            return points;

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
