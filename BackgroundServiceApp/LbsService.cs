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
                var index = line.IndexOf(separator);
                int nextIndex;

                if (
                    lineSpan[0] == 'G' &&
                    lineSpan[1] == 'S' &&
                    lineSpan[2] == 'M' &&
                    TryParseInt(index, nextIndex = NextIndex(index, line), lineSpan, out int mcc) &&
                    TryParseInt(index = NextIndex(nextIndex, line), nextIndex = NextIndex(index, line), lineSpan, out int mnc) &&
                    TryParseInt(index = NextIndex(nextIndex,line), nextIndex = NextIndex(index, line), lineSpan, out int lac) &&
                    TryParseInt(index = NextIndex(nextIndex, line), nextIndex = NextIndex(index, line), lineSpan, out int cellId) &&
                    TrySkip(nextIndex, out index, line) &&
                    TryParseDouble(index = NextIndex(index, line), nextIndex = NextIndex(index, line), lineSpan, out double lon) &&
                    TryParseDouble(index = NextIndex(index, line), nextIndex = NextIndex(index, line), lineSpan, out double lat)
                    )
                {
                    var lbs = new Lbs
                    {
                        Mcc = mcc,
                        Mnc = mnc,
                        Lac = lac,
                        CellId = cellId
                    };

                    lbsDictionary.Add(
                        lbs,
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
                else
                    continue;

                //if (!(lineSpan[0] == 'G' && lineSpan[1] == 'S' && lineSpan[2] == 'M'))
                //    continue;

                //var index = line.IndexOf(separator);
                //if (index == -1)
                //    continue;

                //var nextIndex = NextIndex(index, line);
                //if (nextIndex == -1)
                //    continue;

                //ushort.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out ushort mcc);

                //index = nextIndex;
                //nextIndex = NextIndex(index, line);
                //if (nextIndex == -1)
                //    continue;

                //byte.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out byte mnc);


                //index = nextIndex;
                //nextIndex = NextIndex(index, line);
                //if (nextIndex == -1)
                //    continue;

                //ushort.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out ushort lac);

                //index = nextIndex;
                //nextIndex = NextIndex(index, line);
                //if (nextIndex == -1)
                //    continue;

                //int.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out int cellId);

                //index = nextIndex;
                //nextIndex = NextIndex(index, line);
                //if (nextIndex == -1)
                //    return;

                //index = nextIndex;
                //nextIndex = NextIndex(index, line);
                //if (nextIndex == -1)
                //    return;

                //double.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double lon);

                //index = nextIndex;
                //nextIndex = NextIndex(index, line);
                //if (nextIndex == -1)
                //    return;

                //double.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double lat);

                //var lbs = new Lbs
                //{
                //    Mcc = mcc,
                //    Mnc = mnc,
                //    Lac = lac,
                //    CellId = cellId
                //};

                //lbsDictionary.Add(
                //    lbs,
                //    new StationInfo
                //    {
                //        Lbs = lbs,
                //        Coordinates = new Сoordinates()
                //        {
                //            Lat = lat,
                //            Lon = lon
                //        }
                //    });
            }


            int NextIndex(int index, string line)
            {
                return line.IndexOf(separator, index + 1);
            }
            bool TryParseInt(int index, int nextIndex, ReadOnlySpan<char> lineSpan, out int result)
            {
                return int.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out result);
            }
            bool TryParseDouble(int index, int nextIndex, ReadOnlySpan<char> lineSpan, out double result)
            {
                 return double.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            }
            bool TrySkip(int index, out int resultIndex, string line)
            {
                resultIndex = line.IndexOf(separator, index + 1);
                if (resultIndex < 0) return false;
                else return true;
            }
        }

        public bool TryGetStationInfo(Lbs lbs, out StationInfo stationInfo) => lbsDictionary.TryGetValue(lbs, out stationInfo);

        public Lbs FindLbs(Сoordinates coordinates)
        {
            double min = double.MaxValue;
            double range;
            Lbs result = default;

            foreach (var stationInfo in lbsDictionary)
            {
                range = Math.Pow((stationInfo.Value.Coordinates.Lon - coordinates.Lon), 2) + 
                        Math.Pow((stationInfo.Value.Coordinates.Lat - coordinates.Lat), 2);

                if (range < min)
                {
                    result = stationInfo.Key;
                    min = range;
                }
            }

            return result;
        }
    }
}
