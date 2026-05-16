using Domain;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;

namespace ComicTracking.Windows;


public sealed partial class ManageTagsWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int visibility = 0;
    public bool Background1Visible
    {
        get => (visibility & 1) > 0;
        set
        {
            visibility = value ? 1 : 0;
            NotifyVisibility();
        }
    }
    public bool Background2Visible
    {
        get => (visibility & 0b10) > 0;
        set
        {
            visibility = value ? 0b10 : 0;
            NotifyVisibility();
        }
    }
    public bool TextColorVisible
    {
        get => (visibility & 0b100) > 0;
        set
        {
            visibility = value ? 0b100 : 0;
            NotifyVisibility();
        }
    }

    public string TagName
    {
        get => CurrentTag.Name;
        set 
        {
            CurrentTag.Name = value;
            NotifyTags();
        }
    }
    public Color TagColor1
    {
        get => CurrentTag.Color1;
        set
        {
            CurrentTag.Color1 = value;
            NotifyTags();
        }
    }
    public Color TagColor2
    {
        get => CurrentTag.Color2;
        set
        {
            CurrentTag.Color2 = value;
            NotifyTags();
        }
    }
    public Color TagTextColor
    {
        get => CurrentTag.TextColor;
        set
        {
            CurrentTag.TextColor = value;
            NotifyTags();
        }
    }

    private Tag currentTag = new();
    public Tag CurrentTag
    {
        get => currentTag;
        set 
        {
            currentTag = value;
            NotifyTags();
        }
    }

    private void NotifyTags()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.TagName)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.TagColor1)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.TagColor2)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.TagTextColor)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentTag)));
    }
    private void NotifyVisibility()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Background1Visible)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Background2Visible)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.TextColorVisible)));
    }
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private readonly ITagRepository tagRepository;
    private readonly IUnitOfWork unitOfWork;
    private List<Tag> currentTags = [];

    public ManageTagsWindow(ITagRepository _tagRepository, IUnitOfWork _unitOfWork)
    {
        tagRepository = _tagRepository;
        unitOfWork = _unitOfWork;

        AppWindow.Resize(new(1500, 800));
        InitializeComponent();

        CurrentTag = new Tag()
        {
            Name = "Name"
        };
        ReloadTags();
    }

    #region Change UI

    private void ShowSave()
    {
        SaveButton.Visibility = Visibility.Visible;
        DeleteButton.Visibility = Visibility.Visible;
    }
    private void HideSave()
    {
        SaveButton.Visibility = Visibility.Collapsed;
        DeleteButton.Visibility = Visibility.Collapsed;
    }

    private void ColorPickerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var control = sender as ColorPicker;
        if (control == BackgroundColor1)
        {
            Background1Visible = true;
        }
        else if (control == BackgroundColor2)
        {
            Background2Visible = true;
        }
        else if (control == TextColor)
        {
            TextColorVisible = true;
        }
    }
    #endregion

    private void ReloadTags(object sender, RoutedEventArgs e) => ReloadTags();
    private void ReloadTags()
    {
        currentTags = tagRepository.GetAll();
        TagsList.ItemsSource = currentTags;

        if (currentTags.Any(t => t.Id == CurrentTag.Id)) { ShowSave(); }
        else { HideSave(); }
    }

    private async Task<bool> CheckTagIsValid()
    {
        var result = !string.IsNullOrWhiteSpace(CurrentTag.Name.Trim());
        if (!result)
        {
            var dlg = new ContentDialog()
            {
                Title = "Error",
                Content = "Name can't be empty",
                CloseButtonText = "Okay"
            };
            dlg.XamlRoot = this.Content.XamlRoot;
            await dlg.ShowAsync();
        }
        return result;
    }

    #region Buttons

    private async void AddTag(object sender, RoutedEventArgs e)
    {
        if (currentTags.Any(t => t.Id == CurrentTag.Id)) //Copying new tag should create new Id for it
            CurrentTag.Id = Guid.NewGuid();
        if (!await CheckTagIsValid()) return;
        tagRepository.Add(CurrentTag);
        unitOfWork.Save();
        ReloadTags();
    }

    private async void SaveTag(object sender, RoutedEventArgs e)
    {
        if (!await CheckTagIsValid()) return;
        unitOfWork.Save();
        ReloadTags();
    }

    private void DeleteTag(object sender, RoutedEventArgs e)
    {
        tagRepository.Remove(CurrentTag);
        unitOfWork.Save();
        ReloadTags();
    }

    private void TagSelected(object sender, SelectionChangedEventArgs e)
    {
        if (TagsList.SelectedIndex < 0)
        {
            var index = currentTags.IndexOf(CurrentTag);
            if (index < 0)
            {
                HideSave();
                return;
            }
            TagsList.SelectedIndex = index;
            return;
        }

        var selectedTag = currentTags[TagsList.SelectedIndex];
        if (selectedTag.Id == CurrentTag.Id) return;

        CurrentTag = selectedTag;
        ShowSave();
    }

    #endregion

    private void KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
            TagsList.Focus(FocusState.Pointer);
    }

    private async void Close(object sender, WindowEventArgs args)
    {
        if (unitOfWork.HasChanges())
        {
            var dlg = new ContentDialog()
            {
                Title = "Unsaved Changes",
                Content = "You have unsaved changes, what should be done?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Discard",
                DefaultButton = ContentDialogButton.Close,
            };
            dlg.XamlRoot = Content.XamlRoot;
            args.Handled = true;
            var result = await dlg.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    unitOfWork.Save();
                    Close();
                    break;
                case ContentDialogResult.Secondary:
                    unitOfWork.DiscardChanges();
                    Close();
                    break;
                default: break;
            }
        }
    }
}
