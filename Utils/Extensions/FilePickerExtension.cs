using Microsoft.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Utils.Extensions;

public static class FilePickerExtension
{

    public static byte[]? OpenFilePicker(this Window window, List<string> fileExt, PickerLocationId pickerLocationId = PickerLocationId.DocumentsLibrary, PickerViewMode pickerViewMode = PickerViewMode.Thumbnail)
    {
        var filePicker = new FileOpenPicker()
        {
            SuggestedStartLocation = pickerLocationId,
            ViewMode = pickerViewMode
        };
        foreach (var ext in fileExt)
            filePicker.FileTypeFilter.Add(ext);
        InitializeWithWindow.Initialize(filePicker, WindowNative.GetWindowHandle(window));

        var result = filePicker.PickSingleFileAsync().GetAwaiter().GetResult();
        if (result == null) return null;

        using var fileStream = result.OpenReadAsync().GetAwaiter().GetResult();
        if (fileStream == null) return null;

        using var ms = new MemoryStream();
        fileStream.AsStream().CopyTo(ms);
        return ms.ToArray();
    }

}
