using System.Text;

namespace BackgroundServiceApp
{
    public class Point
    {
        public DateTime Time { get; set; }

        public double Long { get; set; }

        public double Lat { get; set; }

        public int Sat { get; set; }

        public Lbs? Lbs { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine(Time.ToString())
                .AppendLine(Long.ToString())
                .AppendLine(Lat.ToString())
                .AppendLine(Sat.ToString())
                .AppendLine(Lbs.Mnc.ToString())
                .AppendLine(Lbs.Mcc.ToString())
                .AppendLine(Lbs.Lac.ToString())
                .AppendLine(Lbs.CellId.ToString());

            return stringBuilder.ToString();
        }

        public Point Parse(string inputData)
        {
            return new Point {               
            };
        }
    }
}
