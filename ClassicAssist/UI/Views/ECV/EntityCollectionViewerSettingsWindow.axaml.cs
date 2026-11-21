// Copyright (C) 2024 Reetus
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
using ClassicAssist.Data.Misc;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.UI.Views.ECV
{
    /// <summary>
    ///     Interaction logic for EntityCollectionViewerSettingsWindow.xaml
    /// </summary>
    public partial class EntityCollectionViewerSettingsWindow : Window
    {
        public static readonly Avalonia.StyledProperty<EntityCollectionViewerOptions> OptionsProperty =
        AvaloniaCompat.RegisterCompat<EntityCollectionViewerSettingsWindow, EntityCollectionViewerOptions>(nameof( Options ), default( EntityCollectionViewerOptions ), onChanged: PropertyChangedCallback);

        public EntityCollectionViewerSettingsWindow()
        {
            InitializeComponent();
        }

        public EntityCollectionViewerOptions Options
        {
            get => (EntityCollectionViewerOptions) GetValue( OptionsProperty );
            set => SetValue( OptionsProperty, value );
        }

        private static void PropertyChangedCallback( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( !( d is EntityCollectionViewerSettingsWindow window ) )
            {
                return;
            }

            if ( e.NewValue is EntityCollectionViewerOptions options )
            {
                window.DataContext = new EntityCollectionViewerSettingsViewModel { Options = options };
            }
        }
    }
}