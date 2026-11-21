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
using Avalonia.Input;

namespace ClassicAssist.Shared.UI
{
    /// <summary>
    ///     Minimal replacement for the WPF CommandManager. Avalonia has no automatic
    ///     requery infrastructure, so commands register their CanExecuteChanged handlers
    ///     here and <see cref="InvalidateRequerySuggested" /> raises them all.
    /// </summary>
    public static class CommandManager
    {
        private static readonly List<WeakReference> _handlers = new List<WeakReference>();
        private static readonly object _lock = new object();
        private static bool _raising;

        public static event EventHandler RequerySuggested
        {
            add => RegisterHandler( value );
            remove => UnregisterHandler( value );
        }

        public static void InvalidateRequerySuggested()
        {
            if ( _raising )
            {
                return;
            }

            _raising = true;

            try
            {
                lock ( _lock )
                {
                    for ( int i = _handlers.Count - 1; i >= 0; i-- )
                    {
                        if ( _handlers[i].Target is EventHandler handler )
                        {
                            try
                            {
                                handler( null, EventArgs.Empty );
                            }
                            catch
                            {
                                // ignore handler exceptions during requery
                            }
                        }
                        else
                        {
                            _handlers.RemoveAt( i );
                        }
                    }
                }
            }
            finally
            {
                _raising = false;
            }
        }

        internal static IDisposable RegisterHandler( EventHandler handler )
        {
            lock ( _lock )
            {
                _handlers.Add( new WeakReference( handler ) );
            }

            return new HandlerToken( handler );
        }

        internal static void UnregisterHandler( EventHandler handler )
        {
            lock ( _lock )
            {
                _handlers.RemoveAll( r => !r.IsAlive || ReferenceEquals( r.Target, handler ) );
            }
        }

        private sealed class HandlerToken : IDisposable
        {
            private EventHandler _handler;

            public HandlerToken( EventHandler handler )
            {
                _handler = handler;
            }

            public void Dispose()
            {
                if ( _handler != null )
                {
                    UnregisterHandler( _handler );
                    _handler = null;
                }
            }
        }
    }
}
