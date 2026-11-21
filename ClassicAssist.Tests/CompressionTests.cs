using System;
using System.Text;
using ClassicAssist.UO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class CompressionTests
    {
        // zlib (RFC 1950) compressed "Hello, World!", produced by native zlib
        private static readonly byte[] _helloWorldZlibStream =
        {
            0x78, 0x9C, 0xF3, 0x48, 0xCD, 0xC9, 0xC9, 0xD7, 0x51, 0x08, 0xCF, 0x2F, 0xCA, 0x49, 0x51, 0x04, 0x00,
            0x1F, 0x9E, 0x04, 0x6A
        };

        [TestMethod]
        public void WillUncompressKnownZlibStream()
        {
            byte[] destBuffer = new byte[64];
            int destLength = destBuffer.Length;

            bool success = Compression.Uncompress( ref destBuffer, ref destLength, _helloWorldZlibStream,
                _helloWorldZlibStream.Length );

            Assert.IsTrue( success );
            Assert.AreEqual( 13, destLength );
            Assert.AreEqual( "Hello, World!", Encoding.ASCII.GetString( destBuffer, 0, destLength ) );
        }

        [TestMethod]
        public void WillCompressAndUncompressRoundTrip()
        {
            byte[] source = Encoding.ASCII.GetBytes( "{ page { button { text \"button\" } } }" );

            byte[] compressed = Compression.Compress( source );

            Assert.IsTrue( compressed.Length > 0 );
            Assert.AreEqual( 8, compressed[0] & 0x0F ); // zlib deflate method

            byte[] destBuffer = new byte[source.Length];
            int destLength = destBuffer.Length;

            bool success = Compression.Uncompress( ref destBuffer, ref destLength, compressed, compressed.Length );

            Assert.IsTrue( success );
            Assert.AreEqual( source.Length, destLength );
            Assert.AreEqual( Encoding.ASCII.GetString( source ), Encoding.ASCII.GetString( destBuffer, 0, destLength ) );
        }

        [TestMethod]
        public void WillReturnFalseOnCorruptData()
        {
            byte[] compressed = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            byte[] destBuffer = new byte[64];
            int destLength = destBuffer.Length;

            bool success = Compression.Uncompress( ref destBuffer, ref destLength, compressed, compressed.Length );

            Assert.IsFalse( success );
        }

        [TestMethod]
        public void WillReturnFalseOnTruncatedData()
        {
            // Valid zlib header, truncated deflate stream
            byte[] compressed = { 0x78, 0x9C, 0xF3, 0x48, 0xCD };
            byte[] destBuffer = new byte[64];
            int destLength = destBuffer.Length;

            bool success = Compression.Uncompress( ref destBuffer, ref destLength, compressed, compressed.Length );

            Assert.IsFalse( success );
        }

        [TestMethod]
        public void WillReturnFalseOnInvalidAdler32()
        {
            byte[] compressed = ( byte[] ) _helloWorldZlibStream.Clone();
            compressed[compressed.Length - 1] ^= 0xFF; // corrupt Adler-32 trailer
            byte[] destBuffer = new byte[64];
            int destLength = destBuffer.Length;

            bool success = Compression.Uncompress( ref destBuffer, ref destLength, compressed, compressed.Length );

            Assert.IsFalse( success );
        }

        [TestMethod]
        public void WillReturnFalseOnEmptySource()
        {
            byte[] compressed = new byte[0];
            byte[] destBuffer = new byte[64];
            int destLength = destBuffer.Length;

            bool success = Compression.Uncompress( ref destBuffer, ref destLength, compressed, compressed.Length );

            Assert.IsFalse( success );
        }

        [TestMethod]
        public void WillReturnFalseWhenDestinationBufferTooSmall()
        {
            byte[] source = Encoding.ASCII.GetBytes( "A string which is longer than the destination buffer" );

            byte[] compressed = Compression.Compress( source );

            byte[] destBuffer = new byte[4];
            int destLength = destBuffer.Length;

            bool success = Compression.Uncompress( ref destBuffer, ref destLength, compressed, compressed.Length );

            Assert.IsFalse( success );
        }
    }
}
