namespace KE.MSTS.TsUnpack;

internal class ByteReader
{
    private readonly byte[] _bytes;
    private int _position;

    public bool End { get { return _position == _bytes.Length; } }

    public ByteReader(byte[] bytes)
    {
        _bytes = bytes;
        _position = 0;
    }

    public uint ReadUInt()
    {
        uint result =  (uint) ((_bytes[_position + 3] << 24) |
                               (_bytes[_position + 2] << 16) |
                               (_bytes[_position + 1] << 8) |
                               (_bytes[_position]));
        _position += 4;
        return result;
    }

    public char ReadChar()
    {
        char result = (char)((_bytes[_position + 1] << 8) |
                             (_bytes[_position]));
        _position += 2;
        return result;
    }
}