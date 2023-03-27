using System.Globalization;
using System.Text;

namespace BackgroundServiceApp
{
    public class LbsService
    {
        private Dictionary<Lbs, StationInfo> Station => _lbsDictionary.Value;
        private readonly Lazy<Dictionary<Lbs, StationInfo>> _lbsDictionary = new(() =>
        {
            const string path = "257.csv";
            const char separator = ',';

            using var reader = File.OpenText(path);
            Dictionary<Lbs, StationInfo> lbsDictionary = new();

            while (reader.ReadLine() is { } line)
            {
                var index = line.IndexOf(separator);
                var nextIndex = line.IndexOf(separator, index + 1);

                if (line.AsSpan()[0] != 'G' ||
                    line.AsSpan()[1] != 'S' ||
                    line.AsSpan()[2] != 'M' ||
                    !TryParseInt(ref index, ref nextIndex, line, out int mcc) ||
                    !TryParseInt(ref index, ref nextIndex, line, out int mnc) ||
                    !TryParseInt(ref index, ref nextIndex, line, out int lac) ||
                    !TryParseInt(ref index, ref nextIndex, line, out int cellId) ||
                    !TrySkip(ref index, ref nextIndex, line) ||
                    !TryParseDouble(ref index, ref nextIndex, line, out double lon) ||
                    !TryParseDouble(ref index, ref nextIndex, line, out double lat))
                    continue;
                else
                {
                    var lbs = new Lbs
                    {
                        Mcc = mcc,
                        Mnc = mnc,
                        Lac = lac,
                        CellId = cellId
                    };

                    lbsDictionary.Add( lbs, new StationInfo
                        {
                            Lbs = lbs, Coordinates = new Coordinates()
                            {
                                Lat = lat,
                                Lon = lon
                            }
                        });
                }
            }

            return lbsDictionary;

            void NextIndex(ref int index, string line)
            {
                index = line.IndexOf(separator, index + 1);
            }

            bool TryParseInt(ref int index, ref int nextIndex, string line, out int result)
            {
                var resultBool = int.TryParse(line.AsSpan().Slice(index + 1, nextIndex - index - 1), out result);
                NextIndex(ref index, line);
                NextIndex(ref nextIndex, line);
                return resultBool;
            }

            bool TryParseDouble(ref int index, ref int nextIndex, string line, out double result)
            {
                var resultBool = double.TryParse(line.AsSpan().Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
                NextIndex(ref index, line);
                NextIndex(ref nextIndex, line);
                return resultBool;
            }

            bool TrySkip(ref int index, ref int nextIndex, string line)
            {
                NextIndex(ref index, line);
                NextIndex(ref nextIndex, line);
                return index >= 0 && nextIndex >= 0;
            }
        });

        public bool TryGetStationInfo(Lbs lbs, out StationInfo stationInfo) =>
            Station.TryGetValue(lbs, out stationInfo);

        public Lbs FindLbs(Coordinates coordinates)
        {
            var min = double.MaxValue;
            Lbs result = default;

            foreach (var stationInfo in Station.Values)
            {
                var range = Math.Pow(stationInfo.Coordinates.Lon - coordinates.Lon, 2) + 
                            Math.Pow(stationInfo.Coordinates.Lat - coordinates.Lat, 2);


                if (!(range < min)) continue;

                result = stationInfo.Lbs;
                min = range;
            }

            return result;
        }
    }
}
