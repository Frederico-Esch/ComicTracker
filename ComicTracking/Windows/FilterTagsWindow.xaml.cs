using Domain;
using Microsoft.UI.Xaml;
using Persistence.Repositories;
using System.Collections.Generic;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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

    private readonly ITagRepository tagRepository;

    public FilterTagsWindow(ITagRepository _tagRepository)
    {
        tagRepository = _tagRepository;

        InitializeComponent();
    }

    public void SetInitialTags(List<Tag> initialTags)
    {
        var tagIds = initialTags.Select(tag => tag.Id).ToHashSet();
        var tags = tagRepository.GetAll()
            .Select(t => new CheckTag(t, tagIds.Contains(t.Id)))
            .ToList();
        TagList.ItemsSource = tags;
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
}
