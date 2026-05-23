using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils;

public class ComicDisplayer
{
    [DllImport(@"comic_display.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    static extern void DisplayComic(nint name, UInt64 name_length, nint path, UInt64 path_length);

    public static async Task DisplayComic(string root_path, string name, byte[] comicData)
    {
        var path = Path.Join([root_path, "Temp.cbz"]);
        await File.WriteAllBytesAsync(path, comicData);
        
        var name_ptr = Marshal.StringToHGlobalAnsi(name);
        var path_ptr = Marshal.StringToHGlobalAnsi(path);

        DisplayComic(name_ptr, (UInt64)name.Length, path_ptr, (UInt64)path.Length);

        Marshal.FreeHGlobal(name_ptr);
        Marshal.FreeHGlobal(path_ptr);

        if (File.Exists(path))
            File.Delete(path);
    }
}
