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

namespace Framework.CsvImport
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class CsvImportBase
    {
        private readonly NumberFormatInfo _nfi;

        public Encoding Encoding { get; set; } = Encoding.Default;

        public event EventHandler<IList<string>> ReadFirstLine;

        public char ListSeparatorChar { get; set; } = ';';

        public string NewLineInString { get; set; } = "\n";

        public CsvImportBase()
        {
            // Retrieve a writable NumberFormatInfo object.
            CultureInfo enUS = CultureInfo.CreateSpecificCulture("en-US");
            _nfi = enUS.NumberFormat;
        }

        public NumberFormatInfo NumberFormat => _nfi;

        public void SetAustriaNumberFormat()
        {
            _nfi.NumberDecimalSeparator = ",";
            _nfi.NumberGroupSeparator   = ".";
        }

        public IList<IList<string>> ReadStringMatrixFromCsv(string[] lines, bool skipTitleLine)
        {
            var elements       = new List<IList<string>>();
            var lineIdx        = 0;
            var readLineIdx    = 0;
            var compareLineIdx = skipTitleLine ? 1 : 0;

            while (true)
            {
                var row = ReadLine(() =>
                {
                    if (readLineIdx >= lines.Length)
                    {
                        return null;
                    }

                    return lines[readLineIdx++];
                });

                if (row == null)
                {
                    break;
                }

                if (lineIdx == 0)
                {
                    ReadFirstLine?.Invoke(this, row);
                }

                if (lineIdx >= compareLineIdx)
                {
                    elements.Add(row);
                }

                lineIdx++;
            }

            return elements;
        }

        public IList<IList<string>> ReadStringMatrixFromCsv(string fileName, bool skipTitleLine)
        {
            var lines = File.ReadAllLines(fileName, Encoding);
            return ReadStringMatrixFromCsv(lines, skipTitleLine);
        }

        public async Task<IList<IList<string>>> ReadStringMatrixFromCsvAsync(string fileName, bool skipTitleLine)
        {
            var lines = await File.ReadAllLinesAsync(fileName, Encoding);
            return ReadStringMatrixFromCsv(lines, skipTitleLine);
        }

        private IList<string> ReadLine(Func<string> getNextLine)
        {
            var line = getNextLine();
            if (line == null)
            {
                return null;
            }

            var columns     = new List<string>();
            var sb          = new StringBuilder(line.Length);
            var noQuoteChar = '\0';
            var quoteChar   = noQuoteChar;

            while (true)
            {
                for (int idx = 0; idx < line.Length; idx++)
                {
                    var ch = line[idx];

                    if (ch == quoteChar)
                    {
                        // end of " or ""
                        if ((idx + 1) < line.Length && line[idx + 1] == quoteChar)
                        {
                            idx++;
                            sb.Append(ch);
                        }
                        else
                        {
                            quoteChar = noQuoteChar;
                        }
                    }
                    else if (quoteChar == noQuoteChar && ch == '"')
                    {
                        quoteChar = ch;
                    }
                    else if (quoteChar == noQuoteChar && (ch == ListSeparatorChar))
                    {
                        columns.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                if (quoteChar == noQuoteChar)
                {
                    break;
                }

                sb.Append(NewLineInString);

                line = getNextLine();
            }

            columns.Add(sb.ToString());

            return columns;
        }

        public string ExcelString(string excelField)
        {
            return excelField;
        }

        public short ExcelShort(string excelField)
        {
            return short.Parse(excelField);
        }

        public int ExcelInt(string excelField)
        {
            return int.Parse(excelField);
        }

        public long ExcelLong(string excelField)
        {
            return long.Parse(excelField);
        }

        public ushort ExcelUShort(string excelField)
        {
            return ushort.Parse(excelField);
        }

        public uint ExcelUInt(string excelField)
        {
            return uint.Parse(excelField);
        }

        public ulong ExcelULong(string excelField)
        {
            return ulong.Parse(excelField);
        }

        public float ExcelFloat(string excelField)
        {
            return float.Parse(excelField);
        }

        public byte ExcelByte(string excelField)
        {
            return byte.Parse(excelField);
        }

        public bool ExcelBool(string excelField)
        {
            switch (excelField)
            {
                case @"0":
                case @"false":
                    return false;
                case @"1":
                case @"true":
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(excelField), excelField, $@"cannot convert '{excelField}' to 'bool'.");
            }
        }

        public decimal ExcelDecimal(string excelField)
        {
            return decimal.Parse(excelField, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, _nfi);
        }

        public double ExcelDouble(string excelField)
        {
            return double.Parse(excelField, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, _nfi);
        }

        public DateTime ExcelDateDMY(string excelField)
        {
            try
            {
                // Parse date and time with custom specifier.
                // e.g. string dateString = "19.01.2018";
                var format   = "dd.MM.yyyy";
                var provider = CultureInfo.InvariantCulture;

                return DateTime.ParseExact(excelField, format, provider);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DateTime ExcelDateOrDateTime(string excelField)
        {
            if (excelField.Length > "yyyy/MM/dd".Length)
            {
                return ExcelDateTimeYMD(excelField);
            }

            return ExcelDateYMD(excelField);
        }

        public DateTime ExcelDateYMD(string excelField)
        {
            try
            {
                // Parse date and time with custom specifier.
                // e.g. string dateString = "19.01.2018";
                var format   = "yyyy/MM/dd";
                var provider = CultureInfo.InvariantCulture;

                return DateTime.ParseExact(excelField, format, provider);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public object ExcelTimeSpan(string excelField)
        {
            try
            {
                var timeSpan = TimeSpan.Parse(excelField);
                return timeSpan;
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
        }

        public DateTime ExcelDateTimeYMD(string excelField)
        {
            try
            {
                // Parse date and time with custom specifier.
                // e.g. string dateString = "2017/09/20 00:00:00.000";

                var hasFraction = excelField.IndexOf('.') > 0;
                var format      = hasFraction ? "yyyy/MM/dd HH:mm:ss.fff" : "yyyy/MM/dd HH:mm:ss";

                var provider = CultureInfo.InvariantCulture;

                return DateTime.ParseExact(excelField, format, provider);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public object ExcelEnum(Type enumType, string excelField)
        {
            try
            {
                var enumValue = Enum.Parse(enumType, excelField);
                return enumValue;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public byte[] ExcelImage(string excelField)
        {
            byte[] bytes;

            if (excelField.StartsWith(@"0x"))
            {
                if ((excelField.Length % 2) == 1)
                {
                    throw new ArgumentException(@"string has odd length.", nameof(excelField));
                }

                int length = (excelField.Length - 2) / 2;
                int chIdx  = 2;

                bytes = new byte[length];

                for (int i = 0; i < length; i++)
                {
                    bytes[i] =  (byte)(ToHex(excelField[chIdx]) * 16 + ToHex(excelField[chIdx + 1]));
                    chIdx    += 2;
                }
            }
            else
            {
                bytes = System.Convert.FromBase64String(excelField);
            }

            return bytes;
        }

        private int ToHex(char ch)
        {
            if (ch >= '0' && ch <= '9')
            {
                return ch - '0';
            }

            if (ch >= 'a' && ch <= 'f')
            {
                return ch - 'a';
            }

            if (ch >= 'A' && ch <= 'F')
            {
                return ch - 'F';
            }

            throw new ArgumentException(nameof(ch), $@"'{ch}' is not a hex digit.");
        }
    }
}