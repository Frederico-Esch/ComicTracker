using Domain;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ComicTracking.UserControls
{
    public sealed partial class VolumeCard : UserControl
    {


        public Visibility ComicFileVisibility
        {
            get { return (Visibility)GetValue(ComicFileVisibilityProperty); }
            set { SetValue(ComicFileVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ComicFileVisibilityProperty =
            DependencyProperty.Register("ComicFileVisibility", typeof(Visibility), typeof(VolumeCard), new PropertyMetadata(Visibility.Collapsed));

        public bool ComicFileIsFinished
        {
            get { return (bool)GetValue(ComicFileIsFinishedProperty); }
            set {
                ComicFileVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                SetValue(ComicFileIsFinishedProperty, value);
            }
        }
        public static readonly DependencyProperty ComicFileIsFinishedProperty =
            DependencyProperty.Register("ComicFileIsFinished", typeof(bool), typeof(VolumeCard), new PropertyMetadata(false));

        public string ComicFileName
        {
            get { return (string)GetValue(ComicFileNameProperty); }
            set { SetValue(ComicFileNameProperty, value); }
        }
        public static readonly DependencyProperty ComicFileNameProperty =
            DependencyProperty.Register("ComicFileName", typeof(string), typeof(VolumeCard), new PropertyMetadata(string.Empty));

        public VolumeCard()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => {
                if (DataContext is ComicFile file)
                {
                    CFIsFinished.Visibility = file.IsFinished ? Visibility.Visible : Visibility.Collapsed;
                    CFName.Text = file.Name;
                }
            };
        }

        private void UCLoaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
