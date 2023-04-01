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
        public Coordinates Coordinates { get; set; }
        public int Sat { get; set; }
        public Lbs Lbs { get; set; }

        public bool IsValid => this.Sat >= 3;

        private const string DateFormat = "dd-MM-yyyy HH:mm:ss zzz";

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(Time.ToUniversalTime().ToString(DateFormat)).Append(',')
                .Append(Coordinates.Lon.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Coordinates.Lat.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Sat).Append(',')
                .Append(Lbs.Mcc).Append(',')
                .Append(Lbs.Mnc).Append(',')
                .Append(Lbs.Lac).Append(',')
                .Append(Lbs.CellId);

            return stringBuilder.ToString();
        }

        public static bool TryParsePoint(string inputData, out Point? resultPoint)
        {
            const char separator = ',';

            var index = 0;
            var nextIndex = inputData.IndexOf(separator, index + 1);

            if (
                TryParseDateTime(ref index, ref nextIndex, inputData, DateFormat, out var time) &&
                TryParseDouble(ref index, ref nextIndex, inputData, out var lon) &&
                TryParseDouble(ref index, ref nextIndex, inputData, out var lat) &&
                TryParseInt(ref index, ref nextIndex, inputData, out var sat) &&
                TryParseInt(ref index, ref nextIndex, inputData, out var mcc) &&
                TryParseInt(ref index, ref nextIndex, inputData, out var mnc) &&
                TryParseInt(ref index, ref nextIndex, inputData, out var lac) &&
                TryParseInt(ref index, ref nextIndex, inputData, out var cellId)
            )
            {
                resultPoint = new Point
                {
                    Time = time,
                    Coordinates = new Coordinates { Lat = lat, Lon = lon },
                    Sat = sat,
                    Lbs = new Lbs
                    {
                        Mcc = mcc,
                        Mnc = mnc,
                        Lac = lac,
                        CellId = cellId
                    }
                };

                return true;
            }

            resultPoint = null;
            return false;


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

            bool TryParseDateTime(ref int index, ref int nextIndex, string line, string format, out DateTime result)
            {
                var resultBool = DateTime.TryParseExact(line.AsSpan().Slice(index, nextIndex - index), format,
                    CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces |
                                                    System.Globalization.DateTimeStyles.AdjustToUniversal, out result);

                NextIndex(ref index, line);
                NextIndex(ref nextIndex, line);
                return resultBool;
            }
        }
    }
}
