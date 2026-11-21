using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Shared.UI.Compat;
using System.Windows.Input;

namespace ClassicAssist.UI.Views.ECV.Filter
{
    /// <summary>
    ///     Interaction logic for EntityCollectionFilterControl.xaml
    /// </summary>
    public partial class EntityCollectionFilterControl : UserControl
    {
        public static readonly Avalonia.StyledProperty<ICommand> CommandProperty =
        AvaloniaCompat.RegisterCompat<EntityCollectionFilterControl, ICommand>(nameof( Command ), default( ICommand ), onChanged: PropertyChangedCallback);

        public static readonly Avalonia.StyledProperty<ObservableCollection<Assembly>> AssembliesProperty =
        AvaloniaCompat.RegisterCompat<EntityCollectionFilterControl, ObservableCollection<Assembly>>(nameof( Assemblies ), new ObservableCollection<Assembly>(), onChanged: PropertyChangedCallback);

        public static readonly Avalonia.StyledProperty<bool> IsFilterAppliedProperty =
        AvaloniaCompat.RegisterCompat<EntityCollectionFilterControl, bool>(nameof( IsFilterApplied ), default( bool ), onChanged: PropertyChangedCallback);

        public EntityCollectionFilterControl()
        {
            InitializeComponent();

            PropertyChanged += OnPropertyChanged;
        }

        public ObservableCollection<Assembly> Assemblies
        {
            get => (ObservableCollection<Assembly>) GetValue( AssembliesProperty );
            set => SetValue( AssembliesProperty, value );
        }

        public ICommand Command
        {
            get => (ICommand) GetValue( CommandProperty );
            set => SetValue( CommandProperty, value );
        }

        public bool IsFilterApplied
        {
            get => (bool) GetValue( IsFilterAppliedProperty );
            set => SetValue( IsFilterAppliedProperty, value );
        }

        private void OnPropertyChanged( object sender, AvaloniaPropertyChangedEventArgs e )
        {
            if ( e.Property.Name == nameof( IsVisible ) && DataContext is EntityCollectionFilterViewModel viewModel )
            {
                viewModel.SaveFilterProfiles();
            }
        }

        private static void PropertyChangedCallback( AvaloniaObject d, AvaloniaPropertyChangedEventArgs e )
        {
            if ( !( d is EntityCollectionFilterControl control ) )
            {
                return;
            }

            if ( !( control.DataContext is EntityCollectionFilterViewModel viewModel ) )
            {
                return;
            }

            switch ( e.NewValue )
            {
                case ICommand command:
                    viewModel.Command = command;
                    break;
            }

            if ( e.Property == IsFilterAppliedProperty )
            {
                viewModel.IsFilterApplied = (bool) e.NewValue;
            }
        }
    }
}