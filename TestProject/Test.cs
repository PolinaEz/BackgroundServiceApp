using GsmLibrary;
using System.Globalization;
using System.Net.Sockets;
using Aspose.Gis;
using Aspose.Gis.Geometries;
using Point = GsmLibrary.Point;

namespace TestProject
{
    [TestClass]
    public class Test
    {
        private List<Point> Points;

        [ClassInitialize]
        public void Initialize()
        {
            var points = new List<Point>();

            var pointsGpx = Drivers.Gpx.OpenLayer(@"GraphHopper-Track-2023-03-16-3km.gpx");
            foreach (var pointGpx in pointsGpx)
            {
                if (pointGpx.Geometry.GeometryType != GeometryType.MultiLineString)
                    continue;

                var lines = (MultiLineString)pointGpx.Geometry;
                points = points.Concat(ParseMultiLineString(lines.AsText()) ?? new List<Point>()).ToList();
            }

            Points = points;
        }

        [TestMethod]
        public void Test1()
        {
            UdpClient udpClient = new UdpClient(22220);



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
                    Coordinates = new Coordinates() { Lat = lat, Lon = lon },
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