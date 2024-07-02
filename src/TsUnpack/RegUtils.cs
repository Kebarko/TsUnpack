using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KE.MSTS.TsUnpack;

/// <summary>
/// Provides utility methods for managing installation path of the Train Simulator / Open Rails in the Windows registry.
/// </summary>
internal static class RegUtils
{
    /// <summary>
    /// Retrieves the installation path of the Train Simulator from the Windows registry.
    /// </summary>
    /// <returns>A string representing the installation path, or null if the path cannot be found.</returns>
    public static string? GetTrainSimPath()
    {
        RegistryKey? key = Registry.LocalMachine.OpenSubKey(Environment.Is64BitOperatingSystem ? @"SOFTWARE\Wow6432Node\Microsoft\Microsoft Games\Train Simulator\1.0" : @"SOFTWARE\Microsoft\Microsoft Games\Train Simulator\1.0", false);

        return (key?.GetValue("Path") as string)?.Trim('\"');
    }

    /// <summary>
    /// Retrieves all profile names of all Open Rails forks.
    /// </summary>
    /// <returns>A dictionary where the key is the name of the Open Rails fork and the value is a list of profile names.</returns>
    public static IDictionary<string, IList<string>> GetOpenRailsProfiles()
    {
        Dictionary<string, IList<string>> result = [];

        IEnumerable<string> openRailsForks = Registry.CurrentUser.OpenSubKey("SOFTWARE")!.GetSubKeyNames().Where(x => x.StartsWith("OpenRails"));
        foreach (string openRailsFork in openRailsForks)
        {
            List<string> profiles = [];

            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@$"SOFTWARE\{openRailsFork}\ORTS\Folders");
            if (key != null)
            {
                foreach (string profile in key.GetValueNames())
                {
                    profiles.Add(profile);
                }
            }

            if (profiles.Count > 0)
            {
                result.Add(openRailsFork, profiles);
            }
        }

        return result;
    }

    /// <summary>
    /// Retrieves the name of the Open Rails profile from the Windows registry.
    /// </summary>
    /// <returns>A string representing the name of the Open Rails profile, or null if the profile cannot be found.</returns>
    public static string? GetOpenRailsProfile()
    {
        RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TsUnpack", false);

        return key?.GetValue("OpenRailsProfile") as string;
    }

    /// <summary>
    /// Sets or removes the Open Rails profile in the Windows registry.
    /// </summary>
    /// <param name="path">The Open Rails profile to set. If null, the existing profile will be removed.</param>
    public static void SetOpenRailsProfile(string? profile)
    {
        if (profile != null)
        {
            RegistryKey key =
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TsUnpack", true) ??
                Registry.CurrentUser.CreateSubKey(@"SOFTWARE\TsUnpack");

            key.SetValue("OpenRailsProfile", profile);
            key.Close();
        }
        else
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TsUnpack", true);

            if (key != null)
            {
                key.DeleteValue("OpenRailsProfile", false);
                key.Close();
            }
        }
    }

    /// <summary>
    /// Retrieves the installation path of the Open Rails profile from the Windows registry.
    /// </summary>
    /// <returns>A string representing the installation path, or null if the path cannot be found.</returns>
    public static string? GetOpenRailsProfilePath()
    {
        string? profile = GetOpenRailsProfile();
        if (profile != null)
        {
            string[] parts = profile.Split('/');

            if (parts.Length == 2)
            {
                RegistryKey? key = Registry.CurrentUser.OpenSubKey(@$"SOFTWARE\{parts[0]}\ORTS\Folders", false);

                return (key?.GetValue(parts[1]) as string)?.Trim('\"');
            }

        }

        return null;
    }

    /// <summary>
    /// Retrieves the custom installation path of the Train Simulator from the Windows registry.
    /// </summary>
    /// <returns>A string representing the custom installation path, or null if the path cannot be found.</returns>
    public static string? GetCustomPath()
    {
        RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TsUnpack", false);

        return (key?.GetValue("Path") as string)?.Trim('\"');
    }

    /// <summary>
    /// Sets or removes the custom installation path of the Train Simulator in the Windows registry.
    /// </summary>
    /// <param name="path">The custom installation path to set. If null, the existing custom path will be removed.</param>
    public static void SetCustomPath(string? path)
    {
        if (path != null)
        {
            RegistryKey key =
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TsUnpack", true) ??
                Registry.CurrentUser.CreateSubKey(@"SOFTWARE\TsUnpack");

            key.SetValue("Path", path);
            key.Close();
        }
        else
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TsUnpack", true);

            if (key != null)
            {
                key.DeleteValue("Path", false);
                key.Close();
            }
        }
    }

    /// <summary>
    /// Retrieves the final path for unpacking.
    /// </summary>
    /// <returns>
    /// A string representing the final path. The method returns the first non-null path from the following:
    /// 1. Custom path retrieved by <see cref="GetCustomPath"/>
    /// 2. Open Rails profile path retrieved by <see cref="GetOpenRailsProfilePath"/>
    /// 3. Train Simulator path retrieved by <see cref="GetTrainSimPath"/>
    /// If all of these methods return null, the final result is also null.
    /// </returns>
    public static string? GetFinalPath()
    {
        return GetCustomPath() ?? GetOpenRailsProfilePath() ?? GetTrainSimPath();
    }
}
