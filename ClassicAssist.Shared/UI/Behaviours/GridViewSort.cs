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
using System.ComponentModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Shared.UI.Behaviours
{
    /// <summary>
    ///     Sorts a list when a header/button is clicked. Avalonia port of the WPF GridView
    ///     column-sort behaviour - works against DataGridCollectionView.SortDescriptions.
    /// </summary>
    public class GridViewSort : Behavior<ListBox>
    {
        public static readonly Avalonia.StyledProperty<Type> ComparerTypeProperty =
        AvaloniaCompat.RegisterAttachedCompat<GridViewSort, Type>(nameof( ComparerType ), null);

        private ListSortDirection _lastDirection;

        private string _lastProperty;

        public Type ComparerType
        {
            get => (Type) GetValue( ComparerTypeProperty );
            set => SetValue( ComparerTypeProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AddHandler( Button.ClickEvent, ColumnHeader_Click );
        }

        private void ColumnHeader_Click( object sender, RoutedEventArgs e )
        {
            if ( !( e.Source is Control clicked ) )
            {
                return;
            }

            string propertyName = clicked.Tag as string ?? clicked.Name;

            if ( string.IsNullOrEmpty( propertyName ) )
            {
                return;
            }

            DataGridCollectionView view = CollectionViewSource.GetDefaultView( AssociatedObject.ItemsSource );

            if ( view == null )
            {
                return;
            }

            ListSortDirection direction;

            if ( _lastProperty != propertyName )
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                direction = _lastDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            using ( view.DeferRefresh() )
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add( DataGridSortDescription.FromPath( propertyName, direction ) );
            }

            _lastProperty = propertyName;
            _lastDirection = direction;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler( Button.ClickEvent, ColumnHeader_Click );
            base.OnDetaching();
        }
    }
}