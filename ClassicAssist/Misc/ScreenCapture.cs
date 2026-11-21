#region License

// Copyright (C) 2026 Reetus
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace ClassicAssist.Misc
{
    /// <summary>
    ///     Cross-platform screen capture. GDI based on Windows, unsupported
    ///     (returns null) elsewhere.
    /// </summary>
    public static class ScreenCapture
    {
        public static SKBitmap CaptureRectangle( int x, int y, int width, int height )
        {
            if ( width <= 0 || height <= 0 )
            {
                return null;
            }

            if ( !OperatingSystem.IsWindows() )
            {
                return null;
            }

            IntPtr screenDc = GetDC( IntPtr.Zero );

            try
            {
                IntPtr memDc = CreateCompatibleDC( screenDc );
                IntPtr bitmap = CreateCompatibleBitmap( screenDc, width, height );

                try
                {
                    IntPtr old = SelectObject( memDc, bitmap );

                    try
                    {
                        if ( !BitBlt( memDc, 0, 0, width, height, screenDc, x, y, SRCCOPY ) )
                        {
                            return null;
                        }

                        return CopyToSkBitmap( memDc, bitmap, width, height );
                    }
                    finally
                    {
                        SelectObject( memDc, old );
                    }
                }
                finally
                {
                    DeleteObject( bitmap );
                    DeleteDC( memDc );
                }
            }
            finally
            {
                ReleaseDC( IntPtr.Zero, screenDc );
            }
        }

        private static unsafe SKBitmap CopyToSkBitmap( IntPtr dc, IntPtr bitmap, int width, int height )
        {
            var info = new BITMAPINFO
            {
                bmiHeader = new BITMAPINFOHEADER
                {
                    biSize = (uint) Marshal.SizeOf<BITMAPINFOHEADER>(),
                    biWidth = width,
                    biHeight = -height,
                    biPlanes = 1,
                    biBitCount = 32,
                    biCompression = BI_RGB
                }
            };

            var result = new SKBitmap( width, height, SKColorType.Bgra8888, SKAlphaType.Premul );
            IntPtr pixels = result.GetPixels();

            if ( GetDIBits( dc, bitmap, 0, (uint) height, pixels, ref info, DIB_RGB_COLORS ) == 0 )
            {
                result.Dispose();

                return null;
            }

            return result;
        }

        private const uint SRCCOPY = 0x00CC0020;
        private const uint BI_RGB = 0;
        private const uint DIB_RGB_COLORS = 0;

        [StructLayout( LayoutKind.Sequential )]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
        }

        [DllImport( "user32.dll" )]
        private static extern IntPtr GetDC( IntPtr hWnd );

        [DllImport( "user32.dll" )]
        private static extern int ReleaseDC( IntPtr hWnd, IntPtr hDc );

        [DllImport( "gdi32.dll" )]
        private static extern IntPtr CreateCompatibleDC( IntPtr hdc );

        [DllImport( "gdi32.dll" )]
        private static extern IntPtr CreateCompatibleBitmap( IntPtr hdc, int nWidth, int nHeight );

        [DllImport( "gdi32.dll" )]
        private static extern IntPtr SelectObject( IntPtr hdc, IntPtr hgdiobj );

        [DllImport( "gdi32.dll" )]
        private static extern bool DeleteObject( IntPtr hObject );

        [DllImport( "gdi32.dll" )]
        private static extern bool DeleteDC( IntPtr hdc );

        [DllImport( "gdi32.dll" )]
        private static extern bool BitBlt( IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop );

        [DllImport( "gdi32.dll" )]
        private static extern int GetDIBits( IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines,
            IntPtr lpvBits, ref BITMAPINFO lpbi, uint uUsage );
    }
}
