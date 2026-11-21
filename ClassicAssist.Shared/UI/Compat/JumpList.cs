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
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;

namespace ClassicAssist.Shared.UI.Compat
{
    /// <summary>
    ///     Compatibility shim for the WPF <c>System.Windows.Shell.JumpList</c>. On Windows the list is
    ///     applied through the shell <c>ICustomDestinationList</c> API, on other platforms Apply is a no-op.
    /// </summary>
    public class JumpList
    {
        private const string CLSID_DESTINATION_LIST = "77f10cf0-3db5-4966-b520-b7c54fd35ed6";
        private const string CLSID_ENUMERABLE_OBJECT_COLLECTION = "2d3468c1-36a7-43b6-ac24-d3f02fd9607a";
        private const string CLSID_SHELL_LINK = "00021401-0000-0000-C000-000000000046";
        private const string IID_OBJECT_ARRAY = "92CA9DCD-5622-4bba-A805-5E9F541BD8C9";
        private const string IID_UNKNOWN = "00000000-0000-0000-C000-000000000046";
        private const int MAX_PATH = 260;
        private const int INFOTIPSIZE = 1024;
        private const uint SLGP_RAWPATH = 0x4;

        private static JumpList _current;

        /// <summary>
        ///     Whether to show the special "Frequent" category, managed by the shell.
        /// </summary>
        public bool ShowFrequentCategory { get; set; }

        /// <summary>
        ///     Whether to show the special "Recent" category, managed by the shell.
        /// </summary>
        public bool ShowRecentCategory { get; set; }

        /// <summary>
        ///     The items to be shown in the jump list.
        /// </summary>
        public List<JumpItem> JumpItems { get; } = new List<JumpItem>();

        /// <summary>
        ///     Register <paramref name="value" /> as the jump list for <paramref name="application" />.
        /// </summary>
        public static void SetJumpList( Application application, JumpList value )
        {
            _current = value;
        }

        /// <summary>
        ///     Get the jump list registered for <paramref name="application" />.
        /// </summary>
        public static JumpList GetJumpList( Application application )
        {
            return _current;
        }

        /// <summary>
        ///     Apply the jump list in its current state to the shell. No-op on non-Windows platforms.
        /// </summary>
        public void Apply()
        {
            if ( !OperatingSystem.IsWindows() )
            {
                return;
            }

            try
            {
                ApplyToShell();
            }
            catch
            {
                // The shell can reject the list for any number of reasons, failures are never
                // propagated into the caller's path (mirrors the WPF behaviour).
            }
        }

        private void ApplyToShell()
        {
            // Group the items into categories, the unnamed "Tasks" category is kept last.
            List<List<JumpItem>> categories = new List<List<JumpItem>> { new List<JumpItem>() };

            foreach ( JumpItem jumpItem in JumpItems )
            {
                if ( jumpItem == null )
                {
                    continue;
                }

                if ( string.IsNullOrEmpty( jumpItem.CustomCategory ) )
                {
                    categories[0].Add( jumpItem );
                    continue;
                }

                List<JumpItem> category = categories.Find( c => c.Count > 0 && c[0].CustomCategory == jumpItem.CustomCategory );

                if ( category == null )
                {
                    categories.Add( new List<JumpItem> { jumpItem } );
                }
                else
                {
                    category.Add( jumpItem );
                }
            }

            List<object> comObjects = new List<object>();
            ICustomDestinationList destinationList = (ICustomDestinationList)Activator.CreateInstance( Type.GetTypeFromCLSID( new Guid( CLSID_DESTINATION_LIST ) ) );

            try
            {
                comObjects.Add( destinationList );

                string appId = GetRuntimeAppId();

                if ( !string.IsNullOrEmpty( appId ) )
                {
                    destinationList.SetAppID( appId );
                }

                uint slots;
                Guid objectArrayIid = new Guid( IID_OBJECT_ARRAY );
                IObjectArray removedItems = destinationList.BeginList( out slots, ref objectArrayIid );
                comObjects.Add( removedItems );

                List<string> removedLinks = GetRemovedLinkSignatures( removedItems );

                // Categories are added bottom-up so they display top-down in the order they were first
                // seen, the known Recent/Frequent categories are added first so they stay on top.
                categories.Reverse();

                if ( ShowFrequentCategory )
                {
                    destinationList.AppendKnownCategory( KDC.FREQUENT );
                }

                if ( ShowRecentCategory )
                {
                    destinationList.AppendKnownCategory( KDC.RECENT );
                }

                foreach ( List<JumpItem> category in categories )
                {
                    if ( category.Count == 0 )
                    {
                        continue;
                    }

                    IObjectCollection collection = (IObjectCollection)Activator.CreateInstance( Type.GetTypeFromCLSID( new Guid( CLSID_ENUMERABLE_OBJECT_COLLECTION ) ) );
                    comObjects.Add( collection );

                    foreach ( JumpItem jumpItem in category )
                    {
                        JumpTask task = jumpItem as JumpTask;

                        if ( task == null )
                        {
                            continue;
                        }

                        IShellLinkW link = CreateLinkFromJumpTask( task );

                        if ( link == null )
                        {
                            continue;
                        }

                        comObjects.Add( link );

                        // Skip items the user has explicitly removed from the list.
                        if ( !removedLinks.Contains( ShellLinkToString( link ) ) )
                        {
                            collection.AddObject( link );
                        }
                    }

                    string categoryHeader = category[0].CustomCategory;

                    if ( string.IsNullOrEmpty( categoryHeader ) )
                    {
                        destinationList.AddUserTasks( collection );
                    }
                    else
                    {
                        destinationList.AppendCategory( categoryHeader, collection );
                    }
                }

                destinationList.CommitList();
            }
            finally
            {
                foreach ( object comObject in comObjects )
                {
                    Marshal.FinalReleaseComObject( comObject );
                }
            }
        }

        private static IShellLinkW CreateLinkFromJumpTask( JumpTask jumpTask )
        {
            if ( string.IsNullOrEmpty( jumpTask.Title ) )
            {
                return null;
            }

            string appPath = string.IsNullOrEmpty( jumpTask.ApplicationPath ) ? GetExecutablePath() : jumpTask.ApplicationPath;

            if ( string.IsNullOrEmpty( appPath ) )
            {
                return null;
            }

            IShellLinkW link = (IShellLinkW)Activator.CreateInstance( Type.GetTypeFromCLSID( new Guid( CLSID_SHELL_LINK ) ) );

            try
            {
                link.SetPath( appPath );

                if ( !string.IsNullOrEmpty( jumpTask.WorkingDirectory ) )
                {
                    link.SetWorkingDirectory( jumpTask.WorkingDirectory );
                }

                if ( !string.IsNullOrEmpty( jumpTask.Arguments ) )
                {
                    link.SetArguments( jumpTask.Arguments );
                }

                // -1 is a sentinel value indicating not to use the icon.
                if ( jumpTask.IconResourceIndex != -1 )
                {
                    string resourcePath = string.IsNullOrEmpty( jumpTask.IconResourcePath ) ? appPath : jumpTask.IconResourcePath;

                    link.SetIconLocation( resourcePath, jumpTask.IconResourceIndex );
                }

                if ( !string.IsNullOrEmpty( jumpTask.Description ) )
                {
                    link.SetDescription( jumpTask.Description );
                }

                // The shell displays the task title from this property, without it the link path is shown.
                IPropertyStore propertyStore = (IPropertyStore)link;
                PKEY pkeyTitle = PKEY.TITLE;
                PropVariant title = PropVariant.FromString( jumpTask.Title );

                try
                {
                    propertyStore.SetValue( ref pkeyTitle, ref title );
                }
                finally
                {
                    title.Clear();
                }

                propertyStore.Commit();

                return link;
            }
            catch
            {
                Marshal.FinalReleaseComObject( link );
                return null;
            }
        }

        private static List<string> GetRemovedLinkSignatures( IObjectArray removedItems )
        {
            List<string> signatures = new List<string>();
            Guid unknownIid = new Guid( IID_UNKNOWN );

            uint count = removedItems.GetCount();

            for ( uint index = 0; index < count; index++ )
            {
                object item = removedItems.GetAt( index, ref unknownIid );

                if ( item is IShellLinkW link )
                {
                    signatures.Add( ShellLinkToString( link ) );
                }
            }

            return signatures;
        }

        private static string ShellLinkToString( IShellLinkW link )
        {
            StringBuilder pathBuilder = new StringBuilder( MAX_PATH );
            link.GetPath( pathBuilder, pathBuilder.Capacity, IntPtr.Zero, SLGP_RAWPATH );

            StringBuilder argumentsBuilder = new StringBuilder( INFOTIPSIZE );
            link.GetArguments( argumentsBuilder, argumentsBuilder.Capacity );

            string title = GetLinkTitle( link ) ?? string.Empty;

            // The shell compares links by path, title and arguments.
            return pathBuilder.ToString().ToUpperInvariant() + title.ToUpperInvariant() + argumentsBuilder.ToString();
        }

        private static string GetLinkTitle( IShellLinkW link )
        {
            IPropertyStore propertyStore = (IPropertyStore)link;
            PKEY pkeyTitle = PKEY.TITLE;
            PropVariant value = new PropVariant();

            try
            {
                propertyStore.GetValue( ref pkeyTitle, ref value );

                return value.GetString();
            }
            finally
            {
                value.Clear();
            }
        }

        private static string GetRuntimeAppId()
        {
            string appId = null;

            if ( GetCurrentProcessExplicitAppUserModelID( out appId ) != 0 )
            {
                return null;
            }

            return string.IsNullOrEmpty( appId ) ? null : appId;
        }

        private static string GetExecutablePath()
        {
            StringBuilder path = new StringBuilder( MAX_PATH );

            if ( GetModuleFileName( IntPtr.Zero, path, (uint)path.Capacity ) == 0 )
            {
                return null;
            }

            return path.ToString();
        }

        [DllImport( "kernel32.dll", CharSet = CharSet.Unicode )]
        private static extern uint GetModuleFileName( IntPtr hModule, [Out] StringBuilder path, uint size );

        [DllImport( "shell32.dll" )]
        private static extern int GetCurrentProcessExplicitAppUserModelID( [MarshalAs( UnmanagedType.LPWStr )] out string appId );

        [
            ComImport,
            InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
            Guid( "6332debf-87b5-4670-90c0-5e57b408a49e" )
        ]
        internal interface ICustomDestinationList
        {
            void SetAppID( [MarshalAs( UnmanagedType.LPWStr )] string pszAppID );

            [return: MarshalAs( UnmanagedType.Interface )]
            IObjectArray BeginList( out uint pcMinSlots, ref Guid riid );

            void AppendCategory( [MarshalAs( UnmanagedType.LPWStr )] string pszCategory, IObjectArray poa );

            void AppendKnownCategory( KDC category );

            void AddUserTasks( IObjectArray poa );

            void CommitList();

            [return: MarshalAs( UnmanagedType.Interface )]
            IObjectArray GetRemovedDestinations( ref Guid riid );

            void DeleteList( [MarshalAs( UnmanagedType.LPWStr )] string pszAppID );

            void AbortList();
        }

        [
            ComImport,
            InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
            Guid( "92CA9DCD-5622-4bba-A805-5E9F541BD8C9" )
        ]
        internal interface IObjectArray
        {
            uint GetCount();

            [return: MarshalAs( UnmanagedType.IUnknown )]
            object GetAt( uint uiIndex, ref Guid riid );
        }

        [
            ComImport,
            InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
            Guid( "5632b1a4-e38a-400a-928a-d4cd63230295" )
        ]
        internal interface IObjectCollection : IObjectArray
        {
            new uint GetCount();

            [return: MarshalAs( UnmanagedType.IUnknown )]
            new object GetAt( uint uiIndex, ref Guid riid );

            void AddObject( [MarshalAs( UnmanagedType.IUnknown )] object punk );

            void AddFromArray( IObjectArray poaSource );

            void RemoveObjectAt( uint uiIndex );

            void Clear();
        }

        [
            ComImport,
            InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
            Guid( "000214F9-0000-0000-C000-000000000046" )
        ]
        internal interface IShellLinkW
        {
            void GetPath( [Out, MarshalAs( UnmanagedType.LPWStr )] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags );

            IntPtr GetIDList();

            void SetIDList( IntPtr pidl );

            void GetDescription( [Out, MarshalAs( UnmanagedType.LPWStr )] StringBuilder pszFile, int cchMaxName );

            void SetDescription( [MarshalAs( UnmanagedType.LPWStr )] string pszName );

            void GetWorkingDirectory( [Out, MarshalAs( UnmanagedType.LPWStr )] StringBuilder pszDir, int cchMaxPath );

            void SetWorkingDirectory( [MarshalAs( UnmanagedType.LPWStr )] string pszDir );

            void GetArguments( [Out, MarshalAs( UnmanagedType.LPWStr )] StringBuilder pszArgs, int cchMaxPath );

            void SetArguments( [MarshalAs( UnmanagedType.LPWStr )] string pszArgs );

            short GetHotKey();

            void SetHotKey( short wHotKey );

            uint GetShowCmd();

            void SetShowCmd( uint iShowCmd );

            void GetIconLocation( [Out, MarshalAs( UnmanagedType.LPWStr )] StringBuilder pszIconPath, int cchIconPath, out int piIcon );

            void SetIconLocation( [MarshalAs( UnmanagedType.LPWStr )] string pszIconPath, int iIcon );

            void SetRelativePath( [MarshalAs( UnmanagedType.LPWStr )] string pszPathRel, uint dwReserved );

            void Resolve( IntPtr hwnd, uint fFlags );

            void SetPath( [MarshalAs( UnmanagedType.LPWStr )] string pszFile );
        }

        [
            ComImport,
            InterfaceType( ComInterfaceType.InterfaceIsIUnknown ),
            Guid( "886d8eeb-8cf2-4446-8d02-cdba1dbdcf99" )
        ]
        internal interface IPropertyStore
        {
            uint GetCount();

            PKEY GetAt( uint iProp );

            void GetValue( ref PKEY pkey, ref PropVariant pv );

            void SetValue( ref PKEY pkey, ref PropVariant pv );

            void Commit();
        }

        internal enum KDC
        {
            FREQUENT = 1,
            RECENT = 2
        }

        [StructLayout( LayoutKind.Sequential )]
        internal struct PKEY
        {
            public static readonly PKEY TITLE = new PKEY( new Guid( "F29F85E0-4FF9-1068-AB91-08002B27B3D9" ), 2 );

            public Guid FormatId;

            public uint PropertyId;

            public PKEY( Guid formatId, uint propertyId )
            {
                FormatId = formatId;
                PropertyId = propertyId;
            }
        }

        [StructLayout( LayoutKind.Sequential )]
        internal struct PropVariant
        {
            public const ushort VT_LPWSTR = 31;

            public ushort Vt;

            private ushort _reserved1;

            private ushort _reserved2;

            private ushort _reserved3;

            public IntPtr Pointer;

            public static PropVariant FromString( string value )
            {
                return new PropVariant { Vt = VT_LPWSTR, Pointer = Marshal.StringToCoTaskMemUni( value ) };
            }

            public string GetString()
            {
                return Vt == VT_LPWSTR && Pointer != IntPtr.Zero ? Marshal.PtrToStringUni( Pointer ) : null;
            }

            public void Clear()
            {
                if ( Pointer != IntPtr.Zero )
                {
                    PropVariantClear( ref this );
                }
            }

            [DllImport( "ole32.dll" )]
            private static extern int PropVariantClear( ref PropVariant pvar );
        }
    }

    /// <summary>
    ///     Base class for items in a <see cref="JumpList" />.
    /// </summary>
    public abstract class JumpItem
    {
        /// <summary>
        ///     Name of the custom category the item is displayed under, items without a category
        ///     are added to the special "Tasks" category.
        /// </summary>
        public string CustomCategory { get; set; }
    }

    /// <summary>
    ///     A task in a <see cref="JumpList" />, launched as an application shortcut.
    /// </summary>
    public class JumpTask : JumpItem
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string ApplicationPath { get; set; }

        public string Arguments { get; set; }

        public string IconResourcePath { get; set; }

        public int IconResourceIndex { get; set; }

        public string WorkingDirectory { get; set; }
    }
}
