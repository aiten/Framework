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

using System;
using System.Linq;
using System.Text;

public class Parser
{
    private string? _error;

    readonly char   _endCommandChar = ';';
    readonly string _spaceChar      = " \t";

    protected ParserStreamReader Reader { get; private set; }

    public Parser(ParserStreamReader reader)
    {
        Reader = reader;
    }

    public Parser(string line)
    {
        Reader = new ParserStreamReader { Line = line };
    }

    public void Reset(ParserStreamReader stream)
    {
        Reader = stream;
    }

    public void Reset(string line)
    {
        Reader = new ParserStreamReader { Line = line };
    }

    public bool IsEOF()              => Reader.IsEOF();
    public char NextChar             => Reader.NextChar;
    public char NextCharToUpper      => char.ToUpper(Reader.NextChar);
    public char NextBButOneChar      => Reader.NextButOneChar;
    public char Next()               => Reader.Next();
    public int  PushPosition()       => Reader.PushPosition();
    public void PopPosition(int pos) => Reader.PopPosition(pos);

    public char DecimalSeparator { get; set; } = '.';

    #region SkipSpaces

    public char SkipSpaces()
    {
        var ch = Reader.NextChar;
        while (!IsEOF() && IsSpaceChar(ch))
        {
            ch = Reader.Next();
        }

        return ch;
    }

    public char SkipSpacesToUpper()
    {
        return char.ToUpper(SkipSpaces());
    }

    public void SkipEndCommand()
    {
        var ch = Reader.NextChar;
        while (!IsEOF() && ch != _endCommandChar)
        {
            ch = Reader.Next();
        }

        if (ch == _endCommandChar)
        {
            Reader.Next();
        }

        SkipSpaces();
    }

    #endregion

    #region IsXXXX

    public bool IsSpaceChar(char ch)
    {
        return _spaceChar.Contains(ch);
    }

    public bool IsEndCommand()
    {
        SkipSpaces();
        return IsEOF() || Reader.NextChar == _endCommandChar;
    }

    public static bool IsAlpha(char ch)
    {
        ch = char.ToUpper(ch);
        return ch == '_' || IsUpperAZ(ch);
    }

    public static bool IsLowerAZ(char ch)
    {
        return ch >= 'a' && ch <= 'z';
    }

    public static bool IsUpperAZ(char ch)
    {
        return ch >= 'A' && ch <= 'Z';
    }

    public static bool IsDigit(char ch)
    {
        return char.IsDigit(ch);
    }

    public bool IsNumber()
    {
        SkipSpaces();
        return IsNumber(Reader.NextChar);
    }

    public bool IsNumber(char ch)
    {
        return ch == '-' || char.IsDigit(ch) || ch == DecimalSeparator;
    }

    #endregion

    #region Get Numbers

    public int GetInt()
    {
        SkipSpaces();

        var numberStr = new StringBuilder();

        if (Reader.NextChar == '-')
        {
            numberStr.Append(Reader.NextChar);
            Reader.Next();
        }

        while (char.IsDigit(Reader.NextChar))
        {
            numberStr.Append(Reader.NextChar);
            Reader.Next();
        }

        int ret = int.Parse(numberStr.ToString());

        SkipSpaces();
        return ret;
    }

    public double GetDouble(out bool isFloatingPoint)
    {
        return (double)GetDecimal(out isFloatingPoint);
    }

    public decimal GetDecimal(out bool isFloatingPoint)
    {
        isFloatingPoint = false;
        SkipSpaces();
        bool negative = Reader.NextChar == '-';
        if (negative)
        {
            Reader.Next();
        }

        decimal val = 0;
        if (Reader.NextChar != DecimalSeparator)
        {
            val = Math.Abs(GetInt()); // -0.4 will return 0 => will be positive
        }

        if (Reader.NextChar == DecimalSeparator)
        {
            isFloatingPoint = true;
            Reader.Next();
            decimal scale = 0.1m;
            while (char.IsDigit(Reader.NextChar))
            {
                val   += scale * (Reader.NextChar - '0');
                scale /= 10;
                Reader.Next();
            }
        }

        if (negative)
        {
            val = -val;
        }

        return val;
    }

    #endregion

    #region GetString(pattern)

    /// <summary>
    /// Read while "pattern" match.
    /// No termination char is needed. 
    /// </summary>
    public bool TryGetString(string pattern)
    {
        var oldPos = Reader.PushPosition();

        int i  = 0;
        var ch = Reader.NextChar;

        foreach (char chPattern in pattern)
        {
            if (ch != chPattern)
            {
                Reader.PopPosition(oldPos);
                return false;
            }

            ch = Reader.Next();
            i++;
        }

        return true;
    }

    public int TryGetString(params string[] patterns)
    {
        int i = 0;
        foreach (string pattern in patterns)
        {
            if (TryGetString(pattern))
            {
                return i;
            }

            i++;
        }

        return -1;
    }

    #endregion

    #region Read

    public string ReadString(params char[] termChar)
    {
        var sb = new StringBuilder();

        while (!IsEOF() && !termChar.Contains(Reader.NextChar))
        {
            sb.Append(Reader.NextChar);
            Reader.Next();
        }

        return sb.ToString();
    }

    public string ReadDigits()
    {
        var str = new StringBuilder();

        while (char.IsDigit(Reader.NextChar))
        {
            str.Append(Reader.NextChar);
            Reader.Next();
        }

        return str.ToString();
    }

    public string ReadToEnd()
    {
        return ReadString(Array.Empty<char>());
    }

    /// <summary>
    /// Read a string until termChar. Do not check for termChar within quote AND remove quote
    /// </summary>
    public string ReadQuotedString(char[] termChar, char[] quote)
    {
        var sb          = new StringBuilder();
        var noQuoteChar = '\0';
        var quoteChar   = noQuoteChar;
        var ch          = Reader.NextChar;

        while (!IsEOF())
        {
            if (ch == quoteChar)
            {
                // end of " or ""
                if (Reader.NextButOneChar == quoteChar)
                {
                    ch = Reader.Next();
                    sb.Append(ch);
                }
                else
                {
                    quoteChar = noQuoteChar;
                }
            }
            else if (quoteChar == noQuoteChar && quote.Contains(ch))
            {
                quoteChar = ch;
            }
            else if (quoteChar == noQuoteChar && termChar.Contains(ch))
            {
                break;
            }
            else
            {
                sb.Append(ch);
            }

            ch = Reader.Next();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Read a string until termChar. Do not check for termChar within quote
    /// </summary>
    public string ReadCountedQuotedString(char[] termChar, char[] quote)
    {
        var sb             = new StringBuilder();
        var quoteChar      = '\0';
        var quoteCharCount = 0;

        var ch = Reader.NextChar;
        while (!IsEOF())
        {
            if (quoteCharCount == 0)
            {
                if (quote.Contains(ch))
                {
                    quoteChar = ch;
                    quoteCharCount++;
                }
                else if (termChar.Contains(ch))

                {
                    return sb.ToString();
                }
            }
            else if (quoteChar == ch)
            {
                quoteCharCount--;
            }

            sb.Append(ch);
            ch = Reader.Next();
        }

        return sb.ToString();
    }

    #endregion

    #region Error

    public bool IsError()
    {
        return !string.IsNullOrEmpty(_error);
    }

    protected void ErrorAdd(string error)
    {
        if (string.IsNullOrEmpty(_error))
        {
            Error(error);
        }
        else
        {
            Error(_error + "\n" + error);
        }
    }

    protected void Error(string error)
    {
        _error = error;
    }

    #endregion
}