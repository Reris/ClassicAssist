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
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Shared.UI.Behaviours
{
    /// <summary>
    ///     Sorts a DataGrid when a column header is clicked, using the <see cref="IComparer" /> type
    ///     supplied in <see cref="ComparerType" /> (constructed with ( ListSortDirection, string )).
    ///     The comparer property name is taken from the column's SortMemberPath. The bound collection
    ///     is sorted in place using Move so bindings and selection survive.
    ///     Avalonia port of the WPF GridView column-sort behaviour.
    /// </summary>
    public class GridViewSort : Behavior<DataGrid>
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

            AssociatedObject.Sorting += OnSorting;
        }

        private void OnSorting( object sender, DataGridColumnEventArgs e )
        {
            if ( ComparerType == null )
            {
                return;
            }

            string propertyName = e.Column.SortMemberPath ?? e.Column.Header?.ToString();

            if ( string.IsNullOrEmpty( propertyName ) )
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

            if ( !( Activator.CreateInstance( ComparerType, direction, propertyName ) is IComparer comparer ) )
            {
                return;
            }

            e.Handled = true;

            SortInPlace( AssociatedObject.ItemsSource as IList, comparer );

            _lastProperty = propertyName;
            _lastDirection = direction;
        }

        private static void SortInPlace( IList list, IComparer comparer )
        {
            if ( list == null )
            {
                return;
            }

            // Prefer ObservableCollection.Move - raises a Move change action, preserving selection
            MethodInfo moveMethod = list.GetType().GetMethod( nameof( ObservableCollection<object>.Move ),
                new[] { typeof( int ), typeof( int ) } );

            for ( int i = 1; i < list.Count; i++ )
            {
                for ( int j = i; j > 0 && comparer.Compare( list[j - 1], list[j] ) > 0; j-- )
                {
                    if ( moveMethod != null )
                    {
                        moveMethod.Invoke( list, new object[] { j, j - 1 } );
                    }
                    else
                    {
                        object item = list[j];
                        list.RemoveAt( j );
                        list.Insert( j - 1, item );
                    }
                }
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Sorting -= OnSorting;
            base.OnDetaching();
        }
    }
}