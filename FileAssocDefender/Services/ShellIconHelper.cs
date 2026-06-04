using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileAssocDefender.Services;

public static class ShellIconHelper
{
    private const uint ShgfiIcon = 0x000000100;
    private const uint ShgfiLargeIcon = 0x000000000;
    private const uint ShgfiUseFileAttributes = 0x000000010;
    private const uint FileAttributeNormal = 0x00000080;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        ref ShFileInfo psfi,
        uint cbFileInfo,
        uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShFileInfo
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    public static ImageSource? GetIconFromPath(string path, int size = 32)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var shInfo = new ShFileInfo();
        var flags = ShgfiIcon | (size > 32 ? ShgfiLargeIcon : 0u);

        if (!File.Exists(path) && !Directory.Exists(path))
        {
            flags |= ShgfiUseFileAttributes;
            SHGetFileInfo(path, FileAttributeNormal, ref shInfo, (uint)Marshal.SizeOf<ShFileInfo>(), flags);
        }
        else
        {
            SHGetFileInfo(path, 0, ref shInfo, (uint)Marshal.SizeOf<ShFileInfo>(), flags);
        }

        if (shInfo.hIcon == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            var source = Imaging.CreateBitmapSourceFromHIcon(
                shInfo.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            DestroyIcon(shInfo.hIcon);
        }
    }
}
