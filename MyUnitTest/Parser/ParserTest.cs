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

namespace Framework.MyUnitTest.Parser;

using System.Globalization;

using FluentAssertions;

using Framework.Parser;

using Xunit;

public class ParserTest
{
    [Fact]
    public void ParseInt()
    {
        var parser = new Parser("1");

        parser.IsNumber().Should().BeTrue();
        parser.NextChar.Should().Be('1');
        parser.IsEOF().Should().BeFalse();
        parser.Next().Should().Be('\0');
        parser.IsEOF().Should().BeTrue();
    }

    [Theory]
    [InlineData("1")]
    [InlineData("-1")]
    [InlineData("-0")]
    [InlineData("0")]
    public void ParseNumberInt(string line)
    {
        var parser = new Parser(line);

        parser.IsNumber().Should().BeTrue();
        var intValue = parser.GetInt();
        parser.NextChar.Should().Be('\0');
        parser.IsEOF().Should().BeTrue();
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
        var  parser = new Parser(line);
        bool isFloatingPoint;

        parser.IsNumber().Should().BeTrue();
        var decValue = parser.GetDecimal(out isFloatingPoint);
        parser.NextChar.Should().Be('\0');
        parser.IsEOF().Should().BeTrue();
        decValue.Should().Be(decimal.Parse(line, CultureInfo.InvariantCulture));
        isFloatingPoint.Should().Be(shouldBeFloatingPoint);
    }

    [Theory]
    [InlineData("a b c")]
    [InlineData(" a   b   c  ")]
    public void SkipSpacesTest(string line)
    {
        var parser = new Parser(line);

        parser.SkipSpaces().Should().Be('a');
        parser.Next();
        parser.SkipSpaces().Should().Be('b');

        parser.Next();
        parser.SkipSpaces().Should().Be('c');

        parser.Next();
        parser.SkipSpaces();
        parser.IsEOF().Should().BeTrue();
    }

    [Theory]
    [InlineData("Hallo Welt", "Hal")]
    [InlineData("Hallo Welt", "Hallo Welt")]
    public void GetStringTest(string line, string pattern)
    {
        var parser = new Parser(line);

        parser.TryGetString(pattern).Should().BeTrue();

        var remaining = parser.ReadToEnd();
        parser.NextChar.Should().Be('\0');

        line.Should().Be(pattern + remaining);
    }

    [Theory]
    [InlineData("\"Hallo, Welt\"",       "\"Hallo, Welt\"")]
    [InlineData("\"Hallo\", \"Welt\"",   "\"Hallo\"")]
    [InlineData("xx\"Hallo\", \"Welt\"", "xx\"Hallo\"")]
    public void ReadCountedQuotedStringTest(string line, string pattern)
    {
        var parser = new Parser(line);

        parser.ReadCountedQuotedString(new[] { ',' }, new[] { '"' }).Should().Be(pattern);

        var remaining = parser.ReadToEnd();
        parser.NextChar.Should().Be('\0');

        line.Should().Be(pattern + remaining);
    }

    [Theory]
    [InlineData("\"Hallo, Welt\"",             "Hallo, Welt", "")]
    [InlineData("\"Hallo\", \"Welt\"",         "Hallo",       ", \"Welt\"")]
    [InlineData("xx\"Hallo\", \"Welt\"",       "xxHallo",     ", \"Welt\"")]
    [InlineData("\"\"\"Hallo\"\"\", \"Welt\"", "\"Hallo\"",   ", \"Welt\"")]
    public void ReadQuotedStringTest(string line, string pattern, string expectedRemaining)
    {
        var parser = new Parser(line);

        parser.ReadQuotedString(new[] { ',' }, new[] { '"' }).Should().Be(pattern);

        var remaining = parser.ReadToEnd();
        parser.NextChar.Should().Be('\0');

        expectedRemaining.Should().Be(remaining);
    }
}