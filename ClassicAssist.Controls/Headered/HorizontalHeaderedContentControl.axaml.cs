// Copyright (C) 2023 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Controls.Headered
{
    /// <summary>
    ///     Interaction logic for HorizontalHeaderedContentControl.axaml
    /// </summary>
    public partial class HorizontalHeaderedContentControl : HeaderedContentControl
    {
        public static readonly Avalonia.StyledProperty<Thickness> ChildMarginProperty =
        AvaloniaCompat.RegisterCompat<HorizontalHeaderedContentControl, Thickness>(nameof( ChildMargin ), new Thickness( 0, 0, 5, 0 ));

        public static readonly Avalonia.StyledProperty<double> HeaderWidthProperty =
        AvaloniaCompat.RegisterCompat<HorizontalHeaderedContentControl, double>(nameof( HeaderWidth ), double.NaN);

        public static readonly Avalonia.StyledProperty<double> HeaderMinWidthProperty =
        AvaloniaCompat.RegisterCompat<HorizontalHeaderedContentControl, double>(nameof( HeaderMinWidth ), double.NaN);

        public HorizontalHeaderedContentControl()
        {
            InitializeComponent();
        }

        public Thickness ChildMargin
        {
            get => (Thickness) GetValue( ChildMarginProperty );
            set => SetValue( ChildMarginProperty, value );
        }

        public double HeaderMinWidth
        {
            get => (double) GetValue( HeaderMinWidthProperty );
            set => SetValue( HeaderMinWidthProperty, value );
        }

        public double HeaderWidth
        {
            get => (double) GetValue( HeaderWidthProperty );
            set => SetValue( HeaderWidthProperty, value );
        }
    }
}