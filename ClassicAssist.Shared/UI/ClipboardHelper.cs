using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace ClassicAssist.Shared.UI
{
    public static class ClipboardHelper
    {
        public static void SetText( string text )
        {
            _ = SetTextAsync( text );
        }

        public static async Task SetTextAsync( string text )
        {
            try
            {
                IClipboard clipboard = GetClipboard();

                if ( clipboard != null )
                {
                    await clipboard.SetTextAsync( text );
                }
            }
            catch
            {
                // Clipboard may be unavailable or busy, ignore
            }
        }

        public static async Task<string> GetTextAsync()
        {
            try
            {
                IClipboard clipboard = GetClipboard();

                if ( clipboard == null )
                {
                    return string.Empty;
                }

                return await clipboard.GetTextAsync() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static IClipboard GetClipboard()
        {
            TopLevel topLevel = DialogHelper.GetTopLevel();

            return topLevel?.Clipboard;
        }
    }
}
