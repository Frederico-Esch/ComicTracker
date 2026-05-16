using Microsoft.UI.Xaml.Media.Imaging;

namespace Utils.Extensions;

public static class BitmapImageExtension
{
    public static void CreateFrom(this BitmapImage image, byte[] imageBytes)
    {
        using var  ms = new MemoryStream(imageBytes);
        image.SetSource(ms.AsRandomAccessStream());
        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
    }
}
