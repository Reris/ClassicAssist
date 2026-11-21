// Copyright (C) 2024 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.UI.Views.ECV.Settings.Models;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.UI.Views.ECV.Settings
{
    /// <summary>
    ///     Interaction logic for OpenContainersSettingsControl.axaml
    /// </summary>
    public partial class OpenContainersSettingsControl : UserControl
    {
        public static readonly Avalonia.StyledProperty<ObservableCollection<CombineStacksOpenContainersIgnoreEntry>> ItemsProperty =
        AvaloniaCompat.RegisterCompat<OpenContainersSettingsControl, ObservableCollection<CombineStacksOpenContainersIgnoreEntry>>(nameof( Items ), default( ObservableCollection<CombineStacksOpenContainersIgnoreEntry> ), onChanged: PropertyChangedCallback);

        public OpenContainersSettingsControl()
        {
            InitializeComponent();
        }

        public ObservableCollection<CombineStacksOpenContainersIgnoreEntry> Items
        {
            get => (ObservableCollection<CombineStacksOpenContainersIgnoreEntry>) GetValue( ItemsProperty );
            set => SetValue( ItemsProperty, value );
        }

        private static void PropertyChangedCallback( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( !( d is OpenContainersSettingsControl control ) )
            {
                return;
            }

            if ( control.DataContext is OpenContainersSettingsViewModel vm )
            {
                vm.Items = (ObservableCollection<CombineStacksOpenContainersIgnoreEntry>) e.NewValue;
            }
        }
    }
}