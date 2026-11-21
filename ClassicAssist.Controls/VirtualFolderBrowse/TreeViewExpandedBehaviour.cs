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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.Controls.VirtualFolderBrowse
{
    public class TreeViewExpandedBehaviour : Behavior<TreeView>
    {
        public static readonly Avalonia.StyledProperty<ICommand> OnExpandedActionProperty =
        AvaloniaCompat.RegisterCompat<TreeViewExpandedBehaviour, ICommand>(nameof( OnExpandedAction ), default);

        public static readonly Avalonia.StyledProperty<object> SelectedItemProperty =
        AvaloniaCompat.RegisterCompat<TreeViewExpandedBehaviour, object>(nameof( SelectedItem ), default, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public ICommand OnExpandedAction
        {
            get => (ICommand) GetValue( OnExpandedActionProperty );
            set => SetValue( OnExpandedActionProperty, value );
        }

        public object SelectedItem
        {
            get => GetValue( SelectedItemProperty );
            set => SetValue( SelectedItemProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            // ExpandedEvent bubbles from nested TreeViewItems up to the TreeView, so a single
            // handler here covers every level of the hierarchy.
            AssociatedObject.AddHandler( TreeViewItem.ExpandedEvent, TreeViewItemOnExpanded );
            AssociatedObject.SelectionChanged += SelectedItemChanged;
        }

        private void SelectedItemChanged( object sender, SelectionChangedEventArgs e )
        {
            SelectedItem = ( sender as TreeView )?.SelectedItem;
        }

        private void TreeViewItemOnExpanded( object sender, RoutedEventArgs routedEventArgs )
        {
            if ( !( sender is TreeViewItem treeViewItem ) )
            {
                return;
            }

            OnExpandedAction?.Execute( treeViewItem.Header ?? treeViewItem.DataContext );

            routedEventArgs.Handled = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.RemoveHandler( TreeViewItem.ExpandedEvent, TreeViewItemOnExpanded );
            AssociatedObject.SelectionChanged -= SelectedItemChanged;
        }
    }
}