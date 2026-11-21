#region License

// Copyright (C) 2026 Reetus
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace ClassicAssist.Shared.UI
{
    /// <summary>
    ///     Cross-platform file/folder dialogs (StorageProvider) and modal window
    ///     helpers replacing the WinForms/WPF dialog classes.
    /// </summary>
    public static class DialogHelper
    {
        public static Window GetActiveWindow()
        {
            if ( Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                return desktop.MainWindow;
            }

            return null;
        }

        public static TopLevel GetTopLevel()
        {
            return GetActiveWindow();
        }

        /// <summary>
        ///     Parses a WPF style filter string ("Name (*.ext)|*.ext|Name2|*.a;*.b")
        ///     into Avalonia FilePickerFileType choices.
        /// </summary>
        internal static List<FilePickerFileType> ParseFilter( string filter )
        {
            List<FilePickerFileType> results = new List<FilePickerFileType>();

            if ( string.IsNullOrEmpty( filter ) )
            {
                return results;
            }

            string[] parts = filter.Split( '|' );

            for ( int i = 0; i + 1 < parts.Length; i += 2 )
            {
                string name = parts[i];
                string patterns = parts[i + 1];

                List<string> extensions = patterns.Split( ';' )
                    .Select( p => p.Trim().TrimStart( '*' ) )
                    .Where( p => p.StartsWith( "." ) )
                    .ToList();

                results.Add( new FilePickerFileType( name ) { Patterns = extensions } );
            }

            return results;
        }

        public static async Task<string[]> ShowOpenFileDialogAsync( string title = null, string filter = null,
            string initialDirectory = null, bool multiselect = false )
        {
            TopLevel topLevel = GetTopLevel();

            if ( topLevel?.StorageProvider == null )
            {
                return Array.Empty<string>();
            }

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = multiselect,
                FileTypeFilter = ParseFilter( filter )
            };

            if ( !string.IsNullOrEmpty( initialDirectory ) )
            {
                try
                {
                    options.SuggestedStartLocation =
                        await topLevel.StorageProvider.TryGetFolderFromPathAsync( initialDirectory );
                }
                catch
                {
                    // ignore invalid initial directories
                }
            }

            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync( options );

            return files.Select( f => f.Path.LocalPath ).ToArray();
        }

        public static async Task<string> ShowSaveFileDialogAsync( string title = null, string filter = null,
            string fileName = null, string initialDirectory = null )
        {
            TopLevel topLevel = GetTopLevel();

            if ( topLevel?.StorageProvider == null )
            {
                return null;
            }

            var options = new FilePickerSaveOptions { Title = title, SuggestedFileName = fileName };

            List<FilePickerFileType> types = ParseFilter( filter );

            if ( types.Count > 0 )
            {
                options.FileTypeChoices = types;
            }

            if ( !string.IsNullOrEmpty( initialDirectory ) )
            {
                try
                {
                    options.SuggestedStartLocation =
                        await topLevel.StorageProvider.TryGetFolderFromPathAsync( initialDirectory );
                }
                catch
                {
                    // ignore invalid initial directories
                }
            }

            IStorageFile file = await topLevel.StorageProvider.SaveFilePickerAsync( options );

            return file?.Path.LocalPath;
        }

        public static async Task<string> ShowFolderBrowserDialogAsync( string description = null,
            string initialDirectory = null )
        {
            TopLevel topLevel = GetTopLevel();

            if ( topLevel?.StorageProvider == null )
            {
                return null;
            }

            var options = new FolderPickerOpenOptions { Title = description, AllowMultiple = false };

            if ( !string.IsNullOrEmpty( initialDirectory ) )
            {
                try
                {
                    options.SuggestedStartLocation =
                        await topLevel.StorageProvider.TryGetFolderFromPathAsync( initialDirectory );
                }
                catch
                {
                    // ignore invalid initial directories
                }
            }

            IReadOnlyList<IStorageFolder> folders = await topLevel.StorageProvider.OpenFolderPickerAsync( options );

            return folders.Count > 0 ? folders[0].Path.LocalPath : null;
        }
    }

    public static class WindowDialogExtensions
    {
        /// <summary>
        ///     Fire-and-forget modal dialog, mirroring the WPF Window.ShowDialog() usage
        ///     where the result is not consumed.
        /// </summary>
        public static void ShowDialogCompat( this Window window )
        {
            Dispatcher.UIThread.Post( async () =>
            {
                try
                {
                    Window owner = DialogHelper.GetActiveWindow();

                    if ( owner == null || owner == window )
                    {
                        window.Show();
                    }
                    else
                    {
                        await window.ShowDialog( owner );
                    }
                }
                catch
                {
                    // ignore dialog failures
                }
            } );
        }

        public static Task ShowDialogAsync( this Window window )
        {
            Window owner = DialogHelper.GetActiveWindow();

            if ( owner == null || owner == window )
            {
                window.Show();

                return Task.CompletedTask;
            }

            return window.ShowDialog( owner );
        }

        public static Task<TResult> ShowDialogCompat<TResult>( this Window window )
        {
            Window owner = DialogHelper.GetActiveWindow();

            return owner == null || owner == window
                ? Task.FromResult( default( TResult ) )
                : window.ShowDialog<TResult>( owner );
        }
    }
}
