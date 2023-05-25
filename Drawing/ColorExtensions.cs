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

using SkiaSharp;

public static class ColorExtensions
{
    public static byte Saturation(int r)
    {
        if (r <= 0)
        {
            return 0;
        }

        if (r > 255)
        {
            return 255;
        }

        return (byte)r;
    }

    public static SKColor Saturation(this SKColor color, int red, int green, int blue, int alpha)
    {
        return new SKColor(
            Saturation((int)color.Red + red),
            Saturation((int)color.Green + green),
            Saturation((int)color.Blue + blue),
            Saturation((int)color.Alpha + alpha)
        );
    }

    public static int Luminance(this SKColor col)
    {
        // o..255 if RGB is Byte
        return (int)(0.2126 * col.Red + 0.7152 * col.Green + 0.0722 * col.Blue);
    }

    public static SKColor FindNearestColorBw(this SKColor col)
    {
        int drb    = 255 - col.Red;
        int dgb    = 255 - col.Green;
        int dbb    = 255 - col.Blue;
        int errorb = drb * drb + dgb * dgb + dbb * dbb;

        int drw    = col.Red;
        int dgw    = col.Green;
        int dbw    = col.Blue;
        int errorw = drw * drw + dgw * dgw + dbw * dbw;

        if (errorw > errorb)
        {
            return new SKColor(255, 255, 255, 255);
        }

        return new SKColor(0, 0, 0, 255);
    }

    public static SKColor FindNearestColorGrayScale(this SKColor col, int grayThreshold)
    {
        if (Luminance(col) > grayThreshold)
        {
            return new SKColor(255, 255, 255, 255);
        }

        return new SKColor(0, 0, 0, 255);
    }
}