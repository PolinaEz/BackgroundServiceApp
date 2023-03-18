using System.Globalization;
using System.Text;

namespace BackgroundServiceApp
{
    public class LbsService
    {
        //private readonly List<Lbs> lbsList = new();
        private readonly string path = "257.csv";
        private readonly char separator = ',';
        private readonly Dictionary<Lbs, (double, double)> lbsDictionary = new();

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

                //lbsList.Add(new Lbs
                //{
                //    Mcc = mcc,
                //    Mnc = mnc,
                //    Lac = lac,
                //    CellId = cellId
                //});

                var resultLbs = new Lbs
                {
                    Mcc = mcc,
                    Mnc = mnc,
                    Lac = lac,
                    CellId = cellId
                };

                if (!lbsDictionary.ContainsKey(resultLbs))
                {
                    lbsDictionary.Add(resultLbs, (lon, lat));
                }
            }
        }

        private int NextIndex(int index, string line)
        {
            return line.IndexOf(separator, index + 1);
        }

        public bool TryGetLatLng(Lbs lbs, out double lat, out double lng)
        {
            var result = lbsDictionary.TryGetValue(lbs, out (double, double) coordinates);
            (lat, lng) = coordinates;
            return result;
        }

        public Lbs FindLbs(double lat, double lng)
        {
            //if (lbsDictionary.ContainsValue((lng, lat)))
            //{

            //    return lbsDictionary.FirstOrDefault(x => x.Value == (lng, lat)).Key;
            //}

            double min = double.MaxValue;
            double range;
            Lbs result = default;

            foreach (var lbs in lbsDictionary)
            {
                range = Сomparison(lbs.Value, (lng, lat));

                if (range < min)
                {
                    result = lbs.Key;
                    min = range;
                }
            }

            return result;
        }

        private static double Сomparison((double lng, double lat) point1, (double lng, double lat) point2)
        {
            return Math.Sqrt(Math.Pow((point1.lat - point2.lat), 2) + Math.Pow((point1.lng - point2.lng), 2));       
        }
    }
}
