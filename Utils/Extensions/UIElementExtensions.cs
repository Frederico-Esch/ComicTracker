using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using System.Reflection;

namespace Utils.Extensions
{
    public static class UIElementExtensions
    {
        public static void ChangeCursor(this UIElement element, InputCursor cursor)
        {
            Type type = typeof(UIElement);
            type.InvokeMember(
                "ProtectedCursor",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance,
                null,
                element,
                new object[] { cursor }
            );
        }
    }
}
