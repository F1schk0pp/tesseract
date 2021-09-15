using System;
using System.Runtime.InteropServices;

namespace Tesseract
{
    /// <summary>
    /// Represents a colormap.
    /// </summary>
    /// <remarks>
    /// Once the colormap is assigned to a pix it is owned by that pix and will be disposed off automatically 
    /// when the pix is disposed off.
    /// </remarks>
    public sealed class PixColormap : IDisposable
    {
        internal PixColormap(IntPtr handle) => Handle = new HandleRef(this, handle);

        public static PixColormap Create(int depth)
        {
            if (depth is not (1 or 2 or 4 or 8))
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be 1, 2, 4, or 8 bpp.");
            }

            IntPtr handle = Interop.LeptonicaApi.Native.pixcmapCreate(depth);
            return handle == IntPtr.Zero ? throw new InvalidOperationException("Failed to create colormap.") : new PixColormap(handle);
        }

        public static PixColormap CreateLinear(int depth, int levels)
        {
            if (depth is not (1 or 2 or 4 or 8))
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be 1, 2, 4, or 8 bpp.");
            }
            if (levels < 2 || levels > (2 << depth))
            {
                throw new ArgumentOutOfRangeException(nameof(levels), "Depth must be 2 and 2^depth (inclusive).");
            }

            IntPtr handle = Interop.LeptonicaApi.Native.pixcmapCreateLinear(depth, levels);
            return handle == IntPtr.Zero ? throw new InvalidOperationException("Failed to create colormap.") : new PixColormap(handle);
        }

        public static PixColormap CreateLinear(int depth, bool firstIsBlack, bool lastIsWhite)
        {
            if (depth is not (1 or 2 or 4 or 8))
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "Depth must be 1, 2, 4, or 8 bpp.");
            }

            IntPtr handle = Interop.LeptonicaApi.Native.pixcmapCreateRandom(depth, firstIsBlack ? 1 : 0, lastIsWhite ? 1 : 0);
            return handle == IntPtr.Zero ? throw new InvalidOperationException("Failed to create colormap.") : new PixColormap(handle);
        }

        internal HandleRef Handle { get; private set; }

        public int Depth => Interop.LeptonicaApi.Native.pixcmapGetDepth(Handle);

        public int Count => Interop.LeptonicaApi.Native.pixcmapGetCount(Handle);

        public int FreeCount => Interop.LeptonicaApi.Native.pixcmapGetFreeCount(Handle);

        public bool AddColor(PixColor color) => Interop.LeptonicaApi.Native.pixcmapAddColor(Handle, color.Red, color.Green, color.Blue) == 0;

        public bool AddNewColor(PixColor color, out int index) => Interop.LeptonicaApi.Native.pixcmapAddNewColor(Handle, color.Red, color.Green, color.Blue, out index) == 0;

        public bool AddNearestColor(PixColor color, out int index) => Interop.LeptonicaApi.Native.pixcmapAddNearestColor(Handle, color.Red, color.Green, color.Blue, out index) == 0;

        public bool AddBlackOrWhite(int color, out int index) => Interop.LeptonicaApi.Native.pixcmapAddBlackOrWhite(Handle, color, out index) == 0;

        public bool SetBlackOrWhite(bool setBlack, bool setWhite) => Interop.LeptonicaApi.Native.pixcmapSetBlackAndWhite(Handle, setBlack ? 1 : 0, setWhite ? 1 : 0) == 0;

        public bool IsUsableColor(PixColor color) => Interop.LeptonicaApi.Native.pixcmapUsableColor(Handle, color.Red, color.Green, color.Blue, out int usable) == 0
                ? usable == 1
                : throw new InvalidOperationException("Failed to detect if color was usable or not.");

        public void Clear()
        {
            if (Interop.LeptonicaApi.Native.pixcmapClear(Handle) != 0)
            {
                throw new InvalidOperationException("Failed to clear color map.");
            }
        }

        public PixColor this[int index]
        {
            get => Interop.LeptonicaApi.Native.pixcmapGetColor32(Handle, index, out int color) == 0
                    ? PixColor.FromRgb((uint)color)
                    : throw new InvalidOperationException("Failed to retrieve color.");
            set
            {
                if (Interop.LeptonicaApi.Native.pixcmapResetColor(Handle, index, value.Red, value.Green, value.Blue) != 0)
                {
                    throw new InvalidOperationException("Failed to reset color.");
                }
            }
        }

        public void Dispose()
        {
            IntPtr tmpHandle = Handle.Handle;
            Interop.LeptonicaApi.Native.pixcmapDestroy(ref tmpHandle);
            Handle = new HandleRef(this, IntPtr.Zero);
        }
    }
}
