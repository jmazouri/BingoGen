using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BingoGen
{
    public static class Util
    {
        public static int RandSeed = DateTime.Now.Millisecond;
        public static Random FreeRand = new Random(RandSeed);

        public static Color generateRandomColor(Color mix, string basis = "")
        {
            var textRand = new Random(basis.GetHashCode());

            int red = textRand.Next(256);
            int green = textRand.Next(256);
            int blue = textRand.Next(256);

            // mix the color
            if (mix != null)
            {
                red = (red + mix.R) / 2;
                green = (green + mix.G) / 2;
                blue = (blue + mix.B) / 2;
            }

            Color color = Color.FromRgb((byte)red, (byte)green, (byte)blue);
            return color;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            FreeRand = new Random(RandSeed);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = FreeRand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
