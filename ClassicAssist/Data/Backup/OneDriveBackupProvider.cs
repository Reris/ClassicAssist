#region License

// Copyright (C) 2021 Reetus
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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using ClassicAssist.Controls.VirtualFolderBrowse;
using ClassicAssist.Data.Backup.OneDrive;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json.Linq;
using File = System.IO.File;
using UserControl = Avalonia.Controls.UserControl;
using ClassicAssist.Shared.UI.Compat;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Backup
{
    public sealed class OneDriveBackupProvider : BaseBackupProvider
    {
        private byte[] _authentication;

        public byte[] Authentication
        {
            get => _authentication;
            set => SetProperty( ref _authentication, value );
        }

        public override UserControl LoginControl => new OneDriveConfigureControl();

        public override string Name => "Microsoft OneDrive";

        public override async Task<bool> Write( string fileName )
        {
            GraphServiceClient client = await OneDriveClient.GetGraphClient();

            Drive drive = await client.Me.Drive.GetAsync();

            using ( FileStream fileStream = File.OpenRead( fileName ) )
            {
                DriveItem result = await client.Drives[drive.Id].Items[BackupPath]
                    .ItemWithPath( Path.GetFileName( fileName ) ).Content.PutAsync( fileStream );

                return result != null;
            }
        }

        public override async Task<string> GetPath( string currentPath )
        {
            VirtualFolderBrowseViewModel odppvm = new VirtualFolderBrowseViewModel( GetChildren, CreateFolder );

            VirtualFolderBrowseWindow window = new VirtualFolderBrowseWindow { DataContext = odppvm };

            await window.ShowDialogAsync();

            if ( odppvm.DialogResult != DialogResult.OK )
            {
                return await Task.FromResult( currentPath );
            }

            /* Set FirstRun if changing the folder */
            FirstRun = true;
            return await Task.FromResult( odppvm.SelectedItem.Id );
        }

        public override void Serialize( JObject json, bool _ = false )
        {
            base.Serialize( json, _ );

            if ( Authentication != null )
            {
                json?.Add( "Authentication",
                    Convert.ToBase64String( ProtectedData.Protect( Authentication, null,
                        DataProtectionScope.CurrentUser ) ) );
            }
        }

        public override void Deserialize( JObject json, Options options, bool _ = false )
        {
            base.Deserialize( json, options );

            if ( json == null )
            {
                return;
            }

            try
            {
                if ( json["Authentication"] != null )
                {
                    Authentication = ProtectedData.Unprotect(
                        Convert.FromBase64String( json["Authentication"].ToObject<string>() ?? string.Empty ), null,
                        DataProtectionScope.CurrentUser );
                }
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private static async Task<VirtualFolder> CreateFolder( VirtualFolder parent, string folder )
        {
            GraphServiceClient graphClient = await OneDriveClient.GetGraphClient();

            Drive drive = await graphClient.Me.Drive.GetAsync();

            DriveItem newFolder = new DriveItem { Name = folder, Folder = new Folder() };

            DriveItem result = parent == null
                ? await graphClient.Drives[drive.Id].Items["root"].Children.PostAsync( newFolder )
                : await graphClient.Drives[drive.Id].Items[parent.Id].Children.PostAsync( newFolder );

            return result != null
                ? new VirtualFolder { Id = result.Id, Name = result.Name, ContainsChildren = false }
                : null;
        }

        private static async Task<IEnumerable<VirtualFolder>> GetChildren( string id )
        {
            GraphServiceClient client = await OneDriveClient.GetGraphClient();

            Drive drive = await client.Me.Drive.GetAsync();

            DriveItemCollectionResponse files;

            if ( string.IsNullOrEmpty( id ) )
            {
                files = await client.Drives[drive.Id].Items["root"].Children.GetAsync();
            }
            else
            {
                files = await client.Drives[drive.Id].Items[id].Children.GetAsync();
            }

            if ( files?.Value == null )
            {
                return Enumerable.Empty<VirtualFolder>();
            }

            return files.Value.Where( driveItem => driveItem.Folder != null ).Select( driveItem => new VirtualFolder
            {
                Id = driveItem.Id, Name = driveItem.Name, ContainsChildren = driveItem.Folder?.ChildCount > 0
            } );
        }
    }
}
