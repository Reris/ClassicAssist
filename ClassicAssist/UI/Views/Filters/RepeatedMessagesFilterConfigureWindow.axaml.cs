using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassicAssist.Annotations;
using ClassicAssist.Data.Filters;

using Avalonia.Controls;
namespace ClassicAssist.UI.Views.Filters
{
    /// <summary>
    ///     Interaction logic for RepeatedMessagesFilterConfigureWindow.axaml
    /// </summary>
    public partial class RepeatedMessagesFilterConfigureWindow : Window, INotifyPropertyChanged
    {        private RepeatedMessagesFilter.MessageFilterOptions
            _options = new RepeatedMessagesFilter.MessageFilterOptions();

        public RepeatedMessagesFilterConfigureWindow()
        {
            InitializeComponent();
        }

        public RepeatedMessagesFilterConfigureWindow( RepeatedMessagesFilter.MessageFilterOptions options )
        {
            InitializeComponent();
            Options = options;
        }

        public RepeatedMessagesFilter.MessageFilterOptions Options
        {
            get => _options;
            set
            {
                _options = value;
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