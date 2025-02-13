﻿#if SYSTEM_DRAWING_SUPPORT

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Tesseract
{
    /// <summary>
    /// Converts a <see cref="Bitmap"/> to a <see cref="Pix"/>.
    /// </summary>
    public class BitmapToPixConverter
    {
        public BitmapToPixConverter()
        {
        }

        /// <summary>
        /// Converts the specified <paramref name="img"/> to a <see cref="Pix"/>.
        /// </summary>
        /// <param name="img">The source image to be converted.</param>
        /// <returns>The converted pix.</returns>
        public Pix Convert(Bitmap img)
        {
            int pixDepth = GetPixDepth(img.PixelFormat);
            Pix pix = Pix.Create(img.Width, img.Height, pixDepth);
            pix.XRes = (int)Math.Round(img.HorizontalResolution);
            pix.YRes = (int)Math.Round(img.VerticalResolution);

            BitmapData imgData = null;
            PixData pixData = null;
            try
            {
                // TODO: Set X and Y resolution

                if ((img.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
                {
                    CopyColormap(img, pix);
                }

                // transfer data
                imgData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
                pixData = pix.GetData();

                if (imgData.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    TransferDataFormat32bppArgb(imgData, pixData);
                }
                else if (imgData.PixelFormat == PixelFormat.Format32bppRgb)
                {
                    TransferDataFormat32bppRgb(imgData, pixData);
                }
                else if (imgData.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    TransferDataFormat24bppRgb(imgData, pixData);
                }
                else if (imgData.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    TransferDataFormat8bppIndexed(imgData, pixData);
                }
                else if (imgData.PixelFormat == PixelFormat.Format1bppIndexed)
                {
                    TransferDataFormat1bppIndexed(imgData, pixData);
                }
                return pix;
            }
            catch (Exception)
            {
                pix.Dispose();
                throw;
            }
            finally
            {
                if (imgData != null)
                {
                    img.UnlockBits(imgData);
                }
            }
        }

        private void CopyColormap(Bitmap img, Pix pix)
        {
            ColorPalette imgPalette = img.Palette;
            Color[] imgPaletteEntries = imgPalette.Entries;
            PixColormap pixColormap = PixColormap.Create(pix.Depth);
            try
            {
                for (int i = 0; i < imgPaletteEntries.Length; i++)
                {
                    if (!pixColormap.AddColor(imgPaletteEntries[i].ToPixColor()))
                    {
                        throw new InvalidOperationException(string.Format("Failed to add colormap entry {0}.", i));
                    }
                }
                pix.Colormap = pixColormap;
            }
            catch (Exception)
            {
                pixColormap.Dispose();
                throw;
            }
        }

        private int GetPixDepth(PixelFormat pixelFormat) => pixelFormat switch
        {
            PixelFormat.Format1bppIndexed => 1,
            PixelFormat.Format8bppIndexed => 8,
            PixelFormat.Format32bppArgb or PixelFormat.Format32bppRgb or PixelFormat.Format24bppRgb => 32,
            _ => throw new InvalidOperationException(string.Format("Source bitmap's pixel format {0} is not supported.", pixelFormat)),
        };

        private unsafe void TransferDataFormat1bppIndexed(BitmapData imgData, PixData pixData)
        {
            int height = imgData.Height;
            int width = imgData.Width / 8;
            for (int y = 0; y < height; y++)
            {
                byte* imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                for (int x = 0; x < width; x++)
                {
                    byte pixelVal = BitmapHelper.GetDataByte(imgLine, x);
                    PixData.SetDataByte(pixLine, x, pixelVal);
                }
            }
        }

        private unsafe void TransferDataFormat24bppRgb(BitmapData imgData, PixData pixData)
        {
            PixelFormat imgFormat = imgData.PixelFormat;
            int height = imgData.Height;
            int width = imgData.Width;

            for (int y = 0; y < height; y++)
            {
                byte* imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                for (int x = 0; x < width; x++)
                {
                    byte* pixelPtr = imgLine + x * 3;
                    byte blue = pixelPtr[0];
                    byte green = pixelPtr[1];
                    byte red = pixelPtr[2];
                    PixData.SetDataFourByte(pixLine, x, BitmapHelper.EncodeAsRGBA(red, green, blue, 255));
                }
            }
        }

        private unsafe void TransferDataFormat32bppRgb(BitmapData imgData, PixData pixData)
        {
            PixelFormat imgFormat = imgData.PixelFormat;
            int height = imgData.Height;
            int width = imgData.Width;

            for (int y = 0; y < height; y++)
            {
                byte* imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                for (int x = 0; x < width; x++)
                {
                    byte* pixelPtr = imgLine + (x << 2);
                    byte blue = *pixelPtr;
                    byte green = *(pixelPtr + 1);
                    byte red = *(pixelPtr + 2);
                    PixData.SetDataFourByte(pixLine, x, BitmapHelper.EncodeAsRGBA(red, green, blue, 255));
                }
            }
        }

        private unsafe void TransferDataFormat32bppArgb(BitmapData imgData, PixData pixData)
        {
            PixelFormat imgFormat = imgData.PixelFormat;
            int height = imgData.Height;
            int width = imgData.Width;

            for (int y = 0; y < height; y++)
            {
                byte* imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                for (int x = 0; x < width; x++)
                {
                    byte* pixelPtr = imgLine + (x << 2);
                    byte blue = *pixelPtr;
                    byte green = *(pixelPtr + 1);
                    byte red = *(pixelPtr + 2);
                    byte alpha = *(pixelPtr + 3);
                    PixData.SetDataFourByte(pixLine, x, BitmapHelper.EncodeAsRGBA(red, green, blue, alpha));
                }
            }
        }

        private unsafe void TransferDataFormat8bppIndexed(BitmapData imgData, PixData pixData)
        {
            int height = imgData.Height;
            int width = imgData.Width;

            for (int y = 0; y < height; y++)
            {
                byte* imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                uint* pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                for (int x = 0; x < width; x++)
                {
                    byte pixelVal = *(imgLine + x);
                    PixData.SetDataByte(pixLine, x, pixelVal);
                }
            }
        }
    }
}

#endif