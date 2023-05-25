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

public abstract class DitherBase
{
    #region private members

    SKColor[]   _rgbValues;
    int _scanSize;

    #endregion

    #region properties

    protected int Height { get; set; }
    protected int Width  { get; set; }

    public int GrayThreshold { get; set; } = 127;

    #endregion

    #region public

    public SKBitmap Process(SKBitmap image)
    {
        ReadImage(image);

        ConvertImage();

        return WriteImage(image);
    }

    #endregion

    protected int ToByteIdx(int x, int y)
    {
        return y * _scanSize + x;
    }

    protected bool IsPixel(int x, int y)
    {
        return x < Width && y < Height;
    }

    protected SKColor GetPixel(int x, int y)
    {
        return _rgbValues[ToByteIdx(x, y)];
    }

    protected void SetPixel(int x, int y, SKColor color)
    {
        _rgbValues[ToByteIdx(x, y)] = color;
    }

    protected void SetPixel(int x, int y, int r, int g, int b, int a)
    {
        SetPixel(x, y, new SKColor((byte)r, (byte)g, (byte)b, (byte)a));
    }

    protected void AddPixel(int x, int y, int r, int g, int b, int a)
    {
        var pixel = GetPixel(x, y);
        SetPixel(x, y, pixel.Red + r, pixel.Green + g, pixel.Blue + b, pixel.Alpha + a);
    }

    protected void AddPixelSaturation(int x, int y, int r, int g, int b, int a)
    {
        var pixel = GetPixel(x, y);
        SetPixel(x, y, pixel.Saturation(r, g, b, a));
    }

    protected void ReadImage(SKBitmap imageX)
    {
        Height    = imageX.Height;
        Width     = imageX.Width;
        _scanSize = Width;

        _rgbValues = imageX.Pixels;
    }

    protected abstract void ConvertImage();

    protected SKBitmap WriteImage(SKBitmap imageX)
    {
        imageX.Pixels = _rgbValues;
        return imageX;
    }
}