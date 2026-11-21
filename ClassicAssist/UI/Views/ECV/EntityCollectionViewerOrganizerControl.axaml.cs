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
using ClassicAssist.Shared.UI;
using System.Collections.ObjectModel;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO.Objects;
using ClassicAssist.Shared.UI.Compat;
using ItemCollection = ClassicAssist.UO.Objects.ItemCollection;

namespace ClassicAssist.UI.Views.ECV
{
    /// <summary>
    ///     Interaction logic for EntityCollectionViewerOrganizerControl.axaml
    /// </summary>
    public partial class EntityCollectionViewerOrganizerControl : UserControl
    {
        public static readonly Avalonia.StyledProperty<ItemCollection> CollectionProperty =
        AvaloniaCompat.RegisterCompat<EntityCollectionViewerOrganizerControl, ItemCollection>(nameof( Collection ), default, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: PropertyChangedCallback);

        public static readonly Avalonia.StyledProperty<ObservableCollection<QueueAction>> QueueActionsProperty =
        AvaloniaCompat.RegisterCompat<EntityCollectionViewerOrganizerControl, ObservableCollection<QueueAction>>(nameof( QueueActions ), default, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: PropertyChangedCallback);

        public EntityCollectionViewerOrganizerControl()
        {
            InitializeComponent();
        }

        public ItemCollection Collection
        {
            get => (ItemCollection) GetValue( CollectionProperty );
            set => SetValue( CollectionProperty, value );
        }

        public ObservableCollection<QueueAction> QueueActions
        {
            get => (ObservableCollection<QueueAction>) GetValue( QueueActionsProperty );
            set => SetValue( QueueActionsProperty, value );
        }

        private static void PropertyChangedCallback( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( !( d is EntityCollectionViewerOrganizerControl control ) )
            {
                return;
            }

            if ( !( control.DataContext is EntityCollectionViewerOrganizerViewModel viewModel ) )
            {
                return;
            }

            if ( e.Property == CollectionProperty )
            {
                viewModel.Collection = (ItemCollection) e.NewValue;
            }
            else if ( e.Property == QueueActionsProperty )
            {
                viewModel.QueueActions = (ObservableCollection<QueueAction>) e.NewValue;
            }
        }
    }
}
