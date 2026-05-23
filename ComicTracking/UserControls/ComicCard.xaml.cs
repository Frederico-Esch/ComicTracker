using Domain;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Numerics;
using Utils.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ComicTracking.UserControls
{
    public sealed partial class ComicCard : UserControl
    {
        #region Animation Properties
        private float coverScaleValue = 1;
        public float CoverScaleValue
        {
            get => coverScaleValue;
            set
            {
                coverScaleValue = value;
                SetValue(CoverScaleProperty, CoverScale);
                SetValue(CoverTranslateProperty, CoverTranslate);
            }
        }
        public Vector3 CoverScale => new Vector3(CoverScaleValue);
        public static readonly DependencyProperty CoverScaleProperty =
            DependencyProperty.Register("CoverScale", typeof(Vector3), typeof(ComicCard), new PropertyMetadata(new Vector3(1)));

        public Vector3 CoverTranslate => new Vector3((1 - coverScaleValue) * (float)CoverImage.ActualWidth / 2, (1 - coverScaleValue) * (float)CoverImage.ActualHeight / 2, 0);
        public static readonly DependencyProperty CoverTranslateProperty =
            DependencyProperty.Register("CoverTranslate", typeof(Vector3), typeof(ComicCard), new PropertyMetadata(new Vector3(0)));
        #endregion

        public ComicCard()
        {
            InitializeComponent();
        }

        private void LoadData()
        {
            if (DataContext is not Comic comic) return;

            if (comic.Cover is not { } array || array.Length == 0)
            {
                CoverImage.Source = Resources["DefaultCover"] as BitmapImage;
            }
            else
            {
                var image = new BitmapImage();
                image.CreateFrom(array);
                CoverImage.Source = image;
            }

            StatusPanel.Background = App.Current.Resources[comic.Status.ToString()] as SolidColorBrush;
        }

        private void DataContextLoaded(FrameworkElement sender, DataContextChangedEventArgs e)
        {
            LoadData();
        }

        private void LoadedControl(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private enum StyleEffect
        {
            Normal,
            Hover,
            Pressed
        }
        private void ApplyEffect(StyleEffect effect)
        {
            switch (effect)
            {
                case StyleEffect.Normal:
                    VisualStateManager.GoToState(this, "Normal", true);
                    CoverScaleValue = 1;
                    break;
                case StyleEffect.Hover:
                    VisualStateManager.GoToState(this,  "Hover", true);
                    CoverScaleValue = 0.8f;
                    break;
                case StyleEffect.Pressed:
                    VisualStateManager.GoToState(this, "Pressed", true);
                    CoverScaleValue = 1;
                    break;
            }
        }
        private void HoverEffect(bool active)
        {
            VisualStateManager.GoToState(this, active ? "Hover" : "Normal", true);
            CoverScaleValue = active ? 0.8f : 1;
        }

        private void Entered(object sender, PointerRoutedEventArgs e)
        {
            //HoverEffect(true);
            ApplyEffect(StyleEffect.Hover);
        }

        private void Exited(object sender, PointerRoutedEventArgs e)
        {
            //HoverEffect(false);
            ApplyEffect(StyleEffect.Normal);
        }

        private void Clicked(object sender, PointerRoutedEventArgs e)
        {
            ApplyEffect(StyleEffect.Pressed);
        }

        private void Released(object sender, PointerRoutedEventArgs e)
        {
            ApplyEffect(StyleEffect.Hover);
        }
    }
}
