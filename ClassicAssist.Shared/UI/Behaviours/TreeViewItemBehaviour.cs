#region License

// Copyright (C) 2021 Reetus
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Shared.UI.Behaviours
{
    //https://www.codeproject.com/Articles/28959/Introduction-to-Attached-Behaviors-in-WPF
    /// <summary>
    ///     Exposes attached behaviors that can be
    ///     applied to TreeViewItem objects.
    /// </summary>
    public static class TreeViewItemBehavior
    {
        static TreeViewItemBehavior()
        {
            IsBroughtIntoViewWhenSelectedProperty.Changed.AddClassHandler<AvaloniaObject>( OnIsBroughtIntoViewWhenSelectedChanged );
        }

        #region IsBroughtIntoViewWhenSelected

        public static bool GetIsBroughtIntoViewWhenSelected( TreeViewItem treeViewItem )
        {
            return (bool) treeViewItem.GetValue( IsBroughtIntoViewWhenSelectedProperty );
        }

        public static void SetIsBroughtIntoViewWhenSelected( TreeViewItem treeViewItem, bool value )
        {
            treeViewItem.SetValue( IsBroughtIntoViewWhenSelectedProperty, value );
        }

        public static readonly Avalonia.StyledProperty<bool> IsBroughtIntoViewWhenSelectedProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>( "IsBroughtIntoViewWhenSelected", typeof( TreeViewItemBehavior ), false );

        private static void OnIsBroughtIntoViewWhenSelectedChanged( AvaloniaObject depObj,
            AvaloniaPropertyChangedEventArgs e )
        {
            TreeViewItem item = depObj as TreeViewItem;

            if ( item == null )
            {
                return;
            }

            if ( e.NewValue is bool == false )
            {
                return;
            }

            if ( (bool) e.NewValue )
            {
                item.PropertyChanged += OnTreeViewItemPropertyChanged;
            }
            else
            {
                item.PropertyChanged -= OnTreeViewItemPropertyChanged;
            }
        }

        private static void OnTreeViewItemPropertyChanged( object sender, AvaloniaPropertyChangedEventArgs e )
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if ( e.Property != TreeViewItem.IsSelectedProperty || !(bool) e.NewValue )
            {
                return;
            }

            if ( sender is TreeViewItem item )
            {
                item.BringIntoView();
            }
        }

        #endregion // IsBroughtIntoViewWhenSelected
    }
}