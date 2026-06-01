using Domain;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Persistence.Repositories;
using System.Collections.Generic;
using System.Linq;
using FilterType = Persistence.Repositories.IComicRepository.FilterType;
using Windows.System;
using System.Collections.ObjectModel;

namespace ComicTracking.Windows;

internal class CheckTag(Tag tag, bool isChecked)
{
    public Tag Tag { get; set; } = tag;
    public bool IsChecked { get; set; } = isChecked;
}

public sealed partial class FilterTagsWindow : Window
{
    private bool AcceptFilter = false;
    private List<CheckTag> AllTags = [];
    private List<Tag> selectedTags
    {
        get => AllTags.Where(t => t.IsChecked).Select(t => t.Tag).ToList();
    }
    public List<Tag> SelectedTags
    {
        get
        {
            if (!AcceptFilter) return [];

            return selectedTags;
        }
    }
    public FilterType Filter { get; set; } = FilterType.Any;

    private readonly ITagRepository tagRepository;

    public FilterTagsWindow(ITagRepository _tagRepository)
    {
        tagRepository = _tagRepository;

        InitializeComponent();
        FilterTypeCombo.ItemsSource = new List<FilterType>() { FilterType.Any, FilterType.All };
    }

    public void SetInitialTags(List<Tag> initialTags, FilterType filterType, string? filter = null)
    {
        var tagIds = initialTags.Select(tag => tag.Id).ToHashSet();
        AllTags = tagRepository.GetAll()
            .Select(t => new CheckTag(t, tagIds.Contains(t.Id)))
            .ToList();

        var tags = (filter is { } term)
            ? AllTags.Where(t => t.Tag.Name.Contains(term, System.StringComparison.InvariantCultureIgnoreCase))
            : AllTags;

        TagList.ItemsSource = tags;
        FilterTypeCombo.SelectedItem = filterType;
    }

    private void ReloadTags()
    {
        AllTags = tagRepository.GetAll().Select(t => new CheckTag(t, false)).ToList();
        TagList.ItemsSource = AllTags;
    }

    private void Loaded(object sender, RoutedEventArgs e)
    {
        if (TagList.ItemsSourceView == null) ReloadTags();
    }

    private void ClearAll(object sender, RoutedEventArgs e)
    {
        ReloadTags();
    }

    private void Accept(object sender, RoutedEventArgs e)
    {
        AcceptFilter = true;
        Close();
    }

    private void FilterTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox { SelectedItem: FilterType filter }) return;
        Filter = filter;
    }

    private void FilterKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            var search = TagFilterBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(search))
                SetInitialTags(selectedTags, Filter, search);
            else
                SetInitialTags(selectedTags, Filter);
        }
    }
}
