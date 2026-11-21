using System.Collections;
using Avalonia.Collections;

namespace ClassicAssist.Shared.UI.Compat
{
    /// <summary>
    ///     Compatibility shim for WPF CollectionViewSource. Returns a DataGridCollectionView
    ///     (Avalonia's ICollectionView equivalent) around the given source collection.
    /// </summary>
    public static class CollectionViewSource
    {
        public static DataGridCollectionView GetDefaultView( object source )
        {
            switch ( source )
            {
                case null:
                    return null;

                case DataGridCollectionView view:
                    return view;

                case IEnumerable enumerable:
                    return new DataGridCollectionView( enumerable );

                default:
                    return null;
            }
        }
    }
}