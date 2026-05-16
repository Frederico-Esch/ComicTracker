using Microsoft.UI.Xaml;
using Domain;
using Microsoft.UI.Xaml.Media.Imaging;
using Utils.Extensions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Input;
using Windows.UI.Core;
using Persistence.Repositories;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using System;
using WinRT;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;
using System.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ComicTracking.Windows;

public sealed partial class EditComicWindow : Window
{
    private Comic Comic { get; set; }
    private InputCursor defaultCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    private InputCursor handCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);

    private readonly ITagRepository tagRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IComicRepository comicRepository;
    private string? currentSelectedFilter = null;
    private string? currentAvailableFilter = null;

    public EditComicWindow(ITagRepository _tagRepository, IComicRepository _comicRepository, IUnitOfWork _unitOfWork)
    {
        tagRepository = _tagRepository;
        comicRepository = _comicRepository;
        unitOfWork = _unitOfWork;
        Comic = new Comic();

        InitializeComponent();

        UpdateTags();
    }

    public void SetComic(Comic comic)
    {
        Comic = comic;
        NameBox.Text = comic.Name;
        DetBox.Text = comic.Details;
        DeleteButton.Visibility = Visibility.Visible;
        SaveButton.Visibility = Visibility.Visible;
        AddButton.Visibility = Visibility.Collapsed;
        switch (comic.Status)
        {
            case ComicStatus.Unread:
                UnreadButton.IsChecked = true;
                break;
            case ComicStatus.Progress:
                ProgressButton.IsChecked = true;
                break;
            case ComicStatus.Finished:
                FinishedButton.IsChecked = true;
                break;
        }
        UpdateCover();
        UpdateTags();
    }

    private void UpdateTags()
    {
        var comicTags = Comic.Tags.Select(t => t.Id).ToHashSet();
        SelectedTags.ItemsSource =
            string.IsNullOrEmpty(currentSelectedFilter)
            ? Comic.Tags.ToList()
            : Comic.Tags.Where(t => t.Name.Contains(currentSelectedFilter, StringComparison.InvariantCultureIgnoreCase)).ToList();

        AvailableTags.ItemsSource = tagRepository.Exclude(comicTags, currentAvailableFilter);
    }

    private void UpdateCover()
    {
        if (Comic.Cover is { } cover && cover.Length > 0)
        {
            var image = new BitmapImage();
            image.CreateFrom(cover);
            CoverImage.Source = image;
        }
        else
        {
            CoverImage.Source = RootGrid.Resources["DefaultImage"] as BitmapImage;
        }
    }

    private bool TrySaveData()
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return false;
        Comic.Name = name;

        var details = DetBox.Text.Trim();
        Comic.Details = details;

        return true;
    }

    private void ApplyFilters()
    {
        var selectedFilter = SelectedFilter.Text.Trim();
        currentSelectedFilter = string.IsNullOrWhiteSpace(selectedFilter) ? null : selectedFilter;

        var availableFilter = AvailableFilter.Text.Trim();
        currentAvailableFilter = string.IsNullOrWhiteSpace(availableFilter) ? null : availableFilter;
        UpdateTags();
    }

    #region Buttons

    private void ChangeImage(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var prop = e.GetCurrentPoint(sender as UIElement).Properties;
        if (prop.IsLeftButtonPressed)
        {
            var path = this.OpenFilePicker([".jpg", ".png", ".jpeg", ".webp"]);
            if (path is not { } coverBytes || coverBytes.Length == 0) return;

            Comic.Cover = coverBytes;
        }
        else if(prop.IsRightButtonPressed)
        {
            Comic.Cover = null;
        }
        UpdateCover();
    }

    private void CheckedRadioButton(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;

        button.Background = App.Current.Resources[button.Tag ?? "Unread"] as SolidColorBrush;
        button.Foreground = new SolidColorBrush(Colors.Black);

        Comic.Status = Enum.Parse<ComicStatus>(button.Tag as string ?? "Unread");
    }

    private void Save(object sender, RoutedEventArgs e)
    {
        if (!TrySaveData()) return;
        unitOfWork.Save();
        Close();
    }

    private void Delete(object sender, RoutedEventArgs e)
    {
        comicRepository.Delete(Comic);
        unitOfWork.Save();
        Close();
    }

    private void Add(object sender, RoutedEventArgs e)
    {
        if (!TrySaveData()) return;
        comicRepository.Add(Comic);
        unitOfWork.Save();
        Close();
    }

    #endregion

    #region Styling

    private void UncheckedRadioButton(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        button.Background = new SolidColorBrush(Colors.Transparent);
        button.Foreground = new SolidColorBrush(Colors.White);
    }

    private void EnteredRadioButton(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;

        button.BorderBrush = App.Current.Resources[button.Tag] as SolidColorBrush;
        button.ChangeCursor(handCursor);
    }

    private void ExitedRadioButton(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;

        button.BorderBrush = new SolidColorBrush(Colors.Transparent);
        button.ChangeCursor(defaultCursor);
    }

    private void EnteredImage(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Image image) return;
        image.ChangeCursor(handCursor);
    }

    private void ExitedImage(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Image image) return;
        image.ChangeCursor(defaultCursor);
    }
    #endregion

    private void AddTag(object sender, RoutedEventArgs e)
    {
        if (AvailableTags.SelectedItem is not Tag tag) return;
        Comic.Tags.Add(tag);
        UpdateTags();
    }

    private void RemoveTag(object sender, RoutedEventArgs e)
    {
        if (SelectedTags.SelectedItem is not Tag tag) return;
        Comic.Tags.Remove(tag);
        UpdateTags();
    }

    #region Drag n Drop
    private async void IsDragging(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains("origin")) return;
        if (sender is not ListView listView) return;

        var originName = await e.DataView.GetTextAsync("origin").AsTask();
        if (listView.Name == originName) return;

        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private void DragStarted(object sender, DragItemsStartingEventArgs e)
    {
        if (sender is ListView listView)
        e.Data.SetData("origin", listView.Name);
        e.Data.RequestedOperation = DataPackageOperation.Move;
    }

    private void DragComplete(ListViewBase sender, DragItemsCompletedEventArgs e)
    {
        if (e.DropResult == DataPackageOperation.None) return;
        if (e.Items.Cast<Tag>().FirstOrDefault() is not Tag tag) return;

        if (sender == AvailableTags)
        {
            Comic.Tags.Add(tag);
        }
        else
        {
            Comic.Tags.Remove(tag);
        }
        UpdateTags();
    }
    #endregion

    private void KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            ApplyFilters();
        }
    }

    private async void OnClosed(object sender, WindowEventArgs e)
    {
        if (unitOfWork.HasChanges())
        {
            e.Handled = true;

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
                default: break;
            }
            Close();
        }
    }
}
