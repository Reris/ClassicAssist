// Copyright (C) 2024 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;


namespace ClassicAssist.UI.Views.Agents.Autoloot.Import
{
    /// <summary>
    ///     Interaction logic for CSVImportWindow.xaml
    /// </summary>
    public partial class CSVImportWindow : Window
    {
        private const string CSV_IMPORT_WIKI_URL = "https://github.com/Reetus/ClassicAssist/wiki/Importing-Autoloot-Items";

        public CSVImportWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnClick( object sender, RoutedEventArgs e )
        {
            Process.Start( new ProcessStartInfo( CSV_IMPORT_WIKI_URL ) { UseShellExecute = true } );
        }
    }
}