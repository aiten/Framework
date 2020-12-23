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

namespace Framework.Tools.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    using Framework.Parser;

    public static class CertificateExtensions
    {
        private static IList<Tuple<string, string>> Split(string name)
        {
            var parser     = new Parser(name);
            var result     = new List<Tuple<string, string>>();
            var nameTerm   = new[] { ' ', '=', ',' };
            var valueTerm  = new[] { ',' };
            var quoteChars = new[] { '\"', '\'' };

            while (!parser.IsEOF())
            {
                parser.SkipSpaces();
                var partName = parser.ReadString(nameTerm);
                var ch       = parser.SkipSpaces();
                if (ch == '=')
                {
                    parser.Next();
                    parser.SkipSpaces();
                    var partValue = parser.ReadQuotedString(valueTerm, quoteChars);
                    partValue = partValue.TrimEnd();
                    result.Add(new Tuple<string, string>(partName, partValue));
                }
                else if (!string.IsNullOrEmpty(partName))
                {
                    result.Add(new Tuple<string, string>(partName, String.Empty));
                }

                if (parser.NextChar == ',')
                {
                    parser.Next();
                }
            }

            return result;
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