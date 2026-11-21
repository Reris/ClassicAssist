#region License

// Copyright (C) 2025 Reetus
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
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class ItemsControlSelectNewEntryBehaviour : Behavior<ItemsControl>
    {
        public static readonly Avalonia.StyledProperty<object> SelectedItemProperty =
        AvaloniaCompat.RegisterCompat<ItemsControlSelectNewEntryBehaviour, object>(nameof( SelectedItem ), null, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<Type> TypeProperty =
        AvaloniaCompat.RegisterCompat<ItemsControlSelectNewEntryBehaviour, Type>(nameof( Type ), null, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public object SelectedItem
        {
            get => GetValue( SelectedItemProperty );
            set => SetValue( SelectedItemProperty, value );
        }

        public Type Type
        {
            get => (Type) GetValue( TypeProperty );
            set => SetValue( TypeProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            ( (INotifyCollectionChanged) AssociatedObject.Items ).CollectionChanged += OnCollectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            ( (INotifyCollectionChanged) AssociatedObject.Items ).CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( !( sender is ItemCollection ) )
            {
                return;
            }

            if ( e.NewItems == null )
            {
                return;
            }

            object firstItem = e.NewItems[0];

            if ( Type != null )
            {
                if ( firstItem.GetType() != Type )
                {
                    return;
                }
            }

            SelectedItem = firstItem;
        }
    }
}