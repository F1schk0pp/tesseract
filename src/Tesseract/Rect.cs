using System;

namespace Tesseract
{
    public struct Rect : IEquatable<Rect>
    {
        public static readonly Rect Empty = new();

        #region Fields


        #endregion

        #region Constructors + Factory Methods

        public Rect(int x, int y, int width, int height)
        {
            X1 = x;
            Y1 = y;
            Width = width;
            Height = height;
        }

        public static Rect FromCoords(int x1, int y1, int x2, int y2) => new(x1, y1, x2 - x1, y2 - y1);

        #endregion

        #region Properties

        public int X1 { get; }

        public int Y1 { get; }

        public int X2 => X1 + Width;

        public int Y2 => Y1 + Height;

        public int Width { get; }

        public int Height { get; }

        #endregion

        #region Equals and GetHashCode implementation
        public override bool Equals(object? obj) => (obj is Rect rect) && Equals(rect);

        public bool Equals(Rect other) => X1 == other.X1 && Y1 == other.Y1 && Width == other.Width && Height == other.Height;

        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked
            {
                hashCode += 1000000007 * X1.GetHashCode();
                hashCode += 1000000009 * Y1.GetHashCode();
                hashCode += 1000000021 * Width.GetHashCode();
                hashCode += 1000000033 * Height.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(Rect lhs, Rect rhs) => lhs.Equals(rhs);

        public static bool operator !=(Rect lhs, Rect rhs) => !(lhs == rhs);
        #endregion

        #region ToString

        public override string ToString() => string.Format("[Rect X={0}, Y={1}, Width={2}, Height={3}]", X1, Y1, Width, Height);


        #endregion

    }
}
