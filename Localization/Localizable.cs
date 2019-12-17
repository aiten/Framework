/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

using System;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Framework.Localization
{
    public class Localizable
    {
        private ResourceManager _resourceManager;
        private string          _resourceKey;
        private object[]        _placeholders;

        public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;

        public Localizable(ResourceManager resourceManager, string resourceKey, object[] placeholders = null)
        {
            _resourceManager = resourceManager;
            _resourceKey     = resourceKey;
            _placeholders    = placeholders;
        }

        public string ConvertMessageKey()
        {
            return _resourceKey.Replace('_', '.');
        }

        public string GetResourceString()
        {
            return _resourceManager.GetString(_resourceKey, CultureInfo) ?? _resourceManager.GetString(ConvertMessageKey(), CultureInfo);
        }

        public string Message()
        {
            var text = GetResourceString();

            if (_placeholders != null)
            {
                try
                {
                    text = FormattableStringFactory.Create(text, _placeholders).ToString(CultureInfo);
                }
                catch (Exception e)
                {
                    // cannot convert string => set to null
                }
            }

            return text;
        }
    }
}