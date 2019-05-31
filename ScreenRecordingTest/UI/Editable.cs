using Chronos;
using Chronos.Tracking;
using Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ludio.UI
{
    public class Editable : Observable
    {
        Editable Child;
        Editable Parent;

        //scaling base vars
        public virtual bool ScaleEnabled { get; set; }

        private double _width;
        public virtual double Width
        {
            get { return Child != null ? Child.Width : _width; }
            set
            {
                if (Child != null) Child.Width = value;
                else _width = value;
            }
        }

        private double _height;
        public virtual double Height
        {
            get { return Child != null ? Child.Height : _height; }
            set
            {
                if (Child != null) Child.Height = value;
                else _height = value;
            }
        }


        public virtual bool Maximized
        {
            get { return Child != null ? Child.Maximized : false; }
            set
            {
                if (Child != null) Child.Maximized = value;
            }
        }

        //moving base vars
        public virtual bool MoveEnabled {
            get { return Child != null ? Child.MoveEnabled : false; }
            set { if (Child != null) Child.MoveEnabled = value; }
        }

        public virtual Thickness Margin { get { return Child != null ? Child.Margin : default(Thickness); }
                                          set { if (Child != null) Child.Margin = value; } }

        public virtual VerticalAlignment VerticalAlignment { get { return Child != null ? Child.VerticalAlignment : VerticalAlignment.Center; }
                                                             set { if (Child != null) Child.VerticalAlignment = value; } }

        public virtual HorizontalAlignment HorizontalAlignment { get { return Child != null ? Child.HorizontalAlignment : HorizontalAlignment.Center; }
                                                                 set { if (Child != null) Child.HorizontalAlignment = value; } }

        public Editable(Editable child = null)
        {
            Child = child;
            if (Child != null) Child.Parent = this;

            MouseTracker.I.OnMouseClicked += MouseClicked;
            MouseTracker.I.OnMouseMoved += MouseMoved;
        }

        protected virtual void MouseClicked(object sender, MouseClickedArgs e)
        {
            if (Child != null) Child.MouseClicked(sender, e);
        }

        protected virtual void MouseMoved(object sender, MouseMovedArgs e)
        {
            if (Child != null) Child.MouseMoved(sender, e);
        }

        public virtual void MoveTo(HorizontalAlignment hor, VerticalAlignment ver, Thickness margin)
        {
            if (Child != null) Child.MoveTo(hor, ver, margin);
        }


        public virtual void Maximize()
        {
            if (Child != null) Child.Maximize();
        }

        public virtual void Restore()
        {
            if (Child != null) Child.Restore();
        }

        protected override bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            var done = base.Set(ref field, value, propertyName);

            UpdateParent(propertyName, value);

            return done;
        }

        private void UpdateParent<T>(string propertyName, T value)
        {
            if (Parent == null) return;

            var Property = Parent.GetType().GetProperty(propertyName);
            var parentValue = (T)Property.GetValue(Parent);

            if(!EqualityComparer<T>.Default.Equals(parentValue, value))
                Property.SetValue(Parent, value);
            else
                Parent.Notify(propertyName);

            Parent.UpdateParent(propertyName, value);
        }
    }
}
