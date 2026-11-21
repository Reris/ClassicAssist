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
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for TwoColumnSplitterControl.xaml
    /// </summary>
    public partial class TwoColumnSplitterControl : UserControl
    {
        public static readonly Avalonia.StyledProperty<object> LeftContentProperty =
        AvaloniaCompat.RegisterCompat<TwoColumnSplitterControl, object>(nameof( LeftContent ), default);

        public static readonly Avalonia.StyledProperty<object> RightContentProperty =
        AvaloniaCompat.RegisterCompat<TwoColumnSplitterControl, object>(nameof( RightContent ), default);

        public static readonly Avalonia.StyledProperty<GridLength> LeftContentWidthProperty =
        AvaloniaCompat.RegisterCompat<TwoColumnSplitterControl, GridLength>(nameof( LeftContentWidth ), new GridLength( 1, GridUnitType.Star ));

        public TwoColumnSplitterControl()
        {
            InitializeComponent();
        }

        public object LeftContent
        {
            get => GetValue( LeftContentProperty );
            set => SetValue( LeftContentProperty, value );
        }

        public GridLength LeftContentWidth
        {
            get => (GridLength) GetValue( LeftContentWidthProperty );
            set => SetValue( LeftContentWidthProperty, value );
        }

        public object RightContent
        {
            get => GetValue( RightContentProperty );
            set => SetValue( RightContentProperty, value );
        }
    }
}