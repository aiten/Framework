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

using System.Globalization;

namespace Framework.MyUnitTest.Parser
{
    using System;

    using FluentAssertions;

    using Framework.Parser;

    using Xunit;

    public class ParserStreamTest
    {
        [Fact]
        public void ParseInt()
        {
            var stream = new ParserStreamReader() { Line = "1" };

            stream.IsNumber().Should().BeTrue();
            stream.NextChar.Should().Be('1');
            stream.IsEOF().Should().BeFalse();
            stream.Next().Should().Be('\0');
            stream.IsEOF().Should().BeTrue();
        }

        [Theory]
        [InlineData("1")]
        [InlineData("-1")]
        [InlineData("-0")]
        [InlineData("0")]
        public void ParseNumberInt(string line)
        {
            var stream = new ParserStreamReader() { Line = line };

            stream.IsNumber().Should().BeTrue();
            var intValue = stream.GetInt();
            stream.NextChar.Should().Be('\0');
            stream.IsEOF().Should().BeTrue();
            intValue.Should().Be(int.Parse(line));
        }

        [Theory]
        [InlineData("1",     false)]
        [InlineData("-1",    false)]
        [InlineData("0",     false)]
        [InlineData("1.01",  true)]
        [InlineData("-1.01", true)]
        [InlineData("-0.01", true)]
        [InlineData("0.01",  true)]
        [InlineData(".01",   true)]
        [InlineData("-.01",  true)]
        public void ParseNumberDecimal(string line, bool shouldBeFloatingPoint)
        {
            var  stream = new ParserStreamReader() { Line = line };
            bool isFloatingPoint;

            stream.IsNumber().Should().BeTrue();
            var decValue = stream.GetDecimal(out isFloatingPoint);
            stream.NextChar.Should().Be('\0');
            stream.IsEOF().Should().BeTrue();
            decValue.Should().Be(decimal.Parse(line, CultureInfo.InvariantCulture));
            isFloatingPoint.Should().Be(shouldBeFloatingPoint);
        }

        [Theory]
        [InlineData("a b c")]
        [InlineData(" a   b   c  ")]
        public void SkipSpacesTest(string line)
        {
            var stream = new ParserStreamReader() { Line = line };

            stream.SkipSpaces().Should().Be('a');
            stream.Next();
            stream.SkipSpaces().Should().Be('b');
            ;
            stream.Next();
            stream.SkipSpaces().Should().Be('c');
            ;
            stream.Next();
            stream.SkipSpaces();
            stream.IsEOF().Should().BeTrue();
        }

        [Theory]
        [InlineData("Hallo Welt", "Hal")]
        [InlineData("Hallo Welt", "Hallo Welt")]
        public void GetStringTest(string line, string pattern)
        {
            var stream = new ParserStreamReader() { Line = line };

            stream.GetString(pattern).Should().BeTrue();

            var remaining = stream.ReadToEnd();
            stream.NextChar.Should().Be('\0');

            line.Should().Be(pattern + remaining);
        }

        [Theory]
        [InlineData("\"Hallo, Welt\"",       "\"Hallo, Welt\"")]
        [InlineData("\"Hallo\", \"Welt\"",   "\"Hallo\"")]
        [InlineData("xx\"Hallo\", \"Welt\"", "xx\"Hallo\"")]
        public void ReadCountedQuotedStringTest(string line, string pattern)
        {
            var stream = new ParserStreamReader() { Line = line };

            stream.ReadCountedQuotedString(new[] { ',' }, new[] { '"' }).Should().Be(pattern);

            var remaining = stream.ReadToEnd();
            stream.NextChar.Should().Be('\0');

            line.Should().Be(pattern + remaining);
        }

        [Theory]
        [InlineData("\"Hallo, Welt\"",       "Hallo, Welt", "")]
        [InlineData("\"Hallo\", \"Welt\"",   "Hallo",       ", \"Welt\"")]
        [InlineData("xx\"Hallo\", \"Welt\"", "xxHallo",     ", \"Welt\"")]
        [InlineData("\"\"\"Hallo\"\"\", \"Welt\"", "\"Hallo\"",     ", \"Welt\"")]
        public void ReadQuotedStringTest(string line, string pattern, string expectedRemaining)
        {
            var stream = new ParserStreamReader() { Line = line };

            stream.ReadQuotedString(new[] { ',' }, new[] { '"' }).Should().Be(pattern);

            var remaining = stream.ReadToEnd();
            stream.NextChar.Should().Be('\0');

            expectedRemaining.Should().Be(remaining);
        }
    }
}