#region License

// Copyright (C) 2025 Reetus
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Data.Skills;
using ClassicAssist.Shared.UI;
using System.Windows.Input;
using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class SkillsGridViewSortBehaviour : Behavior<DataGrid>
    {
        public static readonly Avalonia.StyledProperty<ICommand> SortChangedCommandProperty =
        AvaloniaCompat.RegisterCompat<SkillsGridViewSortBehaviour, ICommand>(nameof( SortChangedCommand ), default);

        public static readonly Avalonia.StyledProperty<ICommand> SetSortCommandProperty =
        AvaloniaCompat.RegisterCompat<SkillsGridViewSortBehaviour, ICommand>(nameof( SetSortCommand ), null);

        private ListSortDirection _lastDirection;

        private SkillsGridViewColumn.Enums _lastHeaderClicked;
        private ICommand _setSortCommandImpl;

        public ICommand SetSortCommand
        {
            get => (ICommand) GetValue( SetSortCommandProperty );
            set => SetValue( SetSortCommandProperty, value );
        }

        public ICommand SetSortCommandImpl => _setSortCommandImpl ?? ( _setSortCommandImpl = new RelayCommand( SetSort ) );

        public ICommand SortChangedCommand
        {
            get => (ICommand) GetValue( SortChangedCommandProperty );
            set => SetValue( SortChangedCommandProperty, value );
        }

        private void SetSort( object obj )
        {
            if ( !( obj is SkillsSortInfo info ) )
            {
                return;
            }

            ApplySort( info.SortBy, info.Direction );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Sorting += OnSorting;

            SetValue( SetSortCommandProperty, SetSortCommandImpl );
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Sorting -= OnSorting;

            base.OnDetaching();
        }

        private void OnSorting( object sender, DataGridColumnEventArgs e )
        {
            e.Handled = true;

            SkillsGridViewColumn.Enums sortBy = SkillsGridViewColumn.GetSortField( e.Column );

            ListSortDirection direction;

            if ( sortBy == _lastHeaderClicked )
            {
                direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }
            else
            {
                direction = ListSortDirection.Ascending;
            }

            ApplySort( sortBy, direction );

            SortChangedCommand?.Execute( new SkillsSortInfo( sortBy, direction ) );
        }

        private void ApplySort( SkillsGridViewColumn.Enums sortBy, ListSortDirection direction )
        {
            _lastDirection = direction;

            _lastHeaderClicked = sortBy;

            if ( !( AssociatedObject?.ItemsSource is ObservableCollection<SkillEntry> items ) )
            {
                return;
            }

            SkillComparer comparer = new SkillComparer( direction, sortBy );

            // In-place insertion sort using Move so bindings survive
            for ( int i = 1; i < items.Count; i++ )
            {
                for ( int j = i; j > 0 && comparer.Compare( items[j - 1], items[j] ) > 0; j-- )
                {
                    items.Move( j, j - 1 );
                }
            }
        }
    }

    public class SkillsSortInfo
    {
        public SkillsSortInfo( SkillsGridViewColumn.Enums sortBy, ListSortDirection direction )
        {
            SortBy = sortBy;
            Direction = direction;
        }

        public ListSortDirection Direction { get; }
        public SkillsGridViewColumn.Enums SortBy { get; }
    }
}