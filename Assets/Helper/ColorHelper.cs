using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Helper
{
    public static class ColorHelper
    {
        public static List<Color> colors { get; set; } = new();

        /// <summary>
        /// Generates a list of distinct colors based on the specified size.
        /// </summary>
        /// <param name="size">Number of colors to generate.</param>
        /// <returns>List of distinct colors.</returns>
        public static void SetColors(int size)
        {
            colors = new List<Color>();
 
            for (int i = 0; i < size; i++)
            {
                var hue = (i * 360f / size) % 360; // Spread hues evenly across the circle
                var saturation = 0.7f;            // Fixed saturation for vibrant colors
                var lightness = 0.5f;             // Fixed lightness for medium brightness

                colors.Add(HSLToRGB(hue, saturation, lightness));
            }
        }

        /// <summary>
        /// Converts HSL values to an RGB color.
        /// </summary>
        /// <param name="hue">Hue in degrees (0-360).</param>
        /// <param name="saturation">Saturation as a fraction (0-1).</param>
        /// <param name="lightness">Lightness as a fraction (0-1).</param>
        /// <returns>RGB Color.</returns>
        private static Color HSLToRGB(float hue, float saturation, float lightness)
        {
            var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
            var x = c * (1 - Math.Abs((hue / 60f) % 2 - 1));
            var m = lightness - c / 2;

            var r = 0f;
            var g = 0f; 
            var b = 0f;

            if (hue < 60) { r = c; g = x; b = 0; }
            else if (hue < 120) { r = x; g = c; b = 0; }
            else if (hue < 180) { r = 0; g = c; b = x; }
            else if (hue < 240) { r = 0; g = x; b = c; }
            else if (hue < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            // Convert to 0-255 range and return as Color
            return new Color(r + m, g + m, b + m, 1.0f); // Alpha is 1 for fully opaque
        }
    }
}
