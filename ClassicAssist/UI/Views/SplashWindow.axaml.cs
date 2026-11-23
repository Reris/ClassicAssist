// Copyright (C) $CURRENT_YEAR$ Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ClassicAssist.Misc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ClassicAssist.UI.Views;

/// <summary>
///     Interaction logic for SplashWindow.axaml
/// </summary>
public partial class SplashWindow : Window
{
    private readonly Action _mainAction;
    private readonly Task _loading;
    public IImage Image { get; }

    public SplashWindow( Action mainAction, Task loading )
    {
        _mainAction = mainAction;
        _loading = loading;
        Image = Properties.Resources.splash_logo.ToImageSource();

        InitializeComponent();
    }

    [SuppressMessage( "ReSharper", "AsyncVoidEventHandlerMethod" )]
    protected override async void OnLoaded( RoutedEventArgs e )
    {
        await _loading;
        await Dispatcher.InvokeAsync( () =>
        {
            _mainAction?.Invoke();
            Close();
        } );
    }
}