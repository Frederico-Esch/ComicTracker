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

    public static byte[]? OpenFilePicker(this Window window, List<string> fileExt, out string fileName, PickerLocationId pickerLocationId = PickerLocationId.DocumentsLibrary, PickerViewMode pickerViewMode = PickerViewMode.Thumbnail)
    {
        fileName = string.Empty;
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

        fileName = result.DisplayName;
        using var fileStream = result.OpenReadAsync().GetAwaiter().GetResult();
        if (fileStream == null) return null;

        using var ms = new MemoryStream();
        fileStream.AsStream().CopyTo(ms);
        return ms.ToArray();
    }

    public struct NameAndFile
    {
        public string Name;
        public string Extension;
        public byte[]? Content;
    }

    public static async Task<NameAndFile> OpenFilePickerAsync(this Window window, List<string> fileExt, PickerLocationId pickerLocationId = PickerLocationId.DocumentsLibrary, PickerViewMode pickerViewMode = PickerViewMode.Thumbnail)
    {
        var file = new NameAndFile();
        file.Name = string.Empty;
        var filePicker = new FileOpenPicker()
        {
            SuggestedStartLocation = pickerLocationId,
            ViewMode = pickerViewMode
        };
        foreach (var ext in fileExt)
            filePicker.FileTypeFilter.Add(ext);
        InitializeWithWindow.Initialize(filePicker, WindowNative.GetWindowHandle(window));

        var result = await filePicker.PickSingleFileAsync();
        if (result == null) return file;

        file.Name = result.DisplayName;
        file.Extension = result.FileType.Substring(1);
        using var fileStream = await result.OpenReadAsync();
        if (fileStream == null) return file;

        using var ms = new MemoryStream();
        fileStream.AsStream().CopyTo(ms);
        file.Content = ms.ToArray();
        return file;
    }

    public static async Task<string> OpenSaveFileAsync(this Window window, string fileName, string fileExt, PickerLocationId pickerLocationId = PickerLocationId.DocumentsLibrary, PickerViewMode pickerViewMode = PickerViewMode.Thumbnail)
    {
        var filePicker = new FileSavePicker()
        {
            SuggestedStartLocation = pickerLocationId,
            SuggestedFileName = $"{fileName}.{fileExt.ToLower()}",
            CommitButtonText = "Save",
            FileTypeChoices = {
                new ("Comic File Type", [$".{fileExt.ToLower()}"])
            }
        };
        InitializeWithWindow.Initialize(filePicker, WindowNative.GetWindowHandle(window));
        var result = await filePicker.PickSaveFileAsync();

        return result?.Path ?? string.Empty;
    }
}
