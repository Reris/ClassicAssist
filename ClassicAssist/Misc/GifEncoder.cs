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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace ClassicAssist.Misc
{
    public enum GifQuality
    {
        Bit4,
        Bit8,
        Grayscale
    }

    /// <summary>
    ///     Minimal GIF89a encoder with median-cut quantization, replacing the
    ///     System.Drawing based AnimatedGif package.
    /// </summary>
    public sealed class GifEncoder : IDisposable
    {
        private const int MAX_CODE_SIZE = 12;

        private readonly int _defaultDelayMs;
        private readonly int _repeat;
        private readonly Stream _stream;
        private bool _disposed;
        private bool _headerWritten;
        private int _height;
        private int _width;

        public GifEncoder( Stream stream, int delayMs = 100, int repeat = 0 )
        {
            _stream = stream ?? throw new ArgumentNullException( nameof( stream ) );
            _defaultDelayMs = delayMs;
            _repeat = repeat;
        }

        public void Dispose()
        {
            if ( _disposed )
            {
                return;
            }

            _disposed = true;

            _stream.WriteByte( 0x3B );
            _stream.Flush();
        }

        public void AddFrame( SKBitmap bitmap, int delayMs = -1, GifQuality quality = GifQuality.Bit8 )
        {
            if ( bitmap == null )
            {
                throw new ArgumentNullException( nameof( bitmap ) );
            }

            if ( !_headerWritten )
            {
                _width = bitmap.Width;
                _height = bitmap.Height;
                WriteHeader();
                _headerWritten = true;
            }

            int maxColors = quality == GifQuality.Bit4 ? 16 : 256;

            ( byte[] palette, byte[] indices ) = quality == GifQuality.Grayscale
                ? GrayscaleQuantize( bitmap )
                : MedianCutQuantize( bitmap, maxColors );

            int colorDepth = palette.Length <= 16 ? 4 : 8;
            int delay = delayMs < 0 ? _defaultDelayMs : delayMs;

            // Graphic Control Extension
            _stream.WriteByte( 0x21 );
            _stream.WriteByte( 0xF9 );
            _stream.WriteByte( 0x04 );
            _stream.WriteByte( 0x04 ); // disposal: do not dispose, no transparency
            _stream.WriteByte( (byte) ( delay / 10 ) );
            _stream.WriteByte( (byte) ( ( delay / 10 ) >> 8 ) );
            _stream.WriteByte( 0x00 );
            _stream.WriteByte( 0x00 );

            // Image Descriptor
            _stream.WriteByte( 0x2C );
            WriteShort( 0 );
            WriteShort( 0 );
            WriteShort( _width );
            WriteShort( _height );
            _stream.WriteByte( 0x00 );

            // LZW image data
            _stream.WriteByte( (byte) Math.Max( 2, colorDepth ) );

            byte[] lzw = LzwCompress( indices, colorDepth );

            for ( int offset = 0; offset < lzw.Length; offset += 255 )
            {
                int length = Math.Min( 255, lzw.Length - offset );
                _stream.WriteByte( (byte) length );
                _stream.Write( lzw, offset, length );
            }

            _stream.WriteByte( 0x00 );
        }

        private void WriteHeader()
        {
            byte[] header = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // GIF89a
            _stream.Write( header, 0, header.Length );

            WriteShort( _width );
            WriteShort( _height );
            _stream.WriteByte( 0x70 ); // no global color table, 8-bit color resolution
            _stream.WriteByte( 0x00 );
            _stream.WriteByte( 0x00 );

            // Netscape looping extension
            byte[] appId = { 0x21, 0xFF, 0x0B };
            _stream.Write( appId, 0, appId.Length );
            byte[] appName = System.Text.Encoding.ASCII.GetBytes( "NETSCAPE2.0" );
            _stream.Write( appName, 0, appName.Length );
            _stream.WriteByte( 0x03 );
            _stream.WriteByte( 0x01 );
            WriteShort( _repeat );
            _stream.WriteByte( 0x00 );
        }

        private void WriteShort( int value )
        {
            _stream.WriteByte( (byte) value );
            _stream.WriteByte( (byte) ( value >> 8 ) );
        }

        private static (byte[] Palette, byte[] Indices) GrayscaleQuantize( SKBitmap bitmap )
        {
            byte[] palette = new byte[256 * 3];

            for ( int i = 0; i < 256; i++ )
            {
                palette[i * 3] = (byte) i;
                palette[i * 3 + 1] = (byte) i;
                palette[i * 3 + 2] = (byte) i;
            }

            byte[] indices = new byte[bitmap.Width * bitmap.Height];
            IntPtr pixels = bitmap.GetPixels();
            unsafe
            {
                byte* basePtr = (byte*) pixels.ToPointer();

                for ( int y = 0; y < bitmap.Height; y++ )
                {
                    byte* row = basePtr + y * bitmap.RowBytes;

                    for ( int x = 0; x < bitmap.Width; x++ )
                    {
                        byte b = row[x * 4];
                        byte g = row[x * 4 + 1];
                        byte r = row[x * 4 + 2];

                        indices[y * bitmap.Width + x] = (byte) ( ( r * 299 + g * 587 + b * 114 ) / 1000 );
                    }
                }
            }

            return ( palette, indices );
        }

        private static (byte[] Palette, byte[] Indices) MedianCutQuantize( SKBitmap bitmap, int maxColors )
        {
            Dictionary<int, int> histogram = new Dictionary<int, int>();
            IntPtr pixels = bitmap.GetPixels();
            int width = bitmap.Width;
            int height = bitmap.Height;

            unsafe
            {
                byte* basePtr = (byte*) pixels.ToPointer();

                for ( int y = 0; y < height; y++ )
                {
                    byte* row = basePtr + y * bitmap.RowBytes;

                    for ( int x = 0; x < width; x++ )
                    {
                        int key = ( row[x * 4 + 2] << 16 ) | ( row[x * 4 + 1] << 8 ) | row[x * 4];

                        histogram[key] = histogram.TryGetValue( key, out int count ) ? count + 1 : 1;
                    }
                }
            }

            List<ColorEntry> colors = histogram
                .Select( kvp => new ColorEntry( (byte) ( ( kvp.Key >> 16 ) & 0xFF ), (byte) ( ( kvp.Key >> 8 ) & 0xFF ),
                    (byte) ( kvp.Key & 0xFF ), kvp.Value ) )
                .ToList();

            List<Box> boxes = new List<Box> { new Box( colors, 0, colors.Count ) };

            while ( boxes.Count < maxColors )
            {
                Box box = boxes.Where( b => b.Count > 1 ).OrderByDescending( b => b.LongestAxisLength )
                    .ThenByDescending( b => b.PixelCount ).FirstOrDefault();

                if ( box == null )
                {
                    break;
                }

                boxes.Remove( box );
                boxes.AddRange( box.Split() );
            }

            byte[] palette = new byte[boxes.Count * 3];

            for ( int i = 0; i < boxes.Count; i++ )
            {
                ( byte r, byte g, byte b ) = boxes[i].AverageColor();
                palette[i * 3] = r;
                palette[i * 3 + 1] = g;
                palette[i * 3 + 2] = b;
            }

            Dictionary<int, byte> cache = new Dictionary<int, byte>();
            byte[] indices = new byte[width * height];

            unsafe
            {
                byte* basePtr = (byte*) pixels.ToPointer();

                for ( int y = 0; y < height; y++ )
                {
                    byte* row = basePtr + y * bitmap.RowBytes;

                    for ( int x = 0; x < width; x++ )
                    {
                        int key = ( row[x * 4 + 2] << 16 ) | ( row[x * 4 + 1] << 8 ) | row[x * 4];

                        if ( !cache.TryGetValue( key, out byte index ) )
                        {
                            index = NearestColor( palette, row[x * 4 + 2], row[x * 4 + 1], row[x * 4] );
                            cache[key] = index;
                        }

                        indices[y * width + x] = index;
                    }
                }
            }

            return ( palette, indices );
        }

        private static byte NearestColor( byte[] palette, int r, int g, int b )
        {
            byte best = 0;
            int bestDistance = int.MaxValue;

            for ( int i = 0; i < palette.Length; i += 3 )
            {
                int dr = palette[i] - r;
                int dg = palette[i + 1] - g;
                int db = palette[i + 2] - b;

                int distance = dr * dr + dg * dg + db * db;

                if ( distance < bestDistance )
                {
                    bestDistance = distance;
                    best = (byte) ( i / 3 );
                }
            }

            return best;
        }

        private static byte[] LzwCompress( byte[] indices, int colorDepth )
        {
            int clearCode = 1 << colorDepth;
            int eoiCode = clearCode + 1;
            int codeSize = colorDepth + 1;
            int nextCode = eoiCode + 1;

            Dictionary<long, int> dictionary = new Dictionary<long, int>();
            BitWriter writer = new BitWriter();

            void Emit( int code )
            {
                writer.Write( code, codeSize );

                if ( nextCode > ( 1 << codeSize ) - 1 && codeSize < MAX_CODE_SIZE )
                {
                    codeSize++;
                }
            }

            Emit( clearCode );

            int prefix = -1;

            foreach ( byte k in indices )
            {
                if ( prefix == -1 )
                {
                    prefix = k;
                    continue;
                }

                long key = ( (long) prefix << 8 ) | k;

                if ( dictionary.TryGetValue( key, out int code ) )
                {
                    prefix = code;
                }
                else
                {
                    Emit( prefix );

                    if ( nextCode < 4096 )
                    {
                        dictionary[key] = nextCode++;
                    }
                    else
                    {
                        Emit( clearCode );
                        dictionary.Clear();
                        codeSize = colorDepth + 1;
                        nextCode = eoiCode + 1;
                    }

                    prefix = k;
                }
            }

            if ( prefix != -1 )
            {
                Emit( prefix );
            }

            Emit( eoiCode );

            return writer.ToBytes();
        }

        private sealed class ColorEntry
        {
            public ColorEntry( byte r, byte g, byte b, int count )
            {
                R = r;
                G = g;
                B = b;
                Count = count;
            }

            public byte B { get; }
            public byte G { get; }
            public byte R { get; }
            public int Count { get; set; }
        }

        private sealed class Box
        {
            private readonly List<ColorEntry> _colors;

            public Box( List<ColorEntry> colors, int start, int count )
            {
                _colors = colors;
                Start = start;
                Count = count;
                ComputeBounds();
            }

            public int Count { get; }
            public int Start { get; }

            public int LongestAxisLength => Math.Max( RMax - RMin, Math.Max( GMax - GMin, BMax - BMin ) );

            public int PixelCount { get; private set; }

            private int RMax { get; set; }
            private int RMin { get; set; }
            private int GMax { get; set; }
            private int GMin { get; set; }
            private int BMax { get; set; }
            private int BMin { get; set; }

            private void ComputeBounds()
            {
                RMin = GMin = BMin = 255;
                RMax = GMax = BMax = 0;
                PixelCount = 0;

                for ( int i = Start; i < Start + Count; i++ )
                {
                    ColorEntry c = _colors[i];

                    RMin = Math.Min( RMin, c.R );
                    RMax = Math.Max( RMax, c.R );
                    GMin = Math.Min( GMin, c.G );
                    GMax = Math.Max( GMax, c.G );
                    BMin = Math.Min( BMin, c.B );
                    BMax = Math.Max( BMax, c.B );
                    PixelCount += c.Count;
                }
            }

            public (byte R, byte G, byte B) AverageColor()
            {
                long r = 0, g = 0, b = 0, total = 0;

                for ( int i = Start; i < Start + Count; i++ )
                {
                    ColorEntry c = _colors[i];
                    r += (long) c.R * c.Count;
                    g += (long) c.G * c.Count;
                    b += (long) c.B * c.Count;
                    total += c.Count;
                }

                if ( total == 0 )
                {
                    return ( 0, 0, 0 );
                }

                return ( (byte) ( r / total ), (byte) ( g / total ), (byte) ( b / total ) );
            }

            public Box[] Split()
            {
                int axis = RMax - RMin >= GMax - GMin && RMax - RMin >= BMax - BMin
                    ? 0
                    : GMax - GMin >= BMax - BMin
                        ? 1
                        : 2;

                IEnumerable<ColorEntry> segment = _colors.Skip( Start ).Take( Count );

                segment = axis switch
                {
                    0 => segment.OrderBy( c => c.R ),
                    1 => segment.OrderBy( c => c.G ),
                    _ => segment.OrderBy( c => c.B )
                };

                List<ColorEntry> sorted = segment.ToList();

                for ( int i = 0; i < Count; i++ )
                {
                    _colors[Start + i] = sorted[i];
                }

                long half = PixelCount / 2;
                long running = 0;
                int splitIndex = Start;

                for ( int i = Start; i < Start + Count - 1; i++ )
                {
                    running += _colors[i].Count;
                    splitIndex = i + 1;

                    if ( running >= half )
                    {
                        break;
                    }
                }

                if ( splitIndex <= Start )
                {
                    splitIndex = Start + 1;
                }

                if ( splitIndex >= Start + Count )
                {
                    splitIndex = Start + Count - 1;
                }

                return new[]
                {
                    new Box( _colors, Start, splitIndex - Start ),
                    new Box( _colors, splitIndex, Start + Count - splitIndex )
                };
            }
        }

        private sealed class BitWriter
        {
            private readonly List<byte> _data = new List<byte>();
            private uint _bitBuffer;
            private int _bitCount;

            public void Write( int code, int size )
            {
                _bitBuffer |= (uint) ( code << _bitCount );
                _bitCount += size;

                while ( _bitCount >= 8 )
                {
                    _data.Add( (byte) ( _bitBuffer & 0xFF ) );
                    _bitBuffer >>= 8;
                    _bitCount -= 8;
                }
            }

            public byte[] ToBytes()
            {
                if ( _bitCount > 0 )
                {
                    _data.Add( (byte) ( _bitBuffer & 0xFF ) );
                }

                return _data.ToArray();
            }
        }
    }
}
