using System.Globalization;
using System.Text;

namespace BackgroundServiceApp
{
    public class LbsService
    {
        private readonly string path = "257.csv";
        private readonly char separator = ',';
        private readonly Dictionary<Lbs, StationInfo> lbsDictionary = new();

        public LbsService()
        {
            using FileStream fileStream = File.OpenRead(path);
            using var reader = new StreamReader(fileStream);

            string? line;
            var resultLine = new StringBuilder();

            while ((line = reader.ReadLine()) != null)
            {
                var lineSpan = line.AsSpan();

                if (!(lineSpan[0] == 'G' && lineSpan[1] == 'S' && lineSpan[2] == 'M'))
                    continue;

                var index = line.IndexOf(separator);
                if (index == -1)
                    continue;

                var nextIndex = NextIndex(index, line);
                if (nextIndex == -1)
                    continue;

                ushort.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out ushort mcc);

                index = nextIndex;
                nextIndex = NextIndex(index, line);
                if (nextIndex == -1)
                    continue;

                byte.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out byte mnc);


                index = nextIndex;
                nextIndex = NextIndex(index, line);
                if (nextIndex == -1)
                    continue;

                ushort.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out ushort lac);

                index = nextIndex;
                nextIndex = NextIndex(index, line);
                if (nextIndex == -1)
                    continue;

                int.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out int cellId);

                index = nextIndex;
                nextIndex = NextIndex(index, line);
                if (nextIndex == -1)
                    return;

                index = nextIndex;
                nextIndex = NextIndex(index, line);
                if (nextIndex == -1)
                    return;

                double.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double lon);

                index = nextIndex;
                nextIndex = NextIndex(index, line);
                if (nextIndex == -1)
                    return;

                double.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double lat);

                var lbs = new Lbs
                {
                    Mcc = mcc,
                    Mnc = mnc,
                    Lac = lac,
                    CellId = cellId
                };

                lbsDictionary.Add(lbs,
                    new StationInfo
                    {
                        Lbs = lbs,
                        Coordinates = new Сoordinates()
                        {
                            Lat = lat,
                            Lon = lon
                        }
                    });
            }


            int NextIndex(int index, string line)
            {
                return line.IndexOf(separator, index + 1);
            }
            int TryParseInt(int index, int nextIndex, out int result)
            {
                int.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out int result);
            }
            int TryParseDouble(int index, int nextIndex, Span lineSpan, out double result)
            {
                double.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            }
        }

        public bool TryGetStationInfo(Lbs lbs, out StationInfo stationInfo) => lbsDictionary.TryGetValue(lbs, out stationInfo);

        public Lbs FindLbs(Coordinates coordinates)
        {
            double min = double.MaxValue;
            double range;
            Lbs result = default;

            foreach (var lbs in lbsDictionary)
            {
                range = Math.Pow((), 2) + Math.Pow((), 2);

                if (range < min)
                {
                    result = lbs;
                    min = range;
                }
            }

            return result;
        }
    }
}
