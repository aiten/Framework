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

namespace Framework.Drawing;

using System.IO;

using SkiaSharp;

public static class ImageHelperExtensions
{
    public static SKBitmap LoadFromFile(string filePath)
    {
        using (var stream = new StreamReader(filePath))
        {
            return SKBitmap.Decode(stream.BaseStream);
        }
    }

    public static void SaveScaled(this SKBitmap image, int width, int height, int quality, string filePath)
    {
        var newImage = ScaleTo(image, width, height);
        newImage.Save(filePath, SKEncodedImageFormat.Jpeg, quality);
    }

    public static void Save(this SKBitmap image, string filePath, SKEncodedImageFormat format, int quality = 100)
    {
        using (var stream = File.OpenWrite(filePath))
        {
            image.Encode(stream, format, quality);
        }
    }

    public static void Save(this SKBitmap image, MemoryStream memStream, SKEncodedImageFormat imageFormat, int quality = 100)
    {
        using (var wstream = new SKManagedWStream(memStream))
        {
            image.Encode(wstream, imageFormat, quality);
        }
    }

    public static SKBitmap ScaleTo(this SKBitmap image, int resizedWidth, int resizedHeight)
    {
        // obsolete: replaced with the next line: return image.Resize(new SKSizeI(resizedWidth, resizedHeight), SKFilterQuality.High);
        return image.Resize(new SKSizeI(resizedWidth, resizedHeight), new SKSamplingOptions(SKCubicResampler.Mitchell));
    }
}