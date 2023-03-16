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
        

        private readonly UdpClient udpClient;
        private int iterationNumber = 0;
        private List<Point> points = new();

        //private Point[] pointsValid = new[] {
        //new Point().Parse("20.07.2015 11:43:33,27.47788,53.85757,3,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.48073,53.86012,3,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.48306,53.86210,3,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.48562,53.86432,3,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.48850,53.86677,3,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.49098,53.86891,3,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.49317,53.87117,3,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.49569,53.87503,3,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.49789,53.87832,3,0,0,0,0"),
        //};

        //private Point[] pointsNotValid = new[] {
        //new Point().Parse("20.07.2015 11:43:33,27.47788,53.85757,1,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.48073,53.86012,1,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.48306,53.86210,1,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.48562,53.86432,1,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.48850,53.86677,1,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.49098,53.86891,1,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.49317,53.87117,1,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.49569,53.87503,1,0,0,0,0"),
        //new Point().Parse("20.07.2015 11:43:33,27.49789,53.87832,1,0,0,0,0"),
        //};

        /*
         * 53.85757, 27.47788
         * 53.86012, 27.48073
         * 53.86210, 27.48306
         * 53.86432, 27.48562
         * 53.86677, 27.48850
         * 53.86891, 27.49098
         * 53.87117, 27.49317
         * 53.87503, 27.49569
         * 53.87832, 27.49789
        */

        public Sender()
        {
            var pointsGpx = Drivers.Gpx.OpenLayer(@"GraphHopper-Track-2023-03-16-20km.gpx");

            foreach (var pointGpx in pointsGpx)
            {
                if (pointGpx.Geometry.GeometryType == GeometryType.MultiLineString)
                {
                    // Read track
                    var lines = (MultiLineString)pointGpx.Geometry;
                    ParseMultiLineString(lines.AsText());
                }
            }

            this.udpClient = new UdpClient("127.0.0.1", 22220);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (iterationNumber < points.Count)
                {
                    points[iterationNumber].Time = DateTime.Now.AddMinutes(iterationNumber);
                    string message = points[iterationNumber].ToString();
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
            int indexOfSpace = indexOfParenthesis;

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
        }

        private static int NextIndex(int index, string line, char separator)
        {
            return line.IndexOf(separator, index + 1);
        }
    }
}
