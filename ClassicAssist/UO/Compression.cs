using System.IO;
using System.IO.Compression;

namespace ClassicAssist.UO;

public static class Compression
{
    public static bool Uncompress( ref byte[] destBuffer, ref int destLength, byte[] sourceBuffer, int sourceLen )
    {
        try
        {
            using MemoryStream sourceStream = new( sourceBuffer, 0, sourceLen, false );
            using ZLibStream zlibStream = new( sourceStream, CompressionMode.Decompress );
            int totalRead = 0;
            int read;

            while ( totalRead < destBuffer.Length && ( read = zlibStream.Read( destBuffer, totalRead, destBuffer.Length - totalRead ) ) > 0 )
            {
                totalRead += read;
            }

            destLength = totalRead;

            // More decompressed data than will fit in the destination buffer
            if ( zlibStream.ReadByte() >= 0 )
            {
                return false;
            }

            // ZLibStream does not error on truncated input, so verify the Adler-32 trailer like zlib uncompress
            return VerifyAdler32( sourceBuffer, sourceLen, destBuffer, destLength );
        }
        catch ( InvalidDataException )
        {
            return false;
        }
    }

    public static byte[] Compress( byte[] bytes )
    {
        using MemoryStream outputStream = new();

        // ZLibStream must be disposed before reading the output, otherwise the final
        // deflate block and Adler-32 trailer have not yet been written
        using ( ZLibStream zlibStream = new( outputStream, CompressionLevel.Optimal, true ) )
        {
            zlibStream.Write( bytes, 0, bytes.Length );
        }

        return outputStream.ToArray();
    }

    private static bool VerifyAdler32( byte[] sourceBuffer, int sourceLen, byte[] destBuffer, int destLength )
    {
        if ( sourceLen < 4 )
        {
            return false;
        }

        uint expected = (uint) ( ( sourceBuffer[sourceLen - 4] << 24 ) | ( sourceBuffer[sourceLen - 3] << 16 ) |
                                 ( sourceBuffer[sourceLen - 2] << 8 ) | sourceBuffer[sourceLen - 1] );

        uint s1 = 1;
        uint s2 = 0;

        for ( int i = 0; i < destLength; i++ )
        {
            s1 = ( s1 + destBuffer[i] ) % 65521;
            s2 = ( s2 + s1 ) % 65521;
        }

        return ( ( s2 << 16 ) | s1 ) == expected;
    }
}