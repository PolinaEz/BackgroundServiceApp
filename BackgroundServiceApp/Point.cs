using System.Globalization;
using System.Linq;
using System.Text;

namespace BackgroundServiceApp
{
    public class Point
    {
        public DateTime Time { get; set; }

        public double Long { get; set; }

        public double Lat { get; set; }

        public int Sat { get; set; }

        public Lbs Lbs { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(Time.ToString()).Append(',')
                .Append(Long.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Lat.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Sat).Append(',')
                .Append(Lbs.Mcc).Append(',')
                .Append(Lbs.Mnc).Append(',')
                .Append(Lbs.Lac).Append(',')
                .Append(Lbs.CellId);

            return stringBuilder.ToString();
        }

        public Point Parse(string inputData)
        {
            char separator = ',';

            var lineSpan = inputData.AsSpan();

            var index = inputData.IndexOf(separator);
            var nextIndex = NextIndex(index, inputData);

            DateTime.TryParse(lineSpan[..(index - 1)], out DateTime dateTime);

            double.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double lon);

            index = nextIndex;
            nextIndex = NextIndex(index, inputData);

            double.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double lat);

            index = nextIndex;
            nextIndex = NextIndex(index, inputData);

            int.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out int sat);

            index = nextIndex;
            nextIndex = NextIndex(index, inputData);

            ushort.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out ushort mcc);

            index = nextIndex;
            nextIndex = NextIndex(index, inputData);

            byte.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out byte mnc);

            index = nextIndex;
            nextIndex = NextIndex(index, inputData);

            ushort.TryParse(lineSpan.Slice(index + 1, nextIndex - index - 1), out ushort lac);

            index = nextIndex;
            nextIndex = NextIndex(index, inputData);

            int.TryParse(lineSpan[(index + 1)..], out int cellId);

            this.Time = dateTime;
            this.Lat = lat;
            this.Long = lon;
            this.Sat = sat;
            this.Lbs = new Lbs
            {
                Mcc = mcc,
                Mnc = mnc,
                Lac = lac,
                CellId = cellId
            };

            return this;

            int NextIndex(int index, string line)
            {
                return line.IndexOf(separator, index + 1);
            }
        }
    }
}
