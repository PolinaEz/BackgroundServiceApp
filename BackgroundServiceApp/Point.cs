using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BackgroundServiceApp
{
    public class Point
    {
        public DateTime Time { get; set; }

        public Coordinates Сoordinates { get; set; }

        public int Sat { get; set; }

        public Lbs Lbs { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(Time.ToString()).Append(',')
                .Append(Сoordinates.Lon.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Сoordinates.Lat.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Sat).Append(',')
                .Append(Lbs.Mcc).Append(',')
                .Append(Lbs.Mnc).Append(',')
                .Append(Lbs.Lac).Append(',')
                .Append(Lbs.CellId);

            return stringBuilder.ToString();
        }

        public Point? Parse(string inputData)
        {
            char separator = ',';
            int index = 0;
            int nextIndex = inputData.IndexOf(separator, index + 1);
            CultureInfo cultureInfo = new CultureInfo("de-DE");

            if (
                    TryParseDateTime(ref index, ref nextIndex, inputData, cultureInfo, out DateTime time) &&
                    TryParseDouble(ref index, ref nextIndex, inputData, out double lon) &&
                    TryParseDouble(ref index, ref nextIndex, inputData, out double lat) &&
                    TryParseInt(ref index, ref nextIndex, inputData, out int sat) &&
                    TryParseInt(ref index, ref nextIndex, inputData, out int mcc) &&
                    TryParseInt(ref index, ref nextIndex, inputData, out int mnc) &&
                    TryParseInt(ref index, ref nextIndex, inputData, out int lac) &&
                    TryParseInt(ref index, ref nextIndex, inputData, out int cellId)
                )
            {
                this.Time = time;
                this.Сoordinates = new Coordinates { Lat = lat, Lon = lon };
                this.Sat = sat;
                this.Lbs = new Lbs
                {
                    Mcc = mcc,
                    Mnc = mnc,
                    Lac = lac,
                    CellId = cellId
                };

                return this;
            }
            else
                return default;


            void NextIndex(ref int index, string line)
            {
                index = line.IndexOf(separator, index + 1);
            }

            bool TryParseInt(ref int index, ref int nextIndex, string line, out int result)
            {
                if (nextIndex == -1)
                {
                    nextIndex = line.Length;
                }

                var resultBool = int.TryParse(line.AsSpan().Slice(index + 1, nextIndex - index - 1), out result);

                NextIndex(ref index, line);

                if (index == -1)
                {
                    return resultBool;
                }

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

            bool TryParseDateTime(ref int index, ref int nextIndex, string line, CultureInfo cultureInfo, out DateTime result)
            {
                var resultBool = DateTime.TryParse(line.AsSpan().Slice(index, nextIndex - index), out result);
                NextIndex(ref index, line);
                NextIndex(ref nextIndex, line);
                return resultBool;
            }
        }
    }
}
