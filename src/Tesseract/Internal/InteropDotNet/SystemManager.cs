//  Copyright (c) 2014 Andrey Akinshin
//  Project URL: https://github.com/AndreyAkinshin/InteropDotNet
//  Distributed under the MIT License: http://opensource.org/licenses/MIT
using System;

namespace InteropDotNet
{
    static class SystemManager
    {
        public static string GetPlatformName() => IntPtr.Size == sizeof(int) ? "x86" : "x64";

        public static OperatingSystem GetOperatingSystem() =>
            (int)Environment.OSVersion.Platform switch
            {
                (int)PlatformID.Win32NT or (int)PlatformID.Win32S or (int)PlatformID.Win32Windows or (int)PlatformID.WinCE => OperatingSystem.Windows,
                (int)PlatformID.Unix or 128 => OperatingSystem.Unix,
                (int)PlatformID.MacOSX => OperatingSystem.MacOSX,
                _ => OperatingSystem.Unknown,
            };
    }

    enum OperatingSystem
    {
        Windows,
        Unix,
        MacOSX,
        Unknown
    }
}