using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ludio.UI
{
    class Alignable : Editable
    {
        private VerticalAlignment _vAlign;
        private VerticalAlignment _vOldAlign;
        public override VerticalAlignment VerticalAlignment
        {
            get => _vAlign;
            set => Set(ref _vAlign, value);
        }

        private HorizontalAlignment _hOldAlign;
        private HorizontalAlignment _hAlign;
        public override HorizontalAlignment HorizontalAlignment
        {
            get => _hAlign;
            set => Set(ref _hAlign, value);
        }

        public Alignable(VerticalAlignment vAlign, HorizontalAlignment hAlign)
        {
            _vAlign = vAlign;
            _hAlign = hAlign;
        }

        public override void MoveTo(HorizontalAlignment hor, VerticalAlignment ver, Thickness margin)
        {
            VerticalAlignment = ver;
            HorizontalAlignment = hor;

            base.MoveTo(hor, ver, margin);
        }

        public override void Maximize()
        {
            _hOldAlign = _hAlign;
            HorizontalAlignment = HorizontalAlignment.Center;
            _vOldAlign = _vAlign;
            VerticalAlignment = VerticalAlignment.Center;
            base.Maximize();
        }

        public override void Restore()
        {
            HorizontalAlignment = _hOldAlign;
            VerticalAlignment = _vOldAlign;
            base.Restore();
        }
    }
}
