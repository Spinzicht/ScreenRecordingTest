using Chronos;
using Chronos.Tracking;
using System.Windows;

namespace Ludio.UI
{
    class Scalable : Editable
    {
        private double _width = 480;
        private double _oldWidth = 480;
        private double _minWidth = 190;
        private double _maxWidth = 1900;


        public override double Width
        {
            get => _width;
            set
            {
                if (Maximized || (value >= _minWidth && value <= _maxWidth))
                {
                    Set(ref _width, value);
                }
            }
        }

        private double _height = 270;
        private double _oldHeight = 270;
        private double _minHeight = 100;
        private double _maxHeight = 1060;

        public override double Height
        {
            get => _height;
            set
            {
                if (Maximized || (value >= _minHeight && value <= _maxHeight))
                    Set(ref _height, value);
            }
        }

        private bool _maximized = false;
        public override bool Maximized
        {
            get => _maximized;
            set
            {
                if(Set(ref _maximized, value))
                {
                    if (_maximized) Maximize();
                    else Restore();
                }
            }
        }

        private bool _isScaling = false;

        public Scalable(double startWidth, double startHeight, double minWidth, double minHeight, double maxWidth, double maxHeight, Editable child = null) : base(child)
        {
            _minHeight = minHeight;
            _minWidth = minWidth;
            _maxHeight = maxHeight;
            _maxWidth = maxWidth;

            Width = startWidth;
            Height = startHeight;
        }

        Vector startPos;
        protected override void MouseClicked(object sender, MouseClickedArgs e)
        {
            if (e.click.action == MOUSEACTION.DOWN && e.click.button == MOUSEBUTTON.LEFT)
            {
                startPos = MouseTracker.I.Position;
                if (ScaleEnabled) _isScaling = true;
            }
            else if (_isScaling)
            {
                _isScaling = false;
            }
            base.MouseClicked(sender, e);
        }

        protected override void MouseMoved(object sender, MouseMovedArgs e)
        {
            if (_isScaling) Scale(e);
            base.MouseMoved(sender, e);
        }

        public override void Maximize()
        {
            if (!Maximized) Maximized = true;

            _oldHeight = Height;
            _oldWidth = Width;

            Height = SystemParameters.PrimaryScreenHeight * 1.005;
            Width = SystemParameters.PrimaryScreenWidth * 1.005;
            base.Maximize();
        }

        public override void Restore()
        {
            if(Maximized) Maximized = false;

            Width = _oldWidth;
            Height = _oldHeight;

            base.Restore();
        }

        private void Scale(MouseMovedArgs e)
        {
            var deltaPos = startPos - e.Position;

            Height += VerticalAlignment == VerticalAlignment.Top ? -deltaPos.Y : deltaPos.Y;
            Width += HorizontalAlignment == HorizontalAlignment.Left ? -deltaPos.X : deltaPos.X;

            startPos = e.Position;
        }
    }
}
