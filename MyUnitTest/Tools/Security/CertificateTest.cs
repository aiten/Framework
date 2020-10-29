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

namespace Framework.MyUnitTest.Tools.Security
{
    using System.Security.Cryptography.X509Certificates;

    using FluentAssertions;

    using Framework.Tools.Security;

    using Xunit;

    public class CertificateTest
    {
        #region Common Name

        [Fact]
        public void SplitX500DistinguishedNameLocalhost()
        {
            var name = new X500DistinguishedName("CN = localhost");

            var cn = name.CN();

            cn.Should().Be("localhost");
        }

        [Fact]
        public void SplitX500DistinguishedNameTest()
        {
            var name = new X500DistinguishedName("cn=My Development CA  , OU=My Systems Team, O=A123, L=Linz, S=N12, C=AU");

            var cn = name.CN();

            cn.Should().Be("My Development CA");
        }

        [Fact]
        public void SplitX500DistinguishedNameNotFoundTest()
        {
            var name = new X500DistinguishedName("OU=My Systems Team, O=A123, L=Linz, S=N12, C=AU");

            var cn = name.CN();

            cn.Should().BeNullOrEmpty();
        }

        #endregion
    }
}