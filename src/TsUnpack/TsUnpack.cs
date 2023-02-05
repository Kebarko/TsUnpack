using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;

namespace KE.MSTS.TsUnpack;

internal class TsUnpack
{
    private readonly FileInfo _apkFile;

    public TsUnpack(FileInfo apkFile)
    {
        _apkFile = apkFile;
    }

    public void Unpack()
    {
        if (!_apkFile.Exists)
        {
            throw new FileNotFoundException("Failed to open package!");
        }

        MemoryStream decompressedMemoryStream = new MemoryStream();

        // APK file decompression
        using (FileStream apkFileStream = _apkFile.OpenRead())
        {
            using (GZipStream decompressionStream = new GZipStream(apkFileStream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(decompressedMemoryStream);
            }
        }
#if DEBUG
        // Auxiliary output
        using (StreamReader streamReader = new StreamReader(decompressedMemoryStream, Encoding.Unicode))
        {
            using (StreamWriter streamWriter = new StreamWriter("outchars.txt", false, Encoding.Unicode))
            {
                int character;
                while ((character = streamReader.Read()) != -1)
                {
                    streamWriter.WriteLine("0x{0} {1,5} {2}", character.ToString("X4"), character, (char)character);
                }
            }
        }

        // Auxiliary output
        using (StreamWriter streamWriter = new StreamWriter("outbytes.txt", false, Encoding.Unicode))
        {
            foreach (byte @byte in decompressedMemoryStream.ToArray())
            {
                streamWriter.WriteLine("0x{0} {1,3}", @byte.ToString("X2"), @byte);
            }
        }
#endif

        Activity activity = ReadActivityFromBytes(decompressedMemoryStream.ToArray());
        decompressedMemoryStream.Close();

        string mstsPath = GetUnpackingPath();
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
            OverwriteWindow overwriteWindow = new OverwriteWindow();
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
                using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.Unicode))
                {
                    streamWriter.Write(tsFile.Content);
                }
            }
            catch (Exception)
            {
                tsFile.Failed = true;
            }
        }

        ResultWindow resultWindow = new ResultWindow();
        resultWindow.DataContext = activity;
        resultWindow.ShowDialog();
    }

    private Activity ReadActivityFromBytes(byte[] bytes)
    {
        Activity activity = new Activity();
        ByteReader byteReader = new ByteReader(bytes);
        StringBuilder stringBuiler = new StringBuilder();

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
            TsFile activityFile = new TsFile();
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

    private string GetUnpackingPath()
    {
        string mstsPath = (string) Registry.GetValue("HKEY_CURRENT_USER\\Software\\TsUnpack", "Path", null);
        if(mstsPath == null)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                mstsPath = (string) Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Microsoft Games\\Train Simulator\\1.0", "Path", null);
            }
            else
            {
                mstsPath = (string) Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Microsoft Games\\Train Simulator\\1.0", "Path", null);
            }
        }
        if (mstsPath == null)
        {
            throw new IOException("Microsoft Train Simulator installation not found!");
        }
        return mstsPath;
    }

    private string GetRouteId(string mstsPath, string routeDirectory)
    {
        try
        {
            string routeId = null;
            FileInfo trkInfo = new FileInfo(Path.Combine(mstsPath, "ROUTES", routeDirectory, routeDirectory + ".trk"));
            using (TokenReader tokenReader = new TokenReader(trkInfo.OpenRead(), Encoding.Unicode))
            {
                string token;
                while ((token = tokenReader.ReadToken()) != null)
                {
                    if (token.Equals("RouteID", StringComparison.OrdinalIgnoreCase))
                    {
                        tokenReader.ReadToken();
                        routeId = tokenReader.ReadTokenWithoutQuotes();
                    }
                }
            }
            return routeId;
        }
        catch (Exception)
        {
            throw new InvalidDataException("Failed to read route info!");
        }
    }

    private uint GetSerial(string mstsPath, string routeDirectory)
    {
        try
        {
            string fileName = null;
            FileInfo trkInfo = new FileInfo(Path.Combine(mstsPath, "ROUTES", routeDirectory, routeDirectory + ".trk"));
            using (TokenReader tokenReader = new TokenReader(trkInfo.OpenRead(), Encoding.Unicode))
            {
                string token;
                while ((token = tokenReader.ReadToken()) != null)
                {
                    if (token.Equals("Filename", StringComparison.OrdinalIgnoreCase))
                    {
                        tokenReader.ReadToken();
                        fileName = tokenReader.ReadTokenWithoutQuotes();
                    }
                }
            }

            uint serial = 0;
            FileInfo tdbInfo = new FileInfo(Path.Combine(mstsPath, "ROUTES", routeDirectory, fileName + ".tdb"));
            using (TokenReader tokenReader = new TokenReader(tdbInfo.OpenRead(), Encoding.Unicode))
            {
                string token;
                while ((token = tokenReader.ReadToken()) != null)
                {
                    if (token.Equals("Serial", StringComparison.OrdinalIgnoreCase))
                    {
                        tokenReader.ReadToken();
                        serial = uint.Parse(tokenReader.ReadToken());
                    }
                }
            }
            return serial;
        }
        catch (Exception)
        {
            throw new InvalidDataException("Failed to read route info!");
        }
    }
}