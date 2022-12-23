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

namespace Framework.MyUnitTest.Tools;

using Framework.Tools;

using System;
using System.Globalization;

using FluentAssertions;

using Xunit;

public class DateTimeExtensionsUnitTest
{
    [Theory]
    [InlineData("2000/01/31 16:02:29", "2000/01/31 16:00:00")]
    [InlineData("2000/01/31 16:02:30", "2000/01/31 16:05:00")]
    [InlineData("2000/01/31 16:03:25", "2000/01/31 16:05:00")]
    [InlineData("2000/01/31 16:57:30", "2000/01/31 17:00:00")]
    [InlineData("2000/01/31 23:59:25", "2000/02/01 00:00:00")]
    void TestDate(string dtAsStr, string expectedAsStr)
    {
        var dt       = DateTime.ParseExact(dtAsStr,       "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        var expected = DateTime.ParseExact(expectedAsStr, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

        dt.RoundMinute(5).Should().Be(expected);
    }
}