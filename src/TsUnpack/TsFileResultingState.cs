namespace KE.MSTS.TsUnpack;

/// <summary>
/// Represents the resulting state of the file (after unpacking).
/// </summary>
internal enum TsFileResultingState
{
    /// <summary>
    /// A new file has been created.
    /// </summary>
    Created,

    /// <summary>
    /// An existing file has been skipped.
    /// </summary>
    Skipped,

    /// <summary>
    /// An existing file has been overwritten.
    /// </summary>
    Overwritten,

    /// <summary>
    /// Something has gone wrong.
    /// </summary>
    Failed
}
