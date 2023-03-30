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

namespace Framework.MyUnitTest.Tools.Password;

using FluentAssertions;

using Framework.Tools.Password;

using Xunit;

public class AesPasswordProviderTest
{
    private const string MySecretKey      = "MySecretKey123456.";
    private const string MySecretKeyOther = "MySecretKey123456.!";

    [Theory]
    [InlineData("2RBNwD4SWiuVNY4BETFmCW8xHaOVyvnHM+I8PLVQOiE=", "Test123456")]
    public void ValidatePassword(string passwordEncrypted, string passwordPlain)
    {
        var provider = new AesPasswordProvider(MySecretKey);

        provider.ValidatePassword(passwordPlain, passwordEncrypted).Should().BeTrue();
    }

    [Theory]
    [InlineData("2RBNwD4SWiuVNY4BETFmCW8xHaOVyvnHM+I8PLVQOiE=", "Test123456")]
    public void ValidatePasswordFail(string passwordEncrypted, string passwordPlain)
    {
        var provider = new AesPasswordProvider(MySecretKey);

        provider.ValidatePassword(passwordPlain + " ", passwordEncrypted).Should().BeFalse();
        provider.ValidatePassword(passwordPlain,       passwordEncrypted).Should().BeTrue();
    }

    [Theory]
    [InlineData("Test123456")]
    [InlineData("")]
    [InlineData("AaÄäÜü[],.;!")]
    public void CreatePasswordHash(string passwordPlain)
    {
        var providerHash = new AesPasswordProvider(MySecretKey);

        var hash = providerHash.GetPasswordHash(passwordPlain);

        hash.Length.Should().BeGreaterThan(20);
        if (passwordPlain.Length > 3)
        {
            hash.Should().NotContain(passwordPlain);
        }

        var providerValidate = new AesPasswordProvider(MySecretKey);

        providerValidate.ValidatePassword(passwordPlain + " ", hash).Should().BeFalse();
        providerValidate.ValidatePassword(passwordPlain,       hash).Should().BeTrue();

        var providerValidateOther = new AesPasswordProvider(MySecretKeyOther);

        providerValidateOther.ValidatePassword(passwordPlain, hash).Should().BeFalse();
    }
}