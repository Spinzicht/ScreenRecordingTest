using Core.Util;
using System;
using System.Threading;

namespace Ludio.Base
{
    public class ViewModel : Observable
    {
        protected SynchronizationContext _syncContext;

        public ViewModel()
        {
            _syncContext = SynchronizationContext.Current;
        }

        public void OnUI(Action action, object obj = null)
        {
            _syncContext.Post(o => action(), obj);
        }

        public virtual void Init() { }
    }
}