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
        private LbsService _lbsService;
        private List<Point> points = new();

        public Sender(LbsService lbsService)
        {
            this._lbsService = lbsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pointsGpx = Drivers.Gpx.OpenLayer(@"GraphHopper-Track-2023-03-16-20km.gpx");

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
                if (iterationNumber < points.Count)
                {
                    points[iterationNumber].Time = DateTime.Now.AddSeconds(iterationNumber);
                    string message = points[iterationNumber].ToString();
                    Console.WriteLine(message);

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await udpClient.SendAsync(data, stoppingToken);
                    iterationNumber++;
                }

                await Task.Delay(1000, stoppingToken);
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
                indexOfSpace = NextIndex(indexOfComma + 1, line, ' ');
                if (indexOfSpace == -1)
                    continue;

                double.TryParse(lineSpan.Slice(indexOfComma + 1, indexOfSpace - indexOfComma - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double lon);

                int nextIndexOfSpace = NextIndex(indexOfSpace, line, ' ');
                if (nextIndexOfSpace == -1)
                    continue;

                double.TryParse(lineSpan.Slice(indexOfSpace + 1, nextIndexOfSpace - indexOfSpace - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double lat);

                points.Add(new Point
                {
                    Lat = lat,
                    Long = lon
                });
            } while ((indexOfComma = NextIndex(indexOfComma, line, ',')) != -1);

            static int NextIndex(int index, string line, char separator)
            {
                return line.IndexOf(separator, index + 1);
            }
        }
    }
}
