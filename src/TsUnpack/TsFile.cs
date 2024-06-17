namespace KE.MSTS.TsUnpack;

internal class TsFile(string path, string content)
{
    /// <summary>
    /// Gets the full path of the file.
    /// </summary>
    public string Path { get; } = path;

    /// <summary>
    /// Gets the content of the file.
    /// </summary>
    public string Content { get; } = content;

    /// <summary>
    /// Gets or sets the value indicating whether to overwrite the existing file.
    /// </summary>
    public bool Overwrite { get; set; }

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    public string FileName => System.IO.Path.GetFileName(Path);

    /// <summary>
    /// Gets or sets the initial state of the file (before unpacking).
    /// </summary>
    public TsFileInitialState InitialState { get; set; } = TsFileInitialState.New;

    /// <summary>
    /// Gets or sets the resulting state of the file (after unpacking).
    /// </summary>
    public TsFileResultingState? ResultingState { get; set; }
}
