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

using System.IO;

using FluentAssertions;

using Framework.Drawing;

using Xunit;

public class ImageHelperExtensionsTest
{
    [Fact]
    public void CanReadJpg()
    {
        var bmp = ImageHelperExtensions.LoadFromFile("Drawing/Bitmap/Preview3D.jpg");

        bmp.Height.Should().Be(637);
        bmp.Width.Should().Be(801);
        bmp.BytesPerPixel.Should().Be(4);
    }

    [Fact]
    public void ScaleTo()
    {
        var bmp = ImageHelperExtensions.LoadFromFile("Drawing/Bitmap/Preview3D.jpg");

        int newWidth  = 100;
        int newHeight = 100;

        var newImage = bmp.ScaleTo(newWidth, newHeight);

        newImage.Height.Should().Be(newHeight);
        newImage.Width.Should().Be(newWidth);
    }

    [Fact]
    public void SaveTest()
    {
        var bmp     = ImageHelperExtensions.LoadFromFile("Drawing/Bitmap/Preview3D.jpg");
        var tmpFile = Path.GetTempPath() + "test.jpeg";

        try
        {
            int newWidth  = 100;
            int newHeight = 100;

            bmp.SaveScaled(newWidth, newHeight, 80, tmpFile);

            var bmpFromFile = ImageHelperExtensions.LoadFromFile(tmpFile);

            bmpFromFile.Height.Should().Be(newHeight);
            bmpFromFile.Width.Should().Be(newWidth);
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }
}