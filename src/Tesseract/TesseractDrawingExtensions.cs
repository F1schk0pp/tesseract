#if SYSTEM_DRAWING_SUPPORT

using System;
using System.Drawing;

namespace Tesseract
{
    public static class TesseractDrawingExtensions
    {

        /// <summary>
        /// Process the specified bitmap image.
        /// </summary>
        /// <remarks>
        /// Please consider <see cref="Process(Pix, PageSegMode?)"/> instead. This is because
        /// this method must convert the bitmap to a pix for processing which will add additional overhead.
        /// Leptonica also supports a large number of image pre-processing functions as well.
        /// </remarks>
        /// <param name="image">The image to process.</param>
        /// <param name="pageSegMode">The page segmentation mode.</param>
        /// <returns></returns>
        public static Page Process(this TesseractEngine engine, Bitmap image, PageSegMode? pageSegMode = null) => engine.Process(image, new Rect(0, 0, image.Width, image.Height), pageSegMode);

        /// <summary>
        /// Process the specified bitmap image.
        /// </summary>
        /// <remarks>
        /// Please consider <see cref="Process(Pix, string, PageSegMode?)"/> instead. This is because
        /// this method must convert the bitmap to a pix for processing which will add additional overhead.
        /// Leptonica also supports a large number of image pre-processing functions as well.
        /// </remarks>
        /// <param name="image">The image to process.</param>
        /// <param name="inputName">Sets the input file's name, only needed for training or loading a uzn file.</param>
        /// <param name="pageSegMode">The page segmentation mode.</param>
        /// <returns></returns>
        public static Page Process(this TesseractEngine engine, Bitmap image, string inputName, PageSegMode? pageSegMode = null) => engine.Process(image, inputName, new Rect(0, 0, image.Width, image.Height), pageSegMode);

        /// <summary>
        /// Process the specified bitmap image.
        /// </summary>
        /// <remarks>
        /// Please consider <see cref="TesseractEngine.Process(Pix, Rect, PageSegMode?)"/> instead. This is because
        /// this method must convert the bitmap to a pix for processing which will add additional overhead.
        /// Leptonica also supports a large number of image pre-processing functions as well.
        /// </remarks>
        /// <param name="image">The image to process.</param>
        /// <param name="region">The region of the image to process.</param>
        /// <param name="pageSegMode">The page segmentation mode.</param>
        /// <returns></returns>
        public static Page Process(this TesseractEngine engine, Bitmap image, Rect region, PageSegMode? pageSegMode = null) => engine.Process(image, null, region, pageSegMode);

        /// <summary>
        /// Process the specified bitmap image.
        /// </summary>
        /// <remarks>
        /// Please consider <see cref="TesseractEngine.Process(Pix, string, Rect, PageSegMode?)"/> instead. This is because
        /// this method must convert the bitmap to a pix for processing which will add additional overhead.
        /// Leptonica also supports a large number of image pre-processing functions as well.
        /// </remarks>
        /// <param name="image">The image to process.</param>
        /// <param name="inputName">Sets the input file's name, only needed for training or loading a uzn file.</param>
        /// <param name="region">The region of the image to process.</param>
        /// <param name="pageSegMode">The page segmentation mode.</param>
        /// <returns></returns>
        public static Page Process(this TesseractEngine engine, Bitmap image, string inputName, Rect region, PageSegMode? pageSegMode = null)
        {
            Pix pix = PixConverter.ToPix(image);
            Page page = engine.Process(pix, inputName, region, pageSegMode);
            new TesseractEngine.PageDisposalHandle(page, pix);
            return page;
        }

        public static Color ToColor(this PixColor color) => Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

        public static PixColor ToPixColor(this Color color) => new(color.R, color.G, color.B, color.A);

        /// <summary>
        /// gets the number of Bits Per Pixel (BPP)
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static int GetBPP(this System.Drawing.Bitmap bitmap) => bitmap.PixelFormat switch
        {
            System.Drawing.Imaging.PixelFormat.Format1bppIndexed => 1,
            System.Drawing.Imaging.PixelFormat.Format4bppIndexed => 4,
            System.Drawing.Imaging.PixelFormat.Format8bppIndexed => 8,
            System.Drawing.Imaging.PixelFormat.Format16bppArgb1555 or System.Drawing.Imaging.PixelFormat.Format16bppGrayScale or System.Drawing.Imaging.PixelFormat.Format16bppRgb555 or System.Drawing.Imaging.PixelFormat.Format16bppRgb565 => 16,
            System.Drawing.Imaging.PixelFormat.Format24bppRgb => 24,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb or System.Drawing.Imaging.PixelFormat.Format32bppPArgb or System.Drawing.Imaging.PixelFormat.Format32bppRgb => 32,
            System.Drawing.Imaging.PixelFormat.Format48bppRgb => 48,
            System.Drawing.Imaging.PixelFormat.Format64bppArgb or System.Drawing.Imaging.PixelFormat.Format64bppPArgb => 64,
            _ => throw new ArgumentException(string.Format("The bitmap's pixel format of {0} was not recognised.", bitmap.PixelFormat), nameof(bitmap)),
        };
    }
}

#endif
