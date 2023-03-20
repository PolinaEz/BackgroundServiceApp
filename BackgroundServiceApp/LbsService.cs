using System.Globalization;
using System.Text;

namespace BackgroundServiceApp
{
    public class LbsService
    {
        private readonly Dictionary<Lbs, StationInfo> lbsDictionary = new();

        public LbsService()
        {
            string path = "257.csv";

            using FileStream fileStream = File.OpenRead(path);
            using var reader = new StreamReader(fileStream);

            char separator = ',';
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                int index = line.IndexOf(separator);
                int nextIndex = line.IndexOf(separator, index + 1);

                if (
                    line.AsSpan()[0] == 'G' &&
                    line.AsSpan()[1] == 'S' &&
                    line.AsSpan()[2] == 'M' &&
                    TryParseInt(ref index, ref nextIndex, line, out int mcc) &&
                    TryParseInt(ref index, ref nextIndex, line, out int mnc) &&
                    TryParseInt(ref index, ref nextIndex, line, out int lac) &&
                    TryParseInt(ref index, ref nextIndex, line, out int cellId) &&
                    TrySkip(ref index, ref nextIndex, line) &&
                    TryParseDouble(ref index, ref nextIndex, line, out double lon) &&
                    TryParseDouble(ref index, ref nextIndex, line, out double lat)
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
                            Coordinates = new Coordinates()
                            {
                                Lat = lat,
                                Lon = lon
                            }
                        });
                }
                else
                    continue;
            }


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
                if (index < 0 || nextIndex < 0) return false;
                else return true;
            }
        }

        public bool TryGetStationInfo(Lbs lbs, out StationInfo stationInfo)
        {
            var result = lbsDictionary.TryGetValue(lbs, out stationInfo);
            return result;
        }

        public Lbs FindLbs(Coordinates coordinates)
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
