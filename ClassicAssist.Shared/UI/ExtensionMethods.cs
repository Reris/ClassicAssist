using Avalonia;
using Avalonia.VisualTree;

namespace ClassicAssist.Shared.UI
{
    public static class ExtensionMethods
    {
        public static T GetChildOfType<T>( this AvaloniaObject depObj ) where T : AvaloniaObject
        {
            if ( depObj == null )
            {
                return null;
            }

            foreach ( Visual child in ((Visual) depObj).GetVisualChildren() )
            {
                T result = child as T ?? GetChildOfType<T>( child );

                if ( result != null )
                {
                    return result;
                }
            }

            return null;
        }
    }
}