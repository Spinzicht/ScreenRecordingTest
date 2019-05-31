using Core.Networking;

namespace Core.Data
{
    public abstract class DataObject : Syncable
    {
        protected int _id;
        public int ID
        {
            get => _id;
            set => Set(ref _id, value);
        }

        public void StartSync()
        {
            SyncState = SyncState.Syncing;
        }

        public void Sync()
        {
            if(SyncState == SyncState.Syncing)
                Send();
        }

        public void StopSync()
        {
            SyncState = SyncState.Stopping;
        }
    }
}
