using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ClassicAssist.Misc
{
    /// <summary>
    ///     Cross-platform .wav playback. Uses the Windows multimedia API; other
    ///     platforms currently ignore playback requests.
    /// </summary>
    public static class SoundHelper
    {
        private const uint SND_SYNC = 0x0000;
        private const uint SND_ASYNC = 0x0001;
        private const uint SND_NODEFAULT = 0x0002;
        private const uint SND_MEMORY = 0x0004;
        private const uint SND_FILENAME = 0x20000;

        public static void Play( string path )
        {
            PlayFile( path, false );
        }

        public static void PlaySync( string path )
        {
            PlayFile( path, true );
        }

        public static void Play( Stream stream )
        {
            PlayStream( stream );
        }

        private static void PlayFile( string path, bool sync )
        {
            if ( string.IsNullOrEmpty( path ) || !OperatingSystem.IsWindows() )
            {
                return;
            }

            try
            {
                uint flags = SND_FILENAME | SND_NODEFAULT | ( sync ? SND_SYNC : SND_ASYNC );

                PlaySound( path, IntPtr.Zero, flags );
            }
            catch
            {
                // Ignore sound playback failures
            }
        }

        private static void PlayStream( Stream stream )
        {
            if ( stream == null || !OperatingSystem.IsWindows() )
            {
                return;
            }

            try
            {
                using ( MemoryStream memoryStream = new MemoryStream() )
                {
                    stream.CopyTo( memoryStream );

                    // SND_SYNC keeps the buffer alive for the duration of playback
                    PlaySound( memoryStream.ToArray(), IntPtr.Zero, SND_MEMORY | SND_NODEFAULT | SND_SYNC );
                }
            }
            catch
            {
                // Ignore sound playback failures
            }
        }

        [DllImport( "winmm.dll", SetLastError = true, CharSet = CharSet.Unicode )]
        private static extern bool PlaySound( string pszSound, IntPtr hmod, uint fdwSound );

        [DllImport( "winmm.dll", SetLastError = true, EntryPoint = "PlaySoundW" )]
        private static extern bool PlaySound( byte[] pszSound, IntPtr hmod, uint fdwSound );
    }
}
