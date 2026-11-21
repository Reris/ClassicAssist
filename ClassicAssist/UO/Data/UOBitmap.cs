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
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia;

namespace ClassicAssist.UO.Data
{
    /// <summary>
    ///     Cross-platform replacement for the System.Drawing.Bitmap usages around UO
    ///     art. Pixels are stored in the native 16bpp ARGB1555 MUL format.
    /// </summary>
    public class UOBitmap
    {
        private readonly ushort[] _pixels;

        public UOBitmap( int width, int height )
        {
            Width = width;
            Height = height;
            _pixels = new ushort[width * height];
        }

        public UOBitmap( int width, int height, ushort[] pixels )
        {
            Width = width;
            Height = height;
            _pixels = pixels ?? new ushort[width * height];
        }

        public int Width { get; }

        public int Height { get; }

        public ushort[] Pixels => _pixels;

        public ushort this[int x, int y]
        {
            get => _pixels[y * Width + x];
            set => _pixels[y * Width + x] = value;
        }

        public UOBitmap Clone()
        {
            return new UOBitmap( Width, Height, (ushort[]) _pixels.Clone() );
        }

        /// <summary>
        ///     Converts the 16bpp ARGB1555 pixel data into an Avalonia bitmap
        ///     (BGRA8888) suitable for display.
        /// </summary>
        public unsafe Bitmap ToBitmap()
        {
            var bitmap = new WriteableBitmap( new PixelSize( Width, Height ), new Vector( 96, 96 ),
                PixelFormat.Bgra8888, AlphaFormat.Premul );

            using ( ILockedFramebuffer frame = bitmap.Lock() )
            {
                byte* buffer = (byte*) frame.Address;

                int stride = frame.RowBytes;

                fixed ( ushort* pixelsBase = _pixels )
                {
                    for ( int y = 0; y < Height; y++ )
                    {
                        ushort* sourceRow = pixelsBase + y * Width;
                        byte* destRow = buffer + y * stride;

                        for ( int x = 0; x < Width; x++ )
                        {
                            ushort c = sourceRow[x];

                            // ARGB1555 -> BGRA8888
                            byte a = (byte) ( ( c & 0x8000 ) != 0 ? 255 : 0 );
                            byte r5 = (byte) ( ( c >> 10 ) & 0x1F );
                            byte g5 = (byte) ( ( c >> 5 ) & 0x1F );
                            byte b5 = (byte) ( c & 0x1F );

                            destRow[x * 4 + 0] = (byte) ( ( b5 * 255 ) / 31 );
                            destRow[x * 4 + 1] = (byte) ( ( g5 * 255 ) / 31 );
                            destRow[x * 4 + 2] = (byte) ( ( r5 * 255 ) / 31 );
                            destRow[x * 4 + 3] = a;
                        }
                    }
                }
            }

            return bitmap;
        }

        public static implicit operator Bitmap( UOBitmap bitmap )
        {
            return bitmap?.ToBitmap();
        }
    }
}
