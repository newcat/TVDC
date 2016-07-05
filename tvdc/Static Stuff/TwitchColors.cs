using System;
using System.Drawing;

namespace tvdc
{
    class TwitchColors
    {

        static Color[] colors = new Color[15];

        static TwitchColors()
        {
            colors[0] = Color.FromArgb(255, 0, 0);
            colors[1] = Color.FromArgb(0, 255, 0);
            colors[2] = Color.FromArgb(0, 0, 255);
            colors[3] = Color.FromArgb(178, 34, 34);
            colors[4] = Color.FromArgb(255, 127, 80);
            colors[5] = Color.FromArgb(154, 205, 50);
            colors[6] = Color.FromArgb(255, 69, 0);
            colors[7] = Color.FromArgb(46, 139, 87);
            colors[8] = Color.FromArgb(218, 165, 32);
            colors[9] = Color.FromArgb(210, 105, 30);
            colors[10] = Color.FromArgb(95, 158, 160);
            colors[11] = Color.FromArgb(30, 144, 255);
            colors[12] = Color.FromArgb(255, 105, 180);
            colors[13] = Color.FromArgb(138, 43, 226);
            colors[14] = Color.FromArgb(0, 255, 127);
        }

        public static string getColorByUsername(string name)
        {
            int hash = name.GetHashCode();
            return ColorTranslator.ToHtml(colors[Math.Abs(hash % 14)]);
        }

    }
}
