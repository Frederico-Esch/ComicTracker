using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Utils.Extensions
{
    public static class WindowExtensions
    {
        const string DefaultIcon = @"Assets\icon.ico";

        public static void ApplyCustomizations(this Window window)
        {
            window.AppWindow.SetIcon(DefaultIcon);
            window.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
        }

        public static void NavigateTo(this Window mainWindow)
        {
            mainWindow.ApplyCustomizations();
            mainWindow.Activate();
        }

        public static void NavigateTo(this Window owner, Window child, Action? action = null)
        {
            child.Closed += (sender, args) =>
            {
                if (!args.Handled)
                {
                    owner.AppWindow.Show();
                    action?.Invoke();
                }
            };
            var position = owner.AppWindow.Position;
            child.ApplyCustomizations();
            child.AppWindow.Move(position);
            child.Activate();
            owner.AppWindow.Hide();
        }
    }
}
