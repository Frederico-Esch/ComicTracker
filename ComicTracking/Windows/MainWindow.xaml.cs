using Domain;
using Microsoft.UI.Xaml;
using Persistence.Repositories;
using Utils.Collections;
using System.Linq;
using Utils.Extensions;
using Windows.UI.Popups;
using System;
using WinRT.Interop;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ComicTracking.Windows;

public sealed partial class MainWindow : Window
{

    private IComicRepository comicRepository;
    private IUnitOfWork unitOfWork;
    IServiceProvider serviceProvider;
    private ReorderableCollection<Comic> comics = [];
    private List<Tag> FilteredTags = [];


    public MainWindow(IServiceProvider _serviceProvider, IComicRepository _comicRepository, IUnitOfWork _unitOfWork)
    {
        serviceProvider = _serviceProvider;
        comicRepository = _comicRepository;
        unitOfWork = _unitOfWork;

        InitializeComponent();
        ReloadComics();
    }

    private void ReloadComics()
    {
        comics =
            FilteredTags.Count == 0
            ? new(comicRepository.GetAllComics())
            : new(comicRepository.GetFiltered(FilteredTags));
        comics.OnElementInserted += (collection, item, index) => {
            var oldOrder = -1*item.Order;
            if (oldOrder < 0) return;

            var order = index + 1;
            var min = Math.Min(oldOrder, order);
            var max = Math.Max(oldOrder, order);
            var dir = oldOrder > order ? 1 : -1;
            foreach (var c in collection.Where(c => c.Order >= min && c.Order <= max).OrderBy(c => c.Order))
                c.Order += dir;
            item.Order = order;
            unitOfWork.ScheduleSave();
        };
        comics.OnElementRemoved += (_, item, index) => { item.Order *= -1;};
        ComicList.ItemsSource = comics;
    }

    #region Buttons
    private void ButtonReload(object sender, RoutedEventArgs e)
    {
        ReloadComics();
    }

    private void CreateComic(object sender, RoutedEventArgs e)
    {
        var window = serviceProvider.GetRequiredService<EditComicWindow>();
        this.NavigateTo(window, ReloadComics);
    }

    private void ButtonManageTags(object sender, RoutedEventArgs e)
    {
        var window = serviceProvider.GetRequiredService<ManageTagsWindow>();
        this.NavigateTo(window);
    }

    private void FilterTags(object sender, RoutedEventArgs e)
    {
        var window = serviceProvider.GetRequiredService<FilterTagsWindow>();
        if (FilteredTags.Count > 0)
            window.SetInitialTags(FilteredTags);
        this.NavigateTo(window, () =>
        {
            FilteredTags = window.TagsSelected;
            ReloadComics();
        });
    }

    private async void ContextDelete(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: Comic comic }) return;

        var dialog = new ContentDialog()
        {
            Title = "Delete",
            Content = $"Are you sure you want to delete \"{comic.Name}\"?",
            CloseButtonText = "Cancel",
            PrimaryButtonText = "Delete",
            DefaultButton = ContentDialogButton.Close
        };
        dialog.XamlRoot = WindowXamlRoot.XamlRoot;
        var result = await dialog.ShowAsync();

        switch (result)
        {
            default:
                break;
            case ContentDialogResult.Primary:
                comicRepository.Delete(comic);
                unitOfWork.Save();
                ReloadComics();
                break;
        }
    }

    private void ContextEdit(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { DataContext: Comic comic }) return;
        var window = serviceProvider.GetRequiredService<EditComicWindow>();
        window.SetComic(comic);
        this.NavigateTo(window, ReloadComics);
    }

    private void ContextOpen(object sender, RoutedEventArgs e)
    {
    }

    #endregion

    #region Closing
    private async void WindowClosed(object sender, WindowEventArgs e)
    {
        if (!unitOfWork.HasChanges()) return;

        var dialog = new ContentDialog()
        {
            Title = "Changes unsaved",
            Content = "Save changes before exiting?",
            CloseButtonText = "Cancel",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Discard",
            DefaultButton = ContentDialogButton.Close
        };
        dialog.XamlRoot = WindowXamlRoot.XamlRoot;
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
    #endregion
}
