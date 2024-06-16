using KE.MSTS.TsUnpack.Views;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using TK.MSTS.Tokens;

namespace KE.MSTS.TsUnpack;

internal class TsUnpack
{
    private readonly FileInfo apkFile;

    public TsUnpack(FileInfo apkFile)
    {
        this.apkFile = apkFile;
    }

    public void Unpack()
    {
        if (!apkFile.Exists)
        {
            throw new FileNotFoundException("Failed to open package!");
        }

        var decompressedMemoryStream = new MemoryStream();

        // APK file decompression
        using (FileStream apkFileStream = apkFile.OpenRead())
        {
            using (var decompressionStream = new GZipStream(apkFileStream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(decompressedMemoryStream);
            }
        }
#if DEBUG
        // Auxiliary output
        using (var streamReader = new StreamReader(decompressedMemoryStream, Encoding.Unicode))
        {
            using (var streamWriter = new StreamWriter("outchars.txt", false, Encoding.Unicode))
            {
                int character;
                while ((character = streamReader.Read()) != -1)
                {
                    streamWriter.WriteLine("0x{0} {1,5} {2}", character.ToString("X4"), character, (char)character);
                }
            }
        }

        // Auxiliary output
        using (var streamWriter = new StreamWriter("outbytes.txt", false, Encoding.Unicode))
        {
            foreach (byte @byte in decompressedMemoryStream.ToArray())
            {
                streamWriter.WriteLine("0x{0} {1,3}", @byte.ToString("X2"), @byte);
            }
        }
#endif

        Activity activity = ReadActivityFromBytes(decompressedMemoryStream.ToArray());
        decompressedMemoryStream.Close();

        string mstsPath = Utils.GetCustomPath() ?? Utils.GetDefaultPath();
        string routeId = GetRouteId(mstsPath, activity.RouteDirectory);
        if (!Directory.Exists(Path.Combine(mstsPath, "ROUTES", activity.RouteDirectory)) || routeId != activity.RouteId)
        {
            throw new DirectoryNotFoundException(string.Format("This package requires a route called {0}, UID {1}!", activity.RouteName, activity.RouteId));
        }
        MessageBoxResult result = MessageBox.Show(string.Format("Unpackage new activity in route {0}?", activity.RouteName), Program.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.No)
        {
            return;
        }
        uint serial = GetSerial(mstsPath, activity.RouteDirectory);
        if (serial != activity.Serial)
        {
            result = MessageBox.Show(string.Format("The installed track database for this route is version {0}, the activity was built on version {1}. Continue?", serial, activity.Serial), Program.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                return;
            }
        }
        foreach (TsFile tsFile in activity.Files.Where(tsFile => File.Exists(Path.Combine(mstsPath, tsFile.Path))))
        {
            tsFile.Exists = true;
            tsFile.Overwrite = true;
        }

        if (activity.Files.Any(f => f.Exists))
        {
            var overwriteWindow = new OverwriteWindow();
            overwriteWindow.DataContext = activity;
            if (overwriteWindow.ShowDialog() != true)
            {
                foreach (TsFile tsFile in activity.Files)
                {
                    tsFile.Overwrite = false;
                }
            }
        }

        foreach (TsFile tsFile in activity.Files.Where(tsFile => (!tsFile.Exists) || (tsFile.Exists && tsFile.Overwrite)))
        {
            try
            {
                string path = Path.Combine(mstsPath, tsFile.Path);
                string dirPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                using (var streamWriter = new StreamWriter(path, false, Encoding.Unicode))
                {
                    streamWriter.Write(tsFile.Content);
                }
            }
            catch (Exception)
            {
                tsFile.Failed = true;
            }
        }

        var resultWindow = new ResultWindow();
        resultWindow.DataContext = activity;
        resultWindow.ShowDialog();
    }

    private static Activity ReadActivityFromBytes(byte[] bytes)
    {
        var activity = new Activity();
        var byteReader = new ByteReader(bytes);
        var stringBuiler = new StringBuilder();

        stringBuiler.Clear();
        uint routeNameLength = byteReader.ReadUInt();
        for (uint i = 0; i < routeNameLength; i++)
        {
            char c = byteReader.ReadChar();
            if (c != '\0')
            {
                stringBuiler.Append(c);
            }
        }
        activity.RouteName = stringBuiler.ToString();

        stringBuiler.Clear();
        uint routeDirectoryLength = byteReader.ReadUInt();
        for (uint i = 0; i < routeDirectoryLength; i++)
        {
            char c = byteReader.ReadChar();
            if (c != '\0')
            {
                stringBuiler.Append(c);
            }
        }
        activity.RouteDirectory = stringBuiler.ToString();

        stringBuiler.Clear();
        uint routeIdLength = byteReader.ReadUInt();
        for (uint i = 0; i < routeIdLength; i++)
        {
            char c = byteReader.ReadChar();
            if (c != '\0')
            {
                stringBuiler.Append(c);
            }
        }
        activity.RouteId = stringBuiler.ToString();

        activity.Serial = byteReader.ReadUInt();

        uint filesCount = byteReader.ReadUInt();
        if (filesCount == 0)
        {
            throw new InvalidDataException("There are no files in this package!");
        }
        for (int i = 0; i < filesCount; i++)
        {
            var activityFile = new TsFile();
            stringBuiler.Clear();
            uint contentLength = byteReader.ReadUInt() / 2;
            uint pathLength = byteReader.ReadUInt();
            for (int j = 0; j < pathLength; j++)
            {
                char c = byteReader.ReadChar();
                if (c != '\0')
                {
                    stringBuiler.Append(c);
                }
            }
            activityFile.Path = stringBuiler.ToString();
            stringBuiler.Clear();
            for (int j = 0; j < contentLength; j++)
            {
                char c = byteReader.ReadChar();
                if (c != '\0' && c != '\xFEFF')
                {
                    stringBuiler.Append(c);
                }
            }
            activityFile.Content = stringBuiler.ToString();
            activity.Files.Add(activityFile);
        }

        if (!byteReader.End)
        {
            throw new InvalidDataException("Package damaged!");
        }

        return activity;
    }

    private static string GetRouteId(string mstsPath, string routeDirectory)
    {
        try
        {
            TokenFile trk = TokenFile.ReadFile(Path.Combine(mstsPath, "ROUTES", routeDirectory, routeDirectory + ".trk"));

            return trk.GetByName("Tr_RouteFile").GetByName("RouteID").Val.ToString().Trim('\"');
        }
        catch (Exception)
        {
            throw new InvalidDataException("Failed to read route info!");
        }
    }

    private static uint GetSerial(string mstsPath, string routeDirectory)
    {
        try
        {
            TokenFile trk = TokenFile.ReadFile(Path.Combine(mstsPath, "ROUTES", routeDirectory, routeDirectory + ".trk"));

            string fileName = trk.GetByName("Tr_RouteFile").GetByName("FileName").Val.ToString().Trim('\"');

            TokenFile tdb = TokenFile.ReadFile(Path.Combine(mstsPath, "ROUTES", routeDirectory, fileName + ".tdb"));

            return uint.Parse(tdb.GetByName("TrackDB").GetByName("Serial").Val.ToString());
        }
        catch (Exception)
        {
            throw new InvalidDataException("Failed to read route info!");
        }
    }
}
