using Microsoft.UI.Xaml;
using Domain;
using Microsoft.UI.Xaml.Media.Imaging;
using Utils.Extensions;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI.WebUI;
using System;
using Microsoft.UI.Input;
using Persistence.Repositories;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Media.Animation;
using Utils.Collections;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Windowing;
using System.IO;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ComicTracking.Windows;

public sealed partial class ComicWindow : Window, INotifyPropertyChanged
{
    public Comic DataContext
    {
        get => (Comic)Root.DataContext;
        set
        {
            Root.DataContext = value;
            UpdatedDataContext();
            NotifyPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Brushes
    private SolidColorBrush transparentBrush = new SolidColorBrush(Colors.Transparent);
    private SolidColorBrush grayBrush = new SolidColorBrush(Colors.Gray);
    private SolidColorBrush unreadBrush = App.Current.Resources["Unread"] as SolidColorBrush ?? new SolidColorBrush(Colors.Salmon);
    private SolidColorBrush progressBrush = App.Current.Resources["Progress"] as SolidColorBrush ?? new SolidColorBrush(Colors.Goldenrod);
    private SolidColorBrush finishedBrush = App.Current.Resources["Finished"] as SolidColorBrush ?? new SolidColorBrush(Colors.LightGreen);
    #endregion

    private InputCursor defaultCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    private InputCursor handCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    private ReorderableCollection<ComicFile> files = [];

    private IUnitOfWork unitOfWork;
    private IComicRepository comicRepository;

    public ComicWindow(IComicRepository _comicRepository, IUnitOfWork _unitOfWork)
    {
        unitOfWork = _unitOfWork;
        comicRepository = _comicRepository;
        InitializeComponent();
    }

    private void UpdatedDataContext()
    {
        if (DataContext.Cover is { } cover && cover.Length > 0)
        {
            var image = new BitmapImage();
            image.CreateFrom(cover);
            CoverImage.Source = image;
        }
        else
        {
            CoverImage.Source = Root.Resources["DefaultCover"] as BitmapImage;
        }
        ChangeStatus();
        LoadFiles();
    }

    private async Task StartLoad()
    {
        ProgressBackground.Visibility = Visibility.Visible;
        var min = Math.Min(this.AppWindow.Size.Width, this.AppWindow.Size.Width);
        Progress.Width = min / 20;
        Progress.Height =  min / 20;
        Content.IsHitTestVisible = false;
        await Task.Delay(50);
    }

    private void EndLoad()
    {
        Content.IsHitTestVisible = true;
        ProgressBackground.Visibility = Visibility.Collapsed;
    }

    #region Hover
    private void Hover(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Grid grid) return;

        UnreadBox.Visibility = Visibility.Visible;
        ProgressBox.Visibility = Visibility.Visible;
        FinishedBox.Visibility = Visibility.Visible;
        //ProgressBox.Scale = new System.Numerics.Vector3(1);
        grid.Height = double.NaN;
        grid.ChangeCursor(handCursor);
        //TextBlockWidth = double.NaN;
        UnreadBorder.Width = double.NaN;
        ProgressBorder.Width = double.NaN;
        FinishedBorder.Width = double.NaN;

        switch (DataContext.Status)
        {
            case ComicStatus.Unread:
                ProgressBorder.BorderBrush = progressBrush;
                FinishedBorder.BorderBrush = finishedBrush;
                break;
            case ComicStatus.Progress:
                UnreadBorder.BorderBrush = unreadBrush;
                FinishedBorder.BorderBrush = finishedBrush;
                break;
            case ComicStatus.Finished:
                UnreadBorder.BorderBrush = unreadBrush;
                ProgressBorder.BorderBrush = progressBrush;
                break;
        }
    }

    private void HoverEnded(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Grid grid) return;

        UnreadBox.Visibility = Visibility.Collapsed;
        ProgressBox.Visibility = Visibility.Collapsed;
        FinishedBox.Visibility = Visibility.Collapsed;
        //ProgressBox.Scale = new System.Numerics.Vector3(0);
        grid.Height = 35;
        grid.ChangeCursor(defaultCursor);
        //TextBlockWidth = 50;
        UnreadBorder.Width = 50;
        ProgressBorder.Width = 50;
        FinishedBorder.Width = 50;

        UnreadBorder.BorderBrush = transparentBrush;
        ProgressBorder.BorderBrush = transparentBrush;
        FinishedBorder.BorderBrush = transparentBrush;
    }
    #endregion

    #region Loading Functions
    private void ChangeStatus()
    {
        UnreadBorder.Background   = grayBrush;
        ProgressBorder.Background = grayBrush;
        FinishedBorder.Background = grayBrush;

        switch (DataContext.Status)
        {
            case ComicStatus.Unread:
                UnreadBorder.Background = unreadBrush;
                break;
            case ComicStatus.Progress:
                ProgressBorder.Background = progressBrush;
                break;
            case ComicStatus.Finished:
                FinishedBorder.Background = finishedBrush;
                break;
        }
    }
    private void ChangeStatus(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Border border) return;
        var newStatus = Enum.Parse<ComicStatus>(border.Tag as string ?? "");
        DataContext.Status = newStatus;
        SaveButton.Visibility = Visibility.Visible;
        ChangeStatus();
    }
    private void LoadFiles()
    {
        var orderedFiles = DataContext.Files.OrderBy(f => f.Order);
        var unread = orderedFiles.FirstOrDefault(f => !f.IsFinished);
        files = new ReorderableCollection<ComicFile>(orderedFiles);
        files.OnElementInserted += (collection, item, index) => {
            var oldOrder = -1*item.Order;
            if (oldOrder < 0) return;

            var order = index + 1;
            var min = Math.Min(oldOrder, order);
            var max = Math.Max(oldOrder, order);
            var dir = oldOrder > order ? 1 : -1;
            foreach (var c in collection.Where(c => c.Order >= min && c.Order <= max).OrderBy(c => c.Order))
                c.Order += dir;
            item.Order = order;
            unitOfWork.Save();
        };
        files.OnElementRemoved += (collection, item, index) => { item.Order *= -1; };
        ListFiles.ItemsSource = files;
        if (unread is not null)
        {
            ListFiles.ScrollIntoView(unread);
            ListFiles.SelectedItem = unread;
        }
    }
    #endregion

    #region Buttons
    private void EditClick(object sender, RoutedEventArgs e)
    {
        DetailsBox.IsReadOnly = false;
        EditButton.Visibility = Visibility.Collapsed;
        AcceptButton.Visibility = Visibility.Visible;
        DiscardButton.Visibility = Visibility.Visible;
    }

    private void AcceptClick(object sender, RoutedEventArgs e)
    {
        DetailsBox.IsReadOnly = true;
        EditButton.Visibility = Visibility.Visible;
        AcceptButton.Visibility = Visibility.Collapsed;
        DiscardButton.Visibility = Visibility.Collapsed;

        DataContext.Details = DetailsBox.Text.Trim();
        SaveButton.Visibility = Visibility.Visible;
    }

    private void DiscardClick(object sender, RoutedEventArgs e)
    {
        DetailsBox.IsReadOnly = true;
        EditButton.Visibility = Visibility.Visible;
        AcceptButton.Visibility = Visibility.Collapsed;
        DiscardButton.Visibility = Visibility.Collapsed;

        DetailsBox.Text = DataContext.Details;
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        unitOfWork.Save();
        SaveButton.Visibility = Visibility.Collapsed;
    }

    private async void AddFile(object sender, RoutedEventArgs e)
    {
        await StartLoad();
        var file = await this.OpenFilePickerAsync([".cbz"]);
        if (file.Content is not byte[] content || content.Length == 0)
        {
            EndLoad();
            return;
        }
        
        comicRepository.AddFile(content, file.Name, DataContext);
        await unitOfWork.SaveAsync();
        DataContext = comicRepository.FindOne(DataContext.Id)!;
        EndLoad();
    }

    private async void DeleteFile(object sender, RoutedEventArgs e)
    {
        if (ListFiles.SelectedItem is not ComicFile file) return;

        var dialog = new ContentDialog()
        {
            Title = "Are you shure?",
            Content = $"Are you sure you want to delete {file.Name}",
            CloseButtonText = "Cancel",
            PrimaryButtonText = "Delete",
            IsSecondaryButtonEnabled = false,
            DefaultButton = ContentDialogButton.Close
        };
        dialog.XamlRoot = Content.XamlRoot;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.None) return;

        await StartLoad();
        comicRepository.DeleteFile(file);
        await unitOfWork.SaveAsync();
        DataContext = comicRepository.FindOne(DataContext.Id)!;
        EndLoad();
    }

    private void ReadFile(object sender, RoutedEventArgs e)
    {
        if (ListFiles.SelectedItem is not ComicFile file) return;
        file.IsFinished = !file.IsFinished;
        unitOfWork.Save();
        LoadFiles();
        //await Task.Delay(2000);
    }

    private async void OpenFile(object sender, RoutedEventArgs e)
    {
        if (ListFiles.SelectedItem is not ComicFile file) return;
        await StartLoad();

        var data = await comicRepository.LoadComicDataAsync(file);
        if (data.Length <= 0) goto end;


        var path = ApplicationData.Current.TemporaryFolder.Path;
        await Utils.ComicDisplayer.DisplayComic(path, file.Name, data);
    end:
        EndLoad();
    }
    #endregion

    #region ListView style
    private void SymbolLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not SymbolIcon {DataContext: ComicFile file } symbol) return;

        symbol.Visibility = file.IsFinished ? Visibility.Visible : Visibility.Collapsed;
    }
    #endregion

    private void FileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not ComicFile file)
        {
            ReadButton.Visibility = Visibility.Collapsed;
            OpenButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
        }
        else {
            ReadButtonIcon.Foreground = file.IsFinished ? unreadBrush : finishedBrush;
            ReadButton.Visibility = Visibility.Visible;
            OpenButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
        }
    }

    private async void OnClosing(object sender, WindowEventArgs e)
    {
        if (unitOfWork.HasChanges())
        {
            var dialog = new ContentDialog()
            {
                Title = "Unsaved changes",
                Content = "Save changes before exiting?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Discard",
                DefaultButton = ContentDialogButton.Close
            };
            dialog.XamlRoot = Content.XamlRoot;
            e.Handled = true;

            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.None: return;
                case ContentDialogResult.Primary: unitOfWork.Save(); break;
                default: unitOfWork.DiscardChanges(); break;
            }
            Close();
        }
    }
}
