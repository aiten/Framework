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

namespace Framework.MyUnitTest.Drawing;

using FluentAssertions;

using Framework.Drawing;

using SkiaSharp;

using Xunit;

public class DitherTest
{
    public class DitherTestClass : DitherBase
    {
        protected override void ConvertImage()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Color currentPixel = GetPixel(x, y);

                    if ((x + y) % 2 == 0)
                    {
                        SetPixel(x, y, 255, 255, 255, 255);
                    }
                    else
                    {
                        SetPixel(x, y, 0, 0, 0, 255);
                    }
                }
            }
        }
    }

    [Fact]
    public void ReadWriteImage()
    {
        var dt = new DitherTestClass();

        const int WIDTH  = 100;
        const int HEIGHT = 100;

        var b1 = new SKBitmap(WIDTH, HEIGHT);
        var b2 = dt.Process(b1);

        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                var col = b2.GetPixel(x, y);

                if ((x + y) % 2 == 0)
                {
                    col.Red.Should().Be(255);
                    col.Green.Should().Be(255);
                    col.Blue.Should().Be(255);
                }
                else
                {
                    col.Red.Should().Be(0);
                    col.Green.Should().Be(0);
                    col.Blue.Should().Be(0);
                }
            }
        }
    }

    [Fact]
    public void FloydSteinberg1()
    {
        var dt = new FloydSteinbergDither();

        const int WIDTH  = 100;
        const int HEIGHT = 100;

        var b1 = new SKBitmap(WIDTH, HEIGHT);

        b1.SetPixel(50, 50, new SKColor(255, 255, 255, 255));

        var b2 = dt.Process(b1);

        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                var col = b2.GetPixel(x, y);

                if (col.Red != 0)
                {
                    col.Red.Should().Be(255);
                    col.Green.Should().Be(255);
                    col.Blue.Should().Be(255);

                    x.Should().Be(50);
                    y.Should().Be(50);
                }
                else
                {
                    col.Red.Should().Be(0);
                    col.Green.Should().Be(0);
                    col.Blue.Should().Be(0);
                }
            }
        }
    }

    [Fact]
    public void FloydSteinberg2()
    {
        var dt = new FloydSteinbergDither();

        const int WIDTH  = 100;
        const int HEIGHT = 100;

        var b1 = new SKBitmap(WIDTH, HEIGHT);

        b1.SetPixel(50, 50, new SKColor(127, 127, 127, 255));
        b1.SetPixel(50, 51, new SKColor(127, 127, 127, 255));
        b1.SetPixel(50, 52, new SKColor(127, 127, 127, 255));
        b1.SetPixel(50, 53, new SKColor(127, 127, 127, 255));

        var b2 = dt.Process(b1);

        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                var col = b2.GetPixel(x, y);

                if (col.Red != 0)
                {
                    col.Red.Should().Be(255);
                    col.Green.Should().Be(255);
                    col.Blue.Should().Be(255);

                    x.Should().Be(50);
                    y.Should().BeOneOf(53, 51);
                }
                else
                {
                    col.Red.Should().Be(0);
                    col.Green.Should().Be(0);
                    col.Blue.Should().Be(0);
                }
            }
        }
    }
}