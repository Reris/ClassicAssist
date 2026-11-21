using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using ClassicAssist.Data;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.ViewModels
{
    public class BaseViewModel : SetPropertyNotifyChanged
    {
        private static readonly object _instancesLock = new object();
        private static readonly List<WeakReference<BaseViewModel>> _viewModels = new List<WeakReference<BaseViewModel>>();
        private Options _currentOptions = Options.CurrentOptions;
        protected Dispatcher _dispatcher;
        private readonly AssistantOptions.dProfileChanged _profileChangingHandler;

        public BaseViewModel()
        {
            _dispatcher = Avalonia.Threading.Dispatcher.UIThread;

            // Keep a reference to the handler so it can be unsubscribed on finalization, otherwise
            // the static event roots every instance and prevents it from ever being collected.
            _profileChangingHandler = profile => { CurrentOptions = Options.CurrentOptions; };

            AssistantOptions.ProfileChangingEvent += _profileChangingHandler;

            lock ( _instancesLock )
            {
                _viewModels.Add( new WeakReference<BaseViewModel>( this ) );
            }
        }

        public Options CurrentOptions
        {
            get => _currentOptions;
            set => SetProperty( ref _currentOptions, value );
        }

        /// <summary>
        ///     Live instances, oldest first. Where multiple instances of the same type exist (e.g. after
        ///     recreating the main window), only the most recently created is returned.
        /// </summary>
        public static BaseViewModel[] Instances
        {
            get
            {
                lock ( _instancesLock )
                {
                    _viewModels.RemoveAll( reference => !reference.TryGetTarget( out _ ) );

                    return _viewModels.Select( reference => reference.TryGetTarget( out BaseViewModel target ) ? target : null )
                        .Where( target => target != null )
                        .GroupBy( target => target.GetType() )
                        .Select( group => group.Last() )
                        .ToArray();
                }
            }
        }

        ~BaseViewModel()
        {
            AssistantOptions.ProfileChangingEvent -= _profileChangingHandler;

            lock ( _instancesLock )
            {
                _viewModels.RemoveAll( reference => !reference.TryGetTarget( out _ ) );
            }
        }
    }
}