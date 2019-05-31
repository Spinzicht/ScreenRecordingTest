using Chronos.Networking;
using Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chronos
{
    public abstract class CaptureBase : Observable
    {
        protected SynchronizationContext _syncContext;
        protected bool _started = false;

        protected Stopwatch watch;
        protected int count;
        protected long time;

        private Task RecordingTask;
        private bool Recording;

        private int _fps = 0;
        public int FPS
        {
            get => _fps;
            set => Set(ref _fps, value);
        }

        public CaptureBase()
        {
            _syncContext = SynchronizationContext.Current;
            watch = Stopwatch.StartNew();   
        }

        public virtual async Task Run()
        {
            time = watch.ElapsedMilliseconds;
            count++;

            FPS = (int)((1.0f / (time / count) * 1000.0f));

            if (count == 100)
            {
                count = 0;
                watch.Restart();
            }
        }

        public virtual void SendData()
        {

        }

        public virtual void Start()
        {
            _started = true;
            if (!Recording)
            {
                Recording = true;
                RecordingTask = new Task(async () =>
                {
                    while (Recording)
                    {
                        await Run();
                    }
                });

                RecordingTask.Start();
            }
        }

        public virtual void Stop()
        {
            _started = false;
            Recording = false;
        }

        public void Resume()
        {
            if (_started)
                Start();
        }

        public void Pause()
        {
            if (_started)
                 Stop();
            _started = true;
        }

        ~CaptureBase()
        {
            Stop();
        }
    }
}
