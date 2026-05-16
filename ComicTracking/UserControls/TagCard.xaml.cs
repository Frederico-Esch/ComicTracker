using Domain;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Services.Maps;

namespace ComicTracking.UserControls;

public sealed partial class TagCard : UserControl
{
    public TagCard()
    {
        InitializeComponent();
    }

    private void UCLoaded(object sender, RoutedEventArgs e)
    {
        DataContextChanged += (_, _) => LoadTag();
        LoadColors();
    }

    public void SetTag(Tag tag)
    {
        DataContext = tag;
    }

    private void LoadTag()
    {
        if (DataContext is not Tag _) return;
        LoadColors();
    }

    private void LoadColors()
    {
        if (DataContext is not Tag tag) return;

        var gradient = new GradientStopCollection();

        gradient.Add(new GradientStop() { Color = tag.Color1, Offset = 0.5 });
        gradient.Add(new GradientStop() { Color = tag.Color2, Offset = 0.5 });


        var brush = new LinearGradientBrush();
        brush.GradientStops = gradient;

        brush.StartPoint = new(0.25, 0);
        brush.EndPoint = new(0.75, 1);

        BackgroundElement.Background = brush;

        TagName.Text = tag.Name;
        TagName.Foreground = new SolidColorBrush(tag.TextColor);
    }
}
