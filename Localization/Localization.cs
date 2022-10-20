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

namespace Framework.Localization;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

using Framework.Localization.Abstraction;

public class LocalizationCollector : ILocalizationCollector
{
    public LocalizationCollector()
    {
        Resources.Add(ErrorMessages.ResourceManager);
    }

    public IList<ResourceManager> Resources { get; set; } = new List<ResourceManager>();

    public Dictionary<string, object> Generate(CultureInfo cultureInfo)
    {
        var result = new Dictionary<string, object>();
        foreach (var mgr in Resources)
        {
            var allInResource = mgr.GetResourceSet(cultureInfo, true, true);

            if (allInResource != null)
            {
                foreach (DictionaryEntry x in allInResource)
                {
                    var keys  = ((string)x.Key).Split('.');
                    var value = (string)x.Value;

                    var current = result;

                    foreach (var key in keys.Take(keys.Length - 1))
                    {
                        if (current.ContainsKey(key))
                        {
                            var val = current[key];
                            if (val is string)
                            {
                                throw new Exception(ErrorMessages.ResourceManager.ToLocalizable(nameof(ErrorMessages.Localization_Configuration_KeyIsUsed), new object[] { (string)x.Key }).Message());
                            }

                            current = (Dictionary<string, object>)val;
                        }
                        else
                        {
                            var newCurrent = new Dictionary<string, object>();
                            current[key] = newCurrent;
                            current      = newCurrent;
                        }
                    }

                    var lastKey = keys.Last();
                    if (current.ContainsKey(lastKey))
                    {
                        throw new Exception(ErrorMessages.ResourceManager.ToLocalizable(nameof(ErrorMessages.Localization_Configuration_DuplicatedKey), new object[] { (string)x.Key }).Message());
                    }

                    current[lastKey] = value;
                }
            }
        }

        return result;
    }
}