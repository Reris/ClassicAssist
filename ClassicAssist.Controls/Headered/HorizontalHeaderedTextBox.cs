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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Controls.Headered
{
    public class HorizontalHeaderedTextBox : HorizontalHeaderedContentControl
    {
        public static readonly Avalonia.StyledProperty<string> TextProperty =
        AvaloniaCompat.RegisterCompat<HorizontalHeaderedTextBox, string>(nameof( Text ), default( string ), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public HorizontalHeaderedTextBox()
        {
            TextBox textBox = new TextBox();
            Binding binding = new Binding( "Text" )
            {
                Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            textBox[!TextBox.TextProperty] = binding;
            Content = textBox;
        }

        public string Text
        {
            get => (string) GetValue( TextProperty );
            set => SetValue( TextProperty, value );
        }
    }
}