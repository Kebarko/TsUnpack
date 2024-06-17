namespace KE.MSTS.TsUnpack;

/// <summary>
/// Represents the initial state of the file (before unpacking).
/// </summary>
internal enum TsFileInitialState
{
    /// <summary>
    /// New file.
    /// </summary>
    New,

    /// <summary>
    /// Already existing file.
    /// </summary>
    Existing
}
