﻿/*
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

namespace Framework.Tools.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    using Abstraction;

    public static class CertificateExtensions
    {
        private static IList<Tuple<string, string>> Split(string name)
        {
            var split = name.Split(new[] { ',', '/' }, StringSplitOptions.RemoveEmptyEntries);

            return split.Select(x =>
            {
                var name2 = x.Split('=');
                if (name2.Length == 2)
                {
                    return new Tuple<string, string>(name2[0].Trim(), name2[1].Trim());
                }

                return new Tuple<string, string>(name2[0].Trim(), String.Empty);
            }).ToList();
        }

        public static ICollection<Tuple<string, string>> List(this X500DistinguishedName name)
        {
            return Split(name.Name);
        }

        public static string Value(this X500DistinguishedName name, string partName)
        {
            var split = name.List();
            return split.FirstOrDefault(x => string.Compare(x.Item1, partName, StringComparison.InvariantCultureIgnoreCase) == 0)?.Item2;
        }

        public static string CN(this X500DistinguishedName name)
        {
            return name.Value("CN");
        }
    }
}