using System;
using System.Runtime.InteropServices;
using Avalonia.Input;

namespace ClassicAssist.Misc
{
    /// <summary>
    ///     Physical modifier key state. Uses the Win32 async key state on Windows
    ///     so hotkeys work regardless of window focus; other platforms currently
    ///     report no pressed keys.
    /// </summary>
    public static class KeyboardState
    {
        public static bool IsKeyDown( Key key )
        {
            if ( !OperatingSystem.IsWindows() )
            {
                return false;
            }

            int virtualKey = GetVirtualKey( key );

            if ( virtualKey == 0 )
            {
                return false;
            }

            return ( GetAsyncKeyState( virtualKey ) & 0x8000 ) != 0;
        }

        private static int GetVirtualKey( Key key )
        {
            switch ( key )
            {
                case Key.LeftCtrl: return 0xA2; // VK_LCONTROL
                case Key.RightCtrl: return 0xA3; // VK_RCONTROL
                case Key.LeftShift: return 0xA0; // VK_LSHIFT
                case Key.RightShift: return 0xA1; // VK_RSHIFT
                case Key.LeftAlt: return 0xA4; // VK_LMENU
                case Key.RightAlt: return 0xA5; // VK_RMENU
                default: return 0;
            }
        }

        [DllImport( "user32.dll" )]
        private static extern short GetAsyncKeyState( int vKey );
    }
}
