using Domain;
using Microsoft.UI.Xaml;
using Persistence.Repositories;
using Utils.Collections;
using System.Linq;
using Utils.Extensions;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using FilterType = Persistence.Repositories.IComicRepository.FilterType;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ComicTracking.Windows;

public sealed partial class MainWindow : Window
{

    private IComicRepository comicRepository;
    private IUnitOfWork unitOfWork;
    IServiceProvider serviceProvider;
    private ReorderableCollection<Comic> comics = [];
    private string? searchTerm = null;
    private List<Tag> filteredTags = [];
    private FilterType filterType = FilterType.Any;


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
        var query = filteredTags.Count == 0
            ? comicRepository.GetAllComics()
            : comicRepository.GetFiltered(filteredTags, filterType);

        if (searchTerm is { } term)
        {
            query = query.Where(c => c.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        comics = new(query);
    
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
        ComicList.CanReorderItems = filteredTags.Count == 0 && searchTerm is null;
    }

    #region Search
    private void SearchKey(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            var search = SearchTextBox.Text.Trim();
            searchTerm = string.IsNullOrWhiteSpace(search)
                ? null
                : search;
            ReloadComics();
        }
    }
    #endregion

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
        if (filteredTags.Count > 0)
            window.SetInitialTags(filteredTags, filterType);
        this.NavigateTo(window, () =>
        {
            filteredTags = window.SelectedTags;
            filterType = window.Filter;
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
        if (sender is not MenuFlyoutItem { DataContext: Comic comic }) return;
        var window = serviceProvider.GetRequiredService<ComicWindow>();
        window.DataContext = comic;
        this.NavigateTo(window, ReloadComics);
    }

    private void OpenComic(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Comic comic) return;
        var window = serviceProvider.GetRequiredService<ComicWindow>();
        window.DataContext = comic;
        this.NavigateTo(window, ReloadComics);
    }

    #endregion

    #region Closing
    private async void WindowClosed(object sender, WindowEventArgs e)
    {
        if (!unitOfWork.HasChanges()) return;

        var dialog = new ContentDialog()
        {
            Title = "Unsaved Changes",
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
            default: unitOfWork.DiscardChanges(); break;
        }
        Close();
    }
    #endregion
}
