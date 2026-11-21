using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ClassicAssist.Annotations;
using ClassicAssist.Data.Filters;

namespace ClassicAssist.UI.Views.Filters
{
    /// <summary>
    ///     Interaction logic for SoundFilterConfigureWindow.axaml
    /// </summary>
    public partial class SoundFilterConfigureWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<SoundFilterEntry> _items;

        public SoundFilterConfigureWindow()
        {
            InitializeComponent();
        }

        public SoundFilterConfigureWindow( ObservableCollection<SoundFilterEntry> items )
        {
            InitializeComponent();
            Items = items;
        }

        public ObservableCollection<SoundFilterEntry> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}