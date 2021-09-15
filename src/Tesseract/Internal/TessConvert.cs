using System.Globalization;

namespace Tesseract.Internal
{
    /// <summary>
    /// Utility helpers to handle converting variable values.
    /// </summary>
    internal static class TessConvert
    {
        public static bool TryToString(object value, out string result)
        {
            result = value switch
            {
                bool => ToString((bool)value),
                decimal => ToString((decimal)value),
                double => ToString((double)value),
                float => ToString((float)value),
                short => ToString((short)value),
                int => ToString((int)value),
                long => ToString((long)value),
                ushort => ToString((ushort)value),
                uint => ToString((uint)value),
                ulong => ToString((ulong)value),
                string => (string)value,
                _ => null,
            };

            return result != null;
        }

        public static string ToString(bool value) => value ? "TRUE" : "FALSE";

        public static string ToString(decimal value) => value.ToString("R", CultureInfo.InvariantCulture.NumberFormat);

        public static string ToString(double value) => value.ToString("R", CultureInfo.InvariantCulture.NumberFormat);

        public static string ToString(float value) => value.ToString("R", CultureInfo.InvariantCulture.NumberFormat);

        public static string ToString(short value) => value.ToString("D", CultureInfo.InvariantCulture.NumberFormat);

        public static string ToString(int value) => value.ToString("D", CultureInfo.InvariantCulture.NumberFormat);

        public static string ToString(long value) => value.ToString("D", CultureInfo.InvariantCulture.NumberFormat);

        public static string ToString(ushort value) => value.ToString("D", CultureInfo.InvariantCulture.NumberFormat);

        public static string ToString(uint value) => value.ToString("D", CultureInfo.InvariantCulture.NumberFormat);

        public static string ToString(ulong value) => value.ToString("D", CultureInfo.InvariantCulture.NumberFormat);
    }
}
