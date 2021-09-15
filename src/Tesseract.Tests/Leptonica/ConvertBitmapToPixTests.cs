using NUnit.Framework;

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Tesseract.Tests.Leptonica
{
    public class ConvertBitmapToPixTests : TesseractTestBase
    {
        // Test for [Issue #166](https://github.com/charlesw/tesseract/issues/166)
        [Test]
        public unsafe void Convert_ScaledBitmapToPix()
        {
            string sourceFilePath = TestFilePath("Conversion/photo_rgb_32bpp.tif");
            BitmapToPixConverter bitmapConverter = new();
            using Bitmap source = new(sourceFilePath);
            using Bitmap scaledSource = new(source, new Size(source.Width * 2, source.Height * 2));
            Assert.That(scaledSource.GetBPP(), Is.EqualTo(32));
            using Pix dest = bitmapConverter.Convert(scaledSource);
            dest.Save(TestResultRunFile("Conversion/ScaledBitmapToPix_rgb_32bpp.tif"), ImageFormat.Tiff);

            AssertAreEquivalent(scaledSource, dest, true);
        }

        [Test]
        [TestCase(PixelFormat.Format1bppIndexed)] // Note: 1bpp will not save pixmap when writing out the result, this is a limitation of leptonica (see pixWriteToTiffStream)
        [TestCase(PixelFormat.Format4bppIndexed, Ignore = "4bpp images not supported.")]
        [TestCase(PixelFormat.Format8bppIndexed)]
        [TestCase(PixelFormat.Format32bppRgb)]
        [TestCase(PixelFormat.Format32bppArgb)]
        public unsafe void Convert_BitmapToPix(PixelFormat pixelFormat)
        {
            int depth = Image.GetPixelFormatSize(pixelFormat);
            string pixType;
            if (depth < 16)
            {
                pixType = "palette";
            }
            else
            {
                pixType = depth == 16 ? "grayscale" : Image.IsAlphaPixelFormat(pixelFormat) ? "argb" : "rgb";
            }

            string sourceFile = string.Format("Conversion/photo_{0}_{1}bpp.tif", pixType, depth);
            string sourceFilePath = TestFilePath(sourceFile);
            BitmapToPixConverter bitmapConverter = new();
            using Bitmap source = new(sourceFilePath);
            Assert.That(source.PixelFormat, Is.EqualTo(pixelFormat));
            Assert.That(source.GetBPP(), Is.EqualTo(depth));
            using Pix dest = bitmapConverter.Convert(source);
            string destFilename = string.Format("Conversion/BitmapToPix_{0}_{1}bpp.tif", pixType, depth);
            dest.Save(TestResultRunFile(destFilename), ImageFormat.Tiff);

            AssertAreEquivalent(source, dest, true);
        }

        /// <summary>
        /// Test case for https://github.com/charlesw/tesseract/issues/180
        /// </summary>
        [Test]
        public unsafe void Convert_BitmapToPix_Format8bppIndexed()
        {
            string sourceFile = TestFilePath("Conversion/photo_palette_8bpp.png");
            BitmapToPixConverter bitmapConverter = new();
            using Bitmap source = new(sourceFile);
            Assert.That(source.GetBPP(), Is.EqualTo(8));
            Assert.That(source.PixelFormat, Is.EqualTo(PixelFormat.Format8bppIndexed));
            using Pix dest = bitmapConverter.Convert(source);
            string destFilename = TestResultRunFile("Conversion/BitmapToPix_palette_8bpp.png");
            dest.Save(destFilename, ImageFormat.Png);

            AssertAreEquivalent(source, dest, true);
        }

        [Test]
        [TestCase(1, true, false)]
        [TestCase(1, false, false)]
        [TestCase(4, false, false, Ignore = "4bpp images not supported.")]
        [TestCase(4, true, false, Ignore = "4bpp images not supported.")]
        [TestCase(8, false, false)]
        [TestCase(8, true, false, Ignore = "Haven't yet created a 8bpp grayscale test image.")]
        [TestCase(32, false, true)]
        [TestCase(32, false, false)]
        public unsafe void Convert_PixToBitmap(int depth, bool isGrayscale, bool includeAlpha)
        {
            bool hasPalette = depth < 16 && !isGrayscale;
            string pixType;
            if (isGrayscale)
            {
                pixType = "grayscale";
            }
            else
            {
                pixType = hasPalette ? "palette" : "rgb";
            }

            string sourceFile = TestFilePath(string.Format("Conversion/photo_{0}_{1}bpp.tif", pixType, depth));
            PixToBitmapConverter converter = new();
            using Pix source = Pix.LoadFromFile(sourceFile);
            Assert.That(source.Depth, Is.EqualTo(depth));
            if (hasPalette)
            {
                Assert.That(source.Colormap, Is.Not.Null, "Expected source image to have color map\\palette.");
            }
            else
            {
                Assert.That(source.Colormap, Is.Null, "Expected source image to be grayscale.");
            }
            using Bitmap dest = converter.Convert(source, includeAlpha);
            string destFilename = TestResultRunFile(string.Format("Conversion/PixToBitmap_{0}_{1}bpp.tif", pixType, depth));
            dest.Save(destFilename, System.Drawing.Imaging.ImageFormat.Tiff);

            AssertAreEquivalent(dest, source, includeAlpha);
        }

        private void AssertAreEquivalent(Bitmap bmp, Pix pix, bool checkAlpha)
        {
            // verify img metadata
            Assert.That(pix.Width, Is.EqualTo(bmp.Width));
            Assert.That(pix.Height, Is.EqualTo(bmp.Height));
            //Assert.That(pix.Resolution.X, Is.EqualTo(bmp.HorizontalResolution));
            //Assert.That(pix.Resolution.Y, Is.EqualTo(bmp.VerticalResolution));

            // do some random sampling over image
            int height = pix.Height;
            int width = pix.Width;
            for (int y = 0; y < height; y += height)
            {
                for (int x = 0; x < width; x += width)
                {
                    PixColor sourcePixel = bmp.GetPixel(x, y).ToPixColor();
                    PixColor destPixel = GetPixel(pix, x, y);
                    if (checkAlpha)
                    {
                        Assert.That(destPixel, Is.EqualTo(sourcePixel), "Expected pixel at <{0},{1}> to be same in both source and dest.", x, y);
                    }
                    else
                    {
                        Assert.That(destPixel, Is.EqualTo(sourcePixel).Using<PixColor>((c1, c2) => (c1.Red == c2.Red && c1.Blue == c2.Blue && c1.Green == c2.Green) ? 0 : 1), "Expected pixel at <{0},{1}> to be same in both source and dest.", x, y);
                    }
                }
            }
        }

        private unsafe PixColor GetPixel(Pix pix, int x, int y)
        {
            int pixDepth = pix.Depth;
            PixData pixData = pix.GetData();
            uint* pixLine = (uint*)pixData.Data + pixData.WordsPerLine * y;
            uint pixValue;
            if (pixDepth == 1)
            {
                pixValue = PixData.GetDataBit(pixLine, x);
            }
            else if (pixDepth == 4)
            {
                pixValue = PixData.GetDataQBit(pixLine, x);
            }
            else if (pixDepth == 8)
            {
                pixValue = PixData.GetDataByte(pixLine, x);
            }
            else
            {
                pixValue = pixDepth == 32
                    ? PixData.GetDataFourByte(pixLine, x)
                    : throw new ArgumentException(string.Format("Bit depth of {0} is not supported.", pix.Depth), nameof(pix));
            }

            if (pix.Colormap != null)
            {
                return pix.Colormap[(int)pixValue];
            }
            else
            {
                if (pixDepth == 32)
                {
                    return PixColor.FromRgba(pixValue);
                }
                else
                {
                    byte grayscale = (byte)(pixValue * 255 / ((1 << 16) - 1));
                    return new PixColor(grayscale, grayscale, grayscale);
                }
            }
        }
    }
}
