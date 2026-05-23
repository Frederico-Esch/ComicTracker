using Domain;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Persistence.Repositories;
using System.Collections.Generic;
using System.Linq;
using FilterType = Persistence.Repositories.IComicRepository.FilterType;

namespace ComicTracking.Windows;

internal class CheckTag(Tag tag, bool isChecked)
{
    public Tag Tag { get; set; } = tag;
    public bool IsChecked { get; set; } = isChecked;
}

public sealed partial class FilterTagsWindow : Window
{
    private bool AcceptFilter = false;
    public List<Tag> TagsSelected
    {
        get
        {
            if (!AcceptFilter || TagList.ItemsSource is not List<CheckTag> tags) return [];

            return tags.Where(t => t.IsChecked).Select(t => t.Tag).ToList();
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

    public void SetInitialTags(List<Tag> initialTags, FilterType filterType)
    {
        var tagIds = initialTags.Select(tag => tag.Id).ToHashSet();
        var tags = tagRepository.GetAll()
            .Select(t => new CheckTag(t, tagIds.Contains(t.Id)))
            .ToList();
        TagList.ItemsSource = tags;
        FilterTypeCombo.SelectedItem = filterType;
    }

    private void Loaded(object sender, RoutedEventArgs e)
    {
        if (TagList.ItemsSourceView == null)
            TagList.ItemsSource = tagRepository.GetAll().Select(t => new CheckTag(t, false)).ToList();
    }

    private void ClearAll(object sender, RoutedEventArgs e)
    {
        TagList.ItemsSource = tagRepository.GetAll().Select(t => new CheckTag(t, false)).ToList();
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
}
