/*
  This file is part of  https://github.com/aiten/Framework.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace Framework.Parser;

public class ParserStreamReader
{
    #region privat properties/members

    string _line = string.Empty;
    int    _idx;

    #endregion

    public string Line
    {
        set
        {
            _line = value;
            _idx  = 0;
        }
        get => _line.Substring(_idx);
    }

    public char NextChar        => IsEOF() ? ((char)0) : _line[_idx];
    public char NextCharToUpper => char.ToUpper(NextChar);
    public char NextButOneChar  => _idx + 1 >= _line.Length ? ((char)0) : _line[_idx + 1];

    public int PushPosition()
    {
        return _idx;
    }

    public void PopPosition(int idx)
    {
        _idx = idx;
    }

    public char Next()
    {
        if (!IsEOF())
        {
            _idx++;
        }

        return NextChar;
    }

    public bool IsEOF()
    {
        return _line.Length <= _idx;
    }
}