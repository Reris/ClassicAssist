using System.Media;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Input;
using ClassicAssist.Misc;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for AboutControl.axaml
    /// </summary>
    public partial class AboutControl : UserControl
    {
        private Timer _timer;

        public AboutControl()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseEnter( object sender, PointerEventArgs e )
        {
            if ( !( sender is TextBlock textBlock ) )
            {
                return;
            }

            _timer = new Timer( 1000 );
            _timer.Elapsed += ( o, args ) =>
            {
                _timer.Stop();

                if ( !textBlock.IsPointerOver )
                {
                    return;
                }

                SoundHelper.Play( Properties.Resources.kiss );
            };

            _timer.Start();
        }

        private void TextBlock_MouseLeave( object sender, PointerEventArgs e )
        {
            _timer?.Stop();
        }
    }
}