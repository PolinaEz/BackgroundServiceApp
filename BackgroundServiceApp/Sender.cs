using Aspose.Gis;
using Aspose.Gis.Geometries;
using System;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BackgroundServiceApp
{
    public class Sender : BackgroundService
    {
        private int iterationNumber = 0;
        private readonly LbsService _lbsService;
        private readonly List<Point> points = new();

        public Sender(LbsService lbsService)
        {
            this._lbsService = lbsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

            using var udpClient = new UdpClient("127.0.0.1", 22220);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
                Console.WriteLine("/////////////////////////////\n Invalid data \n /////////////////////////////\n");

                for (int i = 0; i < points.Count; i++)
                {
                    points[i].Time = DateTime.Now.AddSeconds(i);
                    string message = points[i].ToString();
                    Console.WriteLine($"Send: {message}");

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await udpClient.SendAsync(data, stoppingToken);
                    await Task.Delay(1000, stoppingToken);
                }

                Console.WriteLine("/////////////////////////////\n Valid data \n /////////////////////////////\n");

                for (int i = 0; i < points.Count; i++)
                {
                    points[i].Time = DateTime.Now.AddSeconds(i);
                    points[i].Sat = 4;
                    string message = points[i].ToString();
                    Console.WriteLine($"Send: {message}");

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await udpClient.SendAsync(data, stoppingToken);
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private void ParseMultiLineString(string line)
        {
            var lineSpan = line.AsSpan();

            int indexOfParenthesis = line.LastIndexOf('(');
            if (indexOfParenthesis == -1)
                return;

            int indexOfComma = indexOfParenthesis;
            int indexOfSpace; 

            do
            {
                if (
                    TryParseDouble(indexOfComma, indexOfSpace = NextIndex(indexOfComma + 1, line, ' '), lineSpan, out double lon) &&
                    TryParseDouble(indexOfSpace, NextIndex(indexOfSpace, line, ' '), lineSpan, out double lat)
                    )
                {
                    Lbs lbs = _lbsService.FindLbs(new Coordinates() { Lat = lat, Lon = lon });
                    points.Add(new Point
                    {
                        Сoordinates = new Coordinates() { Lat = lat, Lon = lon},
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
