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

public class Pbkdf2PasswordProviderTest
{
    [Theory]
    [InlineData("1000:aHMf8WBNLs9XBt9HQiUs261LzEkzwWF0:2YzKPex1DhreMpx3K4iipxO0Jdc=", "Test123456")]
    [InlineData("1000:FimE6/v2L13sJ+G9tPGkXLwhtx4sd2pM:BSxtevme8aJKl/IasbXawJu3qu4=", "1")]
    [InlineData("1000:kFYoTRomfv2aeTnmj9wcZBeaw+2rucq/:WEglHvHAv12voG5ybL/m16XFMPc=", "AaÄäÜü[],.;!")]
    public void ValidatePassword(string passwordEncrypted, string passwordPlain)
    {
        var provider = new Pbkdf2PasswordProvider();

        provider.ValidatePassword(passwordPlain, passwordEncrypted).Should().BeTrue();
    }

    [Theory]
    [InlineData("1000:aHMf8WBNLs9XBt9HQiUs261LzEkzwWF0:2YzKPex1DhreMpx3K4iipxO0Jdc=", "Test123456")]
    [InlineData("1000:FimE6/v2L13sJ+G9tPGkXLwhtx4sd2pM:BSxtevme8aJKl/IasbXawJu3qu4=", "1")]
    [InlineData("1000:kFYoTRomfv2aeTnmj9wcZBeaw+2rucq/:WEglHvHAv12voG5ybL/m16XFMPc=", "AaÄäÜü[],.;!")]
    public void ValidatePasswordFail(string passwordEncrypted, string passwordPlain)
    {
        var provider = new Pbkdf2PasswordProvider();

        provider.ValidatePassword(passwordPlain + " ", passwordEncrypted).Should().BeFalse();
        provider.ValidatePassword(passwordPlain,       passwordEncrypted).Should().BeTrue();
    }

    [Theory]
    [InlineData("Test123456")]
    [InlineData("")]
    [InlineData("AaÄäÜü[],.;!")]
    public void CreatePasswordHash(string passwordPlain)
    {
        var providerHash = new Pbkdf2PasswordProvider();

        var hash = providerHash.GetPasswordHash(passwordPlain);

        hash.Length.Should().BeGreaterThan(20);
        if (passwordPlain.Length > 3)
        {
            hash.Should().NotContain(passwordPlain);
        }

        var providerValidate = new Pbkdf2PasswordProvider();

        providerValidate.ValidatePassword(passwordPlain + " ", hash).Should().BeFalse();
        providerValidate.ValidatePassword(passwordPlain,       hash).Should().BeTrue();
    }
}