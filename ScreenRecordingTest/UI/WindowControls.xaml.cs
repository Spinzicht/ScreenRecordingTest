using Ludio.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ludio.UI
{
    /// <summary>
    /// Interaction logic for WindowControls.xaml
    /// </summary>
    public partial class WindowControls : UserControl
    {
        public Window Window
        {
            get => (Window)GetValue(WindowProperty);
            set => SetValue(WindowProperty, value);
        }

        public static readonly DependencyProperty WindowProperty = 
              DependencyProperty.Register(nameof(Window), typeof(Window), typeof(WindowControls));

        VM.WindowControls vm = new VM.WindowControls();

        public WindowControls()
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void HasLoaded(object sender, RoutedEventArgs e)
        {
            vm.Init(Window);
        }
    }
}
