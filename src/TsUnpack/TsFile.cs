namespace KE.MSTS.TsUnpack;

internal class TsFile
{
    public string Path { get; set; }

    public string Content { get; set; }

    public bool Exists { get; set; }

    public bool Overwrite { get; set; }

    public bool Failed { get; set; }

    public string FileName
    {
        get { return System.IO.Path.GetFileName(Path ?? string.Empty); }
    }

    public UnpackingResultType UnpackingResult
    {
        get
        {
            if (!Exists && !Failed)
            {
                return UnpackingResultType.Created;
            }
            if (Exists && Overwrite && !Failed)
            {
                return UnpackingResultType.Overwritten;
            }
            if (Exists && !Overwrite)
            {
                return UnpackingResultType.Skipped;
            }
            if (Failed)
            {
                return UnpackingResultType.Failed;
            }
            return UnpackingResultType.Unknown;
        }
    }
}
