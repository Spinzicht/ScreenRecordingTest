using Core.Data;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Ludio.Base
{
    //One value converters

    public abstract class MarkupConverter : MarkupExtension, IValueConverter
    {
        public bool Not { get; set; } = false;

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public abstract class OneWayConverter : MarkupConverter
    {
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToImageConverter : OneWayConverter
    {
        public string True { get; set; }

        public string False { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? True : False;
        }
    }

    public class BoolToVisibility : MarkupConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Not ^ (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible ? Not | true : Not | false;
        }
    }

    public abstract class UserConverter : OneWayConverter
    {
        public User User { get; set; }
    }

    public class ActiveUserToVisibility : UserConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Not ^ ((int)value) == User.ID ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class UserRoleToVisibility : OneWayConverter
    {
        public Role Role { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Not ^ ((Role)value) == Role ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    
    public abstract class ChannelConverter : OneWayConverter
    {
        public Channel Channel { get; set; }
    }

    public class ChannelStateToVisibility : ChannelConverter
    {
        public ChannelState State { get; set; }
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Not ^ ((ChannelState)value) == State ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class UserChannelToVisibility : UserConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Not ^ ((int)value) == User.Channel ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    //Multi value converters
    public abstract class MultiMarkupConverter : MarkupExtension, IMultiValueConverter
    {
        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
        public abstract object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
        public override object ProvideValue(IServiceProvider serviceProvider) { return this; }
    }

    public abstract class MultiOneWayConverter : MultiMarkupConverter
    {
        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiVisibilityConverter : MultiOneWayConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.All(x => x != DependencyProperty.UnsetValue && (Visibility)x == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    public class BoolToStaticStringConverter : OneWayConverter
    {
        public string True { get; set; }

        public string False { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? True : False;
        }
    }

    public class BitmapToImageSourceConverter : OneWayConverter
    {
        Bitmap bmp;
        BitmapImage bmpImg;

        public override object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var bmpNew = (Bitmap)value;
            if (bmpNew == null) return bmp;

            bmp = (Bitmap)bmpNew.Clone();

            using (MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                bmpImg = new BitmapImage();
                bmpImg.BeginInit();
                bmpImg.StreamSource = memory;
                bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                bmpImg.EndInit();

                bmpImg.Freeze();
            }

            return bmpImg;
        }
    }

    public class IsTopAligned : OneWayConverter
    {
        public override object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var vdone = Enum.TryParse(value.ToString(), out VerticalAlignment ver);
            if (vdone) return ver == VerticalAlignment.Top;
            return false;
        }
    }

    public class IsBottomAligned : OneWayConverter
    {
        public override object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var vdone = Enum.TryParse(value.ToString(), out VerticalAlignment ver);
            if (vdone) return ver == VerticalAlignment.Bottom;
            return false;
        }
    }

    public class IsLeftAligned : OneWayConverter
    {
        public override object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var done = Enum.TryParse(value.ToString(), out HorizontalAlignment hor);
            if (done) return hor == HorizontalAlignment.Left;
            return false;
        }
    }

    public class IsRightAligned : OneWayConverter
    {
        public override object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var done = Enum.TryParse(value.ToString(), out HorizontalAlignment hor);
            if (done) return hor == HorizontalAlignment.Right;
            return false;
        }
    }
    public class DoubleBoolToVisibilityConverter : MultiMarkupConverter
    {
        public override object Convert(object[] value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var isTrue = (bool)value[0] && (bool)value[1];

            if (isTrue) return Visibility.Visible;
            else return Visibility.Collapsed;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}