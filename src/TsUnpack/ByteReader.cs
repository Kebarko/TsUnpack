namespace KE.MSTS.TsUnpack;

/// <summary>
/// Provides functionality to read data types from a byte array.
/// </summary>
internal class ByteReader(byte[] bytes)
{
    private int position = 0;

    /// <summary>
    /// Gets a value indicating whether the end of the byte array has been reached.
    /// </summary>
    public bool End => position >= bytes.Length - 1;

    /// <summary>
    /// Reads a 32-bit unsigned integer from the byte array. Advances the position by 4 bytes.
    /// </summary>
    /// <returns>The 32-bit unsigned integer read from the byte array.</returns>
    public uint ReadUInt()
    {
        uint result = (uint)((bytes[position + 3] << 24) |
                             (bytes[position + 2] << 16) |
                             (bytes[position + 1] << 8) |
                             (bytes[position]));
        position += 4;
        return result;
    }

    /// <summary>
    /// Reads a character (2 bytes) from the byte array. Advances the position by 2 bytes.
    /// </summary>
    /// <returns>The character read from the byte array.</returns>
    public char ReadChar()
    {
        char result = (char)((bytes[position + 1] << 8) |
                             (bytes[position]));
        position += 2;
        return result;
    }
}
