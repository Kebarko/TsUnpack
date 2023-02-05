using System;
using System.IO;
using System.Text;

namespace KE.MSTS.TsUnpack;

public class TokenReader : IDisposable
{
    private readonly StreamReader _streamReader;
    private readonly StringBuilder _stringBuilder;
    private bool _disposed;

    public TokenReader(Stream stream, Encoding encoding)
    {
        _streamReader = new StreamReader(stream, encoding);
        _stringBuilder = new StringBuilder();
        _disposed = false;
    }

    public TokenReader(string path, Encoding encoding)
    {
        _streamReader = new StreamReader(path, encoding);
        _stringBuilder = new StringBuilder();
        _disposed = false;
    }

    public string ReadToken()
    {
        _stringBuilder.Clear();
            
        int character;
        bool inQuotes = false;
        while ((character =  _streamReader.Read()) != -1)
        {
            if (Char.IsWhiteSpace((char)character) && _stringBuilder.Length == 0) // If char is a whitespace and string builder is still empty then continue.
            {
                continue;
            }
            if (Char.IsWhiteSpace((char)character) && !inQuotes) // If char is a whitespace and reader is not in quotes then break.
            {
                break;
            }
            if (character == 0x22 && LastCharacter() != 0x5C) // Is char is a quotation mark and last appended char is not a backslash then invert in quotes position.
            {
                inQuotes = !inQuotes;
            }

            _stringBuilder.Append((char) character);

            if (!inQuotes && (character == 0x28 || character == 0x29)) // If reader is not in quotes and char is opening or closing parenthesis then break.
            {
                break;
            }
            if (!inQuotes && (_streamReader.Peek() == 0x28 || _streamReader.Peek() == 0x29)) // If reader is not in quotes and next read char is opening or closing parenthesis then break.
            {
                break;
            }
        }

        if (_stringBuilder.Length == 0)
        {
            return null;
        }
        return _stringBuilder.ToString();
    }

    public string ReadTokenWithoutQuotes()
    {
        string token = ReadToken();
        if (token != null)
        {
            return token.Replace("\"", "");
        }
        return null;
    }

    private int LastCharacter()
    {
        if (_stringBuilder.Length > 0)
        {
            return _stringBuilder[_stringBuilder.Length - 1];
        }
        return -1;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _streamReader.Close();
        }
        _disposed = true;
    }
}