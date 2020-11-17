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

namespace Framework.Parser
{
    using System;
    using System.Linq;
    using System.Text;

    public class ParserStreamReader
    {
        #region privat properties/members

        string          _line;
        int             _idx;
        readonly char   _endCommandChar = ';';
        readonly string _spaceChar      = " \t";

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
        public char NextButOneChar  => _idx+1 >= _line.Length ? ((char)0) : _line[_idx+1];

        public int PushPosition()
        {
            return _idx;
        }

        public void PopPosition(int idx)
        {
            _idx = idx;
        }

        public char SkipSpaces()
        {
            while (!IsEOF() && _spaceChar.Contains(_line[_idx]))
            {
                _idx++;
            }

            return NextChar;
        }

        public char SkipSpacesToUpper()
        {
            return char.ToUpper(SkipSpaces());
        }

        public void SkipEndCommand()
        {
            while (!IsEOF() && _line[_idx] != _endCommandChar)
            {
                _idx++;
            }

            if (NextChar == _endCommandChar)
            {
                _idx++;
            }

            SkipSpaces();
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

        public bool IsEndCommand()
        {
            SkipSpaces();
            return IsEOF() || NextChar == _endCommandChar;
        }

        #region next char type

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
            return IsNumber(NextChar);
        }

        public static bool IsNumber(char ch)
        {
            return ch == '-' || char.IsDigit(ch) || ch == '.';
        }

        #endregion

        #region Get Numbers

        public int GetInt()
        {
            SkipSpaces();

            int startIdx = _idx;

            if (NextChar == '-')
            {
                Next();
            }

            while (char.IsDigit(NextChar))
            {
                Next();
            }

            string str = _line.Substring(startIdx, _idx - startIdx);
            int    ret = int.Parse(str);
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
            bool negative = NextChar == '-';
            if (negative)
            {
                Next();
            }

            decimal val = 0;
            if (NextChar != '.')
            {
                val = Math.Abs(GetInt()); // -0.4 will return 0 => will be positive
            }

            if (NextChar == '.')
            {
                isFloatingPoint = true;
                Next();
                decimal scale = 0.1m;
                while (char.IsDigit(NextChar))
                {
                    val   += scale * (NextChar - '0');
                    scale /= 10;
                    Next();
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
        public bool GetString(string pattern)
        {
            if (_line.Length < _idx + pattern.Length)
            {
                return false;
            }

            int i = 0;
            foreach (char ch in pattern)
            {
                if (_line[_idx + i] != ch)
                {
                    return false;
                }

                i++;
            }

            _idx += pattern.Length;
            return true;
        }

        public int GetString(string[] patterns)
        {
            int i = 0;
            foreach (string pattern in patterns)
            {
                if (GetString(pattern))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        #endregion

        #region Read

        public string ReadString(char[] termChar)
        {
            var sb = new StringBuilder();

            while (!IsEOF() && !termChar.Contains(NextChar))
            {
                sb.Append(NextChar);
                Next();
            }

            return sb.ToString();
        }

        public string ReadDigits()
        {
            var str = new StringBuilder();

            while (char.IsDigit(NextChar))
            {
                str.Append(NextChar);
                Next();
            }

            return str.ToString();
        }

        public string ReadToEnd()
        {
            return ReadString(new char[0]);
        }

        /// <summary>
        /// Read a string until terChar. Do not check for termChar within quote AND remove quote
        /// </summary>
        public string ReadQuotedString(char[] termChar, char[] quote)
        {
            var sb          = new StringBuilder();
            var noQuoteChar = '\0';
            var quoteChar   = noQuoteChar;
            var ch          = NextChar;

            while (!IsEOF())
            {
                if (ch == quoteChar)
                {
                    // end of " or ""
                    if (NextButOneChar == quoteChar)
                    {
                        ch = Next();
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

                ch = Next();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Read a string until terChar. Do not check for termChar within quote
        /// </summary>
        public string ReadCountedQuotedString(char[] termChar, char[] quote)
        {
            var sb             = new StringBuilder();
            var quoteChar      = '\0';
            var quoteCharCount = 0;

            var ch = NextChar;
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
                ch = Next();
            }

            return sb.ToString();
        }
    }

    #endregion
}