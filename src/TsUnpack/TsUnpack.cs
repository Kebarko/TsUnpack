using KE.MSTS.TsUnpack.Views;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace KE.MSTS.TsUnpack;

internal class TsUnpack(FileInfo apkFile)
{
    public void Unpack()
    {
        Activity activity;

        // Unzip APK file and read activity
        using (FileStream fs = apkFile.OpenRead())
        using (GZipStream gZip = new(fs, CompressionMode.Decompress, false))
        using (MemoryStream ms = new())
        {
            gZip.CopyTo(ms);
            activity = ReadActivityFromBytes(ms.ToArray());
        }

        // Get the Train Simulator's root path and check that it exists
        string? mstsPath = RegUtils.GetCustomPath() ?? RegUtils.GetDefaultPath();
        if (mstsPath == null)
        {
            throw new DirectoryNotFoundException("Undefined Train Simulator's root path!");
        }
        if (!Directory.Exists(mstsPath))
        {
            throw new DirectoryNotFoundException($"{mstsPath}{Environment.NewLine}The specified path does not exist!");
        }

        // Get the route ID and check that it matches the activity ID
        string? routeId = GetRouteId(mstsPath, activity.RouteDirectory);
        if (!Directory.Exists(Path.Combine(mstsPath, "ROUTES", activity.RouteDirectory)) || routeId != activity.RouteId)
        {
            throw new DirectoryNotFoundException(string.Format("This package requires a route named {0}, UID {1}!", activity.RouteName, activity.RouteId));
        }

        // Ask for unpacking
        MessageBoxResult result = MessageBox.Show(string.Format("Unpack new activity in route {0}?", activity.RouteName), Program.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.No)
        {
            return;
        }

        // Get the route serial number and check that it matches the activity serial number
        uint? serial = GetSerial(mstsPath, activity.RouteDirectory);
        if (serial != activity.Serial)
        {
            // Ask for unpacking
            result = MessageBox.Show(string.Format("The installed track database for this route is version {0}, the activity was built on version {1}. Continue?", serial, activity.Serial), Program.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                return;
            }
        }

        // Iterate over each file that already exists
        foreach (TsFile tsFile in activity.Files.Where(tsFile => File.Exists(Path.Combine(mstsPath, tsFile.Path))))
        {
            // Set initial state to existing
            tsFile.InitialState = TsFileInitialState.Existing;

            // Set overwrite flag to true by default
            tsFile.Overwrite = true;
        }

        if (activity.Files.Any(f => f.InitialState == TsFileInitialState.Existing))
        {
            // If there are existing files, let user decide which ones to overwrite
            OverwriteWindow overwriteWindow = new()
            {
                DataContext = activity
            };

            if (overwriteWindow.ShowDialog() != true)
            {
                // If dialog is cancelled, no file will be overwritten
                foreach (TsFile tsFile in activity.ExistingFiles)
                {
                    tsFile.Overwrite = false;
                }
            }
        }

        // Iterate over each file
        foreach (TsFile tsFile in activity.Files)
        {
            try
            {
                if (tsFile.InitialState == TsFileInitialState.New || tsFile.Overwrite)
                {
                    string path = Path.Combine(mstsPath, tsFile.Path);
                    string? dirPath = Path.GetDirectoryName(path);
                    if (dirPath != null && !Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    using (var streamWriter = new StreamWriter(path, false, Encoding.Unicode))
                    {
                        streamWriter.Write(tsFile.Content);
                    }

                    tsFile.ResultingState = tsFile.Overwrite ? TsFileResultingState.Overwritten : TsFileResultingState.Created;
                }
                else
                {
                    tsFile.ResultingState = TsFileResultingState.Skipped;
                }
            }
            catch
            {
                tsFile.ResultingState = TsFileResultingState.Failed;
            }
        }

        // Show the result
        ResultWindow resultWindow = new()
        {
            DataContext = activity
        };

        resultWindow.ShowDialog();
    }

    /// <summary>
    /// Reads entire activity from byte array.
    /// </summary>
    private static Activity ReadActivityFromBytes(byte[] bytes)
    {
        var byteReader = new ByteReader(bytes);

        uint routeNameLength = byteReader.ReadUInt();

        StringBuilder routeNameBuilder = new();
        for (uint i = 0; i < routeNameLength; i++)
        {
            char c = byteReader.ReadChar();
            if (c != '\0')
            {
                routeNameBuilder.Append(c);
            }
        }

        uint routeDirectoryLength = byteReader.ReadUInt();

        StringBuilder routeDirectoryBuilder = new();
        for (uint i = 0; i < routeDirectoryLength; i++)
        {
            char c = byteReader.ReadChar();
            if (c != '\0')
            {
                routeDirectoryBuilder.Append(c);
            }
        }

        uint routeIdLength = byteReader.ReadUInt();

        StringBuilder routeIdBuilder = new();
        for (uint i = 0; i < routeIdLength; i++)
        {
            char c = byteReader.ReadChar();
            if (c != '\0')
            {
                routeIdBuilder.Append(c);
            }
        }

        uint serial = byteReader.ReadUInt();

        uint filesCount = byteReader.ReadUInt();
        if (filesCount == 0)
        {
            throw new InvalidDataException("There are no files in this package!");
        }

        Activity activity = new(routeNameBuilder.ToString(), routeDirectoryBuilder.ToString(), routeIdBuilder.ToString(), serial);

        for (int i = 0; i < filesCount; i++)
        {
            uint contentLength = byteReader.ReadUInt() / 2;
            uint pathLength = byteReader.ReadUInt();

            StringBuilder pathBuilder = new();
            for (int j = 0; j < pathLength; j++)
            {
                char c = byteReader.ReadChar();
                if (c != '\0')
                {
                    pathBuilder.Append(c);
                }
            }

            StringBuilder contentBuilder = new();
            for (int j = 0; j < contentLength; j++)
            {
                char c = byteReader.ReadChar();
                if (c != '\0' && c != '\xFEFF')
                {
                    contentBuilder.Append(c);
                }
            }

            activity.Files.Add(new TsFile(pathBuilder.ToString(), contentBuilder.ToString()));
        }

        if (!byteReader.End)
        {
            throw new InvalidDataException("Package damaged!");
        }

        return activity;
    }

    /// <summary>
    /// Gets the route id .trk file.
    /// </summary>
    private static string? GetRouteId(string mstsPath, string routeDirectory)
    {
        return GetTokenValue(Path.Combine(mstsPath, "ROUTES", routeDirectory, routeDirectory + ".trk"), "RouteID");
    }

    /// <summary>
    /// Gets the route serial number from .tdb file.
    /// </summary>
    private static uint? GetSerial(string mstsPath, string routeDirectory)
    {
        string? fileName = GetTokenValue(Path.Combine(mstsPath, "ROUTES", routeDirectory, routeDirectory + ".trk"), "RouteID");
        if (fileName != null)
        {
            string? serial = GetTokenValue(Path.Combine(mstsPath, "ROUTES", routeDirectory, routeDirectory + ".tdb"), "Serial");
            if (serial != null)
            {
                return uint.Parse(serial);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the value of the first occurrence of the specified token.
    /// </summary>
    private static string? GetTokenValue(string path, string token)
    {
        using (StreamReader reader = new(path, true))
        {
            while (reader.ReadLine() is string line)
            {
                Match match = Regex.Match(line, $@"\s*{token}\s*\((.+)\)\s*");
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim().Trim('\"');
                }
            }
        }

        return null;
    }
}
