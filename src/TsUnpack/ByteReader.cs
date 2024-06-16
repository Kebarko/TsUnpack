namespace KE.MSTS.TsUnpack;

internal class ByteReader
{
    private readonly byte[] bytes;
    private int position;

    public bool End { get { return position == bytes.Length; } }

    public ByteReader(byte[] bytes)
    {
        this.bytes = bytes;
        position = 0;
    }

    public uint ReadUInt()
    {
        uint result = (uint)((bytes[position + 3] << 24) |
                               (bytes[position + 2] << 16) |
                               (bytes[position + 1] << 8) |
                               (bytes[position]));
        position += 4;
        return result;
    }

    public char ReadChar()
    {
        char result = (char)((bytes[position + 1] << 8) |
                             (bytes[position]));
        position += 2;
        return result;
    }
}
