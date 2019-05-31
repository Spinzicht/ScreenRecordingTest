using Core.Util;
using System;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Core.Networking
{
    public enum SyncState
    {
        Initializing,
        Syncing,
        Paused,
        Stopping,
        Stopped
    }

    public abstract class Syncable : Observable
    {
        private double _delay = 100;
        protected double Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                t.Interval = _delay;
            }
        }

        protected SyncState SyncState { get; set; } = SyncState.Initializing;

        Timer t;

        public Syncable()
        {
            t = new Timer(_delay);
            t.Elapsed += Send;
        }

        protected void Send(object sender = null, ElapsedEventArgs e = null)
        {
            t.Stop();
            Pack().Send(Connection.Server);

            if (SyncState == SyncState.Stopping)
                SyncState = SyncState.Stopped;
        }

        protected bool Sync<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            var result = Set(ref field, value, propertyName);
            if (result && (SyncState == SyncState.Syncing || SyncState == SyncState.Stopping))
            {
                t.Stop();
                t.Start();
            }
            return result;
        }

        public abstract IPPacket Pack();
    }
}