using System;
using System.Diagnostics;

using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;

using Avalonia.Threading;

using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views;
using SkiaSharp;
using System.Windows.Input;

namespace ClassicAssist.UI.ViewModels
{
    public class GIFRecorderViewModel : BaseViewModel
    {
        private readonly GIFRecorderWindow _window;
        private bool _isRecording;
        private MemoryStream _lastStream;
        private ICommand _recordCommand;
        private ICommand _saveCommand;
        private CancellationTokenSource _token;

        public GIFRecorderViewModel( GIFRecorderWindow window ) : this()
        {
            _window = window;
        }

        public GIFRecorderViewModel()
        {
            _dispatcher = Avalonia.Threading.Dispatcher.UIThread;
        }

        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty( ref _isRecording, value );
        }

        public MemoryStream LastStream
        {
            get => _lastStream;
            set => SetProperty( ref _lastStream, value );
        }

        public ICommand RecordCommand => _recordCommand ?? ( _recordCommand = new RelayCommand( Record, o => true ) );

        public ICommand SaveCommand => _saveCommand ?? ( _saveCommand = new RelayCommand( Save, o => !IsRecording ) );

        private void Save( object obj )
        {
            if ( LastStream == null )
            {
                return;
            }

            string directory = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Screenshots" );

            if ( !Directory.Exists( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            DateTime now = DateTime.Now;

            string fileName =
                $"ClassicAssist-{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}.gif";

            string fullPath = Path.Combine( directory, fileName );

            using ( FileStream fs = new FileStream( fullPath, FileMode.Create ) )
            {
                LastStream.WriteTo( fs );
            }

            if ( OperatingSystem.IsWindows() )
            {
                string args = $"/e, /select, \"{fullPath}\"";

                ProcessStartInfo info = new ProcessStartInfo { FileName = "explorer", Arguments = args };
                Process.Start( info );
            }
            else
            {
                Process.Start( new ProcessStartInfo { FileName = directory, UseShellExecute = true } );
            }
        }

        private void Record( object obj )
        {
            if ( IsRecording )
            {
                _token.Cancel();
            }
            else
            {
                IsRecording = true;
                _token = new CancellationTokenSource();

                Window w = _window ?? throw new InvalidOperationException();

                Task.Run( async () =>
                {
                    try
                    {
                        TimeSpan frameInterval = TimeSpan.FromSeconds( 1.0 / 5 );
                        MemoryStream ms = new MemoryStream();

                        using ( GifEncoder gif = new GifEncoder( ms, frameInterval.Milliseconds ) )
                        {
                            PixelPoint position = default;
                            Size clientSize = default;
                            double scaling = 1;

                            _dispatcher.Invoke( () =>
                            {
                                position = w.Position;
                                clientSize = w.ClientSize;
                                scaling = w.RenderScaling;
                            } );

                            int captureWidth = (int) ( clientSize.Width * scaling ) - 10;
                            int captureHeight = (int) ( clientSize.Height * scaling ) - 70;

                            Stopwatch sw = new Stopwatch();
                            sw.Start();

                            while ( true )
                            {
                                _dispatcher.Invoke( () =>
                                {
                                    position = w.Position;
                                    clientSize = w.ClientSize;
                                    scaling = w.RenderScaling;
                                } );

                                using ( SKBitmap frame = ScreenCapture.CaptureRectangle( position.X + 5,
                                    position.Y + 50, captureWidth, captureHeight ) )
                                {
                                    if ( frame == null )
                                    {
                                        // Screen capture unsupported on this platform
                                        return null;
                                    }

                                    gif.AddFrame( frame, -1, GifQuality.Bit8 );
                                }

                                sw.Stop();

                                int wait = frameInterval.Milliseconds - (int) sw.ElapsedMilliseconds;

                                if ( wait > 0 )
                                {
                                    await Task.Delay( wait );
                                }

                                sw.Restart();

                                if ( _token.IsCancellationRequested )
                                {
                                    return ms;
                                }
                            }
                        }
                    }
                    finally
                    {
                        _dispatcher.Invoke( () => IsRecording = false );
                    }
                } ).ContinueWith( t =>
                {
                    if ( !( t.Result is MemoryStream ms ) )
                    {
                        return;
                    }

                    if ( LastStream != null )
                    {
                        LastStream.Dispose();
                        LastStream = null;
                    }

                    LastStream = ms;
                    CommandManager.InvalidateRequerySuggested();
                } );
            }
        }
    }
}