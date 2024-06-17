using Microsoft.Win32;
using System;

namespace KE.MSTS.TsUnpack;

/// <summary>
/// Provides utility methods for managing installation path of the Train Simulator in the Windows registry.
/// </summary>
internal static class RegUtils
{
    private const string defaultRegKey32 = @"SOFTWARE\Microsoft\Microsoft Games\Train Simulator\1.0";
    private const string defaultRegKey64 = @"SOFTWARE\\Wow6432Node\\Microsoft\\Microsoft Games\\Train Simulator\\1.0";
    private const string defaultRegName = "Path";

    private const string customRegKey = @"Software\TsUnpack";
    private const string customRegName = "Path";

    /// <summary>
    /// Retrieves the default installation path of the Train Simulator from the Windows registry.
    /// </summary>
    /// <returns>A string representing the default installation path, or null if the path cannot be found.</returns>
    public static string? GetDefaultPath()
    {
        RegistryKey? key = Registry.LocalMachine.OpenSubKey(Environment.Is64BitOperatingSystem ? defaultRegKey64 : defaultRegKey32, false);

        return (key?.GetValue(defaultRegName) as string)?.Trim('\"');
    }

    /// <summary>
    /// Retrieves the custom installation path of the Train Simulator from the Windows registry.
    /// </summary>
    /// <returns>A string representing the custom installation path, or null if the path cannot be found.</returns>
    public static string? GetCustomPath()
    {
        RegistryKey? key = Registry.CurrentUser.OpenSubKey(customRegKey, false);

        return (key?.GetValue(defaultRegName) as string)?.Trim('\"');
    }

    /// <summary>
    /// Sets or removes the custom installation path of the Train Simulator in the Windows registry.
    /// </summary>
    /// <param name="path">The custom installation path to set. If null, the existing custom path will be removed.</param>
    public static void SetCustomPath(string? path)
    {
        if (path != null)
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(customRegKey, true);

            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(customRegKey);
            }

            key.SetValue(customRegName, path);
            key.Close();
        }
        else
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(customRegKey, true);

            if (key != null)
            {
                key.DeleteValue(customRegName, false);
                key.Close();
            }
        }
    }
}
