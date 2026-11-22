using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

using ClassicAssist.Shared.UI.Compat;

namespace ClassicAssist.Controls.DraggableTreeView
{
    /*
     * Credit: https://www.codeproject.com/Articles/55168/Drag-and-Drop-Feature-in-WPF-TreeView-Control
     */
    public class DraggableTreeView : TreeView
    {
        public static readonly Avalonia.StyledProperty<object> BindableSelectedItemProperty =
        AvaloniaCompat.RegisterAttachedCompat<DraggableTreeView, object>(nameof( BindableSelectedItem ), default, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: OnSelectedPropertyChanged);

        public static readonly Avalonia.StyledProperty<object> BindableSelectedGroupProperty =
        AvaloniaCompat.RegisterAttachedCompat<DraggableTreeView, object>(nameof( BindableSelectedGroup ), default, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, onChanged: OnSelectedPropertyChanged);

        public static readonly Avalonia.StyledProperty<bool> AllowDragGroupsProperty =
        AvaloniaCompat.RegisterAttachedCompat<DraggableTreeView, bool>(nameof( AllowDragGroups ), true);

        public static readonly Avalonia.StyledProperty<bool> AllowDragItemsOntoItemsProperty =
        AvaloniaCompat.RegisterAttachedCompat<DraggableTreeView, bool>(nameof( AllowDragItemsOntoItems ), true);

        private object _dragItem;

        private Point _dragStartPosition;

        private PointerPressedEventArgs _dragTriggerEvent;

        private bool _pointerDown;

        public DraggableTreeView()
        {
            DragDrop.SetAllowDrop( this, true );

            AddHandler( DragDrop.DragOverEvent, OnDragOver );
            AddHandler( DragDrop.DropEvent, OnDrop );
            AddHandler( PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel );
            AddHandler( PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel );
            AddHandler( PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel );

            SelectionChanged += OnSelectionChanged;
            ContainerPrepared += OnContainerPrepared;
        }

        public bool AllowDragGroups
        {
            get => (bool) GetValue( AllowDragGroupsProperty );
            set => SetValue( AllowDragGroupsProperty, value );
        }

        public bool AllowDragItemsOntoItems
        {
            get => (bool) GetValue( AllowDragItemsOntoItemsProperty );
            set => SetValue( AllowDragItemsOntoItemsProperty, value );
        }

        public IDraggableGroup BindableSelectedGroup
        {
            get => (IDraggableGroup) GetValue( BindableSelectedGroupProperty );
            set => SetValue( BindableSelectedGroupProperty, value );
        }

        public IDraggableEntry BindableSelectedItem
        {
            get => (IDraggableEntry) GetValue( BindableSelectedItemProperty );
            set => SetValue( BindableSelectedItemProperty, value );
        }

        protected override void PrepareContainerForItemOverride( Control container, object item, int index )
        {
            base.PrepareContainerForItemOverride( container, item, index );

            // Under container recycling a reused TreeViewItem can keep a stale IsSelected from the
            // item it previously displayed, making several rows appear selected at once. Sync it to
            // the bound selection every time a container is (re)assigned to an item.
            if ( container is TreeViewItem tvi )
            {
                bool selected = ReferenceEquals( item, BindableSelectedItem ) || ReferenceEquals( item, BindableSelectedGroup );

                if ( tvi.IsSelected != selected )
                {
                    tvi.IsSelected = selected;
                }
            }
        }

        private void OnPointerPressed( object sender, PointerPressedEventArgs e )
        {
            if ( !e.GetCurrentPoint( this ).Properties.IsLeftButtonPressed )
            {
                return;
            }

            TreeViewItem container = FindAncestor<TreeViewItem>( e.Source as Visual );

            if ( container == null )
            {
                return;
            }

            _dragItem = container.DataContext ?? container.Header;
            _dragStartPosition = e.GetPosition( this );
            _dragTriggerEvent = e;
            _pointerDown = true;
        }

        private async void OnPointerMoved( object sender, PointerEventArgs e )
        {
            if ( !_pointerDown || _dragItem == null || _dragTriggerEvent == null )
            {
                return;
            }

            Point position = e.GetPosition( this );

            if ( Math.Abs( position.X - _dragStartPosition.X ) < 4 && Math.Abs( position.Y - _dragStartPosition.Y ) < 4 )
            {
                return;
            }

            _pointerDown = false;

            DataTransfer data = new DataTransfer();
            data.Add( DataTransferItem.Create( DataFormat.Text, string.Empty ) );

            await DragDrop.DoDragDropAsync( _dragTriggerEvent, data, DragDropEffects.Move );
        }

        private void OnPointerReleased( object sender, PointerReleasedEventArgs e )
        {
            _pointerDown = false;
            _dragTriggerEvent = null;
        }

        private void OnDragOver( object sender, DragEventArgs e )
        {
            if ( !( _dragItem is IDraggable sourceItem ) )
            {
                return;
            }

            if ( !( ItemsSource is ObservableCollection<IDraggable> items ) )
            {
                return;
            }

            ObservableCollection<IDraggable> sourceParent = GetParent( sourceItem, items );

            object targetItem = GetTargetItem( e );

            if ( !AllowDragItemsOntoItems && sourceItem is IDraggableEntry && sourceParent == items && !( targetItem is IDraggableGroup ) )
            {
                e.DragEffects = DragDropEffects.None;

                return;
            }

            e.DragEffects = DragDropEffects.Move;
        }

        private void OnDrop( object sender, DragEventArgs e )
        {
            object target = GetTargetItem( e );

            switch ( target )
            {
                case IDraggableGroup destinationGroup:
                {
                    IDraggable sourceItem = _dragItem as IDraggable;

                    ObservableCollection<IDraggable> parent = GetParent( sourceItem, ItemsSource as ObservableCollection<IDraggable> );

                    switch ( sourceItem )
                    {
                        case IDraggableGroup _ when !AllowDragGroups:
                        // No parents into children
                        case IDraggableGroup draggableGroup when IsParentOf( destinationGroup, draggableGroup ):
                            return;
                    }

                    int? previousIndex = parent?.IndexOf( sourceItem );

                    int newSelectedIndex = 0;

                    if ( previousIndex.HasValue )
                    {
                        if ( previousIndex.Value > 0 )
                        {
                            newSelectedIndex = previousIndex.Value - 1;
                        }
                        else if ( previousIndex.Value < parent.Count - 1 )
                        {
                            newSelectedIndex = previousIndex.Value;
                        }
                        else
                        {
                            newSelectedIndex = -1;
                        }
                    }

                    parent?.Remove( sourceItem );

                    if ( newSelectedIndex != -1 && parent != null )
                    {
                        IDraggable item = parent[newSelectedIndex];

                        if ( item != null && FindContainer( item ) is TreeViewItem tvi && !tvi.IsSelected )
                        {
                            tvi.IsSelected = true;
                        }
                    }

                    destinationGroup.Children.Add( sourceItem );

                    break;
                }
                case IDraggableEntry destinationItem:
                {
                    IDraggable sourceItem = _dragItem as IDraggable;

                    if ( !( ItemsSource is ObservableCollection<IDraggable> items ) )
                    {
                        return;
                    }

                    ObservableCollection<IDraggable> sourceParent = GetParent( sourceItem, items );

                    if ( !AllowDragItemsOntoItems && sourceItem is IDraggableEntry && sourceParent == items )
                    {
                        return;
                    }

                    if ( sourceItem is IDraggableGroup )
                    {
                        // No groups onto items
                        return;
                    }

                    ObservableCollection<IDraggable> destinationParent = GetParent( destinationItem, ItemsSource as ObservableCollection<IDraggable> );

                    if ( sourceParent != destinationParent )
                    {
                        sourceParent.Remove( sourceItem );
                        destinationParent.Add( sourceItem );
                    }

                    int sourceIndex = destinationParent.IndexOf( sourceItem );
                    int targetIndex = destinationParent.IndexOf( destinationItem );

                    destinationParent.Move( sourceIndex, targetIndex );
                    break;
                }
                case null:
                {
                    IDraggable sourceItem = _dragItem as IDraggable;

                    if ( !( ItemsSource is ObservableCollection<IDraggable> items ) )
                    {
                        return;
                    }

                    ObservableCollection<IDraggable> sourceParent = GetParent( sourceItem, items );

                    sourceParent?.Remove( sourceItem );

                    items.Add( sourceItem );
                    break;
                }
            }
        }

        private void OnContainerPrepared( object sender, ContainerPreparedEventArgs e )
        {
            if ( !( e.Container is TreeViewItem tvi ) )
            {
                return;
            }

            object item = tvi.DataContext ?? tvi.Header;

            if ( item == null )
            {
                return;
            }

            if ( !ReferenceEquals( item, BindableSelectedItem ) && !ReferenceEquals( item, BindableSelectedGroup ) )
            {
                return;
            }

            if ( !tvi.IsSelected )
            {
                tvi.IsSelected = true;
            }
        }

        private void OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            object selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;

            switch ( selected )
            {
                case IDraggableGroup draggableGroup:
                    BindableSelectedGroup = draggableGroup;
                    BindableSelectedItem = null;
                    break;
                case IDraggableEntry draggableEntry:
                    BindableSelectedItem = draggableEntry;
                    BindableSelectedGroup = null;
                    break;
            }
        }

        private object GetTargetItem( DragEventArgs e )
        {
            TreeViewItem container = FindAncestor<TreeViewItem>( e.Source as Visual );

            if ( container == null )
            {
                container = this.GetVisualDescendants().OfType<TreeViewItem>()
                    .FirstOrDefault( tvi => tvi.Bounds.Contains( e.GetPosition( tvi ) ) );
            }

            return container?.DataContext ?? container?.Header;
        }

        private TreeViewItem FindContainer( object item )
        {
            return this.GetVisualDescendants().OfType<TreeViewItem>()
                .FirstOrDefault( tvi => ReferenceEquals( tvi.DataContext ?? tvi.Header, item ) );
        }

        private void SelectAndBringIntoView( object item )
        {
            TreeViewItem container = FindContainer( item );

            if ( container != null )
            {
                container.IsSelected = true;
                container.BringIntoView();

                return;
            }

            // The container hasn't been realized yet - either layout is still pending or, under
            // virtualization, the item is scrolled out of view. Retry once layout has run; nested
            // items are handled by OnContainerPrepared once their container is generated.
            Dispatcher.UIThread.Post( () =>
            {
                TreeViewItem tvi = FindContainer( item );

                if ( tvi != null )
                {
                    tvi.IsSelected = true;
                    tvi.BringIntoView();
                }
            }, DispatcherPriority.Loaded );
        }

        private static bool IsParentOf( IDraggable sourceItem, IDraggableGroup destinationGroup )
        {
            return destinationGroup.Children.Any( e => e == sourceItem ) || GetGroups( destinationGroup.Children )
                .Any( draggableGroup => IsParentOf( sourceItem, draggableGroup ) );
        }

        private static IEnumerable<IDraggableGroup> GetGroups( IEnumerable<IDraggable> collection )
        {
            return collection.Where( i => i is IDraggableGroup ).Cast<IDraggableGroup>();
        }

        private static void OnSelectedPropertyChanged( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( !( d is DraggableTreeView draggableTreeView ) || e.NewValue == null )
            {
                return;
            }

            draggableTreeView.SelectAndBringIntoView( e.NewValue );
        }

        private static T FindAncestor<T>( Visual visual ) where T : Visual
        {
            while ( visual != null && !( visual is T ) )
            {
                visual = visual.GetVisualParent();
            }

            return visual as T;
        }

        private static ObservableCollection<IDraggable> GetParent( IDraggable draggable, ObservableCollection<IDraggable> parent )
        {
            if ( parent.Contains( draggable ) )
            {
                return parent;
            }

            IEnumerable<IDraggableGroup> groups = GetGroups( parent );

            return groups.Select( draggableGroup => GetParent( draggable, draggableGroup.Children ) ).FirstOrDefault( childParent => childParent != null );
        }
    }
}