using NUnit.Framework;

namespace Tesseract.Tests.Leptonica
{
    [TestFixture]
    public class ColorTests
    {
#if NETFULL
        [TestCase]
        public void Color_CastColorToNetColor()
        {
            var color = new PixColor(100, 150, 200);
            var castColor = (System.Drawing.Color)color;
            Assert.That(castColor.R, Is.EqualTo(color.Red));
            Assert.That(castColor.G, Is.EqualTo(color.Green));
            Assert.That(castColor.B, Is.EqualTo(color.Blue));
            Assert.That(castColor.A, Is.EqualTo(color.Alpha));
        }
#endif

        [TestCase]
        public void Color_ConvertColorToNetColor()
        {
            PixColor color = new(100, 150, 200);
            System.Drawing.Color castColor = color.ToColor();
            Assert.That(castColor.R, Is.EqualTo(color.Red));
            Assert.That(castColor.G, Is.EqualTo(color.Green));
            Assert.That(castColor.B, Is.EqualTo(color.Blue));
            Assert.That(castColor.A, Is.EqualTo(color.Alpha));
        }
    }
}
