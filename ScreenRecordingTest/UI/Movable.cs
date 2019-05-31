using Chronos;
using Chronos.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using F = System.Windows.Forms;

namespace Ludio.UI
{
    class Movable : Editable
    {
        private bool _isMoving = false;
        private Thickness _minMargin = new Thickness(10);

        private Thickness _margin;
        private Thickness _oldMargin;
        public override Thickness Margin
        {
            get => _margin;
            set => Set(ref _margin, value);
        }

        private bool _moveEnabled = false;
        public override bool MoveEnabled
        {
            get => _moveEnabled;
            set => Set(ref _moveEnabled, value);
        }

        public Movable(Thickness margin, Vector size, Editable child = null) : base(child)
        {
            Width = size.X;
            Height = size.Y;

            _margin = margin;
            _minMargin = margin;
        }

        public Movable(Thickness margin, Editable child = null) : base(child)
        {
            _margin = margin;
            _minMargin = margin;
        }

        Vector startPos;
        protected override void MouseClicked(object sender, MouseClickedArgs e)
        {
            if (e.click.action == MOUSEACTION.DOWN && e.click.button == MOUSEBUTTON.LEFT)
            {
                startPos = MouseTracker.I.Position;
                if (MoveEnabled) _isMoving = true;
            }
            else if (_isMoving)
            {
                _isMoving = false;
            }

            base.MouseClicked(sender, e);
        }

        public override void MoveTo(HorizontalAlignment hor, VerticalAlignment ver, Thickness margin)
        {
            Margin = margin != default(Thickness) ? margin : _minMargin;
            base.MoveTo(hor, ver, margin);
            EvaluateMove();
        }

        protected override void MouseMoved(object sender, MouseMovedArgs e)
        {
            if (_isMoving) Move(e);
            base.MouseMoved(sender, e);
        }
        
        private void Move(MouseMovedArgs e)
        {
            var deltaPos = startPos - e.Position;

            Margin = new Thickness(Math.Max(_minMargin.Left, HorizontalAlignment == HorizontalAlignment.Left  ? _margin.Left - deltaPos.X : _minMargin.Left),
                                   Math.Max(_minMargin.Top, VerticalAlignment == VerticalAlignment.Top ? _margin.Top - deltaPos.Y : _minMargin.Top),
                                   Math.Max(_minMargin.Right, HorizontalAlignment == HorizontalAlignment.Right ?  _margin.Right + deltaPos.X : _minMargin.Right),
                                   Math.Max(_minMargin.Bottom, VerticalAlignment == VerticalAlignment.Bottom ? _margin.Bottom + deltaPos.Y : _minMargin.Bottom));

            EvaluateMove();

            startPos = e.Position;
        }

        private void EvaluateMove()
        {
            double width = F.Screen.PrimaryScreen.Bounds.Width;
            if (Margin.Right > width / 2 && HorizontalAlignment != HorizontalAlignment.Left)
            {
                HorizontalAlignment = HorizontalAlignment.Left;
                Margin = new Thickness(width - _margin.Right - Width, _margin.Top, _minMargin.Right, _margin.Bottom);
            }
            if (Margin.Left > width / 2 && HorizontalAlignment != HorizontalAlignment.Right)
            {
                HorizontalAlignment = HorizontalAlignment.Right;
                Margin = new Thickness(_minMargin.Left, _margin.Top, width - _margin.Left - Width, _margin.Bottom);
            }   

            double height = F.Screen.PrimaryScreen.Bounds.Height;
            if (Margin.Top > height / 2 && VerticalAlignment != VerticalAlignment.Bottom)
            {
                VerticalAlignment = VerticalAlignment.Bottom;
                Margin = new Thickness(_margin.Left, _minMargin.Top, _margin.Right, height - _margin.Top - Height);
            }
            if (Margin.Bottom > height / 2 && VerticalAlignment != VerticalAlignment.Top)
            {
                VerticalAlignment = VerticalAlignment.Top;
                Margin = new Thickness(_margin.Left, height - _margin.Bottom - Height, _margin.Right, _minMargin.Bottom);
            }
        }

        public override void Maximize()
        {
            Maximized = true;

            _oldMargin = Margin;
            Margin = new Thickness(0);

            base.Maximize();
        }

        public override void Restore()
        {
            Margin = _oldMargin;

            base.Restore();
        }
    }
}
