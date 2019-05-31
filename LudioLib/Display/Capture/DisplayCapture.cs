using Chronos.Tracking;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Chronos.Display
{
    public abstract class DisplayCapture : CaptureBase
    {
        protected Vector mousePos;

        public int Zoom { get; set; } = 1;

        protected List<Bitmap> _displayQueue = new List<Bitmap>() { null };
        protected Bitmap _display;

        private float _delay = 0.0f;
        public float ScreendDelay
        {
            get => _delay;
            set => Set(ref _delay, value > 0 ? value : 0);
        }

        int sendFPS = 30;
        int frameUpdateCounter = 0;

        public virtual Bitmap Display {
            get
            {
                if (ScreendDelay == 0 || _displayQueue.Count == 0)
                    return _display;

                return _displayQueue[0];
            }
            set
            {
                Set(ref _display, value);

                if (value != null)
                {
                    SendDisplay(value);
                    UpdateQueue(value);
                }
            }
        }

        private void SendDisplay(Bitmap value)
        {
            if (sendFPS > FPS || frameUpdateCounter == 0)
            {
                //PeerHandler.I.Send(this);
            }

            frameUpdateCounter++;
            if (frameUpdateCounter > FPS / sendFPS)
                frameUpdateCounter = 0;
        }

        private void UpdateQueue(Bitmap value)
        {
            if (ScreendDelay == 0)
                return;

            if (FPS < 0) _displayQueue.Clear();
            else
            {
                while(_displayQueue.Count > ScreendDelay * FPS)
                {
                    _displayQueue.RemoveAt(0);
                    smallStep = 1.0f / FPS;
                }
            }

            _displayQueue.Add(value);
        }

        public DisplayCapture()
        {
            MouseTracker.I.OnMouseMoved += OnMouseMoved;
            KeyboardTracker.I.OnKeyPressed += OnKeyPressed;
            KeyboardTracker.I.OnKeyReleased += OnKeyReleased;

        }

        private void OnMouseMoved(object sender, MouseMovedArgs e)
        {
            mousePos = e.Position;
            if (mousePos.X < 0) mousePos.X = 0;
            if (mousePos.Y < 0) mousePos.Y = 0;
        }

        private void OnKeyReleased(object sender, Tracking.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case (int)Keys.LControlKey:
                case (int)Keys.RControlKey:
                case (int)Keys.LShiftKey:
                case (int)Keys.RShiftKey:
                    step = 1;
                    break;
            }
        }

        float smallStep = 1 / 30.0f;
        float step = 1;

        private void OnKeyPressed(object sender, Tracking.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case (int)Keys.Add:
                    ScreendDelay += step;
                    break;
                case (int)Keys.Subtract:
                    ScreendDelay -= step;
                    break;
                case (int)Keys.LControlKey:
                case (int)Keys.RControlKey:
                    step = smallStep;
                    break;
                case (int)Keys.LShiftKey:
                case (int)Keys.RShiftKey:
                    step = 0.5f;
                    break;
            }
        }
    }
}
