#region License

// Copyright (C) 2023 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Controls.Headered
{
    public class HorizontalHeaderedComboBox : HorizontalHeaderedContentControl
    {
        public static readonly Avalonia.StyledProperty<object> SelectedItemProperty =
        AvaloniaCompat.RegisterCompat<HorizontalHeaderedComboBox, object>(nameof( SelectedItem ), default, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly Avalonia.StyledProperty<IEnumerable> ItemsSourceProperty =
        AvaloniaCompat.RegisterCompat<HorizontalHeaderedComboBox, IEnumerable>(nameof( ItemsSource ), default( IEnumerable ));

        public HorizontalHeaderedComboBox()
        {
            ComboBox comboBox = new ComboBox();
            Binding binding = new Binding( "SelectedItem" ) { Source = this, Mode = BindingMode.TwoWay };
            Binding itemsSourceBinding = new Binding( "ItemsSource" ) { Source = this };
            comboBox[!ItemsControl.ItemsSourceProperty] = itemsSourceBinding;
            comboBox[!SelectingItemsControl.SelectedItemProperty] = binding;
            Content = comboBox;
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable) GetValue( ItemsSourceProperty );
            set => SetValue( ItemsSourceProperty, value );
        }

        public object SelectedItem
        {
            get => GetValue( SelectedItemProperty );
            set => SetValue( SelectedItemProperty, value );
        }
    }
}